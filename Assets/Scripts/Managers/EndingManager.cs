using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Controls the ending screen sequence: fading in the YOU PEED image,
/// then branching into either a credits screen or restart/quit buttons
/// depending on whether the player found the toilet.
/// Called externally via Show() when the game reaches the Ending state.
/// </summary>
/// <remarks>
/// Despite its name, this class is primarily a UI controller rather than a manager.
/// All ending logic (what ending to show) is driven by GameManager.foundToilet.
/// Consider renaming to EndingScreenUI and moving ending condition logic to GameManager.
/// </remarks>
public class EndingManager : Singleton<EndingManager>
{
    [SerializeField] private Canvas EndingCanvas;
    [SerializeField] private Image YOU_PEED;
    [SerializeField] private Image creditsImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI congratsText;

    private float fadeTimer = 0f;
    private bool fadingIn = false;

    /// <summary>
    /// Registers button listeners and hides all ending UI elements on startup.
    /// </summary>
    private void Start()
    {
        // Setup button listeners with null checking
        quitButton.onClick.AddListener(() => GameManager.Instance?.QuitToMainMenu());
        restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        // Hide at start
        quitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        congratsText.gameObject.SetActive(false);
        creditsImage.gameObject.SetActive(false);
        EndingCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Advances the YOU PEED fade-in each frame using unscaled time.
    /// Once the fade completes, branches to either the credits sequence
    /// or the restart/quit buttons based on GameManager.foundToilet.
    /// </summary>
    void Update()
    {
        if (fadingIn)
        {
            // Fade in effect for YOU PEED image
            fadeTimer += Time.unscaledDeltaTime;
            SetYouPeedAlpha(Mathf.Lerp(0f, 1f, fadeTimer / fadeDuration));

            if (fadeTimer >= fadeDuration)
            {
                fadingIn = false;

                if (GameManager.Instance.foundToilet)
                {
                    congratsText.gameObject.SetActive(true);
                    StartCoroutine(ShowCredits());
                }
                else
                {
                    // Activate the buttons
                    quitButton.gameObject.SetActive(true);
                    restartButton.gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// Activates the ending canvas and starts the YOU PEED fade-in sequence.
    /// Called by GameManager when the Ending state is entered.
    /// </summary>
    public void Show()
    {
        // Show the ending screen
        EndingCanvas.gameObject.SetActive(true);
        // Make image fully transparent
        SetYouPeedAlpha(0f);
        // Fade the image in
        fadeTimer = 0f;
        fadingIn = true;
    }

    /// <summary>
    /// Displays the credits image after a short delay, then waits for any key input
    /// before returning to the main menu.
    /// </summary>
    IEnumerator ShowCredits()
    {
        yield return new WaitForSecondsRealtime(2f);
        creditsImage.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        GameManager.Instance.QuitToMainMenu();
    }

    /// <summary>
    /// Sets the alpha of the YOU PEED image without affecting its other color channels.
    /// </summary>
    private void SetYouPeedAlpha(float a)
    {
        Color c = YOU_PEED.color;
        c.a = a;
        YOU_PEED.color = c;
    }
}
