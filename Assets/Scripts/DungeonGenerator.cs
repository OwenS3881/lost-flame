using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DungeonGenerator : MonoBehaviour
{
    [Header("==========Randomness Controllers==========")]

    [Range(5, 101)]
    [SerializeField] private int dungeonWidth;

    [Range(5, 101)]
    [SerializeField] private int dungeonHeight;

    [Space]

    [Range(1, 99)]
    [SerializeField] private int expansionProbability;

    [Space]

    [Range(0, 256)]
    [SerializeField] private int minRooms;

    [Range(5, 261)]
    [SerializeField] private int maxRooms;

    [Space]

    [Range(0, 256)]
    [SerializeField] private int minDistanceToEnd;

    [Range(5, 261)]
    [SerializeField] private int maxDistanceToEnd;

    [Space]
    [Space]
    [Space]
    [Space]
    [Space]
    [Space]

    [Header("==========Other Fields==========", order = 10)]

    [Space(order = 11)]

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float distanceBetweenRooms;
    [SerializeField] private RoomData[] rooms;
    [SerializeField] private GameObject[] verticalHallways;
    [SerializeField] private GameObject[] horizontalHallways;
    [SerializeField] private Color startRoomColor;
    [SerializeField] private Color endRoomColor;
    [SerializeField] private GameObject endPoint;
    [ReadOnly] public bool dungeonSpawned;
    [ReadOnly] public Player player;
    

    private float xOffset, yOffset;

    public int[,] dungeonArray;

    private Vector3 targetScale;

    [HideInInspector] public Vector2Int startRoom;
    [HideInInspector] public Vector2Int endRoom;
    private int distanceToEnd;

    public static DungeonGenerator instance { get; private set; }

    /* 
     * Array Value Meanings
     * 0 = Empty Space
     * 1 = Spawned Room
     * 2 = Spawned Room that wants to spawn more rooms
     * 3 = Hallway
     */

    private void OnValidate()
    {
        if (dungeonWidth % 2 == 0)
        {
            dungeonWidth++;
        }
        if (dungeonHeight % 2 == 0)
        {
            dungeonHeight++;          
        }

        if ((dungeonWidth + 1) % 4 == 0)
        {
            dungeonWidth += 2;
        }
        if ((dungeonHeight + 1) % 4 == 0)
        {
            dungeonHeight += 2;
        }

        xOffset = (dungeonWidth / 2) * distanceBetweenRooms;
        yOffset = (dungeonHeight / 2) * distanceBetweenRooms;

        if (minRooms > ((dungeonWidth + 1) / 2) * ((dungeonHeight + 1) / 2))
        {
            minRooms = ((dungeonWidth + 1) / 2) * ((dungeonHeight + 1) / 2);
        }

        while (maxRooms < minRooms)
        {
            maxRooms++;
        }

        while (maxDistanceToEnd < minDistanceToEnd)
        {
            maxDistanceToEnd++;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple Dungeon Generators in scene");
        }

        if (AudioManager.instance.activeSong.Equals(""))
        {
            AudioManager.instance.PlaySong("Song1");
        }
    }

    private void Start()
    {
        OnValidate();
        transform.position = Vector3.zero;
        targetScale = transform.localScale;
        CreateDungeon();
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
        StartPoint.instance.PlacePlayer();
    }

    [ContextMenu("Create Dungeon")]
    void CreateDungeon()
    {
        ClearDungeon();
        InitializeArray();   
        while (Does2DArrayContain(dungeonArray, 2))
        {
            ExpandArray();
        }
        FindStartAndEndValues();
        if (ShouldDungeonReset())
        {
            Invoke("CreateDungeon", 0.01f);
            return;
        }
        SpawnRooms();
        SpawnHallways();
        Invoke("SetProperScale", 0.015f);
        Invoke("AssignStartEndRoomColors", 0.01f);
        Invoke("UpdatePathfinding", 0.05f);
        Invoke("UpdatePathfinding", 0.2f);
        dungeonSpawned = true;
    }

    void SetProperScale()
    {
        transform.localScale = targetScale;
    }

    void UpdatePathfinding()
    {
        int width = (int)(dungeonWidth * 2 * transform.localScale.x);
        int height = (int)(dungeonHeight * 2 * transform.localScale.y);
        width += 4;
        height += 4;
        AstarPath.active.data.gridGraph.SetDimensions(width, height, AstarPath.active.data.gridGraph.nodeSize);
        AstarPath.active.Scan(AstarPath.active.data.gridGraph);
    }

    private bool ShouldDungeonReset()
    {
        int roomAmount = ItemCount(1);
        if (roomAmount < minRooms)
        {
            Debug.Log("Room is too small, creating a new one");
            return true;
        }
        else if (roomAmount >= maxRooms)
        {
            Debug.Log("Room is too big, creating a new one");
            return true;
        }
        else if (distanceToEnd < minDistanceToEnd)
        {
            Debug.Log("Goal is too close, creating new dungeon");
            return true;
        }
        else if (distanceToEnd >= maxDistanceToEnd)
        {
            Debug.Log("Goal is too far, creating a new dungeon");
            return true;
        }
        return false;
    }

    void ClearDungeon()
    {
        dungeonSpawned = false;
        dungeonArray = new int[dungeonWidth, dungeonHeight];
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.Equals(transform))
            {
                continue;
            }
            Destroy(t.gameObject);
        }
    }

    [ContextMenu("Display Array")]
    public void DisplayArray()
    {
        for (int y = 0; y < dungeonArray.GetLength(1); y++)
        {
            string row = "";
            for (int x = 0; x < dungeonArray.GetLength(0); x++)
            {
                row += dungeonArray[x, y];
            }
            Debug.Log(row);
        }
    }

    private bool Does2DArrayContain(int[,] array, int a)
    {
        for (int x = 0; x < array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                if (array[x,y] == a)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void InitializeArray()
    {
        transform.localScale = new Vector3(1, 1, 1);
        dungeonArray = new int[dungeonWidth, dungeonHeight];
        for (int x = 0; x < dungeonArray.GetLength(0); x++)
        {
            for (int y = 0; y < dungeonArray.GetLength(1); y++)
            {
                dungeonArray[x, y] = 0;
            }
        }

        Vector2Int center = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
        dungeonArray[center.x, center.y] = 1;

        dungeonArray[center.x + 1, center.y] = 3;
        dungeonArray[center.x + 2, center.y] = 2;

        dungeonArray[center.x - 1, center.y] = 3;
        dungeonArray[center.x - 2, center.y] = 2;

        dungeonArray[center.x, center.y + 1] = 3;
        dungeonArray[center.x, center.y + 2] = 2;

        dungeonArray[center.x, center.y - 1] = 3;
        dungeonArray[center.x, center.y - 2] = 2;
    }

    private void ExpandArray()
    {
        for (int x = 0; x < dungeonArray.GetLength(0); x++)
        {
            for (int y = 0; y < dungeonArray.GetLength(1); y++)
            {
                if (dungeonArray[x, y] == 2)
                {
                    ExpandCell(x, y, 2, 0);
                    ExpandCell(x, y, -2, 0);
                    ExpandCell(x, y, 0, 2);
                    ExpandCell(x, y, 0, -2);
                    dungeonArray[x, y] = 1;
                }
            }
        }
    }

    private void ExpandCell(int x, int y, int xDir, int yDir)
    {
        if (!PointExists(x + xDir, y + yDir))
        {
            return;
        }

        if (dungeonArray[x + xDir, y + yDir] != 0)
        {
            return;
        }

        float rand = UnityEngine.Random.Range(0, 100);
        if (rand > expansionProbability)
        {
            dungeonArray[x + xDir, y + yDir] = 0;
        }
        else
        {
            dungeonArray[x + xDir, y + yDir] = 2;
            dungeonArray[x + xDir/2, y + yDir/2] = 3;
        }
    }

    private void SpawnRooms()
    {
        for (int x = 0; x < dungeonArray.GetLength(0); x++)
        {
            for (int y = 0; y < dungeonArray.GetLength(1); y++)
            {
                if (dungeonArray[x, y] == 1)
                {
                    SpawnRoomAtCell(x, y);
                }
            }
        }
    }

    private bool PointExists(int x, int y)
    {
        return !(x < 0 || x >= dungeonArray.GetLength(0) || y < 0 || y >= dungeonArray.GetLength(1));
    }

    private void SpawnRoomAtCell(int x, int y)
    {
        RoomData roomToSpawn = null;
        bool top, bottom, left, right;

        top = PointExists(x, y + 2) && dungeonArray[x, y + 1] == 3;
        bottom = PointExists(x, y - 2) && dungeonArray[x, y - 1] == 3;
        left = PointExists(x - 2, y) && dungeonArray[x - 1, y] == 3;
        right = PointExists(x + 2, y) && dungeonArray[x + 1, y] == 3;

        List<RoomData> potentialRooms = new List<RoomData>();

        foreach (RoomData r in rooms)
        {
            if (r.top == top && r.bottom == bottom && r.left == left && r.right == right)
            {
                potentialRooms.Add(r);
            }
        }

        roomToSpawn = potentialRooms[UnityEngine.Random.Range(0, potentialRooms.Count)];

        GameObject newRoom = Instantiate(roomToSpawn.gameObject, transform);
        newRoom.transform.position = new Vector3((x * distanceBetweenRooms) - xOffset, (y * distanceBetweenRooms) - yOffset, 0);
        newRoom.GetComponent<RoomData>().coordinates = new Vector2Int(x, y);
    }

    private void SpawnHallways()
    {
        for (int x = 0; x < dungeonArray.GetLength(0); x++)
        {
            for (int y = 0; y < dungeonArray.GetLength(1); y++)
            {
                if (dungeonArray[x, y] == 3)
                {
                    SpawnHallwayAtCell(x, y);
                }
            }
        }
    }

    private void SpawnHallwayAtCell(int x, int y)
    {
        bool top, bottom, left, right;

        top = PointExists(x, y + 1) && dungeonArray[x, y + 1] == 1;
        bottom = PointExists(x, y - 1) && dungeonArray[x, y - 1] == 1;
        left = PointExists(x - 1, y) && dungeonArray[x - 1, y] == 1;
        right = PointExists(x + 1, y) && dungeonArray[x + 1, y] == 1;

        if (top && bottom)
        {
            GameObject newHallway = Instantiate(verticalHallways[UnityEngine.Random.Range(0, verticalHallways.Length)], transform);
            newHallway.transform.position = new Vector3((x * distanceBetweenRooms) - xOffset, (y * distanceBetweenRooms) - yOffset, 0);
        }
        else if (right && left)
        {
            GameObject newHallway = Instantiate(horizontalHallways[UnityEngine.Random.Range(0, horizontalHallways.Length)], transform);
            newHallway.transform.position = new Vector3((x * distanceBetweenRooms) - xOffset, (y * distanceBetweenRooms) - yOffset, 0);
        }
        else
        {
            Debug.LogError("Hallway is not connected on both sides");
        }
    }

    private Vector2 PointToLocalPos(int x, int y)
    {
        if (!PointExists(x, y))
        {
            Debug.LogError(x + ", " + y + " is not a valid point");
            return Vector2.zero;
        }
        Vector2 output = new Vector2((x * distanceBetweenRooms) - xOffset, (y * distanceBetweenRooms) - yOffset);
        return output;
    }

    public GameObject FindObjectAtIndex(int x, int y)
    {
        foreach (RoomData r in GetComponentsInChildren<RoomData>())
        {
            if (r.coordinates.x == x && r.coordinates.y == y)
            {
                return r.gameObject;
            }
        }
        Debug.LogError("No GameObject found at: " + x + ", " + y);
        return null;
    }

    private Vector2 PointToLocalPos(Vector2Int point)
    {
        return PointToLocalPos(point.x, point.y);
    }

    private GameObject FindObjectAtIndex(Vector2Int index)
    {
        return FindObjectAtIndex(index.x, index.y);
    }

    private int ItemCount(int a)
    {
        int count = 0;
        for (int x = 0; x < dungeonArray.GetLength(0); x++)
        {
            for (int y = 0; y < dungeonArray.GetLength(1); y++)
            {
                if (dungeonArray[x, y] == a)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void FindStartAndEndValues()
    {
        int[,] distanceDungeon = dungeonArray.Clone() as int [,];
        //When finding distance, subtract 4
        List<Vector2Int> currentRooms = new List<Vector2Int>();
        List<Vector2Int> newRooms = new List<Vector2Int>();

        startRoom = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
        distanceDungeon[startRoom.x, startRoom.y] = 4;
        currentRooms.Add(new Vector2Int(startRoom.x + 2, startRoom.y));
        currentRooms.Add(new Vector2Int(startRoom.x - 2, startRoom.y));
        currentRooms.Add(new Vector2Int(startRoom.x, startRoom.y + 2));
        currentRooms.Add(new Vector2Int(startRoom.x, startRoom.y - 2));

        for (int i = 5; i > 0; i++)
        {
            if (currentRooms.Count == 0)
            {
                break;
            }

            foreach (Vector2Int pos in currentRooms)
            {
                distanceDungeon[pos.x, pos.y] = i;
                if (PointExists(pos.x + 2, pos.y) && distanceDungeon[pos.x + 1, pos.y] == 3 && distanceDungeon[pos.x + 2, pos.y] == 1)
                {
                    newRooms.Add(new Vector2Int(pos.x + 2, pos.y));
                }
                if (PointExists(pos.x - 2, pos.y) && distanceDungeon[pos.x - 1, pos.y] == 3 && distanceDungeon[pos.x - 2, pos.y] == 1)
                {
                    newRooms.Add(new Vector2Int(pos.x - 2, pos.y));
                }
                if (PointExists(pos.x, pos.y + 2) && distanceDungeon[pos.x, pos.y + 1] == 3 && distanceDungeon[pos.x, pos.y + 2] == 1)
                {
                    newRooms.Add(new Vector2Int(pos.x, pos.y + 2));
                }
                if (PointExists(pos.x, pos.y - 2) && distanceDungeon[pos.x, pos.y - 1] == 3 && distanceDungeon[pos.x, pos.y - 2] == 1)
                {
                    newRooms.Add(new Vector2Int(pos.x, pos.y - 2));
                }
            }

            currentRooms.Clear();
            currentRooms = new List<Vector2Int>(newRooms);
            newRooms.Clear();
        }

        int endDistance = 0;
        Vector2Int endRoomPos = startRoom;
        for (int x = 0; x < distanceDungeon.GetLength(0); x++)
        {
            for (int y = 0; y < distanceDungeon.GetLength(1); y++)
            {
                if (distanceDungeon[x,y] > endDistance)
                {
                    endDistance = distanceDungeon[x, y];
                    endRoomPos = new Vector2Int(x, y);
                }
            }
        }
         distanceToEnd = endDistance - 4;
        endRoom = endRoomPos;
    }

    [Obsolete]
    private void AssignDistanceValues()
    {
        /*
        List<Vector2Int> currentRooms = new List<Vector2Int>();
        List<Vector2Int> newRooms = new List<Vector2Int>();

        Vector2Int center = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
        AssignRoomValue(center, 0);

        RoomData start = FindObjectAtIndex(center).GetComponent<RoomData>();
        start.isStart = true;

        currentRooms.Add(new Vector2Int(center.x + 2, center.y));
        currentRooms.Add(new Vector2Int(center.x - 2, center.y));
        currentRooms.Add(new Vector2Int(center.x, center.y + 2));
        currentRooms.Add(new Vector2Int(center.x, center.y - 2));

        for (int i = 1; i > 0; i++)
        {
            if (currentRooms.Count == 0)
            {
                break;
            }
            
            foreach (Vector2Int pos in currentRooms)
            {
                AssignRoomValue(pos, i);
                if (DoesObjectExist(pos.x + 1, pos.y) && FindObjectAtIndex(pos.x + 2, pos.y).GetComponent<RoomData>().distanceFromStart == 0 && !FindObjectAtIndex(pos.x + 2, pos.y).GetComponent<RoomData>().isStart)
                {
                    newRooms.Add(new Vector2Int(pos.x + 2, pos.y));
                }
                if (DoesObjectExist(pos.x - 1, pos.y) && FindObjectAtIndex(pos.x - 2, pos.y).GetComponent<RoomData>().distanceFromStart == 0 && !FindObjectAtIndex(pos.x - 2, pos.y).GetComponent<RoomData>().isStart)
                {
                    newRooms.Add(new Vector2Int(pos.x - 2, pos.y));
                }
                if (DoesObjectExist(pos.x, pos.y + 1) && FindObjectAtIndex(pos.x, pos.y + 2).GetComponent<RoomData>().distanceFromStart == 0 && !FindObjectAtIndex(pos.x, pos.y + 2).GetComponent<RoomData>().isStart)
                {
                    newRooms.Add(new Vector2Int(pos.x, pos.y + 2));
                }
                if (DoesObjectExist(pos.x, pos.y - 1) && FindObjectAtIndex(pos.x, pos.y - 2).GetComponent<RoomData>().distanceFromStart == 0 && !FindObjectAtIndex(pos.x, pos.y - 2).GetComponent<RoomData>().isStart)
                {
                    newRooms.Add(new Vector2Int(pos.x, pos.y - 2));
                }
            }
            
            currentRooms.Clear();
            currentRooms = new List<Vector2Int>(newRooms);
            newRooms.Clear();
        }

        RoomData end = null;
        foreach (RoomData r in GetComponentsInChildren<RoomData>())
        {
            if (end == null)
            {
                end = r;
            }
            else if (r.distanceFromStart > end.distanceFromStart)
            {
                end = r;
            }
        }
        end.isEnd = true;
        distanceToEnd = end.distanceFromStart;
        */
    }

    private void AssignStartEndRoomColors()
    {
        GameObject start = FindObjectAtIndex(startRoom);
        start.GetComponent<SpriteRenderer>().color = startRoomColor;
        start.name = start.name + "-Start Room";
        start.GetComponent<RoomData>().isStart = true;

        GameObject end = FindObjectAtIndex(endRoom);
        end.GetComponent<SpriteRenderer>().color = endRoomColor;
        end.name = end.name + "-End Room";
        end.GetComponent<RoomData>().isEnd = true;

        StartCoroutine(SpawnEndPoint(end.transform));
    }

    IEnumerator SpawnEndPoint(Transform end)
    {
        yield return new WaitForSeconds(0.05f);
        Instantiate(endPoint, end.position, Quaternion.identity);
    }

    [Obsolete]
    private void AssignRoomValue(Vector2Int pos, int value)
    {
        //FindObjectAtIndex(pos).GetComponent<RoomData>().distanceFromStart = value;
    }

    [Obsolete]
    private void AssignRoomValue(int x, int y, int value)
    {
        //FindObjectAtIndex(x, y).GetComponent<RoomData>().distanceFromStart = value;
    }

    public bool DoesObjectExist(int x, int y)
    {
        if (!PointExists(x, y))
        {
            return false;
        }
        foreach (RoomData r in GetComponentsInChildren<RoomData>())
        {
            if (r.coordinates.x == x && r.coordinates.y == y)
            {
                return true;
            }
        }
        return false;
    }

    private bool DoesObjectExist(Vector2Int pos)
    {
        return DoesObjectExist(pos.x, pos.y);
    }

    private void ShowChildren()
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.Equals(transform))
            {
                continue;
            }
            t.GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
