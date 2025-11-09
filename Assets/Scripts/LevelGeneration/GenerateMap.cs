using UnityEngine;
using System.Collections.Generic;

public class GenerateMap : MonoBehaviour
{
    [Header("Map Seed (set -1 for random)")]
    public int seed = -1;
    [Header("Total Rows")]
    public int maxRow;
    [Header("Total Cols")]
    public int maxCol;
    [Header("Total Rooms")]
    public int maxRoom;
    [Header("Max Path Length")]
    public int maxPath;
    [Header("Room Max Width")]
    public int roomMaxW;
    [Header("Room Max Height")]
    public int roomMaxH;
    [Header("Room Min Width")]
    public int roomMinW;
    [Header("Room Min Height")]
    public int roomMinH;
    [Header("Distance Between Rooms")]
    public int roomDis;
    [Header("Floor Prefab")]
    public GameObject floorPrefab;

    [Header("List of existing rooms")]
    public List<Room> rooms;

    [Header("Horizontal Road Prefab")]
    public GameObject roadHoriPrefab;
    [Header("Vertical Road Prefab")]
    public GameObject roadVertiPrefab;

    // Current room numbers (counts only successfully placed rooms)
    private int currentRoomNum = 0;

    // List of room bounds to avoid overlap
    private List<Bounds> placedBounds = new List<Bounds>();

    void Start()
    {
        if (seed >= 0)
            Random.InitState(seed);
        else
            seed = Random.Range(0, int.MaxValue);  // generate a new seed
        
        Debug.Log($"Using seed: {seed}");
        DrawMap();
    }

    // Main Map Generation
    private void DrawMap()
    {
        currentRoomNum = 0;
        placedBounds.Clear();

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("No room prefabs assigned to GenerateMap.rooms");
            return;
        }

        // Instantiate the starting room and commit it immediately
        Room startRoom = Instantiate(rooms[0], transform.position, Quaternion.identity);
        Bounds startBounds = GetRoomBounds(startRoom);
        placedBounds.Add(startBounds);
        currentRoomNum++;

        // Expand from each available exit on the start room
        foreach (var exit in startRoom.GetAvailableExits())
        {
            TryPlaceRoomAtExit(exit, 0);
            if (currentRoomNum >= maxRoom) break;
        }
    }

    // Try to place a room attached at `fromExit`. If placement succeeds we recurse from that room's exits.
    private void TryPlaceRoomAtExit(Room.Exit fromExit, int pathLength)
    {
        if (currentRoomNum >= maxRoom) return;
        if (pathLength >= maxPath) return;

        Room toDraw;
        if (pathLength + 1 == maxPath)
            toDraw = rooms[rooms.Count - 1];
        else
            toDraw = rooms[Random.Range(1, Mathf.Max(2, rooms.Count - 1))];

        Room newRoom = Instantiate(toDraw, Vector3.zero, Quaternion.identity);

        Room.Exit newExit = newRoom.GetRandomAvailableExit();
        if (newExit == null)
        {
            Destroy(newRoom.gameObject);
            return;
        }

        // Correct variable usage here
        AlignRoomToExit(newRoom, newExit, fromExit);

        Bounds newBounds = GetRoomBounds(newRoom);
        if (RoomOverlaps(newBounds))
        {
            Debug.Log($"[COLLISION] Prevented placing room '{toDraw.roomName}' when connecting from exit '{fromExit.tag}'");
            Destroy(newRoom.gameObject);
            return; // not continue
        }

        // Placement succeeded
        placedBounds.Add(newBounds);
        currentRoomNum++;

        fromExit.isConnected = true;
        newExit.isConnected = true;

        // Recurse from THIS room
        foreach (var nextExit in newRoom.GetAvailableExits())
        {
            if (currentRoomNum >= maxRoom || pathLength + 1 >= maxPath)
                break;

            TryPlaceRoomAtExit(nextExit, pathLength + 1);
        }
    }


    // Returns the specific Room that overlaps, instead of just true/false
    private Room GetOverlappingRoom(Bounds candidateBounds)
    {
        foreach (var existingRoomBounds in placedBounds)
        {
            if (existingRoomBounds.Intersects(candidateBounds))
            {
                // We need to return the actual Room object that owns this Bounds.
                // The easiest way is to do a Physics.OverlapBox check to locate it.
                Collider[] hits = Physics.OverlapBox(existingRoomBounds.center, existingRoomBounds.extents * 0.5f);
                foreach (var hit in hits)
                {
                    Room r = hit.GetComponentInParent<Room>();
                    if (r != null)
                        return r;
                }
                return null; // fallback if no collider is found, but bounds still intersect
            }
        }
        return null;
    }


    // Compute world-space bounds for a room by combining all child renderers
    Bounds GetRoomBounds(Room room)
    {
        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
            return new Bounds(room.transform.position, Vector3.zero);

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);
        return combined;
    }

    // Check candidate bounds against already committed placed rooms
    bool RoomOverlaps(Bounds newBounds)
    {
        foreach (var b in placedBounds)
        {
            if (b.Intersects(newBounds))
                return true;
        }
        return false;
    }



    private void AlignRoomToExit(Room newRoom, Room.Exit newExit, Room.Exit fromExit)
    {
        Transform rt = newRoom.transform;

        // Rotate so newExit.forward points opposite fromExit.forward
        Quaternion rotation = Quaternion.FromToRotation(newExit.transform.forward, -fromExit.transform.forward);
        rt.rotation = rotation * rt.rotation;

        // Move so exit positions overlap
        Vector3 offset = fromExit.transform.position - newExit.transform.position;
        rt.position += offset;
    }


    // Optional: draw placed room bounds in the Scene view for debugging
    // Uncomment to visualize
    /*
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (placedBounds != null)
        {
            foreach (var b in placedBounds)
            {
                Gizmos.DrawWireCube(b.center, b.size);
            }
        }
    }
    */
}
