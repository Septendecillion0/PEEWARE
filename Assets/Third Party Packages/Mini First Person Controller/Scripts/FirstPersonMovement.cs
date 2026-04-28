using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float walkSpeed;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("CharacterController Settings")]
    public float gravity;

    CharacterController controller;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    /// <summary> Current vertical velocity for gravity and jumping. Can be modified by Jump script. </summary>
    public float verticalVelocity { get; set; }


    void Awake()
    {
        // Get the CharacterController on this.
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);

        // Get targetMovingSpeed.
        float targetMovingSpeed = IsRunning ? runSpeed : walkSpeed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        // Get targetVelocity from input.
        Vector2 targetVelocity = new Vector2(Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);

        // Apply gravity.
        if (!controller.isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0)
        {
            verticalVelocity = 0;
        }

        // Build movement vector and apply it.
        Vector3 movementDirection = transform.rotation * new Vector3(targetVelocity.x, 0, targetVelocity.y);
        Vector3 finalVelocity = new Vector3(movementDirection.x, verticalVelocity, movementDirection.z);
        controller.Move(finalVelocity * Time.deltaTime);
    }
}