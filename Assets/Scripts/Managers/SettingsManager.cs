using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Image brightnessOverlay;

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
