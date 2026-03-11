using UnityEngine;

/// <summary>
/// Generic base class for implementing the Singleton pattern.
/// Ensures only one instance of <typeparamref name="T"/> exists at a time.
/// Duplicate instances are automatically destroyed on Awake.
/// </summary>
/// <typeparam name="T">
/// The MonoBehaviour subclass to be used as a singleton (e.g. GameManager).
/// </typeparam>
/// <remarks>
/// Usage: public class GameManager : Singleton<GameManager> { ... }
/// Override <see cref="Awake"/> and <see cref="OnDestroy"/> in subclasses as needed,
/// but always call <c>base.Awake()</c> and <c>base.OnDestroy()</c> to preserve singleton behaviour.
/// If the instance needs to live through multiple scenes, add <c>DontDestroyOnLoad(gameObject);</c> in Awake() override.
/// Reference: https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern
/// </remarks>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    /// <summary>
    /// Gets the singleton instance of <typeparamref name="T"/>.
    /// Returns null if no instance has been initialized yet.
    /// </summary>
    public static T Instance { get { return _instance; } }

    /// <summary>
    /// Enforces the singleton pattern on startup.
    /// If an instance already exists, this duplicate GameObject is destroyed immediately.
    /// </summary>
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

    /// <summary>
    /// Clears the static instance reference when this object is destroyed,
    /// preventing stale references to a destroyed object.
    /// </summary>
    protected virtual void OnDestroy()
    {
        // Clear the instance reference if this instance is being destroyed
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
