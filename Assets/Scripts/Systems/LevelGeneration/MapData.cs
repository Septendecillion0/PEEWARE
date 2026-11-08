using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Holds information about the map
/// Contains functions to search for rooms or find paths from one room to another
/// </summary>

[CreateAssetMenu(fileName = "MapData", menuName = "Scriptable Objects/MapData")]
public class MapData : ScriptableObject
{
    public Room startRoom;
    public Room endRoom;
    public List<Room> rooms;

    public List<Room> FindRoomsOfType(Room.RoomType roomType)
    {
        List<Room> foundRooms = new List<Room>();

        HashSet<Room> visited = new HashSet<Room>();
        List<Room> path = new List<Room>();

        while (Search(startRoom, null, visited, path, roomType))
        {
            Debug.Assert(path[path.Count - 1].roomType == roomType);
            foundRooms.Add(path[path.Count - 1]);
        }

        return foundRooms;
    }

    public List<Room> FindPath(Room start, Room target)
    {
        HashSet<Room> visited = new HashSet<Room>();
        List<Room> path = new List<Room>();

        if (Search(start, target, visited, path))
        {
            return path;
        }

        return null;
    }

    // Searches for a given room or roomType
    // returns true if found, false if not
    // dynamically modifies path to contain the path to the target room
    private bool Search(
        Room current,
        Room target,
        HashSet<Room> visited,
        List<Room> path,
        Room.RoomType roomType = Room.RoomType.None)
    {
        // Prevent revisiting rooms
        if (!visited.Add(current))
            return false;

        // Add current room to path
        path.Add(current);

        // Destination reached
        if (current == target)
            return true;
        if (current.roomType == roomType)
            return true;

        // Explore connected rooms
        List<Room> neighbors = current.GetConnectedRooms();
        foreach (Room neighbor in neighbors)
        {
            if (Search(neighbor, target, visited, path))
                return true;
        }

        // Dead end: backtrack
        path.RemoveAt(path.Count - 1);

        return false;
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        ClearMap();
    }

    public void ClearMap()
    {
        startRoom = null;
        endRoom = null;
        rooms.Clear();
    }
}
