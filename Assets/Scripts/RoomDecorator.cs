using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;
using UnityEngine.SceneManagement;
using Pathfinding;

//////////////////////////////////////////////////////////
//***WARNING: VERY JANKY CODE, TOUCH AT YOUR OWN RISK***//
//////////////////////////////////////////////////////////

[RequireComponent(typeof(Collider2D))]
public class RoomDecorator : MonoBehaviour
{
    private RoomData roomData;

    [Header("Treasure")]
    [SerializeField] private GameObject treasurePrefab;

    [Header("Decorator Variables")]

    [SerializeField] private int decoratorIndex;
    [SerializeField] private Transform[] spawnPoints;

    private List<GameObject> spawnedDecorateObjects = new List<GameObject>();
    private Collider2D roomCollider;
    private RoomDecoratorData myData;

    [Header("Enemy Spawn Variables")]

    [SerializeField] private float width;
    [SerializeField] private float height;
    [SerializeField] private GameObject enemyHolder;
    [SerializeField] private Vector2Int numberOfEnemies;

    private List<BasicEnemySO> enemiesToSpawn = new List<BasicEnemySO>();
    private List<Vector2> enemyLocalSpawnPositions = new List<Vector2>();

    [Header("Room Object Spawn Variables")]
    [SerializeField] private int roomObjectIndex;
    [SerializeField] private Vector2Int numberOfRoomObjects;

    private List<GameObject> roomObjectsToSpawn = new List<GameObject>();
    private List<Vector2> roomObjectLocalSpawnPositions = new List<Vector2>();
    private List<GameObject> spawnedRoomObjects = new List<GameObject>();
    private RoomDecoratorData roomObjectData;

    private void Start()
    {
        roomData = GetComponent<RoomData>();
        myData = RoomDecoratorSingleton.instance.roomDecorators[decoratorIndex];
        roomObjectData = RoomDecoratorSingleton.instance.roomDecorators[roomObjectIndex];
        roomCollider = GetComponent<Collider2D>();
        Invoke("Decorate", 0.025f);
        Invoke("DisableSpriteRenderer", 0.026f);
        if (SceneManager.GetActiveScene().name.Equals("MainLevel"))
        {
            Invoke("SpawnEnemies", 0.1f);
            Invoke("CreateRoomObjects", 0.11f);
        }
    }

    private bool IsPointInPathfindingGrid(Vector2 point)
    {
        if (AstarPath.active.data.gridGraph.CountNodes() <= 1) return false;
        GraphNode nearestNode = AstarPath.active.GetNearest(point, NNConstraint.None).node;
        return nearestNode.Walkable;
    }


    #region Decorator Code
    [ContextMenu("Decorate")]
    void Decorate()
    {
        ClearSpawnedObjects();

        for (int i = 0; i <  spawnPoints.Length; i++)
        {
            for (int j = 0; j < myData.numebrOfObjectsToSpawn; j++)
            {

                Vector3 spawnPos;
                Transform selectedSpawnPoint = spawnPoints[i];
                Vector2 offset = new Vector2(UnityEngine.Random.Range(-myData.spawnRadius, myData.spawnRadius), UnityEngine.Random.Range(-myData.spawnRadius, myData.spawnRadius));
                spawnPos = (Vector2)selectedSpawnPoint.position + offset;

                GameObject current = Instantiate(myData.GetRandomObject(), spawnPos, Quaternion.identity);

                current.transform.SetParent(transform);
                AlterGameObject(current, myData);

                //current.GetComponent<SpriteRenderer>().sortingOrder = (int)-current.transform.position.y * 100;
                SetPosition(current.transform, 2, current.transform.position.y/100);

                OptimizerSingleton.instance.AddToDictionary("Decorations", current);
                spawnedDecorateObjects.Add(current);
            }
        }
    }

    void ClearSpawnedObjects()
    {
        foreach (GameObject g in spawnedDecorateObjects)
        {
            Destroy(g);
        }
        spawnedDecorateObjects.Clear();
    }

