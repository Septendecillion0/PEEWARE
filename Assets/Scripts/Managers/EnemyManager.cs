using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public GameObject player;
    public List<GameObject> existingEnemies = new List<GameObject>();
    private void Awake(){
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
