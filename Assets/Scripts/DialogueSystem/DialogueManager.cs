using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy._8_Bit_Tutorial_1_;
using Action = System.Action;

public class DialogueManager : MonoBehaviour, IArticyFlowPlayerCallbacks
{
    public event Action DialogueEnded;

    private IList<Branch> currentBranches;
    private bool skipWhenBranchesArrive;

    private Queue<ArticyObject> pendingLines;
    private bool sequenceMode;

    [Header("UI")]
    [SerializeField] private UI_Dialogue dialogueUI;

    [Header("Articy")]
    [SerializeField] private ArticyFlowPlayer flowPlayer;

    [Header("Choices")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    private UI ui;
    

    private void Awake()
    {
        // Find Articy Flow Player
        if (flowPlayer == null)
            flowPlayer = GetComponent<ArticyFlowPlayer>();

        // Find Dialogue UI
        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<UI_Dialogue>(FindObjectsInactive.Include);

        // Find main UI manager
        ui = FindFirstObjectByType<UI>();
    }

    // Called by an NPC when interaction starts
    public void StartDialogue(ArticyObject dialogueStart)
    {
        if (flowPlayer == null)
        {
            Debug.LogError("ArticyFlowPlayer not found.");
            return;
        }

        if (dialogueStart == null)
        {
            Debug.LogWarning("No Articy dialogue assigned.");
            return;
        }

        if (ui == null)
        {
            Debug.LogError("UI manager not found.");
            return;
        }

        Debug.Log("Starting Articy dialogue.");

        currentBranches = null;
        skipWhenBranchesArrive = false;
        sequenceMode = false;
        pendingLines = null;

        // Open the existing dialogue UI and disable player controls
        ui.OpenArticyDialogueUI();

        // Start the Articy flow at the object supplied by the NPC
        flowPlayer.StartOn = dialogueStart;
    }

    private static bool HasValidBranch(IList<Branch> branches)
    {
        if (branches == null) return false;

        foreach (Branch b in branches)
            if (b.IsValid) return true;

        return false;
    }

    // Called by Articy whenever the Flow Player pauses
    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        Debug.Log("Articy dialogue paused!");
        if (aObject == null)
        {
            Debug.Log("Articy paused without a flow object. Continuing...");
            return;
        }

        string text = "";
        string speakerName = "";
        Sprite speakerPortrait = null;

        // -------------------------
        // Dialogue text
        // -------------------------

        var objectWithText =
            aObject as IObjectWithLocalizableText;

        if (objectWithText == null || string.IsNullOrEmpty(objectWithText.Text))
        {
            skipWhenBranchesArrive = true;

            return;
        }
        
        skipWhenBranchesArrive = false;

        text = objectWithText.Text;

        Debug.Log("Articy Text: " + text);

        // -------------------------
        // Speaker
        // -------------------------

        var objectWithSpeaker =
            aObject as IObjectWithSpeaker;

        if (objectWithSpeaker != null)
        {
            var speakerEntity =
                objectWithSpeaker.Speaker as Entity;

            if (speakerEntity != null)
            {
                speakerName = speakerEntity.DisplayName;

                Debug.Log(
                    "Articy Speaker: " + speakerName
                );

                // For now, log the preview image.
                // We can connect this to the Sprite next.
                if (speakerEntity.PreviewImage != null)
                {
                    Debug.Log(
                        "Articy Preview Asset: " +
                        speakerEntity.PreviewImage.Asset
                    );

                    Debug.Log(
                        "Articy Preview Asset Type: " +
                        speakerEntity.PreviewImage.Asset?.GetType()
                    );
                }
            }
        }

        // -------------------------
        // Update Dialogue UI
        // -------------------------

        if (dialogueUI != null)
        {
            dialogueUI.ShowArticyLine(
                speakerName,
                speakerPortrait,
                text
            );
        }
        else
        {
            Debug.LogError(
                "UI_Dialogue reference not found."
            );
        }
    }

    // Called whenever Articy updates the available branches
    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        ClearChoices();
        currentBranches = aBranches;

        Debug.Log("Branches updated: " + aBranches.Count);
        foreach (Branch b in aBranches)
        {
            Debug.Log($"Branch -> {b.Target?.GetType().Name}, valid: {b.IsValid}");
        }

        if (skipWhenBranchesArrive)
        {
            skipWhenBranchesArrive = false;

            if (HasValidBranch(aBranches))
                flowPlayer.Play();
            else
                EndDialogue();
        }
    }

    private void CreateChoice(Branch branch)
    {
        if (choiceButtonPrefab == null ||
            choiceContainer == null)
        {
            Debug.LogWarning(
                "Choice button prefab or choice container is missing."
            );

            return;
        }

        GameObject choiceObject =
            Instantiate(
                choiceButtonPrefab,
                choiceContainer
            );

        BranchChoice choice =
            choiceObject.GetComponent<BranchChoice>();

        if (choice != null)
        {
            choice.AssignBranch(
                flowPlayer,
                branch
            );
        }
        else
        {
            Debug.LogWarning(
                "Choice button prefab does not contain BranchChoice."
            );
        }
    }

    private void ClearChoices()
    {
        if (choiceContainer == null)
            return;

        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    // Called when the player advances normal dialogue
    public void ContinueDialogue()
    {
        if (flowPlayer == null)
            return;

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

    // Called when dialogue ends
    public void EndDialogue()
    {
        ClearChoices();
        currentBranches = null;
        skipWhenBranchesArrive = false;
        sequenceMode = false;
        pendingLines = null;

        if (flowPlayer != null)
        {
            flowPlayer.FinishCurrentPausedObject();
        }

        if (dialogueUI != null)
        {
            dialogueUI.EndArticyDialogue();
        }

        DialogueEnded?.Invoke();
    }


    public void StartSequence(IList<ArticyObject> lines)
    {
        if (flowPlayer == null || ui == null || lines == null || lines.Count == 0)
            return;

        currentBranches = null;
        skipWhenBranchesArrive = false;
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

}