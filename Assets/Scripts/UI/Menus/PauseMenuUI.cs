using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/// <summary>
/// Handler for Pause Menu UI
/// Configures button listeners and manages all visual pause menu elements
/// UIManager calls Show/Hide
/// </summary>
public class PauseMenuUI : UIHandler
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Animation")]
    [SerializeField] private Image animationImage;  // Image component to show the animation
    [SerializeField] private Sprite[] animationFrames;  // Frame sprites
    [SerializeField] private float frameRate = 10f;

    protected override void ValidateReferences()
    {
        if (resumeButton == null)
            throw new System.InvalidOperationException("[PauseMenuUI] resumeButton reference is not assigned in inspector");
        if (settingsButton == null)
            throw new System.InvalidOperationException("[PauseMenuUI] settingsButton reference is not assigned in inspector");
        if (quitButton == null)
            throw new System.InvalidOperationException("[PauseMenuUI] quitButton reference is not assigned in inspector");
        if (animationImage == null)
            throw new System.InvalidOperationException("[PauseMenuUI] animationImage reference is not assigned in inspector");
        if (animationFrames == null || animationFrames.Length == 0)
            throw new System.InvalidOperationException("[PauseMenuUI] animationFrames array is not assigned or empty in inspector");
        // Optional: Check for null entries
        for (int i = 0; i < animationFrames.Length; i++)
        {
            if (animationFrames[i] == null)
                throw new System.InvalidOperationException($"[PauseMenuUI] animationFrames[{i}] is null");
        }
    }
    protected override void ConfigureButtons()
    {
        resumeButton.onClick.AddListener(() => GameManager.Instance.SetState(GameManager.GameState.Playing));
        settingsButton.onClick.AddListener(() => GameManager.Instance.SetState(GameManager.GameState.InSettings));
        quitButton.onClick.AddListener(() => GameManager.Instance.QuitToMainMenu());
    }

    protected override void SetButtonsVisible(bool visible)
    {
        resumeButton.gameObject.SetActive(visible);
        settingsButton.gameObject.SetActive(visible);
        quitButton.gameObject.SetActive(visible);
    }

    protected override IEnumerator ShowRoutine()
    {
        yield return StartCoroutine(PlayAnimation());
    }

    // Plays the pause menu animation (frame by frame)
    private IEnumerator PlayAnimation()
    {
        float frameDuration = 1f / frameRate;
        foreach (Sprite frame in animationFrames)
        {
            animationImage.sprite = frame;
            yield return new WaitForSecondsRealtime(frameDuration);
        }
        SetButtonsVisible(true);
    }
}
