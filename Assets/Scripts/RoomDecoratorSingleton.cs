using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomDecoratorSingleton : MonoBehaviour
{

    public static RoomDecoratorSingleton instance { get; private set; }


    public RoomDecoratorData[] roomDecorators;

    public EnemySpawnData[] enemySpawnDatas;

    public float minDistanceBetweenEnemies;

    [Range(0, 100)]
    public int peacefulProbability;

    [SerializeField] private int numberOfFloors;

    private void OnValidate()
    {
        for (int i = 0; i < enemySpawnDatas.Length; i++)
        {
            if (enemySpawnDatas[i].enemy != null)
            {
                enemySpawnDatas[i].name = enemySpawnDatas[i].enemy.name;
            }
            else
            {
                enemySpawnDatas[i].name = "Null";
            }
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
            Destroy(gameObject);
            return;
        }

        OnValidate();
    }

    public EnemySpawnData GetRandomEnemySpawnData()
    {
        int totalProbability = 0;
        foreach (EnemySpawnData e in enemySpawnDatas)
        {
            totalProbability += e.probability;
        }

        float random = UnityEngine.Random.Range(0f, (float)totalProbability);

        int runningSum = 0;
        foreach(EnemySpawnData e in enemySpawnDatas)
        {
            runningSum += e.probability;
            if (runningSum > random)
            {
                int cur = SavedDataManager.instance.GetCurrentDungeonRank();

                bool isValid = false;

                foreach (Vector2Int range in e.dungeonRankRanges)
                {
                    if (range.x <= cur && cur <= range.y)
                    {
                        isValid = true;
                        break;
                    }
                }

                if (isValid)
                {
                    return e;
                }
                else
                {
                    return GetRandomEnemySpawnData();
                }
            }
        }

        return null;
    }

    public void CountEnemiesPerFloor()
    {
        Debug.Log("Begin Count:");
        for (int i = 1; i <= numberOfFloors; i++)
        {
            int count = 0;
            foreach (EnemySpawnData e in enemySpawnDatas)
            {
                foreach (Vector2Int range in e.dungeonRankRanges)
                {
                    if (range.x <= i && i <= range.y)
                    {
                        count++;
                        break;
                    }
                }
            }
            Debug.Log("Floor " + i + " has " + count + " enemies");
        }
        Debug.Log("End Count");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RoomDecoratorSingleton))]
class RoomDecoratorSingletonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RoomDecoratorSingleton attatchedObject = (RoomDecoratorSingleton)target;

        if (GUILayout.Button("Count Enemies"))
        {
            attatchedObject.CountEnemiesPerFloor();
        }
    }
}
#endif