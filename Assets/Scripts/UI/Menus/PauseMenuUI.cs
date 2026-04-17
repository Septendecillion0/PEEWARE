using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Animation")]
    [SerializeField] private Image animationImage;  // Image component to show the animation
    [SerializeField] private Sprite[] animationFrames;  // Frame sprites
    [SerializeField] private float frameRate = 10f;

    private void Start()
    {
        resumeButton.onClick.AddListener(() => GameManager.Instance.SetState(GameManager.GameState.Playing));
        settingsButton.onClick.AddListener(() => GameManager.Instance.SetState(GameManager.GameState.InSettings));
        quitButton.onClick.AddListener(() => GameManager.Instance.QuitToMainMenu());
    }

    // Invoked when UI manager calls pauseMenu.SetActive(true)
    private void OnEnable()
    {
        SetButtonsVisible(false);
        StartCoroutine(PlayAnimation());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

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
    private void SetButtonsVisible(bool visible)
    {
        resumeButton.gameObject.SetActive(visible);
        settingsButton.gameObject.SetActive(visible);
        quitButton.gameObject.SetActive(visible);
    }
}
