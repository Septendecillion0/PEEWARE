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

        Hide();

        GameManager.Instance.OnGameStateChanged += HandleGameStateChange;
    }

    public void Show(string message)
    {
        promptText.text = message;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void HandleGameStateChange(GameManager.GameState state)
    {
        if (state != GameManager.GameState.Playing)
        {
            Hide();
        }
    }
}
