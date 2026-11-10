using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Represents a room in the level generation system. 
/// Contains data about the room's exits, size, and other properties.
/// Exits must be manually assigned in the inspector, but parent room references will be automatically assigned
/// </summary>
public class Room : MonoBehaviour
{
    [System.Serializable]
    public class Exit
    {
        public Transform transform;   // where to attach next room
        public DoorType doorType = DoorType.Single;
        public Exit connectedExit;
        public Room room; // the room this exit belongs to
    }

    public enum DoorType
    {
        Single,
        Double,
        Vent,
        Hallway
    }

    public enum RoomType
    {
        None,
        Start,
        End,
        Generic
    }

    [Header("Room Settings")]
    public string roomName;
    public int complexity = 1; // default complexity = 1
    public RoomType roomType;
    public Vector3 size;                // optional if you want to use bounding boxes
    [Header("Room Exits")]
    public List<Exit> exits = new List<Exit>();
    public int exitCount => exits.Count;
    public int maxSpawnCount = -1;

    [Header ("Camera Preview")]
    [SerializeField] public Transform waypoint;

    public List<Room> preferredNeighbors = new List<Room>(); 

    [Header("Prop Spawning")]
    [Range(0f, 1f)]
    public float bottleSpawnChance = 0.5f; // 0-1, probability that each bottle in room spawns

    // Automatically assign parent room reference to exits
    private void Awake()
    {
        foreach (var e in exits)
        {
            e.room = this;
        }
    }

    public List<Exit> GetAvailableExits(DoorType? type = null)
    {
        List<Exit> list = new List<Exit>();
        foreach (var e in exits)
        {
            if (e.connectedExit != null) continue;
            if (type != null && e.doorType != type.Value) continue;
            list.Add(e);
        }
        return list;
    }


    public Exit GetRandomAvailableExit()
    {
        List<Exit> available = new List<Exit>();
        foreach (var e in exits)
            if (e.connectedExit == null)
                available.Add(e);

        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }

    public List<Room> GetConnectedRooms()
    {   
        List<Room> rooms = new List<Room>();
        foreach (var e in exits) {
            if (e.connectedExit != null)
                rooms.Add(e.connectedExit.room);
        }
        return rooms;
    }

    public Exit GetExitTo(Room targetRoom)
    {
        foreach (var exit in exits)
        {
            if (exit.connectedExit != null &&
                exit.connectedExit.room == targetRoom)
            {
                return exit;
            }
        }

        return null;
    }

}
