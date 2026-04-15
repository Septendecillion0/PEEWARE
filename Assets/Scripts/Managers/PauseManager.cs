using UnityEngine;

/// <summary>
/// Listens for "esc" button press during gameplay to toggle pause status
/// Updates gamestate via GameManager
/// draws UI via UIManager -> called by GameManager event
/// </summary>
public class PauseManager : Singleton<PauseManager>
{
    /// <summary>
    /// Singleton Manager setup
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Polls for "esc" key input
    /// If in a valid GameState, move to HandleEscape()
    /// </summary>
    void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing &&
            GameManager.Instance.State != GameManager.GameState.Paused &&
            GameManager.Instance.State != GameManager.GameState.InSettings)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
    }

    /// <summary>
    /// calls GameManager to update State -> GameManager event calls UIManager to update menu visibility
    /// </summary>
    private void HandleEscape()
    {
        GameManager.Instance.TogglePauseState();
    }
}
