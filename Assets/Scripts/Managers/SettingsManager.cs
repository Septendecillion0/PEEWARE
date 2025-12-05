using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Image brightnessOverlay;


    /// <summary>
    /// Sets the audio volume for a given mixer group parameter
    /// </summary>
    private void SetAudioVolume(string parameterName, float value)
    {
        // Convert slider 0-1 to dB scale (-80 to 0 dB)
        float volumeInDb = Mathf.Log10(value) * 20;
        audioMixer.SetFloat(parameterName, volumeInDb);
        PlayerPrefs.SetFloat(parameterName, value);
    }

    public void SetMasterVolume(float value)
    {
        SetAudioVolume("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        SetAudioVolume("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        SetAudioVolume("SFXVolume", value);
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
