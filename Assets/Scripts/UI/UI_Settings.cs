using UnityEngine;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Controls")]
    [SerializeField] private Slider sensitivitySlider;

    private void Start()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogWarning("UI_Settings: AudioManager instance is missing.");
            return;
        }

        if (musicSlider != null)
        {
            musicSlider.value = AudioManager.instance.GetMusicVolume();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            sensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }
    }

    private void SetMusicVolume(float volume)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicVolume(volume);
    }

    private void SetSFXVolume(float volume)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSFXVolume(volume);
    }

    private void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }
}