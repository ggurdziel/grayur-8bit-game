using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy._8_Bit_Tutorial_1_;
using Action = System.Action;

public class DialogueManager : MonoBehaviour, IArticyFlowPlayerCallbacks
{
    public event Action DialogueEnded;

    // True while any dialogue box is open. NPCs read this to stop wandering.
    public static bool IsDialogueOpen { get; private set; }

    // When the last dialogue closed. Player.cs uses this so the press that
    // closes a dialogue can't immediately start it again.
    public static float LastClosedTime { get; private set; } = -10f;

    [Header("UI")]
    [SerializeField] private UI_Dialogue dialogueUI;

    [Header("Articy")]
    [SerializeField] private ArticyFlowPlayer flowPlayer;

    [Header("Choices")]
    [SerializeField] private BranchChoice[] choiceRows;
    private readonly List<BranchChoice> choiceItems = new List<BranchChoice>();

    [Header("Portraits")]
    [SerializeField] private PortraitDatabaseSO portraitDatabase;

    private UI ui;

    private IList<Branch> currentBranches;
    private bool skipWhenBranchesArrive;

    private bool choicesShowing;
    private int selectedIndex;

    private Queue<ArticyObject> pendingLines;
    private bool sequenceMode;
    private bool dialogueActive;

    private ArticyObject endNode;
    private bool atEndNode;
    private readonly HashSet<string> visited = new HashSet<string>();

    // Space is both "Interact" and "Confirm": ignore confirm right after opening.
    private float openedTime = -10f;
    private const float openGuardSeconds = 0.2f;

    private const string EnterHintFlag = "tutorial.hint.enter";
    private const string ChoiceHintFlag = "tutorial.hint.choices";

    private static bool Seen(string flag) =>
        StoryFlags.Instance != null && StoryFlags.Instance.Has(flag);

    private static void MarkSeen(string flag)
    {
        if (StoryFlags.Instance != null && !StoryFlags.Instance.Has(flag))
            StoryFlags.Instance.Set(flag);
    }

    private void Awake()
    {
        IsDialogueOpen = false;

        if (flowPlayer == null)
            flowPlayer = GetComponent<ArticyFlowPlayer>();

        EnsureUI();
    }

    private void OnDestroy()
    {
        // A scene change mid-dialogue must not leave NPCs frozen.
        IsDialogueOpen = false;
    }

