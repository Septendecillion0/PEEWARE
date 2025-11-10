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

    [Header("Room Prefabs (index 0 = Start, last = End)")]
    public List<Room> rooms;

    private int currentRoomNum = 0; // count successfully placed rooms
    private List<Bounds> placedBounds = new List<Bounds>(); // used for collision checks

    void Start()
    {
        if (seed >= 0)
            Random.InitState(seed);
        else
            seed = Random.Range(0, int.MaxValue);

        Debug.Log($"Using seed: {seed}");
        Generate();
    }

    private void Generate()
    {
        currentRoomNum = 0;
        placedBounds.Clear();

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("No room prefabs assigned to GenerateMap.rooms");
            return;
        }

        // Place start room
        Room startRoom = Instantiate(rooms[0], transform.position, Quaternion.identity);
        placedBounds.Add(GetRoomBounds(startRoom));
        currentRoomNum++;

        // Expand outward from start room exits
        foreach (var exit in startRoom.GetAvailableExits())
        {
            TryPlaceRoomAtExitWithRetries(exit, 0);
            if (currentRoomNum >= maxRoom) break;
        }
    }

    /// <summary>
    /// Tries to place a room at `fromExit`. This method exhaustively tries:
    ///   - each candidate prefab (random order),
    ///   - for each prefab, each available exit on that prefab (random order),
    /// until one placement succeeds or all combinations are tried.
    /// </summary>
    private void TryPlaceRoomAtExitWithRetries(Room.Exit fromExit, int pathLength)
    {
        if (currentRoomNum >= maxRoom) return;
        if (pathLength >= maxPath) return;

        // Build a randomized list of prefab indices to try (skip index 0 - start)
        List<int> prefabIndices = new List<int>();
        for (int i = 1; i < rooms.Count; i++) prefabIndices.Add(i);
        Shuffle(prefabIndices);

        // If this placement would be at terminal depth, prefer the final prefab (end room).
        bool mustPlaceEndRoom = (pathLength + 1 == maxPath);
        if (mustPlaceEndRoom)
        {
            // only try the last prefab (end) first, then others if you want fallback (optional).
            prefabIndices.Remove(rooms.Count - 1);
            prefabIndices.Insert(0, rooms.Count - 1);
        }

        foreach (int prefabIndex in prefabIndices)
        {
            if (currentRoomNum >= maxRoom) break;

            Room prefabToTry = rooms[prefabIndex];

            // Get a randomized list of available exits on this prefab
            List<Room.Exit> availableExits = new List<Room.Exit>(prefabToTry.GetAvailableExits());
            if (availableExits.Count == 0)
            {
                // No available exits on that prefab (weird), skip it
                continue;
            }
            Shuffle(availableExits);

            // Try each exit orientation for this prefab
            foreach (var exitOnPrefab in availableExits)
            {
                if (currentRoomNum >= maxRoom) break;
                // Instantiate candidate at origin (we'll align it)
                Room candidate = Instantiate(prefabToTry, Vector3.zero, Quaternion.identity);

                // IMPORTANT: find the corresponding Exit object on the instantiated candidate.
                // exitOnPrefab is from the prefab asset; we must find the matching index in the prefab's exits list
                int exitIndex = prefabToTry.exits.IndexOf(exitOnPrefab);
                Room.Exit instantiatedExit;
                if (exitIndex >= 0 && exitIndex < candidate.exits.Count)
                    instantiatedExit = candidate.exits[exitIndex];
                else
                {
                    // fallback: pick a random available exit on instantiated object
                    instantiatedExit = candidate.GetRandomAvailableExit();
                    if (instantiatedExit == null)
                    {
                        Destroy(candidate.gameObject);
                        continue;
                    }
                }

                // Align candidate so it connects to fromExit
                AlignRoomToExit(candidate, instantiatedExit, fromExit);

                // Compute bounds and check overlap
                Bounds candidateBounds = GetRoomBounds(candidate);

                bool overlaps = RoomOverlaps(candidateBounds);
                if (overlaps)
                {
                    Debug.Log($"[GEN] Attempt FAILED: '{candidate.roomName}' (prefab idx {prefabIndex}) at exit '{fromExit.transform.name}' — overlaps existing room.");
                    Destroy(candidate.gameObject);
                    // try next orientation (exit) or next prefab
                    continue;
                }

                // Success: commit candidate
                placedBounds.Add(candidateBounds);
                currentRoomNum++;
                fromExit.isConnected = true;
                instantiatedExit.isConnected = true;

                Debug.Log($"[GEN] Placed '{candidate.roomName}' (prefab idx {prefabIndex}) at exit '{fromExit.transform.name}'. Total placed: {currentRoomNum}");

                // Recurse from all available exits on the newly placed room
                foreach (var nextExit in candidate.GetAvailableExits())
                {
                    if (currentRoomNum >= maxRoom) break;
                    if (pathLength + 1 >= maxPath) break;
                    TryPlaceRoomAtExitWithRetries(nextExit, pathLength + 1);
                }

                // We succeeded placing one candidate; stop trying other prefabs for this fromExit
                return;
            } // end foreach exitOnPrefab
        } // end foreach prefabIndex

        // If we get here, we've exhausted all prefabs and orientations without success.
        Debug.Log($"[GEN] Exhausted all prefab×orientation attempts for exit '{fromExit.transform.name}' at depth {pathLength} — giving up on this branch.");
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

    // Compute world-space bounds for a room by combining all child renderers
    private Bounds GetRoomBounds(Room room)
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
    private bool RoomOverlaps(Bounds newBounds)
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
        rt.rotation = Quaternion.FromToRotation(newExit.transform.forward, -fromExit.transform.forward) * rt.rotation;

        // Move so exit positions overlap
        rt.position += (fromExit.transform.position - newExit.transform.position);
    }
}
