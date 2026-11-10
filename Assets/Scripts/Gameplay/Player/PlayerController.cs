using UnityEngine;
/// <summary>
/// Manages the player state
/// Handles restrictions on player inputs based on game state
/// </summary>
public class PlayerController : MonoBehaviour
{
    public bool canMove;
    public bool canInput;
    public bool canLook;
    public bool canSee;
    public bool canHear;
    // TODO:
    //   canSee and canHear are not in use yet (should be inflicted by enemies)
    //   switch to token system to queue different restrictions and correctly handle duplicates

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChange;

        // Initialize based on the current state.
        HandleGameStateChange(GameManager.Instance.State);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChange;
    }

    private void HandleGameStateChange(GameManager.GameState newState)
    {
        GetRestrictionsForGameState(newState);
    }

    // GameStates (switch reference)
    //         MainMenu,
    //         Loading,
    //         Playing,
    //         Cutscene,
    //         Paused,
    //         InSettings,
    //         GameOver,
    //         Victory
    private void GetRestrictionsForGameState(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Cutscene:
                canMove = false;
                canInput = false;
                canLook = false;
                canSee = false;
                canHear = false;
                break;

            case GameManager.GameState.Playing:
                canMove = true;
                canInput = true;
                canLook = true;
                canSee = true;
                canHear = true;
                break;

            case GameManager.GameState.Loading:
                canMove = false;
                canInput = false;
                canLook = false;
                canSee = false;
                canHear = false;
                break;

            default:
                canMove = false;
                canInput = false;
                canLook = false;
                canSee = true;
                canHear = true;
                break;
        }
    }
}
