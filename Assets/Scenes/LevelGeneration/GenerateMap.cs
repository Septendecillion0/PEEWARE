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
    [Header("Horizontal Road Prefab")]
    public GameObject roadHoriPrefab;
    [Header("Vertical Road Prefab")]
    public GameObject roadVertiPrefab;

    //Current room numbers
    private int currentRoomNum = 0;

    //Int array keeping track of where rooms are located in unit blocks
    private int[,] roomMap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Total Rooms" + maxRoom);
        DrawMap();
    }

    // Update is called once per frame
    void Update()
    {

    }
    //Main Map Generation
    private void DrawMap()
    {
        //Initialize spawn point
        Vector3Int currentPoint = Vector3Int.zero;
        currentRoomNum = 0;
        roomMap = new int[maxRow, maxCol];
        roomMap[currentPoint.x, currentPoint.z] = 1;
        DrawRoom(currentPoint, 0);
    }

    private void DrawRoom(Vector3Int currentPoint, int pathLength)
    {
        Debug.Log("New Room Finished" + currentRoomNum);
        //CurrentRoom
        currentRoomNum ++;
        Vector3Int pos = new Vector3Int(currentPoint.x * 10, 0, currentPoint.z * 10);
        GameObject floorObject = Instantiate(floorPrefab, pos, Quaternion.identity);
        //Keep drawing until we run out of rooms
        Vector3Int newPoint;
        for(int i = 0; i < 4; i++){
            if (currentRoomNum <= maxRoom && pathLength <= maxPath){
                newPoint = GetNextRoom(currentPoint);
                if (roomMap[newPoint.x, newPoint.z] == 1) continue;
                roomMap[newPoint.x, newPoint.z] = 1;
                DrawRoom(newPoint, pathLength + 1);
                DrawRoad(currentPoint.x, currentPoint.z, newPoint.x, newPoint.z);
            }
        }
    }

    private void DrawRoad(int room1X, int room1Z, int room2X, int room2Z)
    {
        //Horizontal
        if (room1X == room2X)
        {
            Vector3Int posHori = new Vector3Int(room1X * 10, 0, (room1Z + room2Z) * 5);
            GameObject horiRoad = Instantiate(roadHoriPrefab, posHori, Quaternion.identity);
        }
        //Vertical
        else if (room1Z == room2Z)
        {
            Vector3Int posVerti = new Vector3Int((room1X + room2X) * 5, 0, room1Z * 10);
            GameObject vertiRoad = Instantiate(roadVertiPrefab, posVerti, Quaternion.identity);
        }

    }

    private Vector3Int GetNextRoom(Vector3Int currentPoint){
        while(true)
        {
            Vector3Int dummy = currentPoint;
            //4 possible direction
            switch(Random.Range(0,4)){
                case 0:
                    dummy.x += 1;
                    break;
                case 1: 
                    dummy.z += 1;
                    break;
                case 2:
                    dummy.x -= 1;
                    break;
                default:
                    dummy.z -= 1;
                    break;
            }
            //If it is a valid room
            if (dummy.x >= 0 && dummy.z >= 0 && dummy.x < maxRow && dummy.z < maxCol){
                return dummy;
        }
        }
    }
}
