using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Spawns bottles in rooms after map generation based on each room's bottleSpawnChance.
/// Iterates through all rooms and their child Bottle objects, activating each after a successful roll
/// </summary>
/// <remarks>
/// stores integer data for the total number of bottles and how many were actually spawned
/// </remarks>
public class BottleSpawner : Singleton<BottleSpawner>
{
    /// <summary>
    /// Spawns bottles in all rooms based on each room's bottleSpawnChance.
    /// Call after map generation completes.
    /// </summary>
    public void SpawnBottles(List<Room> placedRooms)
    {
        int totalBottles = 0;
        int spawnedBottles = 0;

        foreach (var room in placedRooms)
        {
            Bottle[] bottlesInRoom = room.GetComponentsInChildren<Bottle>(includeInactive: true);

            foreach (var bottle in bottlesInRoom)
            {
                totalBottles++;

                // Roll chance for this bottle
                if (Random.value < room.bottleSpawnChance)
                {
                    bottle.gameObject.SetActive(true);
                    spawnedBottles++;
                }
                else
                {
                    bottle.gameObject.SetActive(false);
                }
            }
        }

        Debug.Log($"[BottleSpawner] Spawned {spawnedBottles}/{totalBottles} bottles");
    }
}
