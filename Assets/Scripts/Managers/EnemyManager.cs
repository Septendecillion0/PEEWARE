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
    public float minDistanceFromPlayer = 3f; // minimum distance
    public int maxSpawnAttempts = 30;      // how many times to retry if invalid
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
            SpawnRandomEnemy(0);
        }
    }

    void SpawnRandomEnemy(int attempts)
    {
        if (enemyPrefabs.Count == 0 || player == null)
            return;

        // Pick a random enemy prefab
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        // Pick a random position near the player
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = player.transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        if (Physics.CheckSphere(spawnPosition, 1f)) {
            Debug.Log($"failed enemy spawn");
            if (attempts < maxSpawnAttempts) SpawnRandomEnemy(attempts + 1); // try again if blocked
            return;
        }

        // Instantiate enemy and add to the existingEnemies list
        EnemySpawn(enemyPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"[ENEMY] Spawned new enemy");
    }


    public GameObject EnemySpawn(GameObject enemy, Vector3 loc, Quaternion direc){
        GameObject newEnemy = Instantiate(enemy, loc, direc);
        existingEnemies.Add(enemy);
        return newEnemy;
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
