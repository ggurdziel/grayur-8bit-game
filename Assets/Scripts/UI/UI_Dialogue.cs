using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Dialogue : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TMP_Text promptText;

    private UI ui;
    private DialogueManager dialogueManager;

    private UI GetUI()
    {
        if (ui == null)
            ui = FindFirstObjectByType<UI>(FindObjectsInactive.Include);
        return ui;
    }

    private DialogueManager GetDialogueManager()
    {
        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
        return dialogueManager;
    }

    private void Update()
    {
        var dm = GetDialogueManager();
        if (dm == null) return;

        var kb = Keyboard.current;

        if (kb == null) return;

        if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
            dm.MoveSelection(-1);

        if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
        {
            dm.MoveSelection(1);
        }
        if (kb.spaceKey.wasPressedThisFrame)
            dm.PressConfirm();
    }

    public void SetPrompt(string text)
    {
        if (promptText != null)
            promptText.text = text;
    }

    public void ShowArticyLine(string speaker, Sprite portrait, string text)
    {
        speakerName.text = speaker ?? "";

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

        dialogueText.text = text ?? "";
    }

    public void EndArticyDialogue()
    {
        dialogueText.text = "";
        SetPrompt("");
        speakerName.text = "";

        speakerPortrait.sprite = null;
        speakerPortrait.enabled = false;

        var u = GetUI();
        if (u != null)
            u.CloseDialogueUI();
    }
}