using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manager for all on-screen UI elements.
/// Holds references to all UI screens and functions to draw/edit them.
/// Persists between scenes
/// </summary>
/// 
/// TODO: move other UI elements here
/// should be everything but the main menu UI (+ settings)
/// specifically: Pee meter, enemy flash screens
public class UIManager : Singleton<UIManager>
{
    [Header("Pause and Settings Screens")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    [Header("Ending Screen + Objects")]
    [SerializeField] private GameObject endingScreen;
    [SerializeField] private Image youPeed;
    [SerializeField] private GameObject credits;
    [SerializeField] private GameObject congratsText;
    [SerializeField] private GameObject gameOverButtons; // 

    [Header("Overlays")]
    [SerializeField] private Image fadeOverlay;   // black image for screen fades
    [SerializeField] private Image brightnessOverlay; // black image for brightness

    // note: for future may want to replace Image with CanvasGroup


    /// <summary>
    /// Singleton Manager setup (persistent)
    /// Subscribe to GameState changes invoked from GameManager
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        GameManager.Instance.OnGameStateChanged += HandleGameStateChange;

        InitializeUI();
    }
    // event safety function
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChange;
    }

    /// <summary>
    /// Configure UI for the first time the game is loaded:
    /// Hides all UI screens
    /// Sets the screen brightness based on current settings, or to default of max
    /// </summary>
    private void InitializeUI()
    {
        HideAllScreens();

        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        SetBrightness(brightness);
    }

    /// <summary>
    /// Hides (set unactive) all UI screens
    /// </summary>
    private void HideAllScreens()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        endingScreen.SetActive(false);
        //gameOverScreen.SetActive(false);
        //victoryScreen.SetActive(false);
    }

    // =========================================================
    // GAME STATE ENTRY POINT (FUTURE EVENT HOOK)
    // =========================================================

    // TODO: update GameManager to use events, update UIManager to listen for events
    //       remove any calls to HandleGameStateChange in other managers

    public void HandleGameStateChange(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Start:
                StateStart();
                break;

            case GameManager.GameState.Playing:
                StatePlaying();
                break;

            case GameManager.GameState.Paused:
                StatePaused();
                break;

            case GameManager.GameState.InSettings:
                StateSettings();
                break;

            case GameManager.GameState.GameOver:
                StateGameOver();
                break;

            case GameManager.GameState.Victory:
                StateVictory();
                break;
        }
    }


    // =========================================================
    // STATE HANDLERS
    // =========================================================
    private void StateStart()
    {
        HideAllScreens();
    }
    private void StatePlaying()
    {
        HideAllScreens();
    }

    private void StatePaused()
    {
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    private void StateSettings()
    {
        settingsMenu.SetActive(true);
    }

    private void StateGameOver()
    {
        return; // handled by EndingSequenceController
        // gameOverScreen.SetActive(true);
        // pauseMenu.SetActive(false);
        // settingsMenu.SetActive(false);
    }

    private void StateVictory()
    {
        return; // handled by EndingSequenceController
        // victoryScreen.SetActive(true);
        // pauseMenu.SetActive(false);
        // settingsMenu.SetActive(false);
    }
    
    // =========================================================
    // ENDING UI
    // =========================================================
    public void ShowEndingScreen()
    {
        endingScreen.SetActive(true);

        credits.SetActive(false);
        congratsText.SetActive(false);
        gameOverButtons.SetActive(false);
    }

    public void SetYouPeedAlpha(float a)
    {
        Color c = youPeed.color;
        c.a = a;
        youPeed.color = c;
    }

    public void ShowCredits()
    {
        credits.SetActive(true);
    }

    public void ShowCongratsText()
    {
        congratsText.SetActive(true);
    }

    public void ShowGameOverButtons()
    {
        gameOverButtons.SetActive(true);
    }

    // =========================
    // BRIGHTNESS
    // =========================

    /// <summary>
    /// Sets the brightness of the screen by editing the opacity of the brightness overlay (dark fullscreen image)
    /// value ranges from 0f to 1f, where 1f is max brightness (sets the overlay to transparent) and vice versa
    /// </summary>
    /// <param name="value"></param>
    /// 
    /// NOTE: the brightness overlay image is on the UI layer and appears behind other UI elements (does not affect menus, etc.)
    ///       this may or may not be intended behavior
    public void SetBrightness(float value)
    {
        Color c = brightnessOverlay.color;
        c.a = 1f - value;
        brightnessOverlay.color = c;
    }

    // =========================
    // FADE (optional)
    // =========================

    public void SetFade(float alpha)
    {
        Color c = fadeOverlay.color;
        c.a = 1f - alpha;
        fadeOverlay.color = c;
    }
}