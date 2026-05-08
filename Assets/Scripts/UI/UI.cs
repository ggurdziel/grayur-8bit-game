using UnityEngine;

public class UI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_Menu menuUI;
    [SerializeField] private UI_Dialogue dialogueUI;

    private Player player;

    private void Awake()
    {
        player = FindFirstObjectByType<Player>();

        if (menuUI != null)
            menuUI.CloseMenu();

        if (dialogueUI != null)
            dialogueUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleMenu();
        }

        if (dialogueUI != null && dialogueUI.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
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

    public void ToggleMenu()
    {
        if (menuUI == null)
            return;

        if (menuUI.IsOpen())
        {
            menuUI.CloseMenu();
            StopPlayerControls(false);
        }
        else
        {
            StopPlayerControls(true);
            menuUI.OpenMenu();
        }
    }

    public void OpenDialogueUI(DialogueLineSO firstLine)
    {
        if (dialogueUI == null)
        {
            Debug.LogError("Dialogue UI reference is missing on UI_Manager.");
            return;
        }

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