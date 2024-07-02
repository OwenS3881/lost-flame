using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomDecoratorData 
{
    [SerializeField] private GameObject[] objectsToSpawn;
    [SerializeField] private Vector2Int[] objectsToSpawnRange;
    public int numebrOfObjectsToSpawn;
    public float spawnRadius;
    public bool canFlipX;
    public Vector2 sizeDifference;

    public GameObject GetRandomObject()
    {
        if (objectsToSpawn.Length > objectsToSpawnRange.Length)
        {
            Debug.LogError("Not every object has a range to spawn at");
            return null;
        }

        int index = UnityEngine.Random.Range(0, objectsToSpawn.Length);
        int cur = SavedDataManager.instance.GetCurrentDungeonRank();

        if (objectsToSpawnRange[index].x <= cur && cur <= objectsToSpawnRange[index].y)
        {
            return objectsToSpawn[index];
        }
        else
        {
            return GetRandomObject();
        }      
    }
}
