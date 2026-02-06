using UnityEngine;
using System.Collections.Generic;

public class GenerateMap : MonoBehaviour
{
    [Header("Map Seed (set -1 for random)")]
    public int seed = -1;

    [Header("Room Complexity Constraints")]
    public int maxRoomComplexity;
    public int minRoomComplexity;

    [Header("Max Path Complexity Allowed")]
    public int maxPathComplexity;

    [Header("Room Prefabs (index 0 = Start, 1 = End)")]
    public List<Room> rooms;

    private int totalComplexity = 0; // count successfully placed rooms
    private List<Bounds> placedBounds = new List<Bounds>(); // used for collision checks
    private List<Room> placedRooms = new List<Room>(); // parallel list so we can name which room overlaps

    private bool endRoomPlaced = false;
    private GameObject generationRoot;
    private const int maxGenerationRetries = 20;

    private Dictionary<Room, int> spawnCounts = new Dictionary<Room, int>();

    void Start()
    {
        if (seed >= 0)
            Random.InitState(seed);
        else
            seed = Random.Range(0, int.MaxValue);

        Debug.Log($"Using seed: {seed}");

        int attempts = 0;
        while (attempts < maxGenerationRetries) {
            attempts++;
            bool success = GenerationInit();
            if (success) {
                Debug.Log($"Map generation succeeded after {attempts} attempt(s).");
                return;
            }
            else {
                Debug.LogWarning($"Generation attempt {attempts} FAILED — restarting.");
            }
        }
        Debug.LogError("Map generation failed after maximum retries.");
    }

    private bool GenerationInit()
    {
        // Reset state
        endRoomPlaced = false;
        totalComplexity = 0;
        placedBounds.Clear();
        placedRooms.Clear();
        spawnCounts.Clear();

        // Clear previous geometry
        if (generationRoot != null) {
            DestroyImmediate(generationRoot);
        }

        generationRoot = new GameObject("GeneratedRooms");

        // Place start room (assume index 0)
        Room startRoom = Instantiate(rooms[0], transform.position, Quaternion.identity, generationRoot.transform);

        Bounds startBounds = GetRoomBounds(startRoom);
        placedBounds.Add(startBounds);
        placedRooms.Add(startRoom);
        totalComplexity += startRoom.complexity;

        Debug.Log($"[GEN] Placed start room '{startRoom.roomName}' at {startRoom.transform.position}. Bounds center {startBounds.center}, size {startBounds.size}");

        // Expand outward from start room exits
        foreach (var exit in startRoom.GetAvailableExits())
        {
            Generate(exit, 0);
            if (totalComplexity >= maxRoomComplexity) break;
        }

        return endRoomPlaced && totalComplexity >= minRoomComplexity;
    }

    private void Generate(Room.Exit fromExit, int pathComplexity)
    {
        if (totalComplexity >= maxRoomComplexity) return;
        if (pathComplexity >= maxPathComplexity) return;
        if (fromExit.isConnected) return;

        bool mustPlaceEndRoom =
            !endRoomPlaced &&
            (pathComplexity + 1 >= maxPathComplexity || totalComplexity + 1 >= maxRoomComplexity);

        // Try end room first if required
        if (mustPlaceEndRoom)
        {
            Room endRoomPrefab = rooms[1];
            if (TryPlaceRoom(fromExit, endRoomPrefab, pathComplexity))
            {
                endRoomPlaced = true;
                return;
            }
        }

        // Otherwise, get all candidate prefabs for this exit
        List<Room> candidates = GetCandidatePrefabs(fromExit, mustPlaceEndRoom);
        Shuffle(candidates);

        int count = 0;
        foreach (var prefab in candidates)
        {
            count++;
            if (TryPlaceRoom(fromExit, prefab, pathComplexity)) {
                Debug.Log($"Chose '{count}'th choice prefab");
                return; // stop after first success
            }
        }

        // All prefabs failed for this exit
        Debug.Log($"[GEN] Exhausted all candidate prefabs for exit '{fromExit.transform.name}' at depth {pathComplexity}");
    }

    // Fisher-Yates shuffle for List<T>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    private List<Room> GetCandidatePrefabs(Room.Exit fromExit, bool mustPlaceEndRoom)
    {
        List<Room> preferred = new List<Room>();
        List<Room> normal = new List<Room>();

        Room fromRoom = fromExit.transform.GetComponentInParent<Room>();

        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == 0) continue;                // never place start room again
            if (i == 1 && !mustPlaceEndRoom) continue; // end room only when needed

            Room r = rooms[i];
            
            // Check spawn limits on special rooms
            if (r.maxSpawnCount >= 0 && GetSpawnCount(r) >= r.maxSpawnCount) {
                Debug.Log($"spawn limit for {r.roomName} reached");
                continue;
            }
            // Must have compatible exits
            if (r.GetAvailableExits(fromExit.doorType).Count == 0)
                continue;

