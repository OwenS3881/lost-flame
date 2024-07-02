using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public abstract class BasicAttack : MonoBehaviour
{
    [ReadOnly] public int power;
    [ReadOnly, SerializeField] protected int powerUpgradeLevel;
    [ReadOnly] public int attack;
    [ReadOnly] public int level;
    [HideInInspector] public CharacterType attacker;
    protected GameObject graphicsObject;
    protected GameObject instantiatedGraphicsObject;
    [HideInInspector] public AttackType attackType;
    [HideInInspector] protected GameObject[] uiObjects;
    [ReadOnly] public float cooldownTimer;
    [ReadOnly] public BasicAttackSO basicAttackSO;
    protected ICanAttack attatchedCharacter;
    [SerializeField, ReadOnly] protected float cooldown;
    [ReadOnly, SerializeField] protected int cooldownLevel;

    protected virtual void Awake()
    {
        attatchedCharacter = GetComponentInHierarchy<ICanAttack>(transform);
        power = basicAttackSO.power;
        attack = attatchedCharacter.GetAttack();
        level = attatchedCharacter.GetLevel();
        attacker = basicAttackSO.attacker;

        if (basicAttackSO.graphicsObject != null) graphicsObject = basicAttackSO.graphicsObject;

        attackType = basicAttackSO.attackType;
        uiObjects = basicAttackSO.uiObjects;
        cooldown = basicAttackSO.cooldown;
        if (graphicsObject != null && attackType != AttackType.Projectile && attackType != AttackType.Dash && attackType != AttackType.Collision && attackType != AttackType.MultiProjectile && attackType != AttackType.Boomerang)
        {
            instantiatedGraphicsObject = Instantiate(graphicsObject, transform);
        }
    }

    protected int DeterminePlayerPower(string skill)
    {
        if (attacker != CharacterType.Player) return -1;

        int pLevel = 0;

        for (int i = 0; i < basicAttackSO.playerPowerUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased(skill + (i + 1)))
            {
                power += basicAttackSO.playerPowerUpgrades[i];
                pLevel++;
            }
            else
            {
                return pLevel;
            }
        }
        return pLevel;
    }

    protected int DeterminePlayerCooldown(string skill)
    {
        if (attacker != CharacterType.Player) return -1;

        int pLevel = 0;

        for (int i = 0; i < basicAttackSO.playerCooldownUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased(skill + (i + 1)))
            {
                cooldown -= basicAttackSO.playerCooldownUpgrades[i];
                Mathf.Clamp(cooldown, 0, Mathf.Infinity);
                pLevel++;
            }
            else
            {
                return pLevel;
            }
        }
        return pLevel;
    }


    protected void FinishAttack()
    {
        BasicEnemy enemy = GetComponentInHierarchy<BasicEnemy>(transform);
        if (enemy != null)
        {
            enemy.FinishAttack();
        }
    }

    protected virtual void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            cooldownTimer = 0;
        }
    }
}
