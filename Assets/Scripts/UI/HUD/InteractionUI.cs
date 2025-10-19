using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    // Singleton instance
    private static InteractionUI _instance;
    public static InteractionUI Instance { get { return _instance; } }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        // Singleton instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void Show(string message)
    {
        promptText.text = message;
        canvasGroup.alpha = 1;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
    }
}
