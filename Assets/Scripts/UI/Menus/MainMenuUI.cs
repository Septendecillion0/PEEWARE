using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;
    public GameObject settingsCanvas;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        settingButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(ExitGame);
        settingsCanvas.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Prerelease Build");  // replace with the game scene name
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        settingsCanvas.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsCanvas.SetActive(false);
    }
}
