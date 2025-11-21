using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Prerelease Build");  // replace with the game scene name
        //SceneManager.LoadScene("TestScene");  // replace with the game scene name
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
