using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for all UI handler panels
/// Manages visibility via CanvasGroup and provides common UI patterns
/// All derived classes must have a CanvasGroup component
/// </summary>
public abstract class UIHandler : MonoBehaviour
{
    [SerializeField] protected CanvasGroup canvasGroup;

    protected virtual void Awake()
    {
        // Ensure gameObject stays active to allow coroutines
        gameObject.SetActive(true);
        
        // Get or add CanvasGroup if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
            throw new System.InvalidOperationException($"[{GetType().Name}] CanvasGroup component is required");

        ValidateReferences();
        ConfigureButtons();
        Hide();
    }

    /// <summary>
    /// Validate all required inspector references
    /// Must be implemented by subclasses
    /// </summary>
    protected abstract void ValidateReferences();

    /// <summary>
    /// Configure button listeners and other setup
    /// Must be implemented by subclasses
    /// </summary>
    protected abstract void ConfigureButtons();

    /// <summary>
    /// Set visibility of interactive elements (buttons, etc.)
    /// Must be implemented by subclasses
    /// </summary>
    protected abstract void SetButtonsVisible(bool visible);

    /// <summary>
    /// Show the UI panel and play animation
    /// Restores visibility, interactivity, and starts animation sequence
    /// </summary>
    public virtual void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        SetButtonsVisible(false);
        StartCoroutine(ShowRoutine());
    }

    /// <summary>
    /// Hide the UI panel and stop all animations
    /// Makes panel fully transparent and non-interactive
    /// </summary>
    public virtual void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// Override to define the animation sequence for this UI
    /// Default implementation has no animation
    /// </summary>
    protected virtual IEnumerator ShowRoutine()
    {
        yield return null;
    }
}
