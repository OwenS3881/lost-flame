using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    public bool top, bottom, left, right;
    [Space]
    public bool isStart, isEnd;
    public bool containsTreasure;
    public Vector2Int coordinates;
}
