using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelInitController : MonoBehaviour
{   
    [SerializeField] MapGenerator mapGenerator;
    [SerializeField] CutsceneController cutsceneController;
    [SerializeField] LevelPreviewCutscene levelPreviewCutscene;
    [SerializeField] EnemyManager enemyManager;

    [HideInInspector] MapData mapData;
    [HideInInspector] GameObject player;
    [HideInInspector] Camera playerCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(InitializeLevel());
    }

    IEnumerator InitializeLevel()
    {
        mapData = ScriptableObject.CreateInstance<MapData>();

        // Begin map generation; fill MapData and get player reference
        yield return StartCoroutine(mapGenerator.GenerateMap(mapData));

        // Map Data fetched, pass to all dependent scripts
        levelPreviewCutscene.map = mapData;
        enemyManager.map = mapData;

        // Player Data fetched, pass to all dependent scripts
        player = mapGenerator.player;
        playerCam = player.GetComponentInChildren<Camera>(true);
        cutsceneController.playerCam = playerCam;
        enemyManager.player = player;
        enemyManager.playerCam = playerCam;

        // begin playing music
        GameManager.Instance.PlaySceneMusic();
        AudioManager.Instance.ChangeMusicVolume(-25f);

        // Call Cutscene Controller to begin the level
        yield return StartCoroutine(cutsceneController.PlayCutscene(false));

        // Level started; call Enemy Manager to begin spawning
        StartCoroutine(enemyManager.BeginSpawning());

        
    }
}
