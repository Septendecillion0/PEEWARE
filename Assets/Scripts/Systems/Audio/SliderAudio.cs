using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip releaseSound;

    private Slider slider;
    private float startingValue;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        startingValue = slider.value;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Only play if the slider actually changed
        if (!Mathf.Approximately(startingValue, slider.value))
        {
            audioSource.PlayOneShot(releaseSound);
        }
    }
}