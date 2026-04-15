using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

/// <summary>
/// Central manager for game state, scene transitions, and top-level audio.
/// Also manages timescale based on states
/// Persistent across scenes
/// Use SetState() to trigger all state changes.
/// </summary>
/// <remarks>
/// GameManager executes before other managers (Edit->Project Settings->Script Execution Order)
/// so that subscription to events works correctly (does not cause null reference)
public class GameManager : Singleton<GameManager>
{
    // TODO: reorganize header variables to make more sense
    //       update AudioManager, remove music audioclips and audiomanager reference
    public GameObject firstPersonAudio;
    public bool foundToilet = false; // TODO: remove this variable and tie to Victory game state. This is also likely responsible for replay or exit bugs
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
        GameOver,
        Victory
    }

    // compatibility line for scripts that don't distinguish between Victory/GameOver
    public bool IsGameEnded =>
    State == GameState.GameOver ||
    State == GameState.Victory;

    /// <summary>
    /// The current game state. Read-only externally, call SetState() to change.
    /// </summary>
    public GameState State { get; private set; } = GameState.Playing;

    /// <summary>
    /// Backwards-compatible boolean (read-only). Convenience property for scripts not yet migrated to GameState.
    /// 
    /// TODO: remove references to these bools and remove bools
    /// </summary>
    public bool IsGameOver => State == GameState.GameOver;
    public bool IsPaused => State == GameState.Paused;

    [Header("Music")] // TODO remove
    public AudioClip mainMenuMusic; // TODO remove
    public AudioClip gameplayMusic; //TODO remove

    [SerializeField] private EndingSequenceController endingController;

    public event System.Action<GameState> OnGameStateChanged;


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
    /// starts scene music.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MainMenu)
        {
            SetState(GameState.Start);
        }
        else if (scene.name == Level)
        {
            SetState(GameState.Playing);
        }
        ResetGame();
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
    public void ResetGame()
    {
        foundToilet = false;
        if (State == GameState.Start)
        {
            Time.timeScale = 0f;

            Jump.canJump = false;
            Crouch.canCrouch = false;

            FirstPersonLook.canLook = false;
        }
        else
        {
            Time.timeScale = 1f;

            Jump.canJump = true;
            Crouch.canCrouch = true;

            FirstPersonLook.canLook = true;
        }
        
        if (firstPersonAudio != null) firstPersonAudio.SetActive(true);
    }

    /// <summary>
    /// Transitions to a new GameState and applies all associated side effects:
    /// Time.timeScale, cursor lock state, player control flags, audio, and UI.
    /// Directly return without action if the requested state is already active.
    /// </summary>
    public void SetState(GameState newState)
    {
        if (State == null) State = newState;
        else if (State == newState) return;

        State = newState;
        Debug.Log("Game State changed to: " + newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;

                Jump.canJump = true;
                Crouch.canCrouch = true;
                FirstPersonLook.canLook = true;

                if (firstPersonAudio != null)
                    firstPersonAudio.SetActive(true);
                break;

            case GameState.Paused:
            case GameState.InSettings:
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;

                Jump.canJump = false;
                Crouch.canCrouch = false;
                FirstPersonLook.canLook = false;

                if (firstPersonAudio != null)
                    firstPersonAudio.SetActive(false);
                break;

            case GameState.GameOver:
                EnterEndingCommon();
                endingController.PlayGameOver();
                break;

            case GameState.Victory:
                EnterEndingCommon();
                endingController.PlayVictory();
                break;
        }

        // invoke the state change event AFTER all game state logic has been executed locally
        OnGameStateChanged.Invoke(State);
    }

    // TEMPORARY Function for ending states compatibility
    // TODO: refactor GameManager and separate timescale, cursor, and player controls
    // TODO: small bug: cursor does not lock when pressing "esc" to leave pause menu, only when clicking on the button
    private void EnterEndingCommon()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;

        Jump.canJump = false;
        Crouch.canCrouch = false;
        FirstPersonLook.canLook = false;
    }

    /// <summary>
    /// Plays the appropriate music track for the currently active scene.
    /// 
    /// TODO: remove audio
    /// </summary>
    private void PlaySceneMusic()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName == MainMenu)
        {
            AudioManager.Instance.PlayMusic(mainMenuMusic);
        }
        else if (sceneName == Level)
        {
            AudioManager.Instance.PlayMusic(gameplayMusic);
        }
    }

    /// <summary>
    /// Restarts the current scene from the beginning. Only valid when game is over.
    /// </summary>
    public void RestartGame()
    {
        SetState(GameState.Playing);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Restores timescale and loads the MainMenu scene.
    /// </summary>
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Set the next GameState when "esc" (pause) is pressed
    /// This helper is ONLY referenced by PauseManager
    /// </summary>
    public void TogglePauseState()
    {
        SetState(State switch
        {
            GameState.Playing => GameState.Paused,
            GameState.Paused => GameState.Playing,
            GameState.InSettings => GameState.Paused,
            _ => State
        });
    }

}
