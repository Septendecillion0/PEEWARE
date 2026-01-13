using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    public GameObject firstPersonAudio;
    public bool foundToilet = false;

    public enum GameState
    {
        Start,
        Playing,
        Paused,
        InSettings,
        Ending
    }

    // Current state (public read, private set - use SetState() to change)
    public GameState State { get; private set; } = GameState.Playing;

    // Backwards-compatible boolean (read-only). Some scripts can keep using this for now.
    public bool IsGameOver => State == GameState.Ending;
    public bool IsPaused => State == GameState.Paused;

    [Header("Music")]
    public AudioManager audioManager;
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetState(GameState.Start);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        audioManager = FindObjectOfType<AudioManager>();

        if (scene.name == "MainMenu")
        {
            SetState(GameState.Start);
        }
        else if (scene.name == "Prerelease Build")
        {
            SetState(GameState.Playing);
            ResetGameState();
        }

        PlaySceneMusic();
    }

    public void ResetGameState()
    {
        SetState(GameState.Playing);

        foundToilet = false;
        Time.timeScale = 1f;

        Jump.canJump = true;
        Crouch.canCrouch = true;

        FirstPersonLook.canLook = true;
        if (firstPersonAudio != null) firstPersonAudio.SetActive(true);
    }

    public void SetState(GameState newState)
    {
        if (State == newState) return;
        State = newState;
        Debug.Log("Game State changed to: " + newState.ToString());

        if (newState == GameState.Playing)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;

            Jump.canJump = true;
            Crouch.canCrouch = true;

            FirstPersonLook.canLook = true;
            if (firstPersonAudio != null) firstPersonAudio.SetActive(true);
        }
        else if (newState == GameState.Paused || newState == GameState.InSettings)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;

            Jump.canJump = false;
            Crouch.canCrouch = false;

            FirstPersonLook.canLook = false;
            if (firstPersonAudio != null) firstPersonAudio.SetActive(false);
        }
        else if (newState == GameState.Ending)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;

            Jump.canJump = false;
            Crouch.canCrouch = false;
            FirstPersonLook.canLook = false;

            // Show ending screen
            EndingManager.Instance?.Show();
        }
    }

    private void PlaySceneMusic()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName == "MainMenu")
        {
            audioManager.PlayMusic(mainMenuMusic);
        }
        else if (sceneName == "Prerelease Build")
        {
            audioManager.PlayMusic(gameplayMusic);
        }
    }

    public void RestartGame()
    {
        if (!IsGameOver) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        SetState(GameState.Playing);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
