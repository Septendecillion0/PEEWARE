using UnityEngine;

public class DraggableObject : MonoBehaviour, IInteractable
{
    public KeyCode interactKey = KeyCode.E;
    // distance in front of the player where the object should be held
    public float forwardDistance = 1.5f;
    // vertical offset relative to player's position
    public float heightOffset = 0.5f;
    // smoothing speed for movement
    public float followSmoothTime = 0.01f;
    public Transform playerTransform;

    private bool isDragging;
    private bool hoverEntered;
    private Rigidbody rb;
    private Vector3 currentVelocity = Vector3.zero;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        hoverEntered = false;
        isDragging = false;
    }

    // when the player looks at the object
    public void OnHoverEnter()
    {
        InteractionUI.Instance.Show("[E] Drag the object");
        hoverEntered = true;
    }

    // when the player looks away from the object
    public void OnHoverExit()
    {
        InteractionUI.Instance.Hide();
        hoverEntered = false;
        isDragging = false;
        rb.isKinematic = false;
    }

    // when the player presses the interact button
    public void Interact()
    {
        // switch dragging state
        isDragging = !isDragging;
        rb.isKinematic = isDragging; // the object won't be affected by physics while being dragged

        if (isDragging)
        {
            InteractionUI.Instance.Show("Press [E] to drop");
        }
        else
        {
            if (hoverEntered)
                InteractionUI.Instance.Show("Press [E] to drag");
            else
                InteractionUI.Instance.Hide();
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            if (playerTransform != null)
            {
                // compute desired hold position in front of player
                Vector3 forward = playerTransform.forward;
                Vector3 targetPos = playerTransform.position + forward.normalized * forwardDistance + Vector3.up * heightOffset;

                // Use rigidbody MovePosition with smooth position change
                Vector3 smoothPos = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, followSmoothTime);
                rb.MovePosition(smoothPos);

            }
        }
    }
}
