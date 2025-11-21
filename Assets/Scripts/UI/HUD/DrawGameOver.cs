using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// Fade YOU PEED in when pee meter full

public class DrawGameOver : MonoBehaviour
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

    void Start()
    {
        // Setup button listeners
        quitButton.onClick.AddListener(GameManager.Instance.QuitToMainMenu);
        restartButton.onClick.AddListener(GameManager.Instance.RestartGame);
        // Hide at start
        quitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        congratsText.gameObject.SetActive(false);
        creditsImage.gameObject.SetActive(false);
        EndingCanvas.gameObject.SetActive(false);
    }

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

    IEnumerator ShowCredits()
    {
        yield return new WaitForSecondsRealtime(2f);
        creditsImage.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(3f);
        GameManager.Instance.QuitToMainMenu();
    }

    private void SetYouPeedAlpha(float a)
    {
        Color c = YOU_PEED.color;
        c.a = a;
        YOU_PEED.color = c;
    }
}
