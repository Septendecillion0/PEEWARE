using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    [Header("Back Button")]
    [SerializeField] private Button backButton;

    private void Start()
    {
        float v = PlayerPrefs.GetFloat("MasterVolume", 1f);
        volumeSlider.value = v;
        SetMasterVolume(v);
        volumeSlider.onValueChanged.AddListener(SetMasterVolume);

        backButton.onClick.AddListener(SettingsManager.Instance.CloseSettings);
    }

    public void SetMasterVolume(float value)
    {
        float volume = Mathf.Log10(value) * 20;   // slider 0~1 → dB
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}
