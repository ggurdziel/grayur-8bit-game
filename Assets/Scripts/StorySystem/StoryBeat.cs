using Articy.Unity;
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Story Beat")]
public class StoryBeat : ScriptableObject
{
    [Header("Dialogue")]
    public ArticyRef dialogue;

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

    [Header("Outcomes (when dialogue ends)")]
    public string[] flagsToSet;
    public QuestDataSO questToStart;
    public QuestDataSO questToComplete;
    public Inventory_Item[] itemsToGive;

    [Tooltip("If false, the beat plays once and never again.")]
    public bool repeatable = false;

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
    private bool Fail(string reason)
    {
        Debug.Log($"Beat '{name}' blocked: {reason}");
        return false;
    }

    public bool CanPlay(Player player)
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

        if (turnInItem != null && player != null)
            player.GetComponent<Inventory_Player>().RemoveSelectedItem();

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
            ApplyOutcomes();

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
            dialogueManager.StartDialogue(dialogue.GetObject());
        }

        return true;
    }

    private void ApplyOutcomes()
    {
        var flags = StoryFlags.Instance;

        if (!repeatable)
            flags.Set(PlayedFlag);

        foreach (string f in flagsToSet)
            flags.Set(f);

        Debug.Log($"[ApplyOutcomes] itemsToGive = {(itemsToGive == null ? "null" : itemsToGive.Length.ToString())}");

        if (itemsToGive != null && itemsToGive.Length > 0)
        {
            var p = Object.FindFirstObjectByType<Player>();
            var inv = p != null ? p.GetComponent<Inventory_Base>() : null;

            Debug.Log($"[ApplyOutcomes] player={(p != null)}, inventory={(inv != null)}");

            if (inv != null)
            {
                foreach (var item in itemsToGive)
                {
                    string name = item == null ? "NULL ENTRY" : item.itemData == null ? "NO ITEMDATA" : item.itemData.itemName;
                    Debug.Log($"[ApplyOutcomes] giving: {name}, canAdd={(item != null && inv.CanAddItem(item))}");

                    if (item != null) inv.AddItem(item);
                }
            }
        }

        if (questToStart != null &&
            !QuestManager.Instance.HasQuest(questToStart))
            QuestManager.Instance.StartQuest(questToStart);

        if (questToComplete != null)
            QuestManager.Instance.CompleteQuest(questToComplete);
    }
}