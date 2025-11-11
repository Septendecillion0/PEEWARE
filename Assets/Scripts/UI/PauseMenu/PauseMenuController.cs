using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    private bool isPaused = false;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
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
            pauseMenuUI.SetActive(isPaused);
        }
    }
}
