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
    private void Awake(){
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blind.enabled = false;
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

    public void Blinded(){
        StartCoroutine(BlindBuff());
    }
    
    public IEnumerator BlindBuff(){
        blind.enabled = true;
        yield return new WaitForSeconds(3.0f);
        blind.enabled = false;
    }
}
