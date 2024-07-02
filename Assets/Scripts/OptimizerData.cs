using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OptimizerData
{
    public List<GameObject> objectsList;
    public float disappearDistance;

    public OptimizerData (List<GameObject> list, float distance)
    {
        objectsList = new List<GameObject>(list);
        disappearDistance = distance;
    }

    public OptimizerData(float distance)
    {
        objectsList = new List<GameObject>();
        disappearDistance = distance;
    }
}
