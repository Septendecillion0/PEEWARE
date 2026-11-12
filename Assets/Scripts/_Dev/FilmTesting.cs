using UnityEngine;
using System.Collections;

public class FilmTesting : MonoBehaviour
{   
    [SerializeField] MapGenerator mapGenerator;

    [HideInInspector] MapData mapData;
    [HideInInspector] GameObject player;
    [HideInInspector] Camera playerCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {      
        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(InitializeLevel());
    }

    IEnumerator InitializeLevel()
    {
        mapData = ScriptableObject.CreateInstance<MapData>();

        // Begin map generation; fill MapData and get player reference
        yield return StartCoroutine(mapGenerator.GenerateMap(mapData));

        // Player Data fetched, pass to all dependent scripts
        player = mapGenerator.player;
        playerCam = player.GetComponentInChildren<Camera>(true);
        player.SetActive(false);

    }
}
