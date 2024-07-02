using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyBouncerSO", menuName = "My Scriptable Objects/EnemyBouncerSO")]
public class EnemyBouncerSO : BasicEnemySO
{
    [Header("Enemy Bouncer")]
    public float rotateSpeed;
    public float playerBias;

    protected override void OnValidate()
    {
        base.OnValidate();
    }
}
