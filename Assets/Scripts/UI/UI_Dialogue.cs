using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UI_Dialogue : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI dialogueChoices;

    private UI ui;
    private DialogueManager dialogueManager;

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            ContinueDialogue();
        }
    }

    private void ContinueDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager not found.");
            return;
        }

        dialogueManager.ContinueDialogue();
    }

    public void ShowArticyLine(
        string speaker,
        Sprite portrait,
        string text)
    {
        // Speaker name
        speakerName.text = speaker ?? "";

        // Speaker portrait
        if (portrait != null)
        {
            speakerPortrait.enabled = true;
            speakerPortrait.sprite = portrait;
            speakerPortrait.color = Color.white;
        }
        else
        {
            speakerPortrait.enabled = false;
        }

        // Dialogue text
        dialogueText.text = text ?? "";

        // Temporary until choice UI is connected
        dialogueChoices.text = "Left click to continue";
    }

    public void EndArticyDialogue()
    {
        dialogueText.text = "";
        dialogueChoices.text = "";
        speakerName.text = "";

        speakerPortrait.sprite = null;
        speakerPortrait.enabled = false;

        if (ui != null)
        {
            ui.CloseDialogueUI();
        }
    }
}