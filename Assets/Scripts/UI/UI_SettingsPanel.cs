using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// In-game SettingsPanel: volume slider, Save (opens the slot picker), Quit (to main menu).
public class UI_SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;        // VolumeSlider (found automatically if left empty)
    [SerializeField] private Button saveButton;          // opens the save slot picker
    [SerializeField] private Button quitButton;          // back to the main menu
    [SerializeField] private UI_SaveGamePanel savePanel; // the in-game SavePanel
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string quitConfirmText = "Quit? Unsaved progress is lost";

    private bool confirmingQuit;
    private bool quitting;
    private TMP_Text quitLabel;
    private string quitDefaultText;

    private void Awake()
    {
        if (volumeSlider == null)
            volumeSlider = GetComponentInChildren<Slider>(true);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            Debug.LogWarning("UI_SettingsPanel: no volume slider assigned or found.", this);
        }

        if (saveButton != null) saveButton.onClick.AddListener(OpenSavePanel);
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(Quit);
            quitLabel = quitButton.GetComponentInChildren<TMP_Text>();
            if (quitLabel != null) quitDefaultText = quitLabel.text;
        }
    }

    private void OnEnable()
    {
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(AudioListener.volume);

        CancelQuit();
    }

    private void SetVolume(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMasterVolume(value);
        else
            AudioListener.volume = value;
    }

    private void OpenSavePanel()
    {
        CancelQuit();
        UI_SaveSlotList.ClearFocus(); // so Space in the picker doesn't re-click this button

        if (savePanel != null)
            savePanel.Open();
        else
            Debug.LogWarning("UI_SettingsPanel: no save panel assigned.", this);
    }

    private void Quit()
    {
        if (quitting) return;

        // First press asks, second press quits.
        if (!confirmingQuit)
        {
            confirmingQuit = true;
            if (quitLabel != null) quitLabel.text = quitConfirmText;
            return;
        }

        quitting = true;
        quitButton.interactable = false;
        if (saveButton != null) saveButton.interactable = false;

        PlayerPrefs.Save(); // volume setting

        // Leave the save slot first so the scene change doesn't auto-save on the way out.
        SaveManager.ClearSlot();

        if (GameManager.instance != null)
            GameManager.instance.ChangeScene(mainMenuScene);
        else
            SceneManager.LoadScene(mainMenuScene);
    }

    private void CancelQuit()
    {
        confirmingQuit = false;
        if (quitLabel != null && quitDefaultText != null)
            quitLabel.text = quitDefaultText;
    }
}