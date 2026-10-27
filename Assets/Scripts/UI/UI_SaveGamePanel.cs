using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// In-game "choose a slot to save to" screen, opened by the Save button.
// Saves and stays in the game. Put this on the in-game SavePanel
// (a copy of the main menu's SavePanel).
public class UI_SaveGamePanel : MonoBehaviour
{
    [SerializeField] private UI_SaveSlotList saveList;  // SaveList inside this panel
    [SerializeField] private Button saveButton;         // confirms saving to the selected slot
    [SerializeField] private Button backButton;         // closes the panel (optional)
    [SerializeField] private TMP_Text promptText;       // optional, e.g. the panel's Header
    [SerializeField] private string defaultPrompt = "Choose a slot";

    private int pendingOverwrite = -1;

    private void Awake()
    {
        saveButton.onClick.AddListener(SaveSelected);
        if (backButton != null) backButton.onClick.AddListener(Close);

        saveList.SelectionChanged += _ => ResetPrompt();
        saveList.Confirmed += SaveSelected;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        saveList.InputEnabled = true;
        saveList.Refresh();

        // Start on the slot this game came from, otherwise the first empty one.
        int start = SaveManager.CurrentSlot;
        if (start < 0) start = Mathf.Max(0, saveList.FirstEmptySlot());
        saveList.Select(start);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            Close();
    }

    private void SaveSelected()
    {
        int slot = saveList.Selected;
        bool occupied = saveList.SelectedData != null;
        bool ownSlot = slot == SaveManager.CurrentSlot;

        // Overwriting a different save needs a second press to confirm.
        // Saving over the slot this game came from doesn't.
        if (occupied && !ownSlot && pendingOverwrite != slot)
        {
            pendingOverwrite = slot;
            SetPrompt($"Overwrite Save {slot + 1}? Press again");
            return;
        }

        PlayerPrefs.Save(); // volume setting

        if (SaveManager.instance != null)
            SaveManager.instance.SaveGameToSlot(slot);

        // Show the updated slot and confirm.
        saveList.Refresh();
        saveList.Select(slot);
        SetPrompt($"Saved to Save {slot + 1}");
        pendingOverwrite = -1;
    }

    private void SetPrompt(string text)
    {
        if (promptText != null) promptText.text = text;
    }

    private void ResetPrompt()
    {
        pendingOverwrite = -1;
        SetPrompt(defaultPrompt);
    }
}