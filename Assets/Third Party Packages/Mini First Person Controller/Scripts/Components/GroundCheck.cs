using UnityEngine;

[ExecuteInEditMode]
public class GroundCheck : MonoBehaviour
{
    [Tooltip("Maximum distance from the ground.")]
    public float distanceThreshold = .15f;

    [Tooltip("Whether this transform is grounded now.")]
    public bool isGrounded = true;
    /// <summary>
    /// Called when the ground is touched again.
    /// </summary>
    public event System.Action Grounded;

    CharacterController controller;
    const float OriginOffset = .001f;

    Vector3 RaycastOrigin
    {
        get
        {
            // If CharacterController exists, raycast from its bottom position
            if (controller != null)
            {
                Vector3 controllerBottom = transform.position + controller.center - Vector3.up * controller.height * 0.5f;
                return controllerBottom + Vector3.up * OriginOffset;
            }
            // Fallback: use transform position
            return transform.position + Vector3.up * OriginOffset;
        }
    }

    float RaycastDistance => distanceThreshold + OriginOffset;

    void Awake()
    {
        // Cache CharacterController reference if available
        controller = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        // Check if we are grounded now.
        bool isGroundedNow = Physics.Raycast(RaycastOrigin, Vector3.down, distanceThreshold * 2);

        // Call event if we were in the air and we are now touching the ground.
        if (isGroundedNow && !isGrounded)
        {
            Grounded?.Invoke();
        }

        // Update isGrounded.
        isGrounded = isGroundedNow;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a line in the Editor to show whether we are touching the ground.
        Debug.DrawLine(RaycastOrigin, RaycastOrigin + Vector3.down * RaycastDistance, isGrounded ? Color.white : Color.red);
    }
}
