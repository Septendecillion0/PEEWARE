using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

/// <summary>
/// Central manager for game state, scene transitions, and top-level audio.
/// Persists across all scenes (MainMenu and Level) as a DontDestroyOnLoad singleton.
/// Use SetState() to trigger all state changes.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public GameObject firstPersonAudio;
    public bool foundToilet = false;
    private const string MainMenu = "MainMenu";
    private const string Level = "Level";

    /// <summary>
    /// Represents the high-level states of the game at any point in time.
    /// </summary>
    public enum GameState
    {
        Start,
        Playing,
        Paused,
        InSettings,
        Ending
    }

    /// <summary>
    /// The current game state. Read-only externally, call SetState() to change.
    /// </summary>
    public GameState State { get; private set; } = GameState.Playing;

    /// <summary>
    /// Backwards-compatible boolean (read-only). Convenience property for scripts not yet migrated to GameState.
    /// </summary>
    public bool IsGameOver => State == GameState.Ending;
    public bool IsPaused => State == GameState.Paused;

    [Header("Music")]
    public AudioManager audioManager;
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;


    /// <summary>
    /// Initializes the singleton, marks this object as persistent across scenes,
    /// and subscribes to the sceneLoaded event.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded; // Remove old reference, in case of repeated subscription
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Responds to scene load events， sets the appropriate GameState for the loaded scene.
    /// Reassigns AudioManager reference, and starts scene music.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        audioManager = FindObjectOfType<AudioManager>();

        if (scene.name == MainMenu)
        {
            SetState(GameState.Start);
        }
        else if (scene.name == Level)
        {
            SetState(GameState.Playing);
            ResetGameState();
        }

        PlaySceneMusic();
    }

    /// <summary>
    /// Unsubscribes from SceneManager.sceneLoaded to prevent duplicate event handlers
    /// accumulating across scene loads. Without this, each time a duplicate GameManager
    /// is destroyed by the singleton base class, the subscription persists and
    /// OnSceneLoaded fires multiple times per scene load.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Resets all gameplay variables to their default values.
    /// Called on Level scene load to ensure a clean slate on each playthrough.
    /// </summary>
    public void ResetGameState()
    {
        foundToilet = false;
        Time.timeScale = 1f;

        Jump.canJump = true;
        Crouch.canCrouch = true;

        FirstPersonLook.canLook = true;
        if (firstPersonAudio != null) firstPersonAudio.SetActive(true);
    }

    /// <summary>
    /// Transitions to a new GameState and applies all associated side effects:
    /// Time.timeScale, cursor lock state, player control flags, audio, and UI.
    /// Directly return without action if the requested state is already active.
    /// </summary>
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

    /// <summary>
    /// Plays the appropriate music track for the currently active scene.
    /// </summary>
    private void PlaySceneMusic()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName == MainMenu)
        {
            audioManager.PlayMusic(mainMenuMusic);
        }
        else if (sceneName == Level)
        {
            audioManager.PlayMusic(gameplayMusic);
        }
    }

    /// <summary>
    /// Restarts the current scene from the beginning. Only valid when game is over.
    /// </summary>
    public void RestartGame()
    {
        if (!IsGameOver) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        SetState(GameState.Playing);
    }

    /// <summary>
    /// Restores timescale and loads the MainMenu scene.
    /// </summary>
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
