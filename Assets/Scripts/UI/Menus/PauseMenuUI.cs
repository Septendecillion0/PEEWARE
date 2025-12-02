using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        settingsButton.onClick.AddListener(SettingsManager.Instance.OpenSettings);
        quitButton.onClick.AddListener(GameManager.Instance.QuitToMainMenu);
    }

    private void OnResumeClicked()
    {
        GameManager.Instance.ResumeGame();
        gameObject.SetActive(false); // hide pause menu UI
    }

    private void OnSettingsClicked()
    {
        SettingsManager.Instance.OpenSettings();
    }
}
