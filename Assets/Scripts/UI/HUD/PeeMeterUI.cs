using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class PeeMeterUI : MonoBehaviour
{
    public Slider peeMeter;
    public AudioClip deathSound;
    [SerializeField] private CanvasGroup canvasGroup;

    private Image fillImage;

    // Colors for the pee meter
    private Color32 startPee = new Color32(255, 255, 165, 255);
    private Color32 endPee = new Color32(255, 208, 20, 255);

    public void Show()
    {
        canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        fillImage = peeMeter.fillRect.GetComponent<Image>();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameManager.GameState.Playing)
            return;

        // Trigger global game over
        if (!GameManager.Instance.IsGameOver && PeeMeterManager.Instance.IsFull())
        {
            AudioManager.Instance.FadeOutMusic(0.2f);
            AudioManager.Instance.PlaySFX(deathSound);
            GameManager.Instance.SetState(GameManager.GameState.GameOver);
        }

        // Update color and volume based on current pee
        float normalizedPee = PeeMeterManager.Instance.currentPee / PeeMeterManager.Instance.maxPee;
        fillImage.color = Color.Lerp(startPee, endPee, normalizedPee);

        // ramp music volume as pee increases
        // TODO: move to audio script
        float dB = Mathf.Lerp(-20f, 0f, normalizedPee);
        AudioManager.Instance.ChangeMusicVolume(dB);
    }

    private void UpdateUI(float value)
    {
        peeMeter.value = value;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForManager());
    }

    private IEnumerator WaitForManager()
    {
        while (PeeMeterManager.Instance == null)
            yield return null;
        PeeMeterManager.Instance.OnPeeValueChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (PeeMeterManager.Instance != null)
        {
            PeeMeterManager.Instance.OnPeeValueChanged -= UpdateUI;
        }
    }
}