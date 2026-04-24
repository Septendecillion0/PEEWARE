using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Handles enemy spawning, despawning, and player-affecting visual effects (blind, hurt).
/// On startup, waits for the player and camera to be available before beginning
/// the spawn loop. Spawns enemies at intervals in valid positions across generated rooms.
/// </summary>
/// <remarks>
/// Spawn positions are validated against enemy-specific rules (defined in Enemy.cs)
/// and environment checks (floor raycast, overlap sphere).
/// TODO: update and rework spawn mechanics
/// 
/// Visual effects (blind/hurt) are called by enemy subclasses, then outsourced to UIManager
/// 
/// TODO: confirm audio implementation and reorganize to make sense
/// </remarks>
public class EnemyManager : Singleton<EnemyManager>
{
    /// <summary>
    /// Singleton manager setup, non persistent
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    [Header("Player Settings")]
    public GameObject player;
    public Camera playerCam;
    public List<GameObject> existingEnemies = new List<GameObject>();

    [Header("Enemy Class Settings")]
    public List<GameObject> enemyPrefabs; // prefabs that each contain Enemy.cs with spawn rules
    public float SpawnInterval = 10f;
    public float spawnRadius = 5f;
    public int maxSpawnAttempts = 30; // attempts per spawn

    [Header("Map Related")]
    private List<Room> generatedRooms;
    private List<Bounds> roomBounds;

    /// <summary>
    /// Begins the startup coroutine that waits for the player and camera before starting the spawn loop.
    /// </summary>
    void Start()
    {
        StartCoroutine(FindPlayerAndStartSpawning());
    }

    /// <summary>
    /// Loops until the player and main camera are present in the scene,
    /// then fetches room data from the map generator and starts the spawn loop.
    /// </summary>
    IEnumerator FindPlayerAndStartSpawning()
    {
        // Find Player
        while (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
            {
                player = found;
                break;
            }
            yield return null;
        }

        // Find Camera
        while (playerCam == null)
        {
            Camera camFound = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
            if (camFound != null)
            {
                playerCam = camFound;
                break;
            }
            yield return null;
        }

        // Room data
        generatedRooms = MapGenerationManager.Instance.GetAllPlacedRooms();
        roomBounds = MapGenerationManager.Instance.GetAllRoomBounds();

        StartCoroutine(SpawnEnemiesRoutine());
    }

    /// <summary>
    /// Spawns an enemy at a regular interval while the game is playing
    /// </summary>
    /// <remarks>
    /// The coroutine runs until the game ends (gameover). 
    /// However, spawning WITHIN the coroutine is stopped when the game is not playing (paused, etc.)
    /// The pause is tied to TimeScale, so the spawn interval should correctly track the elapsed time
    /// and resume accordingly
    /// 
    /// (optional TODO) tie spawn routine to gameState events, manually track elapsed time
    /// </remarks>
    IEnumerator SpawnEnemiesRoutine()
    {
        while (!GameManager.Instance.IsGameOver)
        {
            yield return new WaitForSeconds(SpawnInterval);

            if (GameManager.Instance.State == GameManager.GameState.Playing)
            {
                SpawnRandomEnemy();
            }
        }
    }

    /// <summary>
    /// Returns a random point inside the given room bounds,
    /// inset by 2 units on each side to avoid spawning in walls.
    /// </summary>
    private Vector3 GetRandomPointInsideRoom(Bounds b)
    {
        float x = Random.Range(b.min.x + 2f, b.max.x - 2f);
        float z = Random.Range(b.min.z + 2f, b.max.z - 2f);
        float y = b.center.y;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Validates that a spawn position is within acceptable distance ranges from the player.
    /// Checks horizontal distance, vertical distance, and direction relative to player forward.
    /// </summary>
    private bool IsBaseDistanceValid(Vector3 playerPos, Vector3 playerForward, Vector3 spawnPos, Enemy enemyRules)
    {
        // Flat distances on XZ plane
        Vector3 flatPlayerPos = new Vector3(playerPos.x, 0, playerPos.z);
        Vector3 flatSpawnPos = new Vector3(spawnPos.x, 0, spawnPos.z);

        float horizontalDist = Vector3.Distance(flatPlayerPos, flatSpawnPos);
        if (horizontalDist < enemyRules.minHorizontalDistance || horizontalDist > enemyRules.maxHorizontalDistance)
            return false;

        // Vertical distance
        float verticalDelta = spawnPos.y - playerPos.y;
        if (verticalDelta < enemyRules.minVerticalDistance || verticalDelta > enemyRules.maxVerticalDistance)
            return false;

        // Direction relative to player looking direction
        Vector3 dirToSpawn = (flatSpawnPos - flatPlayerPos).normalized;
        float dot = Vector3.Dot(playerForward.normalized, dirToSpawn);

        if (dot < enemyRules.minDirectionDot || dot > enemyRules.maxDirectionDot)
            return false;

        return true;
    }

    /// <summary>
    /// Validates environment: must have ground below and no obstacles.
    /// </summary>
    private bool IsEnvironmentClear(Vector3 pos)
    {
        // Must hit floor
        if (!Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f))
            return false;

        // No obstacles
        if (Physics.CheckSphere(pos, 0.5f))
            return false;

        return true;
    }

