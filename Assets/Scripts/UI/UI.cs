using UnityEngine;
using UnityEngine.InputSystem;

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
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame)
            ToggleMenu();

        if (dialogueUI != null && dialogueUI.gameObject.activeSelf && kb.escapeKey.wasPressedThisFrame)
        {
            var dm = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
            if (dm != null) dm.EndDialogue();
            else CloseDialogueUI();
        }
    }

    private Player GetPlayer()
    {
        if (player == null)
        {
            player = Player.instance != null ? Player.instance
                                             : FindFirstObjectByType<Player>();
        }
        return player;
    }

    private void StopPlayerControls(bool stopControls)
    {
        var p = GetPlayer();
        if (p == null || p.input == null) return;

        if (stopControls) p.input.Player.Disable();
        else p.input.Player.Enable();
    }

    public void ToggleMenu()
    {
        if (menuUI == null)
            return;

        if (dialogueUI != null && dialogueUI.gameObject.activeSelf) return;

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

    public void CloseDialogueUI()
    {
        dialogueUI.gameObject.SetActive(false);
        if (CutsceneManager.Instance == null ||
            !CutsceneManager.Instance.IsInCutscene)
        {
            StopPlayerControls(false);
        }
    }

    public void OpenArticyDialogueUI()
    {
        if (CutsceneManager.Instance == null ||
            !CutsceneManager.Instance.IsInCutscene)
        {
            StopPlayerControls(true);
        }

        dialogueUI.gameObject.SetActive(true);
    }

}