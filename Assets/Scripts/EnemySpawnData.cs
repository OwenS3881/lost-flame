using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnData
{
    [HideInInspector] public string name;
    public BasicEnemySO enemy;
    [Range(1, 99)]
    public int probability;
    public Vector2Int[] dungeonRankRanges;
}
