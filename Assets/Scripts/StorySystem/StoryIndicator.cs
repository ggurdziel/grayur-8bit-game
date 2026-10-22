using UnityEngine;

// Shows a bobbing marker while its conditions are met, hides it when they stop.
// Conditions are optional and combine with AND, except Beats (shows if ANY can play).
// On an Object_NPCStory, leave Beats empty and it uses the NPC's own beats.
public class StoryIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicator;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 3f;

    [Header("Show when (all optional)")]
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private string[] forbiddenFlags;

    [Tooltip("Shows only while this quest is started and not completed.")]
    [SerializeField] private QuestDataSO quest;

    [Tooltip("With a Quest set: only show once the player is carrying every required item.")]
    [SerializeField] private bool requireQuestItems;

    [Tooltip("Shows if ANY of these beats can currently play. Leave empty on an Object_NPCStory to use its own beats.")]
    [SerializeField] private StoryBeat[] beats;

    [Tooltip("Optional: hides once this cutscene has played.")]
    [SerializeField] private CutsceneSequence hideAfter;

    [Tooltip("Hide the marker while a cutscene is running.")]
    [SerializeField] private bool hideDuringCutscenes = true;

    private Vector3 start;
    private Object_NPC npc;
    private Object_NPCStory npcStory;

    private void Awake()
    {
        npc = GetComponent<Object_NPC>();
        npcStory = GetComponent<Object_NPCStory>();
        if (indicator != null) start = indicator.transform.localPosition;
    }

    private bool ShouldShow()
    {
        var flags = StoryFlags.Instance;
        if (flags == null) return false;

        if (hideDuringCutscenes &&
            CutsceneManager.Instance != null &&
            CutsceneManager.Instance.IsInCutscene)
            return false;

        if (hideAfter != null && hideAfter.HasPlayed) return false;

        if (requiredFlags != null)
            foreach (var f in requiredFlags)
                if (!string.IsNullOrEmpty(f) && !flags.Has(f)) return false;

        if (forbiddenFlags != null)
            foreach (var f in forbiddenFlags)
                if (!string.IsNullOrEmpty(f) && flags.Has(f)) return false;

        if (quest != null)
        {
            var qm = QuestManager.Instance;
            if (qm == null || !qm.HasQuest(quest) || qm.IsQuestCompleted(quest)) return false;

            if (requireQuestItems)
            {
                var inv = Player.instance != null ? Player.instance.GetComponent<Inventory_Base>() : null;
                if (inv == null || !inv.HasItems(quest.requiredItems)) return false;
            }
        }

        StoryBeat[] list = (beats != null && beats.Length > 0)
            ? beats
            : (npcStory != null ? npcStory.Beats : null);

        bool hasBeats = list != null && list.Length > 0;
        bool hasSeq = npcStory != null && npcStory.HasSequence;

        if (hasBeats || hasSeq)
        {
            bool any = false;

            if (hasSeq && npcStory.SequenceAvailable) any = true;

            if (!any && hasBeats)
            {
                var player = Player.instance;
                foreach (var b in list)
                    if (b != null && b.IsAvailable(player, npc)) { any = true; break; }
            }

            if (!any) return false;
        }

        return true;
    }

    private void Update()
    {
        if (indicator == null) return;

        bool show = ShouldShow();
        if (indicator.activeSelf != show) indicator.SetActive(show);

        if (show)
        {
            var p = start;
            p.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            indicator.transform.localPosition = p;
        }
    }
}