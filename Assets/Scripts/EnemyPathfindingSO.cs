using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyPathfindingSO : BasicEnemySO
{
    [Header("Enemy Pathfinding")]
    public float stopMovingDistance;
    public float playerNoticeDistance;
    public float lossInterestDistance;

    public float nextWaypointDistance;
    public float pathUpdateRate;

    public float idleSpeed;
    public float idleAnimSpeed;

    public float moveAnimSpeed;

    public GameObject noticeEffect;

    public LayerMask playerLayer;
    [Range(0, 360)]
    public float noticeRange;

    protected override void OnValidate()
    {
        base.OnValidate();
    }
}
