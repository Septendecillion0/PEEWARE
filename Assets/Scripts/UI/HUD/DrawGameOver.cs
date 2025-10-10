using UnityEngine;
using UnityEngine.UI;

// Fade YOU PEED in when pee meter full

public class DrawGameOver : MonoBehaviour
{
    public Image YOU_PEED;
    public float fadeDuration = 1f;
    private float fadeTimer = 0f;
    private bool fadingIn = false;

    void Start()
    {
        // Hide at start
        YOU_PEED.gameObject.SetActive(false);
    }

    void Update()
    {
        if (fadingIn)
        {
            fadeTimer += Time.deltaTime / fadeDuration;
            SetAlpha(Mathf.Lerp(0f, 1f, fadeTimer));
            if (fadeTimer >= 1f) fadingIn = false;
        }
    }

    public void Show()
    {
        // Make image fully transparent
        SetAlpha(Mathf.Lerp(0f, 1f, 0f));
        // Show the image
        YOU_PEED.gameObject.SetActive(true);
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
