using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;
    public GameObject settingsCanvas;
    public AudioManager audioManager;
    public ScreenFade screenFade;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        settingButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(ExitGame);
        settingsCanvas.SetActive(false);
    }

    public void StartGame()
    {
        StartCoroutine(Loading("Level")); //replace with gameplay scene
    }

    private IEnumerator Loading(string sceneName)
    {
        float duration = 2.0f;

        // Kick off both fades at the same time
        screenFade.FadeOut(duration);  
        audioManager.FadeOutMusic(duration);

        // Wait for the fade duration
        yield return new WaitForSecondsRealtime(duration);

        // Load new scene
        SceneManager.LoadScene(sceneName);
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
