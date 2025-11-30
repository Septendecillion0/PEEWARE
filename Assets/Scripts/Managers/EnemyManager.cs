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

    public TextMeshProUGUI blind;

    [Header("Enemy Settings")]
    public List<GameObject> enemyPrefabs; // drag your enemy prefabs here
    public float SpawnInterval = 10f;
    public float spawnRadius = 5f;        // maximum distance from player
    public float minDistanceFromPlayer = 20f; // minimum distance
    public float maxDistanceFromPlayer = 40f;
    public int maxSpawnAttempts = 30;      // how many times to retry if invalid

    [Header("Map Related")]
    public GenerateMap mapGenerator;
    public GameObject mapGenManager;
    private List<Room> generatedRooms;
    private List<Bounds> roomBounds;

    private void Awake(){
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blind.enabled = false;
        //Find PeeMeter
        // Start the spawn coroutine
        StartCoroutine(FindPlayerAndStartSpawning());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator FindPlayerAndStartSpawning()
    {
        // Wait until a player exists in the scene
        while (player == null)
        {   
            Debug.Log($"looking for player");
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
            {
                player = found;
                Debug.Log($"player found");
                break;
            }
            yield return null; // wait for next frame
        }

        while(playerCam == null){
            Camera camFound = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (camFound != null){
                playerCam = camFound;
                Debug.Log($"Camera Found");
                break;
            }
            yield return null;
        }
        Debug.Log($"starting enemy spawns");

        //Getting reference to the rooms
        mapGenerator = mapGenManager.GetComponent<GenerateMap>();
        generatedRooms = mapGenerator.GetAllPlacedRooms();
        roomBounds = mapGenerator.GetAllRoomBounds();

        // Now start spawning enemies
        StartCoroutine(SpawnEnemiesRoutine());
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            // Wait for the interval
            yield return new WaitForSeconds(SpawnInterval);

            // Spawn a random enemy
            SpawnRandomEnemy();
        }
    }

    //Pick a random position in the room bound
    Vector3 GetRandomPointInsideRoom(Bounds b)
    {
        float x = Random.Range(b.min.x + 1f, b.max.x - 1f);
        float z = Random.Range(b.min.z + 1f, b.max.z - 1f);
        float y = b.center.y;  // typically same floor height

        return new Vector3(x, y, z);
    }

    //Avoid spawning too close to player
    bool IsWithinSpawnRange(Vector3 pos)
    {
        float dist = Vector3.Distance(pos, player.transform.position);
        return dist > minDistanceFromPlayer && dist < maxDistanceFromPlayer;
    }

    //Check if the enemy is hitting the wall
    bool IsPositionValid(Vector3 pos)
    {
        // Check there's ground
        if (!Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f))
            return false;

        // Check no walls or other obstacles
        return !Physics.CheckSphere(pos, 0.5f);
    }

    void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Count == 0 || player == null || roomBounds == null || roomBounds.Count == 0)
            return;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Pick a random room
            int index = Random.Range(0, roomBounds.Count);
            Bounds b = roomBounds[index];

            // Get random point inside it
            Vector3 pos = GetRandomPointInsideRoom(b);

            // Reject if too close to player
            if (!IsWithinSpawnRange(pos))
                continue;

            // Reject if inside objects / walls
            if (!IsPositionValid(pos))
                continue;

            // Pick enemy prefab
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

            // Spawn it
            GameObject newEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            existingEnemies.Add(newEnemy);

            Debug.Log($"[ENEMY] Spawned '{enemyPrefab.name}' in room {index}");
            return;
        }

        Debug.Log("[ENEMY] Failed to find valid spawn point");
    }

    public void EnemyVanish(GameObject enemy){
        if (existingEnemies.Contains(enemy)){
            existingEnemies.Remove(enemy); 
        }
        Destroy(enemy);
    }

    public void Blinded(){
        StartCoroutine(BlindBuff());
    }
    
    public IEnumerator BlindBuff(){
        blind.enabled = true;
        yield return new WaitForSeconds(3.0f);
        blind.enabled = false;
    }
}
