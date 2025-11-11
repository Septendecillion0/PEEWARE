using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;

    private void Start()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
    }

    private void OnResumeClicked()
    {
        GameManager.Instance.ResumeGame();
        gameObject.SetActive(false); // hide pause menu UI
    }
}
