using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public DrawGameOver drawGameOver;

    public GameObject firstPersonAudio;

    // track whether the game is over、
    public bool IsGameOver { get; private set; } = false;


    private void Awake()
    {
        // enforce singleton pattern
        if (_instance != null && _instance != this)
        {
            // multiple instances detected, destroy this one
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // ensure timescale is normal when starting
        Time.timeScale = 1f;

        // enable player jump and crouch
        Jump.canJump = true;
        Crouch.canCrouch = true;

        // ensure camera and audio work at start
        FirstPersonLook.canLook = true;
        firstPersonAudio.SetActive(true);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        // disable player jump and crouch
        Jump.canJump = false;
        Crouch.canCrouch = false;
        // disable camera look and audio
        FirstPersonLook.canLook = false;
        firstPersonAudio.SetActive(false);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        // enable player jump and crouch
        Jump.canJump = true;
        Crouch.canCrouch = true;
        // enable camera look and audio
        FirstPersonLook.canLook = true;
        firstPersonAudio.SetActive(true);
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        if (drawGameOver != null)
        {
            drawGameOver.Show();
        }

        PauseGame();
    }

    public void RestartGame()
    {
        if (!IsGameOver) return;
        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        IsGameOver = false;
        ResumeGame();
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
