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
    public List<Bounds> roomBounds;

    // Find every room matching the requested type
    public List<Room> FindRoomsOfType(Room.RoomType roomType)
    {
        List<Room> foundRooms = new List<Room>();

        if (rooms == null)
            return foundRooms;

        foreach (Room room in rooms)
        {
            if (room != null && room.roomType == roomType)
                foundRooms.Add(room);
        }

        return foundRooms;
    }

    // Find a path between two rooms
    public List<Room> FindPath(Room start, Room target)
    {
        HashSet<Room> visited = new HashSet<Room>();
        List<Room> path = new List<Room>();

        if (Search(start, target, visited, path))
            return path;

        return null;
    }

    // Depth-first search for a specific target room
    private bool Search(
        Room current,
        Room target,
        HashSet<Room> visited,
        List<Room> path)
    {
        if (current == null || !visited.Add(current))
            return false;

        path.Add(current);

        // Destination reached
        if (current == target)
            return true;

        // Explore connected rooms
        foreach (Room neighbor in current.GetConnectedRooms())
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

        if (rooms == null)
            rooms = new List<Room>();
        else
            rooms.Clear();
    }
}
