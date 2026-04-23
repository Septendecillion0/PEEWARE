using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PeeMeterUI : MonoBehaviour
{
    public Slider peeMeter;
    public AudioClip deathSound;
    [SerializeField] private float deltaPee = 0f; // Rate of pee increase per second

    private Image fillImage;

    // Colors for the pee meter
    private Color32 startPee = new Color32(255, 255, 165, 255);
    private Color32 endPee = new Color32(255, 208, 20, 255);

    private void Start()
    {
        fillImage = peeMeter.fillRect.GetComponent<Image>();
        PeeMeterManager.Instance.deltaPee = deltaPee;
        PeeMeterManager.Instance.OnPeeValueChanged += UpdateUI;
        UpdateUI(PeeMeterManager.Instance.currentPee);
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameOver && PeeMeterManager.Instance.IsFull())
        {
            // Trigger global game over
            AudioManager.Instance.FadeOutMusic(0.2f);
            AudioManager.Instance.PlaySFX(deathSound);
            GameManager.Instance.SetState(GameManager.GameState.GameOver);
        }

        // Update color and volume based on current pee
        float normalizedPee = PeeMeterManager.Instance.currentPee / PeeMeterManager.Instance.maxPee;
        fillImage.color = Color.Lerp(startPee, endPee, normalizedPee);
        float dB = Mathf.Lerp(-20f, 0f, normalizedPee);
        AudioManager.Instance.ChangeMusicVolume(dB);
    }

    private void UpdateUI(float value)
    {
        peeMeter.value = value;
    }

    private void OnDestroy()
    {
        if (PeeMeterManager.Instance != null)
        {
            PeeMeterManager.Instance.OnPeeValueChanged -= UpdateUI;
        }
    }
}