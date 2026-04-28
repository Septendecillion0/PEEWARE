using UnityEngine;

public class Crouch : MonoBehaviour
{
    public KeyCode key = KeyCode.LeftControl;

    [Header("Slow Movement")]
    [Tooltip("Movement to slow down when crouched.")]
    public FirstPersonMovement movement;
    [Tooltip("Movement speed when crouched.")]
    public float movementSpeed = 2;

    [Header("Low Head")]
    [Tooltip("Head to lower when crouched.")]
    public Transform headToLower;
    [HideInInspector]
    public float? defaultHeadYLocalPosition;
    [Tooltip("Height fraction when crouched (0-1, where 1 is standing height).")]
    public float crouchHeightFraction = 0.6f;

    [Tooltip("CharacterController to lower when crouched.")]
    public CharacterController controller;
    [HideInInspector]
    public float? defaultControllerHeight;
    [HideInInspector]
    public Vector3? defaultControllerCenter;

    public bool IsCrouched { get; private set; }
    public event System.Action CrouchStart, CrouchEnd;

    public GroundCheck groundCheck;

    public static bool canCrouch = true;

    void Reset()
    {
        // Try to get components.
        movement = GetComponentInParent<FirstPersonMovement>();
        headToLower = movement.GetComponentInChildren<Camera>().transform;
        controller = movement.GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        if (Input.GetKey(key) && canCrouch && (!groundCheck || groundCheck.isGrounded))
        {
            // Save original values on first crouch.
            if (!defaultHeadYLocalPosition.HasValue)
            {
                defaultHeadYLocalPosition = headToLower.localPosition.y;
                defaultControllerHeight = controller.height;
                defaultControllerCenter = controller.center;
            }

            // Calculate crouch height.
            float newHeight = defaultControllerHeight.Value * crouchHeightFraction;
            float heightDifference = defaultControllerHeight.Value - newHeight;

            // Move camera down by the same amount that the collider height is reducing.
            // This keeps the camera aligned with where the collider's top surface has moved to.
            if (headToLower)
            {
                float newCameraY = defaultHeadYLocalPosition.Value - heightDifference;
                headToLower.localPosition = new Vector3(headToLower.localPosition.x, newCameraY, headToLower.localPosition.z);
            }

            // Adjust controller height while maintaining the original center offset.
            if (controller && defaultControllerCenter.HasValue)
            {
                float originalCenterOffset = defaultControllerCenter.Value.y;
                float originalBottomY = originalCenterOffset - (defaultControllerHeight.Value * 0.5f);
                
                // New center: keep bottom at same position, shrink from top.
                float newCenterY = originalBottomY + (newHeight * 0.5f);
                controller.height = newHeight;
                controller.center = new Vector3(defaultControllerCenter.Value.x, newCenterY, defaultControllerCenter.Value.z);
            }

            // Set IsCrouched state.
            if (!IsCrouched)
            {
                IsCrouched = true;
                SetSpeedOverrideActive(true);
                CrouchStart?.Invoke();
            }
        }
        else
        {
            if (IsCrouched && CanStand())
            {
                // Restore head position.
                if (headToLower && defaultHeadYLocalPosition.HasValue)
                {
                    headToLower.localPosition = new Vector3(headToLower.localPosition.x, defaultHeadYLocalPosition.Value, headToLower.localPosition.z);
                }

                // Restore controller height and center.
                if (controller && defaultControllerHeight.HasValue && defaultControllerCenter.HasValue)
                {
                    controller.height = defaultControllerHeight.Value;
                    controller.center = defaultControllerCenter.Value;
                }

                // Reset IsCrouched.
                IsCrouched = false;
                SetSpeedOverrideActive(false);
                CrouchEnd?.Invoke();
            }
        }
    }

    /// <summary>
    /// Check for overhead obstacles
    /// </summary>
    /// <returns>true if there is room to stand up.</returns>
    private bool CanStand()
    {
        float standingHeight = defaultControllerHeight.Value; // set this to your full stand height

        Vector3 center = transform.position + defaultControllerCenter.Value;

        float radius = 0.01f;

        float halfHeight = standingHeight / 2f;

        Vector3 bottom = center + Vector3.down * (halfHeight - radius);
        Vector3 top    = center + Vector3.up * (halfHeight - radius);

        int mask = ~LayerMask.GetMask("Player");
        return !Physics.CheckCapsule(bottom, top, radius, mask);
    }


    #region Speed override.
    void SetSpeedOverrideActive(bool state)
    {
        // Stop if there is no movement component.
        if (!movement)
        {
            return;
        }

        // Update SpeedOverride.
        if (state)
        {
            // Try to add the SpeedOverride to the movement component.
            if (!movement.speedOverrides.Contains(SpeedOverride))
            {
                movement.speedOverrides.Add(SpeedOverride);
            }
        }
        else
        {
            // Try to remove the SpeedOverride from the movement component.
            if (movement.speedOverrides.Contains(SpeedOverride))
            {
                movement.speedOverrides.Remove(SpeedOverride);
            }
        }
    }

    float SpeedOverride() => movementSpeed;
    #endregion
}
