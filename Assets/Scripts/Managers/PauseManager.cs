using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    public GameObject pausePanel;
    public GameObject settingsPanel;

    private void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        var state = GameManager.Instance.State;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == GameManager.GameState.Playing)
                GameManager.Instance.SetState(GameManager.GameState.Paused);
            else if (state == GameManager.GameState.Paused)
                GameManager.Instance.SetState(GameManager.GameState.Playing);
            else if (state == GameManager.GameState.InSettings)
                GameManager.Instance.SetState(GameManager.GameState.Paused);
        }

        // Game state might be updated by other means (e.g. buttons), so update panels here
        pausePanel.SetActive(state == GameManager.GameState.Paused);
        settingsPanel.SetActive(state == GameManager.GameState.InSettings);

    }
}
