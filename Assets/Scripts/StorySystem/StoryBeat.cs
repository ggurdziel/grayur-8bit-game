using Articy.Unity;
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Story Beat")]
public class StoryBeat : ScriptableObject
{
    [Header("Dialogue")]
    public ArticyRef dialogue;
    [Tooltip("Optional. The flow stops after this line.")]
    public ArticyRef dialogueEnd;

    [Tooltip("Optional. If filled, these lines play in order and the " +
             "Articy connections between them are ignored.")]
    public ArticyRef[] lineSequence;

    [Header("Behavior")]
    [Tooltip("On: locks the player and freezes NPCs (cutscenes). " +
             "Off: quest-style talk, only the listed NPCs pause.")]
    public bool isCutscene = true;

    [Header("Conditions")]
    public string[] requiredFlags;
    public string[] forbiddenFlags;

    [Tooltip("Player must have this item selected in the hotbar. It is removed when the beat plays.")]
    public ItemDataSO turnInItem;

    [Tooltip("Plays only on the quest's turn-in NPC, while the quest is active and the player " +
             "holds all of its required items. The items are removed and the quest completes.")]
    public QuestDataSO turnInQuest;

    [Header("Outcomes (when dialogue ends)")]
    public string[] flagsToSet;
    public QuestDataSO questToStart;
    public QuestDataSO questToComplete;
    public Inventory_Item[] itemsToGive;

    [Tooltip("If false, the beat plays once and never again.")]
    public bool repeatable = false;

    [Header("Success (optional)")]
    [Tooltip("Rewards only happen if the dialogue passed through ANY of these lines. Empty = always succeeds.")]
    public ArticyRef[] successNodes;

    [Tooltip("Set when the dialogue ends, whether or not it succeeded. e.g. max.asked")]
    public string[] flagsToSetAlways;

    private string PlayedFlag => "beat." + name + ".played";

    private bool HasContent
    {
        get
        {
            if (dialogue.HasReference) return true;
            if (lineSequence == null) return false;
            foreach (var r in lineSequence)
                if (r.HasReference) return true;
            return false;
        }
    }

    private bool quiet;

    public bool IsAvailable(Player player, Object_NPC npc = null)
    {
        quiet = true;
        bool ok = CanPlay(player, npc);
        quiet = false;
        return ok;
    }

    private bool Fail(string reason)
    {
        if (!quiet) Debug.Log($"Beat '{name}' blocked: {reason}");
        return false;
    }

    public bool CanPlay(Player player, Object_NPC npc = null)
    {
        var flags = StoryFlags.Instance;

        if (flags == null || !HasContent)
            return Fail("no StoryFlags or no dialogue/lines");

        if (!repeatable && flags.Has(PlayedFlag))
            return Fail("already played");

        foreach (string f in requiredFlags)
            if (!flags.Has(f)) return Fail("missing required flag: " + f);

        foreach (string f in forbiddenFlags)
            if (flags.Has(f)) return Fail("forbidden flag set: " + f);

        if (turnInItem != null)
        {
            var inv = player != null ? player.GetComponent<Inventory_Player>() : null;
            var sel = inv != null ? inv.GetSelectedItem() : null;
            if (sel == null || sel.itemData != turnInItem) return Fail("turn-in item not selected");
        }

        if (turnInQuest != null)
        {
            var qm = QuestManager.Instance;
            var inv = player != null ? player.GetComponent<Inventory_Base>() : null;

            if (qm == null || !qm.HasQuest(turnInQuest) || qm.IsQuestCompleted(turnInQuest))
                return Fail("quest not active");

            if (npc == null || npc.NpcId != turnInQuest.turnInNpcId)
                return Fail("wrong NPC for quest");

            if (inv == null || !inv.HasItems(turnInQuest.requiredItems))
                return Fail("missing quest items");
        }

        if (itemsToGive != null && itemsToGive.Length > 0)
        {
            var inv = player != null ? player.GetComponent<Inventory_Base>() : null;
            if (inv == null) return Fail("no inventory");

            foreach (var item in itemsToGive)
                if (item != null && !inv.CanAddItem(item)) return Fail("cannot give item");
        }

        return true;
    }

    public bool Play(
        Player player,
        DialogueManager dialogueManager,
        Object_NPC[] participants)
    {
        if (dialogueManager == null)
            return false;

        if (isCutscene &&
            (CutsceneManager.Instance == null ||
             CutsceneManager.Instance.IsInCutscene))
            return false;

        if (isCutscene)
        {
            CutsceneManager.Instance.StartCutscene(participants);
        }
        else if (participants != null)
        {
            foreach (var npc in participants)
                if (npc != null) npc.EnterCutscene(); // just pauses wandering
        }

        System.Action onEnded = null;
        onEnded = () =>  
        {
            dialogueManager.DialogueEnded -= onEnded;

            bool hasNodes = false;
            if (successNodes != null)
                foreach (var r in successNodes)
                    if (r.HasReference) { hasNodes = true; break; }

            bool success = !hasNodes || dialogueManager.HasVisitedAny(successNodes);

            ApplyOutcomes(success, player);

            if (isCutscene)
            {
                CutsceneManager.Instance.EndCutscene();
            }
            else if (participants != null)
            {
                foreach (var npc in participants)
                    if (npc != null) npc.ExitCutscene();
            }
        };

        dialogueManager.DialogueEnded += onEnded;

        if (lineSequence != null && lineSequence.Length > 0)
        {
            var lines = new System.Collections.Generic.List<ArticyObject>();
            foreach (ArticyRef r in lineSequence)
                if (r.HasReference) lines.Add(r.GetObject());

            dialogueManager.StartSequence(lines);
        }
        else
        {
            dialogueManager.StartDialogue(dialogue.GetObject(), dialogueEnd.HasReference ? dialogueEnd.GetObject() : null);
        }

        return true;
    }

    private void ApplyOutcomes(bool success, Player player)
    {
        var flags = StoryFlags.Instance;

        foreach (string f in flagsToSetAlways)
            flags.Set(f);

        if (!success)
        {
            Debug.Log($"Beat '{name}' ended without success; it will play again.");
            return;
        }

        var inv = player != null ? player.GetComponent<Inventory_Base>() : null;

        if (turnInItem != null && player != null)
            player.GetComponent<Inventory_Player>().RemoveSelectedItem();

        if (turnInQuest != null && inv != null && turnInQuest.requiredItems != null)
            foreach (var data in turnInQuest.requiredItems)
                if (data != null) inv.RemoveItemData(data);

        if (!repeatable)
            flags.Set(PlayedFlag);

        foreach (string f in flagsToSet)
            flags.Set(f);

        if (itemsToGive != null && inv != null)
            foreach (var item in itemsToGive)
                if (item != null) inv.GiveItem(item);

        if (questToStart != null && !QuestManager.Instance.HasQuest(questToStart))
            QuestManager.Instance.StartQuest(questToStart);

        if (questToComplete != null)
            QuestManager.Instance.CompleteQuest(questToComplete);

        if (turnInQuest != null)
            QuestManager.Instance.CompleteQuest(turnInQuest);
    }
}