            // Check if this room is listed as preferred
            if (fromRoom.preferredNeighbors.Contains(r))
                preferred.Add(r);
            else
                normal.Add(r);
        }

        // Shuffle both groups independently
        Shuffle(preferred);
        Shuffle(normal);

        // Combine preferred first, normal after
        List<Room> result = new List<Room>(preferred.Count + normal.Count);
        result.AddRange(preferred);
        result.AddRange(normal);

        return result;
    }



    private bool TryPlaceRoom(Room.Exit fromExit, Room prefab, int pathComplexity)
    {
        // Shuffle available exits of the prefab matching the current door type
        List<Room.Exit> exitsToTry = prefab.GetAvailableExits(fromExit.doorType);
        Shuffle(exitsToTry);

        foreach (var prefabExit in exitsToTry)
        {
            // Instantiate prefab
            Room candidate = Instantiate(prefab, Vector3.zero, Quaternion.identity, generationRoot.transform);

            // Find matching exit on instance
            int exitIndex = prefab.exits.IndexOf(prefabExit);
            Room.Exit instantiatedExit = candidate.exits[exitIndex];
            instantiatedExit.isConnected = false;

            // Align to current exit
            AlignRoomToExit(candidate, instantiatedExit, fromExit);

            // Check overlap
            Bounds candidateBounds = GetRoomBounds(candidate);
            if (RoomOverlaps(candidateBounds))
            {
                Destroy(candidate.gameObject);
                continue; // try next exit
            }

            // SUCCESS
            Debug.Log($"[GEN] Placed room '{candidate.roomName}' at depth {pathComplexity}, total complexity {totalComplexity}");
            // update room bounds
            placedRooms.Add(candidate);
            placedBounds.Add(candidateBounds);
            // connect exits
            fromExit.isConnected = true;
            instantiatedExit.isConnected = true;
            // remove placeholder doors
            Destroy(fromExit.transform.gameObject);
            Destroy(instantiatedExit.transform.gameObject);
            // increase totalComplexity
            totalComplexity += candidate.complexity;
            IncrementSpawnCount(prefab);

            // Recurse from one random available exit
            List<Room.Exit> nextExits = candidate.GetAvailableExits();
            Shuffle(nextExits);
            foreach (var nextExit in nextExits)
            {
                Generate(nextExit, pathComplexity + candidate.complexity);
            }

            return true;
        }

        // All exits failed
        return false;
    }

    // Width of wall to shrink bounds by
    private const float WALL_THICKNESS = 0.3f;
    // Compute world-space bounds for a room by combining all child renderers
    private Bounds GetRoomBounds(Room room)
    {
        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
            return new Bounds(room.transform.position, Vector3.one * 0.1f);

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        Vector3 size = combined.size;
        size.x = Mathf.Max(0.01f, size.x - WALL_THICKNESS * 2f);
        size.y = Mathf.Max(0.01f, size.y - WALL_THICKNESS * 2f);
        size.z = Mathf.Max(0.01f, size.z - WALL_THICKNESS * 2f);

        return new Bounds(combined.center, size);
    }

    // Check candidate bounds against already committed placed rooms
    private bool RoomOverlaps(Bounds newBounds)
    {
        for (int i = 0; i < placedBounds.Count; i++)
        {
            if (placedBounds[i].Intersects(newBounds))
                return true;
        }
        return false;
    }

    private void AlignRoomToExit(Room newRoom, Room.Exit newExit, Room.Exit fromExit)
    {
        Transform rt = newRoom.transform;

        // Step 1: compute the target forward (pointing out of the new room)
        Vector3 targetForward = -fromExit.transform.forward;

        // Step 2: compute the target up (keep the room upright)
        Vector3 targetUp = fromExit.transform.up;

        // Step 3: build a rotation that matches BOTH directions
        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);

        // Step 4: rotate the ROOM so that newExit.forward and newExit.up align with target
        Quaternion delta = targetRotation * Quaternion.Inverse(newExit.transform.rotation);

        rt.rotation = delta * rt.rotation;

        // Step 5: move so exit positions overlap exactly
        rt.position += (fromExit.transform.position - newExit.transform.position);
    }

    //Function returning all room bounds and all rooms for enemy generation reference
    public List<Bounds> GetAllRoomBounds()
    {
        return placedBounds;
    }

    public List<Room> GetAllPlacedRooms()
    {
        return placedRooms;
    }

    private int GetSpawnCount(Room prefab)
    {
        if (!spawnCounts.TryGetValue(prefab, out int count))
            return 0;
        return count;
    }

    private void IncrementSpawnCount(Room prefab)
    {
        if (!spawnCounts.ContainsKey(prefab))
            spawnCounts[prefab] = 0;

        spawnCounts[prefab]++;
    }
}
