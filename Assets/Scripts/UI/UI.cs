using UnityEngine;

public class UI : MonoBehaviour
{
    public UI_Dialogue dialogueUI { get; private set; }
    private Player player;

    private void Awake()
    {
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);
        player = FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        if (dialogueUI.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseDialogueUI();
        }
    }

    private void StopPlayerControls(bool stopControls)
    {
        if (player == null || player.input == null)
            return;

        if (stopControls)
            player.input.Player.Disable();
        else
            player.input.Player.Enable();
    }

    public void OpenDialogueUI(DialogueLineSO firstLine)
    {
        StopPlayerControls(true);
        dialogueUI.gameObject.SetActive(true);
        dialogueUI.PlayDialogueLine(firstLine);
    }

    public void CloseDialogueUI()
    {
        dialogueUI.gameObject.SetActive(false);
        StopPlayerControls(false);
    }
}