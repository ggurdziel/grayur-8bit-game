using Articy.Unity;
using System.Collections.Generic;
using UnityEngine;

public class Object_CheckStatus : MonoBehaviour, IInteractable
{
    [Header("Quest (optional)")]
    [Tooltip("Leave empty to skip all quest/item checks and just play the sequence.")]
    [SerializeField] private QuestDataSO quest;

    [Header("Cutscene")]
    [SerializeField] private CutsceneSequence sequence;

    [Header("Flags (optional)")]
    [Tooltip("Set when this plays. Empty = can play every time.")]
    [SerializeField] private string playedFlag;
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private string[] forbiddenFlags;

    [Header("Indicator (e.g. a bobbing arrow child object)")]
    [SerializeField] private GameObject indicator;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 3f;

    [Header("Shown if Marie is missing items")]
    [SerializeField] private ArticyRef[] notReadyLines;

    private Vector3 indicatorStart;

    private void Start()
    {
        if (indicator != null)
            indicatorStart = indicator.transform.localPosition;
    }

    private bool Played()
    {
        return !string.IsNullOrEmpty(playedFlag) &&
               StoryFlags.Instance != null &&
               StoryFlags.Instance.Has(playedFlag);
    }

    private bool QuestOk()
    {
        if (quest == null) return true;   // no quest = no quest requirement

        var qm = QuestManager.Instance;
        return qm != null && qm.HasQuest(quest) && !qm.IsQuestCompleted(quest);
    }

    private bool FlagsOk()
    {
        var flags = StoryFlags.Instance;
        if (flags == null) return true;

        if (requiredFlags != null)
            foreach (string f in requiredFlags)
                if (!string.IsNullOrEmpty(f) && !flags.Has(f)) return false;

        if (forbiddenFlags != null)
            foreach (string f in forbiddenFlags)
                if (!string.IsNullOrEmpty(f) && flags.Has(f)) return false;

        return true;
    }

    private bool CanInteract()
    {
        return !Played() && QuestOk() && FlagsOk();
    }

    private void Update()
    {
        if (indicator == null) return;

        bool show = CanInteract();

        if (indicator.activeSelf != show)
            indicator.SetActive(show);

        if (show)
        {
            Vector3 p = indicatorStart;
            p.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            indicator.transform.localPosition = p;
        }
    }

    public void Interact(Player player)
    {
        Debug.Log($"CheckStatus.Interact played={Played()} questOk={QuestOk()} flagsOk={FlagsOk()}");

        if (player == null || !CanInteract()) return;

        if (quest != null)
        {
            var inv = player.GetComponent<Inventory_Base>();
            if (inv == null) return;

            if (!inv.HasItems(quest.requiredItems))
            {
                PlayNotReady();
                return;
            }

            foreach (var data in quest.requiredItems)
                if (data != null) inv.RemoveItemData(data);

            QuestManager.Instance.CompleteQuest(quest);
        }

        if (!string.IsNullOrEmpty(playedFlag) && StoryFlags.Instance != null)
            StoryFlags.Instance.Set(playedFlag);

        if (sequence != null)
            sequence.Play(player);
    }

    private void PlayNotReady()
    {
        var dm = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
        if (dm == null || notReadyLines == null) return;

        var list = new List<ArticyObject>();
        foreach (var r in notReadyLines)
            if (r != null && r.HasReference) list.Add(r.GetObject());

        if (list.Count > 0)
            dm.StartSequence(list);
    }
}