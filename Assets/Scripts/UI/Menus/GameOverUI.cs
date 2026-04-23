using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handler for Game Over UI
/// Configures button listeners and manages all visual game over menu elements
/// UIManager calls Show/Hide
/// </summary>
public class GameOverUI : UIHandler
{
    [SerializeField] private Image youPeed;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button restartButton;

    // Hide exception since youPeed is not in the CanvasGroup
    public override void Hide()
    {
        base.Hide();
        SetYouPeedAlpha(0f);
    }

    protected override void ConfigureButtons()
    {
        quitButton.onClick.AddListener(GameManager.Instance.QuitToMainMenu);
        restartButton.onClick.AddListener(GameManager.Instance.RestartGame);
    }
    protected override void ValidateReferences()
    {
        if (youPeed == null)
            throw new System.InvalidOperationException("[GameOverUI] youPeed Image reference is not assigned in inspector");
        if (quitButton == null)
            throw new System.InvalidOperationException("[GameOverUI] quitButton is not assigned in inspector");
        if (restartButton == null)
            throw new System.InvalidOperationException("[GameOverUI] restartButton is not assigned in inspector");
    }

    protected override void SetButtonsVisible(bool visible)
    {
        quitButton.gameObject.SetActive(visible);
        restartButton.gameObject.SetActive(visible);
    }

    protected override IEnumerator ShowRoutine()
    {
        yield return StartCoroutine(PlayAnimation());
    }

    /// <summary>
    /// Plays the game over animation:
    /// 1. Fades in the "You Peed" image over 6 seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayAnimation()
    {   
        SetYouPeedAlpha(0f);

        float fadeDuration = 6f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetYouPeedAlpha(t / fadeDuration);
            yield return null;
        }

        SetButtonsVisible(true);
    }

    /// <summary>
    /// Helper method to set the alpha of the "YOU_PEED" image
    /// </summary>
    /// <param name="alpha"></param>
    private void SetYouPeedAlpha(float alpha)
    {
        Color c = youPeed.color;
        c.a = alpha;
        youPeed.color = c;
    }
}