using UnityEngine;

[ExecuteInEditMode]
public class Zoom : MonoBehaviour
{
    [SerializeField] private Camera cam; // assign in inspector or default to main
    public float defaultFOV = 60;
    public float maxZoomFOV = 15;
    [Range(0, 1)]
    public float currentZoom;
    public float sensitivity = 1;


    void Awake()
    {   
        // default to main camera smth idk
        if (cam == null)
        {
            cam = Camera.main;
        }
        if (cam)
        {
            defaultFOV = cam.fieldOfView;
        }
    }

    void Update()
    {
        // Update the currentZoom and the camera's fieldOfView.
        currentZoom += Input.mouseScrollDelta.y * sensitivity * .05f;
        currentZoom = Mathf.Clamp01(currentZoom);
        cam.fieldOfView = Mathf.Lerp(defaultFOV, maxZoomFOV, currentZoom);
    }
}
