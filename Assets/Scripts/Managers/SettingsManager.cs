using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    public GameObject settingsCanvas;
    public bool InSettings = false;
    // Set to true in the frame when settings are closed to prevent other
    // systems from also handling the same Escape key press.
    public bool JustClosedSettings = false;

    void Start()
    {
        settingsCanvas.SetActive(false);
    }

    public void OpenSettings()
    {
        InSettings = true;
        settingsCanvas.SetActive(true);
    }

    public void CloseSettings()
    {
        InSettings = false;
        settingsCanvas.SetActive(false);
        JustClosedSettings = true;
    }

    public void Update()
    {
        if (InSettings && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
        }
    }

    // Reset the transient flag at the end of the frame so it only applies
    // to the same frame in which CloseSettings() was called.
    private void LateUpdate()
    {
        JustClosedSettings = false;
    }
}
