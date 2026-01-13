using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PeeMeterUpdate : MonoBehaviour
{
    public Slider peeMeter;
    public float deltaPee;

    private Image fillImage;
    public AudioManager audioManager;
    public AudioClip deathSound;

    public int maxPee { get; private set; } = 100;

    // Colors for the pee meter
    Color32 startPee = new Color32(255, 255, 165, 255);
    Color32 endPee = new Color32(255, 208, 20, 255);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        peeMeter.value = 0;
        fillImage = peeMeter.fillRect.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        peeMeter.value += deltaPee * Time.deltaTime;
        fillImage.color = Color.Lerp(startPee, endPee, peeMeter.value / 100);
        float dB = Mathf.Lerp(-40f, 0f, peeMeter.value / maxPee);
        audioManager.ChangeMusicVolume(dB);

        if (!GameManager.Instance.IsGameOver && Mathf.Approximately(peeMeter.value, 100f))
        {
            // trigger global game over
            audioManager.FadeOutMusic(0.2f);
            audioManager.PlaySFX(deathSound);
            GameManager.Instance.SetState(GameManager.GameState.Ending);
        }
    }

    //A sudden PeeMeter increase due to scare
    public void Scare(float drink)
    {
        peeMeter.value += drink;
    }

    public void Pee(float amount)
    {
        peeMeter.value -= amount;
        if (peeMeter.value < 0)
        {
            peeMeter.value = 0;
        }
    }
}
