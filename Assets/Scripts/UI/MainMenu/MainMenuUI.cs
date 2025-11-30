using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Prerelease Build");  // replace with the game scene name
        //SceneManager.LoadScene("TestScene");  // replace with the game scene name
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
