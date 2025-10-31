using UnityEngine;
using System.Collections.Generic;

//written with assistance by ChatGPT

public class Room : MonoBehaviour
{
    [System.Serializable]
    public class Exit
    {
        public Transform transform;   // where to attach next room
        public string tag;             // optional: e.g. "door", "stairs", etc.
        [HideInInspector] public bool isConnected = false;
    }

    public string roomName;
    public Vector3 size;                // optional if you want to use bounding boxes
    public List<Exit> exits = new List<Exit>();

    // Called once after instantiation
    public IEnumerable<Exit> GetAvailableExits()
    {
        foreach (var e in exits)
            if (!e.isConnected)
                yield return e;
    }
}
