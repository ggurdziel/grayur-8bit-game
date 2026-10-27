using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Put this on UI_MainMenu.
public class UI_MainMenu : MonoBehaviour
{
    [Header("Scene to start a new game in")]
    [SerializeField] private string firstGameScene = "House_1";

    [Header("References")]
    [SerializeField] private UI_SaveSlotList saveList; // SaveList
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;

    private bool confirmingOverwrite;
    private bool starting;
    private TMP_Text newGameLabel;
    private string newGameDefaultText;

    private void Start()
    {
        SaveManager.ClearSlot(); // in the menu, no save is active

        newGameButton.onClick.AddListener(NewGame);
        loadGameButton.onClick.AddListener(LoadSelected);
        quitButton.onClick.AddListener(Quit);

        newGameLabel = newGameButton.GetComponentInChildren<TMP_Text>();
        if (newGameLabel != null) newGameDefaultText = newGameLabel.text;

        saveList.SelectionChanged += OnSelectionChanged;
        saveList.Confirmed += LoadSelected;
        saveList.Refresh();

        // Start on the first slot that has a save, otherwise slot 1.
        int start = 0;
        for (int i = 0; i < saveList.SlotCount; i++)
            if (saveList.GetData(i) != null) { start = i; break; }
        saveList.Select(start);
    }

    private void OnSelectionChanged(int slot)
    {
        loadGameButton.interactable = !starting && saveList.GetData(slot) != null;
        CancelOverwrite();
    }

    private void LoadSelected()
    {
        if (starting) return;

        GameData save = saveList.SelectedData;
        if (save == null) return; // empty slot: nothing to load

        SaveManager.SelectSlot(saveList.Selected);

        string scene = string.IsNullOrEmpty(save.sceneName) ? firstGameScene : save.sceneName;
        GoToScene(scene);
    }

    private void NewGame()
    {
        if (starting) return;

        // Prefer the selected slot if it's empty, otherwise the first empty one.
        int slot = saveList.SelectedData == null ? saveList.Selected : saveList.FirstEmptySlot();

        if (slot < 0)
        {
            // All slots full: first press asks, second press overwrites the selected slot.
            if (!confirmingOverwrite)
            {
                confirmingOverwrite = true;
                if (newGameLabel != null) newGameLabel.text = $"Overwrite Save {saveList.Selected + 1}?";
                return;
            }

            slot = saveList.Selected;
        }

        SaveManager.StartNewSlot(slot);
        GoToScene(firstGameScene);
    }

    private void GoToScene(string scene)
    {
        starting = true;
        saveList.InputEnabled = false;
        newGameButton.interactable = false;
        loadGameButton.interactable = false;

        if (GameManager.instance != null)
            GameManager.instance.ChangeScene(scene);
        else
            SceneManager.LoadScene(scene);
    }

    private void CancelOverwrite()
    {
        confirmingOverwrite = false;
        if (newGameLabel != null && newGameDefaultText != null)
            newGameLabel.text = newGameDefaultText;
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}