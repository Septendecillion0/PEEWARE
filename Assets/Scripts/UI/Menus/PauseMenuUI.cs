using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;


    private void Start()
    {
        resumeButton.onClick.AddListener(PauseManager.Instance.ClosePauseMenu);
        settingsButton.onClick.AddListener(PauseManager.Instance.OpenSettings);
        quitButton.onClick.AddListener(GameManager.Instance.QuitToMainMenu);
    }
}
