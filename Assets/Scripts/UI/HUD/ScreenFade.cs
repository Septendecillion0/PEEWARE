using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    public Image fadeImage;
    
    // fades out the SCREEN, drawing the image
    public void FadeOut(float duration, System.Action onComplete = null)
    {
        StartCoroutine(Fade(0f, 1f, duration, onComplete));
    }
    // fades in the SCREEN, hiding the image
    public void FadeIn(float duration, System.Action onComplete = null)
    {
        StartCoroutine(Fade(1f, 0f, duration, onComplete));
    }

    private IEnumerator Fade(float from, float to, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = to;
        fadeImage.color = c;
        onComplete?.Invoke();
    }
}
