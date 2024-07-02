using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PlayerSO", menuName = "My Scriptable Objects/PlayerSO")]
public class PlayerSO : BasicCharacterSO
{
    [Header("IV's")]
    [Range(0, 15)]
    public int healthIV;
    [Range(0, 15)]
    public int attackIV;
    [Range(0, 15)]
    public int defenseIV;

    [Header("Skill Tree Stat Upgrades")]
    [Tooltip("Health, Attack, Defense")]
    public Vector3Int[] allSkillBoosts;
    public int[] healthSkillBoosts;
    public int[] attackSkillBoosts;
    public int[] defenseSkillBoosts;

    [Header("Skill Tree Experience Upgrades")]
    public float[] experienceMultiplierUpgrades;

    [Header("Skill Tree Passive Heal Upgrades")]
    public float defaultPassiveHealPercent;
    public float[] passiveHealUpgrades;

    [Header("Projectile Aim")]
    public GameObject projectileAimDot;
    public float distanceBetweenDots;
    public int numberOfDots;

    protected override void OnValidate()
    {
        base.OnValidate();
        characterType = CharacterType.Player;
    }

    public void ResetIVs()
    {
        healthIV = 0;
        attackIV = 0;
        defenseIV = 0;
    }
}