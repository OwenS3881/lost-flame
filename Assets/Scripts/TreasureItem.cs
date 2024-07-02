using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreasureItem
{
    public GameObject objectToSpawn;
    [Range(0, 100)]
    public int probability;
}
