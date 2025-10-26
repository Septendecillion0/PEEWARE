using UnityEngine;
using UnityEngine.InputSystem;
public class FreeCamera : MonoBehaviour
{
    public float moveSpeed;// Movement speed
    public float lookSpeed;// Mouse sensitivity
    public float sprintMultiplier; // Shift for faster movement

    float rotationX;
    float rotationY;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 rot = transform.localEulerAngles;
        rotationX = rot.y;
        rotationY = rot.x;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        float speed = moveSpeed;

        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal"), 
            0f,
            Input.GetAxis("Vertical")
        );

        // Move in local space
        transform.Translate(direction * speed * Time.deltaTime, Space.Self);

        // Up/down movement with Q/E
        if (Input.GetKey(KeyCode.Space))
            transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
        if (Input.GetKey(KeyCode.LeftShift))
            transform.Translate(Vector3.down * speed * Time.deltaTime, Space.Self);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX += mouseX;
        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, -89f, 89f);

        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0);
    }
}
