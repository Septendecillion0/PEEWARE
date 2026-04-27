using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    public Image fadeImage;

    public void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
    
    // fades out the SCREEN, drawing the image
    public void FadeOut(float duration, System.Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f, duration, onComplete));
    }
    // fades in the SCREEN, hiding the image
    // note: fade in is separated into 2 phases bc the first 5% alpha change is very noticeable
    public void FadeIn(float duration, System.Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(FadeInHelper(duration, onComplete));
    }

    private IEnumerator FadeInHelper(float duration, System.Action onComplete)
    {
        // yield return StartCoroutine(Fade(1f, 0.95f, Mathf.Clamp(duration, 0f, duration - 0.5f), null));
        // yield return StartCoroutine(Fade(0.95f, 0f, 0.5f, onComplete)); // smooth out the last 5% of the fade
        yield return StartCoroutine(Fade(1f, 0f, duration, onComplete));
    }

    private IEnumerator Fade(float from, float to, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t); // or fadeCurve.Evaluate(t)

            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;

            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

        onComplete?.Invoke();
    }
}
