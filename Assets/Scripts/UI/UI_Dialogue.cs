using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UI_Dialogue : MonoBehaviour
{

    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI dialogueChoices;

    private DialogueLineSO currentDialogue;
    private int currentTextIndex;
    private UI ui;

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
    }

    private void Update()
    {
        if (currentDialogue == null)
            return;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShowNextLine();
        }
    }

    public void PlayDialogueLine(DialogueLineSO line)
    {
        Debug.Log("PlayDialogueLine called");

        if (line == null)
        {
            Debug.LogWarning("No dialogue line provided.");
            return;
        }

        if (line.textLine == null || line.textLine.Length == 0)
        {
            Debug.LogWarning("Dialogue has no text lines.");
            return;
        }

        currentDialogue = line;
        currentTextIndex = 0;

        if (line.speaker != null)
        {
            speakerName.text = line.speaker.speakerName;

            if (line.speaker.speakerPortrait != null)
            {
                speakerPortrait.enabled = true;
                speakerPortrait.sprite = line.speaker.speakerPortrait;
                speakerPortrait.color = Color.white;
            }
            else
            {
                speakerPortrait.enabled = false;
            }
        }
        else
        {
            speakerName.text = "";
            speakerPortrait.enabled = false;
        }

        dialogueChoices.text = "Left click to continue";
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        dialogueText.text = currentDialogue.textLine[currentTextIndex];
    }

    private void ShowNextLine()
    {
        if (currentDialogue == null)
            return;
        currentTextIndex++;

        if (currentTextIndex >= currentDialogue.textLine.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        currentDialogue = null;
        currentTextIndex = 0;

        dialogueText.text = "";
        dialogueChoices.text = "";
        speakerName.text = "";
        speakerPortrait.enabled = false;

        if (ui != null)
            ui.CloseDialogueUI();
    }
}