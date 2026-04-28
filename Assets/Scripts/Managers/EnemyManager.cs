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
/// TODO: implement player manager and get references from there
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
    [HideInInspector] public GameObject player;
    [HideInInspector] public Camera playerCam;

    [Tooltip("List of existing enemies; READ-ONLY")]
    public List<GameObject> existingEnemies = new List<GameObject>();

    [Header("Enemy Class Settings")]
    public List<GameObject> enemyPrefabs;
    List<GameObject> validPrefabsList;
    List<GameObject> enemyPrefabDict;
    public float SpawnInterval = 10f;
    public int maxSpawnAttempts = 30; // attempts per spawn

    [Header("Map Related")]
    private List<Room> generatedRooms;
    private List<Bounds> roomBounds;

    /// <summary>
    /// Initialize the validPrefabsList (pulled from when spawning) and enemyPrefabDict (reference holder for all prefabs by id)
    /// Begins the startup coroutine that waits for the player and camera before starting the spawn loop.
    /// 
    /// TODO: can change prefabdict to dictionary
    /// </summary>
    void Start()
    {
        validPrefabsList = new List<GameObject>(enemyPrefabs);
        enemyPrefabDict = new List<GameObject>(new GameObject[10]);
        foreach (GameObject enemyPrefab in enemyPrefabs)
        {
            Enemy enemy = enemyPrefab.GetComponent<Enemy>();
            enemyPrefabDict[enemy.id] = enemyPrefab;
        }

        StartCoroutine(FindPlayerAndStartSpawning());
    }

    /// <summary>
    /// Loops until the player and main camera are present in the scene,
    /// then fetches room data from the map generator and starts the spawn loop.
    /// </summary>
    /// <remarks>
    /// Doesn't proceed to spawning if there is no map: used for test scenes
    /// </remarks>
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
        generatedRooms = MapGenerationManager.Instance?.GetAllPlacedRooms();
        roomBounds = MapGenerationManager.Instance?.GetAllRoomBounds();

        if (generatedRooms == null || roomBounds == null)
        {
            Debug.LogWarning("No generated rooms found. Spawning disabled.");
            yield break;
        }

        StartCoroutine(SpawnEnemiesRoutine());
    }

    /// <summary>
    /// Spawns an enemy at a regular interval until the game ends
    /// </summary>
    /// <remarks>
    /// Timer uses TimeScale, so if GameState time change behavior is correct, will correctly pause/unpause
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
    /// Attempts to spawn an enemy with appropriate validation.
    /// First checks if the enemy has a custom spawn override (e.g., Ghost).
    /// Otherwise uses the base spawn algorithm with base distance validation
    /// and optional enemy-specific filtering.
    /// 
    /// TODO: this method needs to be separated into more helper methods so the control flow is easy to understand
    /// </summary>
    void SpawnRandomEnemy()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (validPrefabsList.Count == 0 || player == null || roomBounds == null || roomBounds.Count == 0)
            return;

        Vector3 playerPos = player.transform.position;
        Vector3 playerForward = playerCam != null ? playerCam.transform.forward : player.transform.forward;

        // Pick a random enemy type
        GameObject enemyPrefab = validPrefabsList[Random.Range(0, validPrefabsList.Count)];
        Enemy enemy = enemyPrefab.GetComponent<Enemy>();

        // Check if this enemy type has a full custom spawn override
        Vector3? customSpawnPos = enemy.GetFullCustomSpawnPosition(playerPos, playerForward, roomBounds);
        if (customSpawnPos.HasValue)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, customSpawnPos.Value, Quaternion.identity);
            existingEnemies.Add(newEnemy);
            Debug.Log($"[ENEMY] Spawned {enemyPrefab.name} at {customSpawnPos.Value} (custom spawn)");
            return;
        }

        // Base spawn algorithm: attempt to find a valid spawn position
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Pick random room and point inside room
            // note: ignoreY is true for grounded enemies to avoid spawning in ground or ceiling; flying enemies can spawn at any Y
            // note: using offset of 0.1 to avoid touching walls; Environment check will validate
            // TODO: uses maxHorizontalDistnace as 3-dimensional radius, doesn't align
            Vector3 candidatePos = MapGenerationManager.Instance.GetRandomPointFromDistance(playerPos, enemy.maxHorizontalDistance, ignoreY: enemy.IsGrounded, offset: 0.1f);
            
            if (!enemy.CheckSpawnDistanceAndDirection(playerPos, playerForward, candidatePos))
                continue;

            // Check environment (ground exists, no obstacles)
            if (!enemy.CheckEnvironmentClear(candidatePos))
                continue;

            // Check enemy-specific filtering (e.g., must be near player, must be in certain areas, etc.)
            if (!enemy.CheckSpecialSpawnRules(playerPos, playerForward, candidatePos))
                continue;

            // All validations passed - spawn the enemy
            GameObject newEnemy = Instantiate(enemyPrefab, candidatePos, Quaternion.identity);
            existingEnemies.Add(newEnemy);
            if (enemy.uniqueEnemy)
            {
                validPrefabsList.Remove(enemyPrefab);
            }
            Debug.Log($"[ENEMY] Spawned {enemyPrefab.name} at {candidatePos}");
            return;
        }

        Debug.LogWarning($"[ENEMY] Failed to find valid spawn point for {enemyPrefab.name} after {maxSpawnAttempts} attempts");
    }

    /// <summary>
    /// Removes the enemy from the tracked list and destroys its GameObject.
    /// Called by individual enemies based on their despawning/death behavior
    /// Not called on scene resets
    /// </summary>
    public void EnemyVanish(GameObject enemy)
    {
        Enemy enemyType = enemy.gameObject.GetComponent<Enemy>();
        if (enemyType.uniqueEnemy)
        {   
            GameObject enemyPrefab = enemyPrefabDict[enemyType.id];
            validPrefabsList.Add(enemyPrefab);
        }
        
        if (existingEnemies.Contains(enemy)) // cannot assert bc of enemies like Fairy
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



    // /// <summary>
    // /// Validates that a spawn position is within acceptable distance ranges from the player.
    // /// Checks horizontal distance, vertical distance, and direction relative to player forward.
    // /// 
    // /// DEPRECATED METHOD - using MapGenerationManager methods for validation checks
    // /// </summary>
    // private bool IsBaseDistanceValid(Vector3 playerPos, Vector3 playerForward, Vector3 spawnPos, Enemy enemyRules)
    // {
    //     // Flat distances on XZ plane
    //     Vector3 flatPlayerPos = new Vector3(playerPos.x, 0, playerPos.z);
    //     Vector3 flatSpawnPos = new Vector3(spawnPos.x, 0, spawnPos.z);

    //     float horizontalDist = Vector3.Distance(flatPlayerPos, flatSpawnPos);
    //     if (horizontalDist < enemyRules.minHorizontalDistance || horizontalDist > enemyRules.maxHorizontalDistance)
    //         return false;

    //     // Vertical distance
    //     float verticalDelta = spawnPos.y - playerPos.y;
    //     if (verticalDelta < enemyRules.minVerticalDistance || verticalDelta > enemyRules.maxVerticalDistance)
    //         return false;

    //     // Direction relative to player looking direction
    //     Vector3 dirToSpawn = (flatSpawnPos - flatPlayerPos).normalized;
    //     float dot = Vector3.Dot(playerForward.normalized, dirToSpawn);

    //     if (dot < enemyRules.minDirectionDot || dot > enemyRules.maxDirectionDot)
    //         return false;

    //     return true;
    // }
}
