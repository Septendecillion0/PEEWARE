using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

/// <summary>
/// Base class for all enemies.
/// Handles audio setup, basic player rotation, despawning, and spawn position validation.
/// </summary>
public abstract class Enemy : MonoBehaviour
{   

    // TODO: separate audio in EnemyAudio component

    [Header("Audio")]
    private AudioSource thisAudio;
    public AudioClip passiveSound;
    public AudioClip spawnSound;
    public AudioClip deathSound;

    // TODO: properties and spawn properties can be separated into ScriptableObject data

    [Header("Base Properties")]
    [SerializeField] public virtual int id => 0;
    [SerializeField] protected virtual float despawnTime => 30f;
    [SerializeField] protected virtual bool naturallyDespawns => true;
    [SerializeField] protected virtual float scareAmount => 99f; // default should be overridden in all enemies
    [SerializeField] public virtual bool uniqueEnemy => false;

    [Header("Spawn Constraints")]
    // Indicator for whether the enemy stays on the ground
    // Note: default value is true but explicitly write in the value in all enemies for clarity
    public virtual bool IsGrounded => true;

    [Tooltip("Minimum allowed horizontal (XZ plane) distance from player.")]
    public float minHorizontalDistance = 5f;

    [Tooltip("Maximum allowed horizontal (XZ plane) distance from player.")]
    public float maxHorizontalDistance = 20f;

    [Tooltip("Minimum vertical difference allowed between player and spawn position.")]
    public float minVerticalDistance = -5f;

    [Tooltip("Maximum vertical difference allowed between player and spawn position.")]
    public float maxVerticalDistance = 5f;

    // Center of allowed spawn direction relative to player forward
    [Tooltip("Center of allowed spawn direction cone, from 0 (front of player) to 180 (behind)")]
    [Range(-180f, 180f)]
    public float viewAngleCenter = 0f; // default to in front of player

    [Tooltip("Width of allowed spawn direction cone, from 0 (exact direction) to 180 (any direction)")]
    // How wide the allowed cone is (half-angle)
    [Range(0f, 180f)]
    public float viewAngleWidth = 180f; // default to any direction (no constraint)

    // currently just used to configure audio
    // TODO: separate into helper method and/or refactor to separate component
    protected virtual void Awake()
    {
        InitializeAudio();
    }

    // TODO: separate into audio component
    protected virtual void InitializeAudio()
    {
        thisAudio = GetComponent<AudioSource>();
        if (thisAudio == null)
        {
            thisAudio = gameObject.AddComponent<AudioSource>();
        }
    }

    protected virtual void Start()
    {
        StartAudio();
        if (naturallyDespawns)
        {
            StartCoroutine(NaturallyDespawnCoroutine(despawnTime));
        }
    }

    // TODO: separate into audio component
    protected virtual void StartAudio()
    {
        if (spawnSound != null) thisAudio.PlayOneShot(spawnSound);
        if (passiveSound != null)
        {
            thisAudio.clip = passiveSound;
            thisAudio.loop = true;
            thisAudio.Play();
        }
    }

