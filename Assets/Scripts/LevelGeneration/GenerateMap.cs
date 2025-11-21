using UnityEngine;
using System.Collections.Generic;

public class GenerateMap : MonoBehaviour
{
    [Header("Map Seed (set -1 for random)")]
    public int seed = -1;

    [Header("Total Rooms Allowed")]
    public int maxRoom = 20;

    [Header("Max Path Depth Allowed")]
    public int maxPath = 8;

    [Header("Room Prefabs (index 0 = Start, 1 = End)")]
    public List<Room> rooms;

    private int currentRoomNum = 0; // count successfully placed rooms
    private List<Bounds> placedBounds = new List<Bounds>(); // used for collision checks
    private List<Room> placedRooms = new List<Room>(); // parallel list so we can name which room overlaps

    private bool endRoomPlaced = false;
    private GameObject generationRoot;
    private const int maxGenerationRetries = 20;

    void Start()
    {
        if (seed >= 0)
            Random.InitState(seed);
        else
            seed = Random.Range(0, int.MaxValue);

        Debug.Log($"Using seed: {seed}");

        int attempts = 0;
        while (attempts < maxGenerationRetries)
        {
            attempts++;

            bool success = Generate();

            if (success)
            {
                Debug.Log($"Map generation succeeded after {attempts} attempt(s).");
                return;
            }
            else
            {
                Debug.LogWarning($"Generation attempt {attempts} FAILED — restarting.");
            }
        }

        Debug.LogError("Map generation failed after maximum retries.");
    }

    private bool Generate()
    {
        // Reset state
        endRoomPlaced = false;
        currentRoomNum = 0;
        placedBounds.Clear();
        placedRooms.Clear();

        // Clear previous geometry
        if (generationRoot != null)
        {
            DestroyImmediate(generationRoot);
        }

        generationRoot = new GameObject("GeneratedRooms");

        if (rooms == null || rooms.Count < 2)
        {
            Debug.LogError("You must assign at least StartRoom (0) and EndRoom (1).");
            return false;
        }

        // Place start room (parent under generationRoot)
        Room startRoom = Instantiate(rooms[0], transform.position, Quaternion.identity, generationRoot.transform);
        // ensure its exits show as unconnected
        foreach (var e in startRoom.exits) e.isConnected = false;

        Bounds startBounds = GetRoomBounds(startRoom);
        placedBounds.Add(startBounds);
        placedRooms.Add(startRoom);
        currentRoomNum++;

        Debug.Log($"[GEN] Placed start room '{startRoom.roomName}' at {startRoom.transform.position}. Bounds center {startBounds.center}, size {startBounds.size}");

        // Expand outward from start room exits
        foreach (var exit in startRoom.GetAvailableExits())
        {
            TryPlaceRoomAtExitWithRetries(exit, 0);
            if (currentRoomNum >= maxRoom) break;
        }

        return endRoomPlaced;
    }

    private void TryPlaceRoomAtExitWithRetries(Room.Exit fromExit, int pathLength)
    {
        if (currentRoomNum >= maxRoom) return;
        if (pathLength >= maxPath) return;
        if (fromExit.isConnected) return;

        bool mustPlaceEndRoom =
            !endRoomPlaced &&
            (pathLength + 1 >= maxPath || currentRoomNum + 1 >= maxRoom);

        // Try end room first if required
        if (mustPlaceEndRoom)
        {
            Room endRoomPrefab = rooms[1];
            if (TryPlaceRoomFromPrefab(fromExit, endRoomPrefab, pathLength))
            {
                endRoomPlaced = true;
                return;
            }
        }

        // Otherwise, get all candidate prefabs for this exit
        List<Room> candidates = GetCandidatePrefabs(fromExit, mustPlaceEndRoom);
        Shuffle(candidates);

        foreach (var prefab in candidates)
        {
            if (TryPlaceRoomFromPrefab(fromExit, prefab, pathLength))
                return; // stop after first success
        }

        // All prefabs failed for this exit
        Debug.Log($"[GEN] Exhausted all candidate prefabs for exit '{fromExit.transform.name}' at depth {pathLength}");
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
        List<Room> candidates = new List<Room>();

        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == 0) continue; // never place start room again
            if (i == 1 && !mustPlaceEndRoom) continue; // end room only when needed

            Room r = rooms[i];
            // Only include if it has at least one exit matching the current exit's door type
            if (r.GetAvailableExits(fromExit.doorType).Count > 0)
                candidates.Add(r);
        }

        // Weighted shuffle: generate a random key biased by weight
        candidates.Sort((a, b) =>
        {
            // Generate a random number in [0,1) and scale inversely by weight
            float keyA = Mathf.Pow(Random.value, 1f / a.weight);
            float keyB = Mathf.Pow(Random.value, 1f / b.weight);
            return keyA.CompareTo(keyB); // smaller key → earlier in list
        });

        return candidates;
    }


    private bool TryPlaceRoomFromPrefab(Room.Exit fromExit, Room prefab, int pathLength)
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
            Debug.Log($"[GEN] Placed room '{candidate.roomName}' at {candidate.transform.position}.");
            placedRooms.Add(candidate);
            placedBounds.Add(candidateBounds);
            // don't add to count if placed room is a door
            if (!candidate.isDoor) currentRoomNum++;
            fromExit.isConnected = true;
            instantiatedExit.isConnected = true;

            // Recurse from one random available exit
            List<Room.Exit> nextExits = candidate.GetAvailableExits();
            Shuffle(nextExits);
            foreach (var nextExit in nextExits)
            {
                TryPlaceRoomAtExitWithRetries(nextExit, pathLength + 1);
            }

            return true;
        }

        // All exits failed
        return false;
    }


    // Width of wall to shrink bounds by
    private const float WALL_THICKNESS = 0.15f;
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
}
