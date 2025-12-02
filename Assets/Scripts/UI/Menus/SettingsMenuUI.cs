using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Start()
    {
        backButton.onClick.AddListener(SettingsManager.Instance.CloseSettings);
    }
}
