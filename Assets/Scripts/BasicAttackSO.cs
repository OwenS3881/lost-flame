using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicAttackSO : ScriptableObject
{
    public int power;
    public int[] playerPowerUpgrades;
    public CharacterType attacker;
    public GameObject graphicsObject;
    public AttackType attackType;
    public GameObject[] uiObjects;
    public BasicAttack attackObject;
    public float cooldown;
    public float[] playerCooldownUpgrades;
    public float attackStartDelay;

    public abstract GameObject[] CreateUI(GameObject canvas);
}
