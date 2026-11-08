using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds init information for map generation
/// Does not contain any information about instantiated GameObjects
/// </summary>

[CreateAssetMenu(fileName = "MapSettings", menuName = "Scriptable Objects/MapSettings")]
public class MapSettings : ScriptableObject
{
    [SerializeField] public List<Room> roomCatalogue;
}
