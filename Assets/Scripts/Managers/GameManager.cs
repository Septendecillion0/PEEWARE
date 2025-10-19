using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public DrawGameOver drawGameOver;

    // track whether the game is over、
    public bool IsGameOver { get; private set; } = false;


    private void Awake()
    {
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
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        if (drawGameOver != null)
        {
            drawGameOver.Show();
        }
        // pause the game
        Time.timeScale = 0f;
    }

    // optional: resume the game (useful for restart or testing)
    public void ResumeGame()
    {
        if (!IsGameOver) return;
        IsGameOver = false;
        Time.timeScale = 1f;
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
