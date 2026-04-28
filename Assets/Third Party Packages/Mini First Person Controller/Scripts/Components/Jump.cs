using UnityEngine;

public class Jump : MonoBehaviour
{
    private FirstPersonMovement movement;
    public float jumpStrength = 2;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;

    public static bool canJump = true;

    void Reset()
    {
        // Try to get groundCheck.
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Awake()
    {
        // Get FirstPersonMovement component.
        movement = GetComponent<FirstPersonMovement>();
    }

    void LateUpdate()
    {
        // Jump when the Jump button is pressed and we are on the ground.
        if (Input.GetButtonDown("Jump") && canJump && (!groundCheck || groundCheck.isGrounded))
        {
            movement.verticalVelocity = Mathf.Sqrt(jumpStrength * 2f * movement.gravity);
            Jumped?.Invoke();
        }
    }
}
