using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
    [Header("Screens")]
    [SerializeField] private PauseMenuUI pauseMenuUI;
    [SerializeField] private SettingsMenuUI settingsMenuUI;
    [SerializeField] private VictoryUI victoryUI;
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Screen Overlays")]
    [SerializeField] private ScreenFade fadeOverlay;   // black image for screen fades
    [SerializeField] private Image brightnessOverlay; // black image for brightness
    [SerializeField] private ScreenFade enemyBlind; // blind image (placeholder for blinding animation)
    [SerializeField] private ScreenFade enemyHurt; // hurt image (placehodler for hurting animation)

    // note: for future may want to replace Image with CanvasGroup


    /// <summary>
    /// Singleton Manager setup (persistent)
    /// Subscribe to GameState changes invoked from GameManager
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        ValidateReferences();
        InitializeUI();

        GameManager.Instance.OnGameStateChanged += HandleGameStateChange;
    }

    private void ValidateReferences()
    {
        if (pauseMenuUI == null) throw new System.InvalidOperationException("[UIManager] pauseMenuUI is not assigned");
        if (settingsMenuUI == null) throw new System.InvalidOperationException("[UIManager] settingsMenuUI is not assigned");
        if (victoryUI == null) throw new System.InvalidOperationException("[UIManager] victoryUI is not assigned");
        if (gameOverUI == null) throw new System.InvalidOperationException("[UIManager] gameOverUI is not assigned");
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
    /// Hides all UI screens
    /// Does not affect overlays or HUD elements
    /// </summary>
    private void HideAllScreens()
    {
        pauseMenuUI.Hide();
        settingsMenuUI.Hide();
        victoryUI.Hide();
        gameOverUI.Hide();
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
        HideAllScreens();
        pauseMenuUI.Show();
    }

    private void StateSettings()
    {
        HideAllScreens();
        settingsMenuUI.Show();
    }

    private void StateGameOver()
    {
        HideAllScreens();
        gameOverUI.Show();
    }

    private void StateVictory()
    {
        HideAllScreens();
        victoryUI.Show();
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
    // OVERLAYS
    // =========================

    public void PlayFadeIn(float duration)
    {
        Debug.Log("playing fade in");
        fadeOverlay.FadeIn(duration);
    }

    // public void SetFade(float alpha)
    // {
    //     Color c = fadeOverlay.color;
    //     c.a = 1f - alpha;
    //     fadeOverlay.color = c;
    // }

    // plays the blind animation
    // TODO: replace with the animation instead of image fade
    //   
    public void PlayBlind()
    {
        StartCoroutine(BlindBuff());
        
        IEnumerator BlindBuff()
        {
            enemyBlind.FadeOut(0.1f);
            yield return new WaitForSeconds(3.0f);
            enemyBlind.FadeIn(2.0f);
            // uses ScreenFade.cs to edit alpha of the image
            // replace this when adding animation
        }
    }

    // plays the hurt animation
    // TODO: replace with the animation instead of image fade
    public void PlayHurt()
    {
        StartCoroutine(HurtBuff());

        IEnumerator HurtBuff()
        {
            enemyHurt.FadeOut(0.2f);
            yield return new WaitForSeconds(0.5f);
            enemyHurt.FadeIn(2.0f);
        }
    }

        
    // event safety function
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChange;
    }
}