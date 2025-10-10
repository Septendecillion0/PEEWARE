using UnityEngine;
using UnityEngine.UI;

public class PeeMeterUpdate : MonoBehaviour
{
    public DrawGameOver drawGameOver;
    public Slider peeMeter;
    public float deltaPee;

    private Image fillImage;
    private bool gameOver = false;

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

        if (!gameOver && Mathf.Approximately(peeMeter.value, 100f))
        {
            drawGameOver.Show();
            gameOver = true;
        }
    }
}
