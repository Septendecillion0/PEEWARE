using UnityEngine;
using System.Collections.Generic;

//written with assistance by ChatGPT

public class Room : MonoBehaviour
{
    [System.Serializable]
    public class Exit
    {
        public Transform transform;   // where to attach next room
        public DoorType doorType = DoorType.Single;
        [HideInInspector] public bool isConnected = false;
    }

    public enum DoorType
    {
        Single,
        Double,
        Vent,
        Hallway
    }

    [Header("Room Settings")]
    public string roomName;
    public float weight = 1f; // default weight = 1
    public bool isDoor = false;
    public Vector3 size;                // optional if you want to use bounding boxes
    [Header("Room Exits")]
    public List<Exit> exits = new List<Exit>();
    public int exitCount => exits.Count;

    public List<Exit> GetAvailableExits(DoorType? type = null)
    {
        List<Exit> list = new List<Exit>();
        foreach (var e in exits)
        {
            if (e.isConnected) continue;
            if (type != null && e.doorType != type.Value) continue;
            list.Add(e);
        }
        return list;
    }


    public Exit GetRandomAvailableExit()
    {
        List<Exit> available = new List<Exit>();
        foreach (var e in exits)
            if (!e.isConnected)
                available.Add(e);

        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }

}
