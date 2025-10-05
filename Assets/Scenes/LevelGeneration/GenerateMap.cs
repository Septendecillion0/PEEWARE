using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    [Header("Map Seed")]
    public int seed;
    [Header("Total Rows")]
    public int maxRow;
    [Header("Total Cols")]
    public int maxCol;
    [Header("Total Rooms")]
    public int maxRoom;
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
    [Header("Horizontal Road Prefab")]
    public GameObject roadHoriPrefab;
    [Header("Vertical Road Prefab")]
    public GameObject roadVertiPrefab;

    //Int array keeping track of where rooms are located in unit blocks
    private int[,] roomMap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawMap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void DrawMap()
    {
        DrawRoom(0, 0);
        DrawRoom(1, 0);
        DrawRoad(0, 0, 1, 0);
        DrawRoom(0, 1);
        DrawRoad(0, 0, 0, 1);
    }

    private void DrawRoom(int roomX, int roomY)
    {
        Vector3 pos = new Vector3(roomX * 10, 0, roomY * 10);
        GameObject floorObject = Instantiate(floorPrefab, pos, Quaternion.identity);
    }

    private void DrawRoad(int room1X, int room1Y, int room2X, int room2Y)
    {
        //Horizontal
        if (room1X == room2X)
        {
            Vector3 posHori = new Vector3(room1X * 10, 0, (room1Y + room2Y) * 5);
            GameObject horiRoad = Instantiate(roadHoriPrefab, posHori, Quaternion.identity);
        }
        //Vertical
        else if (room1Y == room2Y)
        {
            Vector3 posVerti = new Vector3((room1X + room2X) * 5, 0, room1Y * 10);
            GameObject vertiRoad = Instantiate(roadVertiPrefab, posVerti, Quaternion.identity);
        }

    }
}
