using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossAttackData
{
    public string attackName;
    public BasicAttackSO attackSO;
    [Range(0, 100)]
    public int probability;
    [HideInInspector] public BasicAttack attackObject;
    [HideInInspector] public AttackType type;
}
