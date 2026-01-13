using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;


    private void Start()
    {
        resumeButton.onClick.AddListener(() => GameManager.Instance.SetState(GameManager.GameState.Playing));
        settingsButton.onClick.AddListener(() => GameManager.Instance.SetState(GameManager.GameState.InSettings));
        quitButton.onClick.AddListener(() => GameManager.Instance?.QuitToMainMenu());
    }
}
