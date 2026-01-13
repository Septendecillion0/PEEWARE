using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Button backButton;

    private void Start()
    {
        // Initialize master volume slider
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        masterVolume = Mathf.Clamp(masterVolume, 0.001f, 1f);
        volumeSlider.value = masterVolume;
        SettingsManager.Instance.SetMasterVolume(masterVolume);
        volumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMasterVolume);

        // Initialize music volume slider
        float musicVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        musicVolume = Mathf.Clamp(musicVolume, 0.001f, 1f);
        musicSlider.value = musicVolume;
        SettingsManager.Instance.SetMusicVolume(musicVolume);
        musicSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusicVolume);

        // Initialize SFX volume slider
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxVolume = Mathf.Clamp(sfxVolume, 0.001f, 1f);
        sfxSlider.value = sfxVolume;
        SettingsManager.Instance.SetSFXVolume(sfxVolume);
        sfxSlider.onValueChanged.AddListener(SettingsManager.Instance.SetSFXVolume);

        // Initialize brightness slider
        float b = PlayerPrefs.GetFloat("Brightness", 1f);
        b = Mathf.Clamp(b, 0f, 1f);
        brightnessSlider.value = b;
        SettingsManager.Instance.SetBrightness(b);
        brightnessSlider.onValueChanged.AddListener(SettingsManager.Instance.SetBrightness);

        // Back button listener
        backButton.onClick.AddListener(() => GameManager.Instance.SetState(GameManager.GameState.Paused));
    }
}
