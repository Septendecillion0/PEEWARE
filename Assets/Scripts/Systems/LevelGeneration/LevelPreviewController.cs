using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Controls the camera zoom preview that executes at the start of each level
/// Receives map from MapGenerationManager
/// </summary>
public class LevelPreview : MonoBehaviour
{
    [SerializeField] private MapData map;
    private List<Room> levelPath = new List<Room>();

    void Start()
    {
        GetLevelPath();
        RunCameraSequence();
    }

    void GetLevelPath()
    {
        levelPath = map.FindPath(map.endRoom, map.startRoom);
    }

    void RunCameraSequence()
    {

    }


    
}
