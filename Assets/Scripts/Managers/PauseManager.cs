using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    public GameObject pauseCanvas;
    private bool isPaused = false;

    private void Start()
    {
        pauseCanvas.SetActive(false);
    }

    private void Update()
    {
        // Skip handling if the game is over, the settings menu were opened or just closed this frame - prevents the Escape key that closed the settings also toggling pause in the same frame.
        if (GameManager.Instance.IsGameOver || SettingsManager.Instance.InSettings || SettingsManager.Instance.JustClosedSettings) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                GameManager.Instance.PauseGame();
            }

            else
            {
                GameManager.Instance.ResumeGame();
            }

            isPaused = !isPaused;
            pauseCanvas.SetActive(isPaused);
        }
    }
}