    private void EnsureUI()
    {
        if (ui == null)
            ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);

        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<UI_Dialogue>(FindObjectsInactive.Include);
    }

    private void ResetState()
    {
        currentBranches = null;
        skipWhenBranchesArrive = false;
        sequenceMode = false;
        pendingLines = null;
        atEndNode = false;
        endNode = null;
    }

    // ------------------------------------------------------------
    // Starting dialogue
    // ------------------------------------------------------------

    // Plays an Articy flow from dialogueStart. If dialogueEnd is given,
    // the flow stops after that line, even if the graph continues.
    public void StartDialogue(ArticyObject dialogueStart, ArticyObject dialogueEnd = null)
    {
        EnsureUI();

        if (flowPlayer == null) { Debug.LogError("StartDialogue: no ArticyFlowPlayer"); return; }
        if (ui == null)         { Debug.LogError("StartDialogue: no UI found"); return; }
        if (dialogueStart == null) { Debug.LogWarning("StartDialogue: no Articy dialogue assigned."); return; }

        RefreshChoiceRows();
        ClearChoices();
        ResetState();
        visited.Clear();
        endNode = dialogueEnd;
        dialogueActive = true;
        IsDialogueOpen = true;
        openedTime = Time.unscaledTime;

        ui.OpenArticyDialogueUI();
        flowPlayer.StartOn = dialogueStart;
    }

    public void StartSequence(IList<ArticyObject> lines)
    {
        EnsureUI();

        if (flowPlayer == null) { Debug.LogError("StartSequence: no ArticyFlowPlayer"); return; }
        if (ui == null)         { Debug.LogError("StartSequence: no UI found"); return; }
        if (lines == null || lines.Count == 0) { Debug.LogError("StartSequence: no lines resolved"); return; }

        RefreshChoiceRows();
        ClearChoices();
        ResetState();
        visited.Clear();
        dialogueActive = true;
        IsDialogueOpen = true;
        openedTime = Time.unscaledTime;
        sequenceMode = true;
        pendingLines = new Queue<ArticyObject>(lines);

        ui.OpenArticyDialogueUI();
        PlayNextQueued();
    }

    private bool PlayNextQueued()
    {
        if (pendingLines == null || pendingLines.Count == 0)
            return false;

        flowPlayer.StartOn = pendingLines.Dequeue();
        return true;
    }

    private void RefreshChoiceRows()
    {
        EnsureUI();

        if (dialogueUI != null)
        {
            // Only the rows that belong to the dialogue box we're showing.
            choiceRows = dialogueUI.GetComponentsInChildren<BranchChoice>(true);
        }
        else
        {
            choiceRows = FindObjectsByType<BranchChoice>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            System.Array.Sort(choiceRows, (a, b) =>
                a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        }
    }

    private bool IsEndNode(IFlowObject o)
    {
        if (endNode == null) return false;

        var a = o as ArticyObject;
        return a != null && a.Id == endNode.Id;   // use TechnicalName if Id doesn't compile
    }

    // ------------------------------------------------------------
    // Articy callbacks
    // ------------------------------------------------------------

    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        if (!dialogueActive || aObject == null)
            return;

        atEndNode = IsEndNode(aObject);

        var node = aObject as ArticyObject;
        if (node != null) visited.Add(node.Id.ToString());

        var withText = aObject as IObjectWithLocalizableText;

        if (withText == null || string.IsNullOrEmpty(withText.Text))
        {
            skipWhenBranchesArrive = true;
            return;
        }

        skipWhenBranchesArrive = false;

        string speakerName = "";
        Sprite speakerPortrait = null;

        var withSpeaker = aObject as IObjectWithSpeaker;
        if (withSpeaker != null && withSpeaker.Speaker is Entity speaker)
        {
            speakerName = speaker.DisplayName;
            speakerPortrait = portraitDatabase != null ? portraitDatabase.Get(speakerName) : null;
        }

        EnsureUI();

        if (dialogueUI == null)
        {
            Debug.LogError("UI_Dialogue not found.");
            return;
        }

        dialogueUI.ShowArticyLine(speakerName, speakerPortrait, withText.Text);
        dialogueUI.SetPrompt(Seen(EnterHintFlag) ? "" : "Press Space to continue");
    }

    public bool HasVisited(ArticyObject node)
    {
        return node != null && visited.Contains(node.Id.ToString());
    }

    public bool HasVisitedAny(IEnumerable<ArticyRef> nodes)
    {
        if (nodes == null) return false;

        foreach (var r in nodes)
            if (r != null && r.HasReference && HasVisited(r.GetObject()))
                return true;

        return false;
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        if (!dialogueActive)
            return;

        ClearChoices();
        currentBranches = aBranches;

        // Textless node: skip through it.
        if (skipWhenBranchesArrive)
        {
            skipWhenBranchesArrive = false;

            if (atEndNode)
            {
                EndDialogue();
            }
            else if (sequenceMode)
            {
                if (!PlayNextQueued())
                    EndDialogue();
            }
            else if (HasValidBranch(aBranches))
            {
                flowPlayer.Play();
            }
            else
            {
                EndDialogue();
            }

            return;
        }

        // The end line shows normally; the next press closes. No choices after it.
        if (atEndNode)
            return;

        if (sequenceMode)
            return;

        var options = new List<Branch>();

        foreach (Branch b in aBranches)
            if (b.IsValid)
                options.Add(b);

        bool isChoice = options.Count > 1 ||
            (options.Count == 1 && !string.IsNullOrEmpty(GetMenuText(options[0])));

        if (!isChoice)
            return;

        foreach (Branch b in options)
            CreateChoice(b);

        if (choiceItems.Count == 0)
        {
            // Wait here instead of letting a press auto-pick a branch.
            Debug.LogError("[Dialogue] A choice was expected but no choice rows were found. " +
                           "Check the BranchChoice rows under UI_Dialogue.");
            return;
        }

        choicesShowing = true;
        selectedIndex = 0;
        RefreshSelection();
        dialogueUI?.SetPrompt(Seen(ChoiceHintFlag) ? "" : "Up/Down to choose, Space to confirm");
    }

    private static bool HasValidBranch(IList<Branch> branches)
    {
        if (branches == null) return false;

        foreach (Branch b in branches)
            if (b.IsValid) return true;

        return false;
    }

    private static string GetMenuText(Branch b)
    {
        var m = b.Target as IObjectWithLocalizableMenuText;
        if (m == null) return "";

        try { return m.MenuText ?? ""; }
        catch { return ""; }   // empty ArticyString throws on conversion
    }

    // ------------------------------------------------------------
    // Advancing, choosing, ending
    // ------------------------------------------------------------

    // Space: picks the highlighted choice if choices are showing, otherwise advances.
    public void PressConfirm()
    {
        // The press that opened the dialogue must not also act inside it.
        if (Time.unscaledTime - openedTime < openGuardSeconds)
            return;

        if (choicesShowing)
        {
            ConfirmSelection();
        }
        else
        {
            MarkSeen(EnterHintFlag);
            ContinueDialogue();
        }
    }

    public void ContinueDialogue()
    {
        if (!dialogueActive || flowPlayer == null)
            return;

        if (choicesShowing)
            return;

        if (atEndNode)
        {
            EndDialogue();
            return;
        }

        if (sequenceMode)
        {
            if (!PlayNextQueued())
                EndDialogue();
            return;
        }

        if (!HasValidBranch(currentBranches))
        {
            EndDialogue();
            return;
        }

        flowPlayer.Play();
    }

    public void MoveSelection(int delta)
    {
        if (!choicesShowing || choiceItems.Count == 0)
            return;

        selectedIndex = (selectedIndex + delta + choiceItems.Count) % choiceItems.Count;
        RefreshSelection();
    }

    public void ConfirmSelection()
    {
        if (!choicesShowing || choiceItems.Count == 0)
            return;

        MarkSeen(ChoiceHintFlag);

        Branch chosen = choiceItems[selectedIndex].Branch;
        SelectBranch(chosen);
    }

    private void SelectBranch(Branch branch)
    {
        if (!dialogueActive || branch == null || flowPlayer == null)
            return;

        ClearChoices();
        currentBranches = null;

        if (dialogueUI != null)
            dialogueUI.SetPrompt("");

        flowPlayer.Play(branch);
    }

    public void EndDialogue()
    {
        if (!dialogueActive)
            return;

        dialogueActive = false;
        IsDialogueOpen = false;               // NPCs may move again
        LastClosedTime = Time.unscaledTime;   // Player.cs ignores Interact for a moment

        ClearChoices();
        ResetState();

        if (flowPlayer != null)
            flowPlayer.FinishCurrentPausedObject();

        EnsureUI();

        if (dialogueUI != null)
            dialogueUI.EndArticyDialogue();

        DialogueEnded?.Invoke();
    }

    // ------------------------------------------------------------
    // Choice list
    // ------------------------------------------------------------

    private void CreateChoice(Branch branch)
    {
        int slot = choiceItems.Count;

        if (choiceRows == null || slot >= choiceRows.Length)
        {
            Debug.LogWarning("More choices than rows (max " + (choiceRows?.Length ?? 0) + ").");
            return;
        }

        choiceRows[slot].Assign(branch);
        choiceItems.Add(choiceRows[slot]);
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < choiceItems.Count; i++)
            choiceItems[i].SetSelected(i == selectedIndex);
    }

    private void ClearChoices()
    {
        choicesShowing = false;
        choiceItems.Clear();
        selectedIndex = 0;

        if (choiceRows == null) return;

        foreach (var row in choiceRows)
            if (row != null) row.Clear();
    }
}