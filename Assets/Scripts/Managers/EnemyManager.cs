using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Player Settings")]
    public GameObject player;
    public Camera playerCam;
    public GameObject peeMeter;
    public List<GameObject> existingEnemies = new List<GameObject>();

    public ScreenFade blind; //TODO: replace with blind image 
    public ScreenFade hurt; //TODO: replace with hurt image 

    [Header("Enemy Settings")]
    public List<GameObject> enemyPrefabs; // prefabs that each contain Enemy.cs with spawn rules
    public float SpawnInterval = 10f;
    public float spawnRadius = 5f;
    public int maxSpawnAttempts = 30; // attempts per spawn

    [Header("Map Related")]
    public GenerateMap mapGenerator;
    public GameObject mapGenManager;
    private List<Room> generatedRooms;
    private List<Bounds> roomBounds;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        blind.FadeIn(1.0f);
        StartCoroutine(FindPlayerAndStartSpawning());
    }

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
        mapGenerator = mapGenManager.GetComponent<GenerateMap>();
        generatedRooms = mapGenerator.GetAllPlacedRooms();
        roomBounds = mapGenerator.GetAllRoomBounds();

        StartCoroutine(SpawnEnemiesRoutine());
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(SpawnInterval);
            SpawnRandomEnemy();
        }
    }

    // Pick a random position inside a room
    Vector3 GetRandomPointInsideRoom(Bounds b)
    {
        float x = Random.Range(b.min.x + 2f, b.max.x - 2f);
        float z = Random.Range(b.min.z + 2f, b.max.z - 2f);
        float y = b.center.y;

        return new Vector3(x, y, z);
    }

    // Validate if spawn hits the ground & not inside walls
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

    void SpawnRandomEnemy()
    {
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

    public void EnemyVanish(GameObject enemy)
    {
        if (existingEnemies.Contains(enemy))
            existingEnemies.Remove(enemy);

        Destroy(enemy);
    }

    public void Blinded()
    {
        StartCoroutine(BlindBuff());
    }

    IEnumerator BlindBuff()
    {
        blind.FadeOut(0.1f);
        yield return new WaitForSeconds(3.0f);
        blind.FadeIn(2.0f);
    }

    public void Hurt()
    {
        StartCoroutine(HurtBuff());
    }

    IEnumerator HurtBuff()
    {
        hurt.FadeOut(0.2f);
        yield return new WaitForSeconds(0.5f);
        hurt.FadeIn(2.0f);
    }
}
