using UnityEngine;

// Base class for singleton instances (e.g. GameManager)
// Reference: https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance { get { return _instance; } }

    protected virtual void Awake()
    {
        // enforce singleton pattern
        if (_instance != null && _instance != this)
        {
            // multiple instances detected, destroy this one
            Destroy(this.gameObject);
            return;
        }
        _instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        // Clear the instance reference if this instance is being destroyed
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
