using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Initializes Quit and Restart buttons on the GameOver screen
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button quitButton;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        quitButton.onClick.AddListener(GameManager.Instance.QuitToMainMenu);
        restartButton.onClick.AddListener(GameManager.Instance.RestartGame);
    }
}