    // Base Update
    // Enforces looking at the player
    // NOTE: should only be overridden for special behavior OR if 3D enemies are added
    protected virtual void Update()
    {
        // Always look at the player
        if (EnemyManager.Instance != null && EnemyManager.Instance.playerCam != null)
        {
            Vector3 direction = EnemyManager.Instance.playerCam.transform.position - transform.position;
            if (IsGrounded)
            {
                direction.y = 0;
            }
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// Automatically despawn the enemy after a certain amount of time has passed.
    /// Note: uses TimeScale, so will correctly pause/unpause with game state changes.
    /// </summary> <remarks>
    /// Called by base class Start if naturallyDespawns is true
    /// </remarks>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    private IEnumerator NaturallyDespawnCoroutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
    }

    /// <summary>
    /// Handle enemy death behavior
    /// </summary>
    /// <remarks>
    /// NOTE: distinct from despawning or disappearing; enemy death is specifically for enemies that can "die"
    protected virtual void EnemyDeath(){
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
    }
    
    public virtual void OnTriggerDetected(Collider other)
    {
        // Default implementation does nothing; override in child classes for specific trigger behavior
    }

    public virtual void OnTriggerExitDetected(Collider other)
    {
        // Default implementation does nothing; override in child classes for specific trigger behavior
    }

    /// -------------------------------------
    /// SPAWN VALIDATION LOGIC
    /// -------------------------------------

    /// <summary>
    /// Validates that a given position is within the given ranges of distance and direction from the player.
    /// 1st layer of spawn validation
    /// </summary>
    /// <returns>True if the position is valid, false otherwise</returns>
    /// 
    /// note: max spawn distance check is redundant with MapGenerationManager.GetRoomsWithinDistance
    /// TODO: spawn direction overrides are not added to enemies
    /// TODO: add some sort of visibility parameter to define enemy spawning based on whether the player can see them
    ///       ^ revisit spawn direction to define if enemies should spawn based on visibility or direction
    public bool CheckSpawnDistanceAndDirection(Vector3 playerPos, Vector3 playerForward, Vector3 spawnPos)
    {
        Vector3 flatPlayerPos = new Vector3(playerPos.x, 0, playerPos.z);
        Vector3 flatSpawnPos = new Vector3(spawnPos.x, 0, spawnPos.z);

        float horizontalDist = Vector3.Distance(flatPlayerPos, flatSpawnPos);
        if (horizontalDist < minHorizontalDistance || horizontalDist > maxHorizontalDistance)
        {   
            Debug.LogWarning($"[ENEMY] Spawn validation failed: horizontal distance {horizontalDist} not in range [{minHorizontalDistance}, {maxHorizontalDistance}]");
            return false;
        }

        float verticalDelta = spawnPos.y - playerPos.y;
        if (verticalDelta < minVerticalDistance || verticalDelta > maxVerticalDistance)
        {
            Debug.LogWarning($"[ENEMY] Spawn validation failed: vertical distance {verticalDelta} not in range [{minVerticalDistance}, {maxVerticalDistance}]");
            return false;
        }

        // Direction check using ViewAngle
        if (viewAngleWidth >= 180f)
        {
            Debug.Log("position validation passed");
            return true; // no direction constraint
        }

        Vector3 flatForward = Flatten(playerForward);
        Vector3 desiredDir = GetRotatedDirection(flatForward, viewAngleCenter);
        
        Vector3 flatToSpawn = Flatten(spawnPos - playerPos);
        float angle = Vector3.Angle(desiredDir, flatToSpawn);

        Debug.Log("shouldn't see this");
        return angle <= viewAngleWidth;
    }

    /// Helper methods for vector math
    Vector3 Flatten(Vector3 v)
    {
        v.y = 0;
        return v.normalized;
    }
    Vector3 GetRotatedDirection(Vector3 forward, float angleDegrees)
    {
        return Quaternion.AngleAxis(angleDegrees, Vector3.up) * forward;
    }
    /// <summary>
    /// Check if the surrounding geometry is clear
    /// 2nd layer of spawn validation
    /// </summary>
    /// <returns>True if the environment is clear, false otherwise.</returns>
    public bool CheckEnvironmentClear(Vector3 candidatePos)
    {
        // Check if there's ground below the candidate position
        // In theory, should be redundant given the room bounds
        // note: raycast checks from 0.5 units above the candidate down to 5 units below
        if (!Physics.Raycast(candidatePos + Vector3.up * 0.5f, Vector3.down, 5f))
        {
            Debug.LogWarning($"[ENEMY] Spawn validation failed: no ground detected below candidate position {candidatePos}");
            return false;
        }

        // Check for obstacles at the candidate position
        // TODO: replace CheckSphere
        if (gameObject.GetComponent<Collider>() != null && Physics.CheckSphere(candidatePos, 0.5f))
        {
            Debug.LogWarning($"[ENEMY] Spawn validation failed: obstacle detected at candidate position {candidatePos}");
            return false;
        }

        Debug.Log("environment validation passed");
        return true;
    }

    /// <summary>
    /// Enemy-specific spawn position checks
    /// 3rd layer of spawn validation
    /// </summary>
    /// <returns>True if the spawn location is valid, false otherwise</returns>
    public virtual bool CheckSpecialSpawnRules(Vector3 playerPos, Vector3 playerForward, Vector3 candidatePos)
    {   
        Debug.Log("enemy-specific validation passed");
        return true;  // Default: accept all positions
    }

    /// <summary>
    /// Fully custom spawn position override
    /// Passes base class checks of distance, environment, etc.
    /// 4th layer (special exception) of spawn validation
    /// </summary>
    /// <returns>A valid spawn position, or null to use standard algorithm</returns>
    public virtual Vector3? GetFullCustomSpawnPosition(Vector3 playerPos, Vector3 playerForward, List<Bounds> roomBounds)
    {
        return null;  // Default: null indicates no override, proceed to standard spawn algorithm
    }
}