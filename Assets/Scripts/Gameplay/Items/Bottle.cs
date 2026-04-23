using UnityEngine;
/// <summary>
/// Interactable bottle object
/// Single-use trigger
/// Activating fills the bottle and lowers pee meter
/// </summary>
public class Bottle : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject liquid; // Assign the "liquid" child GameObject in the Inspector
    [SerializeField] private float peeAmount; // Amount to reduce from the pee meter when filled]
    private bool isFilled = false;

    private void Start()
    {
        // Set default pee amount if not assigned
        if (peeAmount == 0) peeAmount = 5f;
        
        // Ensure the bottle starts empty
        if (liquid != null)
        {
            liquid.SetActive(false);
        }
        isFilled = false;
    }

    // When the player looks at the object
    public void OnHoverEnter()
    {
        if (!isFilled)
        {
            InteractionUI.Instance.Show("[E] Fill Bottle");
        }
    }

    // When the player looks away from the object
    public void OnHoverExit()
    {
        InteractionUI.Instance.Hide();
    }

    // When the player presses the interact button
    public void Interact()
    {
        if (!isFilled)
        {
            // Trigger filling: make liquid visible
            if (liquid != null)
            {
                liquid.SetActive(true);
            }
            isFilled = true;

            PeeMeterManager.Instance.Pee(peeAmount);

            // Hide UI since the bottle is now filled and no longer interactable
            InteractionUI.Instance.Hide();
        }
    }
}