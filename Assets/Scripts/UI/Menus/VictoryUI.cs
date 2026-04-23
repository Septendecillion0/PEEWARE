using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handler for Victory UI
/// Configures button listeners and manages all visual victory screen elements
/// UIManager calls Show/Hide
/// </summary>
public class VictoryUI : UIHandler
{
    [SerializeField] private Image youPeed;
    [SerializeField] private GameObject credits;
    [SerializeField] private GameObject congratsText;

    // Hide exception since youPeed is not in the CanvasGroup
    public override void Hide()
    {
        base.Hide();
        SetYouPeedAlpha(0f);
    }
    protected override void ValidateReferences()
    {
        if (youPeed == null)
            throw new System.InvalidOperationException("[VictoryUI] youPeed Image reference is not assigned in inspector");
        if (credits == null)
            throw new System.InvalidOperationException("[VictoryUI] credits GameObject reference is not assigned in inspector");
        if (congratsText == null)
            throw new System.InvalidOperationException("[VictoryUI] congratsText GameObject reference is not assigned in inspector");
    }

    protected override void ConfigureButtons()
    {
        // No buttons to configure for Victory UI currently, but this is where you would set up any button listeners if needed in the future.
    }

    protected override void SetButtonsVisible(bool visible)
    {
        // No buttons to show/hide for Victory UI currently, but this is where you would set active state of any buttons if needed in the future.
    }

    protected override IEnumerator ShowRoutine()
    {
        yield return StartCoroutine(PlayAnimation());
        yield return new WaitForSecondsRealtime(2f);
        yield return StartCoroutine(WaitForInput());
    }

    /// <summary>
    /// Plays the victory animation:
    /// 1. Fades in the "You Peed" image over 6 seconds
    /// 2. Shows "Congrats" text for 2 seconds
    /// 3. Shows credits screen
    /// </summary>
    /// <returns></returns>
    /// 
    /// TODO: don't use SetActive, instead modify alpha or use CanvasGroup
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

        congratsText.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);

        credits.SetActive(true);
    }

    private IEnumerator WaitForInput()
    {
        // Wait until player presses any key to quit after credits are shown
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        GameManager.Instance.QuitToMainMenu();
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
