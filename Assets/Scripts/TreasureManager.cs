using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureManager : MonoBehaviour
{
    public static TreasureManager instance { get; private set; }

    [SerializeField, Range(0, 100)] private int treasureProbability;
    [SerializeField] private TreasureItem[] treasureItems;

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
        DontDestroyOnLoad(gameObject);
    }

    public TreasureItem GetRandomTreasureItem()
    {
        int totalProbability = 0;
        foreach (TreasureItem t in treasureItems)
        {
            totalProbability += t.probability;
        }

        float random = UnityEngine.Random.Range(0f, (float)totalProbability);

        int runningSum = 0;
        foreach (TreasureItem t in treasureItems)
        {
            runningSum += t.probability;
            if (runningSum > random)
            {
                return t;
            }
        }

        return null;
    }

    public bool ShouldTreasureSpawn()
    {
        return UnityEngine.Random.Range(0f, 100f) <= treasureProbability;
    }
}
