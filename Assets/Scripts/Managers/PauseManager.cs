using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    public GameObject pausePanel;
    private bool isPaused = false;
    public GameObject settingsPanel;
    public bool InSettings = false;

    private void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // Skip handling if the game is over
        if (GameManager.Instance.IsGameOver) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (InSettings)
            {
                CloseSettings();
                return;
            }

            if (!isPaused)
                OpenPauseMenu();
            else
                ClosePauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        GameManager.Instance.PauseGame();
        isPaused = true;
        pausePanel.SetActive(true);
    }

    public void ClosePauseMenu()
    {
        GameManager.Instance.ResumeGame();
        isPaused = false;
        pausePanel.SetActive(false);
    }

    public void OpenSettings()
    {
        InSettings = true;
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        InSettings = false;
        settingsPanel.SetActive(false);
    }
}
