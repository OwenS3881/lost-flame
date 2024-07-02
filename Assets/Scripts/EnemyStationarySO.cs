using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyStationarySO", menuName = "My Scriptable Objects/EnemyStationarySO")]
public class EnemyStationarySO : BasicEnemySO
{
    [Header("Enemy Stationary")]
    public GameObject noticeEffect;

    public float playerNoticeDistance;
    public float lossInterestDistance;
    public LayerMask playerLayer;
    [Range(0, 360)]
    public float noticeRange;

    protected override void OnValidate()
    {
        if (speed != 0)
        {
            speed = 0;
            Debug.Log("Enemy **STATIONARY** SO... unbelievable (Don't worry, I fixed it)");
        }
        base.OnValidate();
    }
}
