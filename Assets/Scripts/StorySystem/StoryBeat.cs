using Articy.Unity;
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Story Beat")]
public class StoryBeat : ScriptableObject
{
    [Header("Dialogue")]
    public ArticyRef dialogue;

    [Header("Conditions")]
    public string[] requiredFlags;
    public string[] forbiddenFlags;

    [Tooltip("Optional. If filled, these lines play in order and the " +
         "Articy connections between them are ignored.")]
    public ArticyRef[] lineSequence;

    private bool HasContent =>
        dialogue.HasReference ||
        (lineSequence != null && lineSequence.Length > 0);

    [Tooltip("If set, the player must be holding this quest's item. " +
             "The item is handed in and the quest completes.")]
    public QuestDataSO turnInQuest;

    [Header("Outcomes (when dialogue ends)")]
    public string[] flagsToSet;
    public QuestDataSO questToStart;

    [Tooltip("If false, the beat plays once and never again.")]
    public bool repeatable = false;

    private string PlayedFlag => "beat." + name + ".played";

    public bool CanPlay(Player player)
    {
        var flags = StoryFlags.Instance;

        if (flags == null || !HasContent)
            return false;

        if (!repeatable && flags.Has(PlayedFlag))
            return false;

        foreach (string f in requiredFlags)
            if (!flags.Has(f)) return false;

        foreach (string f in forbiddenFlags)
            if (flags.Has(f)) return false;

        if (turnInQuest != null && !HasTurnInItem(player))
            return false;

        return true;
    }

    private bool HasTurnInItem(Player player)
    {
        var qm = QuestManager.Instance;

        if (qm == null || player == null ||
            !qm.HasQuest(turnInQuest) ||
            qm.IsQuestCompleted(turnInQuest))
            return false;

        var inv = player.GetComponent<Inventory_Player>();
        var selected = inv != null ? inv.GetSelectedItem() : null;

        return selected != null &&
               selected.itemData == turnInQuest.itemToCollect;
    }

    public bool Play(
        Player player,
        DialogueManager dialogueManager,
        Object_NPC[] participants)
    {
        if (dialogueManager == null ||
            CutsceneManager.Instance == null ||
            CutsceneManager.Instance.IsInCutscene)
            return false;

        if (turnInQuest != null)
        {
            player.GetComponent<Inventory_Player>().RemoveSelectedItem();
            QuestManager.Instance.CompleteQuest(turnInQuest);
        }

        CutsceneManager.Instance.StartCutscene(participants);

        System.Action onEnded = null;
        onEnded = () =>
        {
            dialogueManager.DialogueEnded -= onEnded;
            ApplyOutcomes();
            CutsceneManager.Instance.EndCutscene();
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

        if (questToStart != null &&
            !QuestManager.Instance.HasQuest(questToStart))
            QuestManager.Instance.StartQuest(questToStart);
    }
}