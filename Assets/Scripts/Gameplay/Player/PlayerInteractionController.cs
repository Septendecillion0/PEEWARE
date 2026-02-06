using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Camera playerCamera;
    private IInteractable currentTarget;

    void Update()
    {
        DetectInteractable();
        HandleInput();
    }

    void DetectInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);

        if (Physics.Raycast(ray, out hit, interactRange, interactableMask))
        {
            // Debug.Log("Hit: " + hit.collider.name);
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != currentTarget)
            {
                // change current target to the new interactable
                currentTarget?.OnHoverExit();
                currentTarget = interactable;
                currentTarget?.OnHoverEnter();
            }
        }
        else
        {
            currentTarget?.OnHoverExit();
            currentTarget = null;
        }
    }

    void HandleInput()
    {
        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            currentTarget.Interact();
        }
    }
}
