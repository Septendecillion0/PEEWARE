using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

/// <summary>
/// Base class for all enemies.
/// Handles audio setup, basic player rotation, despawning, and spawn position validation.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Audio")]
    private AudioSource thisAudio;
    public AudioClip passiveSound;
    public AudioClip spawnSound;
    public AudioClip deathSound;

    [Header("Behavior")]
    [SerializeField] protected float scareAmount = 10f;

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
    void Awake()
    {
        thisAudio = GetComponent<AudioSource>();
        if (thisAudio == null)
            throw new System.InvalidOperationException($"[{GetType().Name}] AudioSource component not found on enemy prefab");

        if (spawnSound != null) thisAudio.PlayOneShot(spawnSound);
        if (passiveSound != null)
        {
            thisAudio.clip = passiveSound;
            thisAudio.loop = true;
            thisAudio.Play();
        }
    }

    // Base Update that always orients toward player
    protected virtual void Update()
    {
        // Always look at the player
        if (EnemyManager.Instance != null && EnemyManager.Instance.player != null)
        {
            Vector3 direction = EnemyManager.Instance.player.transform.position - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void EnemyDeath(){
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
    }

    public void NaturallyDespawn(float age)
    {
        StartCoroutine(Nd(age));
    }

    private IEnumerator Nd(float age)
    {
        yield return new WaitForSeconds(age);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
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