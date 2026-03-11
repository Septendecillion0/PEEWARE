using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Manages persistent audio and display settings, applying changes to the AudioMixer
/// and saving values to PlayerPrefs for persistence across sessions.
/// Persists across all scenes as a DontDestroyOnLoad singleton.
/// </summary>
/// <remarks>
/// Audio volumes are stored in PlayerPrefs on a 0-1 linear scale and converted
/// to decibels internally before being passed to the AudioMixer.
/// Brightness is implemented as an overlay Image whose alpha is the inverse of the slider value.
/// </remarks>
public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Image brightnessOverlay;


    /// <summary>
    /// Initializes the singleton and marks this object as persistent across scenes.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sets the audio volume for a given mixer group parameter.
    /// Converts a linear 0-1 volume value to decibels and applies it, then saves the raw value to PlayerPrefs.
    /// </summary>
    /// <remarks>
    /// Passing value = 0 will produce -Infinity dB due to Mathf.Log10(0).
    /// Callers should clamp input to a small positive minimum (e.g. 0.0001) to avoid this.
    /// </remarks>
    private void SetAudioVolume(string parameterName, float value)
    {
        // Convert slider 0-1 to dB scale (-80 to 0 dB)
        float volumeInDb = Mathf.Log10(value) * 20;
        audioMixer.SetFloat(parameterName, volumeInDb);
        PlayerPrefs.SetFloat(parameterName, value);
    }

    /// <summary>
    /// Sets the master volume. Expects a linear value in the range 0-1.
    /// </summary>
    public void SetMasterVolume(float value)
    {
        SetAudioVolume("MasterVolume", value);
    }

    /// <summary>
    /// Sets the background music volume. Expects a linear value in the range 0-1.
    /// </summary>
    public void SetMusicVolume(float value)
    {
        SetAudioVolume("BGMVolume", value);
    }

    /// <summary>
    /// Sets the sound effects volume. Expects a linear value in the range 0-1.
    /// </summary>
    public void SetSFXVolume(float value)
    {
        SetAudioVolume("SFXVolume", value);
    }

    /// <summary>
    /// Adjusts screen brightness via a full-screen dark overlay.
    /// A slider value of 1 is fully transparent (brightest); 0 is fully opaque (darkest).
    /// Saves the value to PlayerPrefs.
    /// </summary>
    public void SetBrightness(float value)
    {
        Color c = brightnessOverlay.color;
        // Invert value: slider 0 = black overlay, slider 1 = transparent
        c.a = 1f - value;
        brightnessOverlay.color = c;
        PlayerPrefs.SetFloat("Brightness", value);
    }
}
