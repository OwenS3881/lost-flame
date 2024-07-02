using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyRetreatSO", menuName = "My Scriptable Objects/EnemyRetreatSO")]
public class EnemyRetreatSO : EnemyPathfindingSO
{
    [Header("Enemy Retreat")]
    public float retreatDistance;
    public float retreatSpeed;

    protected override void OnValidate()
    {
        base.OnValidate();
    }
}
