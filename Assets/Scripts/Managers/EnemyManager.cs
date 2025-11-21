using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public GameObject player;
    public List<GameObject> existingEnemies = new List<GameObject>();

    public TextMeshProUGUI blind;

    [Header("Enemy Settings")]
    public List<GameObject> enemyPrefabs; // drag your enemy prefabs here
    public float SpawnInterval = 20f;
    public float spawnRadius = 5f;        // maximum distance from player
    public float minDistanceFromPlayer = 3f; // minimum distance
    public int maxSpawnAttempts = 10;      // how many times to retry if invalid
    private void Awake(){
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blind.enabled = false;
        // Start the spawn coroutine
        StartCoroutine(SpawnEnemiesRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
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

    void SpawnRandomEnemy()
    {
        if (enemyPrefabs.Count == 0 || player == null)
            return;

        // Pick a random enemy prefab
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        // Pick a random position near the player
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = player.transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        if (Physics.CheckSphere(spawnPosition, 1f)) return; // skip if blocked


        // Instantiate enemy and add to the existingEnemies list
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        existingEnemies.Add(newEnemy);

        Debug.Log($"[ENEMY] Spawned new enemy");
    }


    public void EnemySpawn(GameObject enemy, Vector3 loc, Quaternion direc){
        Instantiate(enemy, loc, direc);
        existingEnemies.Add(enemy);
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
