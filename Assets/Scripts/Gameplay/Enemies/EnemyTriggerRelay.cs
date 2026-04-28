using UnityEngine;

/// <summary>
/// Helper component to pass triggers upwards from detection zones to the main Enemy
/// Necessary for enemies with multiple trigger zones
/// </summary>
/// <remarks>
/// Use OnTriggerDetected and OnTriggerExitDetected in the parent, NOT OnTriggerEnter/Exit
/// </remarks>
public class EnemyTriggerRelay : MonoBehaviour
{
    Enemy parent;

    void Awake()
    {
        parent = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        parent.OnTriggerDetected(other);
    }

    void OnTriggerExit(Collider other)
    {
        parent.OnTriggerExitDetected(other);
    }
}