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

        // Build weighted list of prefab indices to try (skip index 0 - start room)
        List<int> prefabIndices = BuildDepthBiasedPrefabList(pathLength);

        // If this placement would be at terminal depth, force end room first
        bool mustPlaceEndRoom = (pathLength + 1 == maxPath);
        if (mustPlaceEndRoom)
        {
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

    /// <summary>
    /// Returns a randomized, depth-biased ordering of room prefabs.
    /// Prefers high-exit-count rooms at low depth, and low-exit-count rooms at high depth.
    /// </summary>
    private List<int> BuildDepthBiasedPrefabList(int depth)
    {
        List<int> result = new List<int>();
        List<(int prefabIndex, float weight)> weighted = new();

        float t = Mathf.Clamp01((float)depth / (float)maxPath);
        // t = 0   → very early depth → want high exit count
        // t = 1   → at/near end     → want low exit count

        for (int i = 1; i < rooms.Count; i++) // skip start room
        {
            Room r = rooms[i];
            int exitCount = Mathf.Max(1, r.exits.Count);

            // Normalize exitCount across all rooms for fairness
            // (optional but helps equalize variations in prefab design)
            float minExits = 1f;
            float maxExits = 1f;
            foreach (var rm in rooms)
                maxExits = Mathf.Max(maxExits, rm.exits.Count);

            float norm = (exitCount - minExits) / (maxExits - minExits + 0.001f);

            // ★ Depth-biased weighting ★
            // At low depth (t=0): weight = norm      → prefer high-exit rooms
            // At high depth (t=1): weight = 1-norm   → prefer low-exit rooms
            float weight = Mathf.Lerp(norm, 1f - norm, t);

            // Add a small constant to avoid zero weights
            weight = Mathf.Max(0.01f, weight);

            weighted.Add((i, weight));
        }

        // Weighted random ordering (roulette-wheel style)
        while (weighted.Count > 0)
        {
            float total = 0f;
            foreach (var w in weighted) total += w.weight;

            float pick = Random.value * total;
            float acc = 0f;

            for (int i = 0; i < weighted.Count; i++)
            {
                acc += weighted[i].weight;
                if (acc >= pick)
                {
                    result.Add(weighted[i].prefabIndex);
                    weighted.RemoveAt(i);
                    break;
                }
            }
        }

        return result;
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