    void DisableSpriteRenderer()
    {
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            sr.enabled = false;
        }
    }

    void AlterGameObject(GameObject g, RoomDecoratorData data)
    {
        if (data.canFlipX && g.TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            sr.flipX = (UnityEngine.Random.Range(0f, 1f) > 0.5f);
        }

        float sizeChange = UnityEngine.Random.Range(data.sizeDifference.x, data.sizeDifference.y);
        g.transform.localScale = new Vector3(g.transform.localScale.x + sizeChange * g.transform.localScale.x, g.transform.localScale.y + sizeChange * g.transform.localScale.y, g.transform.localScale.z);
    }
    #endregion

    #region Enemy Spawn Code

    void SpawnEnemies()
    {
        if (enemyHolder == null || numberOfEnemies.y < 1 || roomData.isStart || roomData.isEnd) return;

        if (UnityEngine.Random.Range(0f, 100f) < RoomDecoratorSingleton.instance.peacefulProbability)
        {
            return;
        }

        PopulateEnemyLists();
        CreateEnemies();
    }

    private void PopulateEnemyLists()
    {
        int spawnCount = UnityEngine.Random.Range(numberOfEnemies.x, numberOfEnemies.y + 1);
        for (int i = 0; i < spawnCount; i++)
        {
            EnemySpawnData data = RoomDecoratorSingleton.instance.GetRandomEnemySpawnData();

            enemiesToSpawn.Add(data.enemy);

            while (enemiesToSpawn.Count != enemyLocalSpawnPositions.Count)
            {
                Vector2 pos = GetPosInRoom();
                bool isValid = true;
                foreach (Vector2 p in enemyLocalSpawnPositions)
                {
                    if (Vector2.Distance(pos, p) < RoomDecoratorSingleton.instance.minDistanceBetweenEnemies)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    enemyLocalSpawnPositions.Add(pos);
                }
            }
        }
    }

    private Vector2 GetPosInRoom()
    {
        Vector2 pos = new Vector2(UnityEngine.Random.Range(-width / 2, width / 2), UnityEngine.Random.Range(-height / 2, height / 2));
        
        if (IsPointInPathfindingGrid(pos + (Vector2)transform.position))
        {
            return pos;
        }
        else
        {
            return GetPosInRoom();
        }
    
    }

    private void CreateEnemies()
    {
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            GameObject e = Instantiate(enemyHolder, transform.position + (Vector3)enemyLocalSpawnPositions[i], Quaternion.identity);
            e.GetComponent<EnemyHolder>().enemySO = enemiesToSpawn[i];
        }
    }

    #endregion

    #region Room Object Spawn Code
    private void CreateRoomObjects()
    {
        if (numberOfRoomObjects.y < 1 || roomData.isStart || roomData.isEnd) return;

        ClearSpawnedRoomObjects();
        PopulateRoomObjectsLists();
        AddTreasure();
        SpawnRoomObjects();
    }

    private void PopulateRoomObjectsLists()
    {
        int spawnCount = UnityEngine.Random.Range(numberOfRoomObjects.x, numberOfRoomObjects.y + 1);
        for (int i = 0; i < spawnCount; i++)
        {
            roomObjectsToSpawn.Add(roomObjectData.GetRandomObject());
            while (roomObjectsToSpawn.Count > roomObjectLocalSpawnPositions.Count)
            {
                Vector2 pos = GetPosInRoom();
                bool isValid = true;
                foreach (Vector2 p in roomObjectLocalSpawnPositions)
                {
                    if (Vector2.Distance(pos, p) < RoomDecoratorSingleton.instance.minDistanceBetweenEnemies)
                    {
                        isValid = false;
                        break;
                    }
                }


                foreach (Vector2 p in enemyLocalSpawnPositions)
                {
                    if (Vector2.Distance(pos, p) < RoomDecoratorSingleton.instance.minDistanceBetweenEnemies)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    roomObjectLocalSpawnPositions.Add(pos);
                }
            }
        }       
    }

    private void AddTreasure()
    {
        if (!TreasureManager.instance.ShouldTreasureSpawn()) return;

        roomObjectsToSpawn.Add(treasurePrefab);
        while (roomObjectsToSpawn.Count > roomObjectLocalSpawnPositions.Count)
        {
            Vector2 pos = GetPosInRoom();
            bool isValid = true;
            foreach (Vector2 p in roomObjectLocalSpawnPositions)
            {
                if (Vector2.Distance(pos, p) < RoomDecoratorSingleton.instance.minDistanceBetweenEnemies)
                {
                    isValid = false;
                    break;
                }
            }


            foreach (Vector2 p in enemyLocalSpawnPositions)
            {
                if (Vector2.Distance(pos, p) < RoomDecoratorSingleton.instance.minDistanceBetweenEnemies)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                roomObjectLocalSpawnPositions.Add(pos);
            }
        }

        GetComponent<RoomData>().containsTreasure = true;

    }

    private void ClearSpawnedRoomObjects()
    {
        foreach (GameObject g in spawnedRoomObjects)
        {
            Destroy(g);
        }
        spawnedRoomObjects.Clear();
        roomObjectLocalSpawnPositions.Clear();
        roomObjectsToSpawn.Clear();
    }

    private void SpawnRoomObjects()
    {
        for (int i = 0; i < roomObjectsToSpawn.Count; i++)
        {
            GameObject r = Instantiate(roomObjectsToSpawn[i], transform.position + (Vector3)roomObjectLocalSpawnPositions[i], Quaternion.identity);
            OptimizerSingleton.instance.AddToDictionary("Decorations", r);
            AlterGameObject(r, roomObjectData);
            spawnedRoomObjects.Add(r);
        }
    }
    #endregion
}
