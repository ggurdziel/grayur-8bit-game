using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        ui = GetComponentInParent<UI>();
    }

    private void Update()
    {
        if (!gameObject.activeSelf || currentDialogue == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            ShowNextLine();
        }
    }

    public void PlayDialogueLine(DialogueLineSO line)
    {
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
            Debug.Log("Portrait is: " + line.speaker.speakerPortrait);

            if (line.speaker.speakerPortrait != null)
            {
                speakerPortrait.enabled = true;
                speakerPortrait.sprite = line.speaker.speakerPortrait;
                speakerPortrait.color = Color.white;
                speakerPortrait.SetNativeSize();
            }
            else
            {
                Debug.LogWarning($"Speaker portrait is missing for {line.speaker.speakerName}");
                speakerPortrait.enabled = false;
            }
        }

        dialogueChoices.text = "Left click to continue";
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentDialogue == null)
            return;

        if (currentTextIndex < 0 || currentTextIndex >= currentDialogue.textLine.Length)
            return;

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

        if (ui != null)
            ui.CloseDialogueUI();
        else
            gameObject.SetActive(false);
    }
}