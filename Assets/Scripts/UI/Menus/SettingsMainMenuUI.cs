using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SettingsMainMenuUI : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Button backButton;

    private void Start()
    {
        // Initialize volume slider
        float v = PlayerPrefs.GetFloat("MasterVolume", 1f);
        v = Mathf.Clamp(v, 0.001f, 1f);
        volumeSlider.value = v;
        SettingsManager.Instance.SetMasterVolume(v);
        volumeSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMasterVolume);

        // Initialize brightness slider
        float b = PlayerPrefs.GetFloat("Brightness", 1f);
        b = Mathf.Clamp(b, 0f, 1f);
        brightnessSlider.value = b;
        SettingsManager.Instance.SetBrightness(b);
        brightnessSlider.onValueChanged.AddListener(SettingsManager.Instance.SetBrightness);

        // Back button listener
        backButton.onClick.AddListener(CloseSettings);
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
        }
    }

}
