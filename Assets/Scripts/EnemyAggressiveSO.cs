using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyAggressiveSO", menuName = "My Scriptable Objects/EnemyAggressiveSO")]
public class EnemyAggressiveSO : EnemyPathfindingSO
{
    protected override void OnValidate()
    {
        base.OnValidate();
    }
}
