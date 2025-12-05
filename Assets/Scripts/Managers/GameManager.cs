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
        Playing,
        Paused,
        Ending
    }

    // Current state (public read, private set - use SetState() to change)
    public GameState State { get; private set; } = GameState.Playing;

    // Backwards-compatible boolean (read-only). Some scripts can keep using this for now.
    public bool IsGameOver => State == GameState.Ending;

    // Event fired when the state changes: (oldState, newState)
    public event Action<GameState, GameState> OnStateChanged;

    protected override void Awake()
    {
        base.Awake();
        ResetGameState();
    }

    public void ResetGameState()
    {
        SetState(GameState.Playing, invokeEvent: false);

        foundToilet = false;
        Time.timeScale = 1f;

        Jump.canJump = true;
        Crouch.canCrouch = true;

        FirstPersonLook.canLook = true;
        if (firstPersonAudio != null) firstPersonAudio.SetActive(true);
    }

    public void SetState(GameState newState, bool invokeEvent = true)
    {
        if (State == newState) return;

        GameState old = State;
        State = newState;

        if (newState == GameState.Playing)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;

            Jump.canJump = true;
            Crouch.canCrouch = true;

            FirstPersonLook.canLook = true;
            if (firstPersonAudio != null) firstPersonAudio.SetActive(true);
        }
        else if (newState == GameState.Paused)
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
            Cursor.lockState = CursorLockMode.None;

            Jump.canJump = false;
            Crouch.canCrouch = false;

            FirstPersonLook.canLook = false;
        }

        if (invokeEvent)
            OnStateChanged?.Invoke(old, newState);
    }

    // Convenience methods
    public void PauseGame()
    {
        if (State == GameState.Ending) return; // Do not allow pause during ending
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (State == GameState.Ending) return;
        SetState(GameState.Playing);
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        SetState(GameState.Ending);

        EndingManager.Instance?.Show();
    }

    public void RestartGame()
    {
        if (!IsGameOver) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        SetState(GameState.Playing);
        ResumeGame();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
