using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    public GameObject settingsMenu;
    public bool InSettings = false;
    // Set to true in the frame when settings are closed to prevent other
    // systems from also handling the same Escape key press.
    public bool JustClosedSettings = false;

    void Start()
    {
        settingsMenu.SetActive(false);
    }

    public void OpenSettings()
    {
        InSettings = true;
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        InSettings = false;
        settingsMenu.SetActive(false);
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
