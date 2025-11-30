using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    [SerializeField] private GameObject pauseCanvas;
    private bool isPaused = false;

    private void Start()
    {
        pauseCanvas.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
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
