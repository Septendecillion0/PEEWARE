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
    public GameObject peeMeter;
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
    Vector3 GetRandomPointInsideRoom(Bounds b)
    {
        float x = Random.Range(b.min.x + 2f, b.max.x - 2f);
        float z = Random.Range(b.min.z + 2f, b.max.z - 2f);
        float y = b.center.y;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Returns true if the spawn hits the ground and no overlapping colliders.
    /// </summary>
    bool IsPositionValid(Vector3 pos)
    {
        // must hit floor
        if (!Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f))
            return false;

        // no walls
        if (Physics.CheckSphere(pos, 0.5f))
            return false;

        return true;
    }

    /// <summary>
    /// Attempts to find a valid spawn position across random rooms and instantiates
    /// a random enemy prefab there. Validates against both enemy-specific rules
    /// and environment geometry. Gives up after maxSpawnAttempts tries.
    /// </summary>
    /// 
    /// TODO: COMPLETELY OVERHAUL SPAWN MECHANICS. this sucks also split this into separate responsibilities (finding spawn locations, actually spawning)
    void SpawnRandomEnemy()
    {
        if (GameManager.Instance.IsGameOver) return;
        if (enemyPrefabs.Count == 0 || player == null || roomBounds == null || roomBounds.Count == 0)
            return;

        Vector3 playerPos = player.transform.position;
        Vector3 playerForward = playerCam != null ? playerCam.transform.forward : player.transform.forward;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // pick random room
            int index = Random.Range(0, roomBounds.Count);
            Bounds b = roomBounds[index];

            // random point inside the room
            Vector3 pos = GetRandomPointInsideRoom(b);

            // pick a random enemy type
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Enemy enemyRules = enemyPrefab.GetComponent<Enemy>();

            // check enemy-specific spawn rules
            if (!enemyRules.IsValidSpawnPosition(playerPos, playerForward, pos))
                continue;

            // environment validation (walls, floor)
            if (!IsPositionValid(pos))
                continue;

            // spawn
            GameObject newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            existingEnemies.Add(newEnemy);

            Debug.Log($"[ENEMY] Spawned {enemyPrefab.name} at {pos}");
            return;
        }

        Debug.Log("[ENEMY] Failed to find valid spawn point after attempts");
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