    /// <summary>
    /// Attempts to spawn an enemy with appropriate validation.
    /// First checks if the enemy has a custom spawn override (e.g., Ghost).
    /// Otherwise uses the base spawn algorithm with base distance validation
    /// and optional enemy-specific filtering.
    /// </summary>
    void SpawnRandomEnemy()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (enemyPrefabs.Count == 0 || player == null || roomBounds == null || roomBounds.Count == 0)
            return;

        Vector3 playerPos = player.transform.position;
        Vector3 playerForward = playerCam != null ? playerCam.transform.forward : player.transform.forward;

        // Pick a random enemy type
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        Enemy enemyRules = enemyPrefab.GetComponent<Enemy>();

        // Check if this enemy type has a full custom spawn override
        Vector3? customSpawnPos = enemyRules.GetFullCustomSpawnPosition(playerPos, playerForward, roomBounds);
        if (customSpawnPos.HasValue)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, customSpawnPos.Value, Quaternion.identity);
            existingEnemies.Add(newEnemy);
            Debug.Log($"[ENEMY] Spawned {enemyPrefab.name} at {customSpawnPos.Value} (custom spawn)");
            return;
        }

        // Base spawn algorithm: try to find valid position within room bounds and distance constraints
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Pick random room and point within bounds
            int index = Random.Range(0, roomBounds.Count);
            Bounds b = roomBounds[index];
            Vector3 candidatePos = GetRandomPointInsideRoom(b);

            // Check base distance validation (horizontal, vertical, direction)
            if (!IsBaseDistanceValid(playerPos, playerForward, candidatePos, enemyRules))
                continue;

            // Check environment (ground exists, no obstacles)
            if (!IsEnvironmentClear(candidatePos))
                continue;

            // Check enemy-specific filtering (e.g., must be near player, must be in certain areas, etc.)
            if (!enemyRules.IsAcceptableSpawnLocation(playerPos, playerForward, candidatePos))
                continue;

            // All validations passed - spawn the enemy
            GameObject newEnemy = Instantiate(enemyPrefab, candidatePos, Quaternion.identity);
            existingEnemies.Add(newEnemy);
            Debug.Log($"[ENEMY] Spawned {enemyPrefab.name} at {candidatePos}");
            return;
        }

        Debug.Log($"[ENEMY] Failed to find valid spawn point for {enemyPrefab.name} after {maxSpawnAttempts} attempts");
    }

    /// <summary>
    /// Removes the enemy from the tracked list and destroys its GameObject.
    /// Called by individual enemies when they are done (e.g. walked past the player).
    /// </summary>
    public void EnemyVanish(GameObject enemy)
    {
        if (existingEnemies.Contains(enemy))
            existingEnemies.Remove(enemy);

        Destroy(enemy);
    }

    /// <summary>
    /// Calls UIManager to play the blinded visual animation
    /// Called when the player is blinded by an enemy.
    /// </summary>
    public void Blinded()
    {
        UIManager.Instance.PlayBlind();
    }

    /// <summary>
    /// Calls UIManager to play the hurt visual animation
    /// Called when the player takes damage.
    /// </summary>
    public void Hurt()
    {
        UIManager.Instance.PlayHurt();
    }
}
