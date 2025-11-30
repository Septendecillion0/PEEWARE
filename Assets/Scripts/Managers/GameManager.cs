using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : Singleton<GameManager>
{
    public DrawGameOver drawGameOver;

    public GameObject firstPersonAudio;

    public bool IsGameOver { get; private set; } = false;

    public bool foundToilet = false;


    protected override void Awake()
    {
        base.Awake();
        ResetGameState();
    }

    public void ResetGameState()
    {
        IsGameOver = false;
        foundToilet = false;
        Time.timeScale = 1f;

        Jump.canJump = true;
        Crouch.canCrouch = true;

        FirstPersonLook.canLook = true;
        firstPersonAudio.SetActive(true);
    }


    public void PauseGame()
    {
        if (IsGameOver) return;
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
        if (IsGameOver) return;
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
        PauseGame();
        IsGameOver = true;
        if (drawGameOver != null)
        {
            drawGameOver.Show();
        }
    }

    public void RestartGame()
    {
        if (!IsGameOver) return;
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        IsGameOver = false;
        ResumeGame();
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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
