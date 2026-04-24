using UnityEngine;
using System.Collections.Generic;

public class FairyTreeScript : Enemy
{
    [Header("Fairy Tree Spawning")]
    [SerializeField] private GameObject fairyPrefab;
    [SerializeField] private int maxPathDistanceForTree = 5;
    public override bool IsGrounded => true;
    private GameObject spawnedFairy;

    /// <summary>
    /// Additional spawn location filtering: only accept tree spawns within pathable rooms from player
    /// </summary>
    public override bool CheckSpecialSpawnRules(Vector3 playerPos, Vector3 playerForward, Vector3 candidatePos)
    {
        MapGenerationManager mapGen = MapGenerationManager.Instance;

        // Get player's current room
        Room playerRoom = mapGen.GetRoomAtPosition(playerPos);

        // Get candidate room
        Room candidateRoom = mapGen.GetRoomAtPosition(candidatePos);

        // Get all rooms within pathable distance from player
        List<Room> accessibleRooms = mapGen.GetRoomsWithinPathDistance(playerRoom, maxPathDistanceForTree);
        
        // Accept if candidate room is in the accessible rooms list
        return accessibleRooms.Contains(candidateRoom);
    }

    void Start()
    {
        // Spawn fairy in front of player
        if (EnemyManager.Instance != null && EnemyManager.Instance.playerCam != null)
        {
            Camera playerCam = EnemyManager.Instance.playerCam;
            float fairyDistance = 2f;
            Vector3 fairySpawnPos = playerCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, fairyDistance));
            
            spawnedFairy = Instantiate(fairyPrefab, fairySpawnPos, Quaternion.identity);
            
            // Pass tree reference to fairy
            FairyScript fairy = spawnedFairy.GetComponent<FairyScript>();
            if (fairy != null)
            {
                fairy.SetTreeReference(this.gameObject);
            }
            
            Debug.Log($"[FAIRY TREE] Spawned fairy at {fairySpawnPos}");
        }
        else
        {
            Debug.LogWarning("[FAIRY TREE] Could not spawn fairy - EnemyManager or camera not found");
        }
    }
}
