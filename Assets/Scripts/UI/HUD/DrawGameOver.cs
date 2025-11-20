using UnityEngine;
using UnityEngine.UI;

// Fade YOU PEED in when pee meter full

public class DrawGameOver : MonoBehaviour
{
    [SerializeField] private Canvas EndingCanvas;
    [SerializeField] private Image YOU_PEED;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button restartButton;
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
        EndingCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (fadingIn)
        {
            // Fade in effect for YOU PEED image
            fadeTimer += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, fadeTimer / fadeDuration));

            if (fadeTimer >= fadeDuration)
            {
                fadingIn = false;
                // Activate the buttons
                quitButton.gameObject.SetActive(true);
                restartButton.gameObject.SetActive(true);
            }
        }
    }

    public void Show(bool foundToilet)
    {
        // Make image fully transparent
        SetAlpha(0f);
        // Show the image
        EndingCanvas.gameObject.SetActive(true);
        // Fade the image in
        fadeTimer = 0f;
        fadingIn = true;
    }

    private void SetAlpha(float a)
    {
        Color c = YOU_PEED.color;
        c.a = a;
        YOU_PEED.color = c;
    }
}
