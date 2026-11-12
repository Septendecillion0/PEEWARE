using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
// TODO: controls a screen fade, should be delegated elsewhere
// TODO: replace scene loading with scene types, not string
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;
    public GameObject settingsCanvas;
    public ScreenFade screenFade;

    [Header("Audio")]
    [SerializeField] private AudioSource startSFX;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        settingButton.onClick.AddListener(OpenSettings);
        exitButton.onClick.AddListener(ExitGame);
        settingsCanvas.SetActive(false);

        startButton.onClick.AddListener(PlayStartSFX);
    }

    public void StartGame()
    {
        StartCoroutine(Loading("Level")); //replace with gameplay scene
    }

    private IEnumerator Loading(string sceneName)
    {
        float duration = 4.0f;

        GameManager.Instance.SetState(GameManager.GameState.Loading);

        // Kick off both fades at the same time
        
        screenFade.FadeOut(duration);  
        AudioManager.Instance.FadeOutMusic(duration);

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

    private void PlayStartSFX()
    {
        startSFX.PlayOneShot(startSFX.clip);
    }
}
