using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    [Header("Brightness Settings")]
    [SerializeField] private Image brightnessOverlay;
    [SerializeField] private Slider brightnessSlider;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    private void Start()
    {
        // Initialize volume slider
        float v = PlayerPrefs.GetFloat("MasterVolume", 1f);
        v = Mathf.Clamp(v, 0.001f, 1f);
        volumeSlider.value = v;
        SetMasterVolume(v);
        volumeSlider.onValueChanged.AddListener(SetMasterVolume);

        // Initialize brightness slider
        float b = PlayerPrefs.GetFloat("Brightness", 1f);
        b = Mathf.Clamp(b, 0f, 1f);
        brightnessSlider.value = b;
        SetBrightness(b);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);

        // Back button listener
        backButton.onClick.AddListener(SettingsManager.Instance.CloseSettings);
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp(value, 0.001f, 1f);
        // slider 0-1 -> dB
        float volume = Mathf.Log10(value) * 20;
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetBrightness(float value)
    {
        Color c = brightnessOverlay.color;
        // Invert value: slider 0 = black overlay, slider 1 = transparent
        c.a = 1f - value;
        brightnessOverlay.color = c;
        PlayerPrefs.SetFloat("Brightness", value);
    }
}
