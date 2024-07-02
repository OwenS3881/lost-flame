using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class ShockwaveAttack : BasicAttack
{
    private ShockwaveAttackSO shockwaveAttackSO;

    private float knockback;

    private GameObject shockwave = null;
    private bool isAttacking;

    protected override void Awake()
    {
        if (basicAttackSO != null)
        {
            shockwaveAttackSO = (ShockwaveAttackSO)basicAttackSO;
            knockback = shockwaveAttackSO.knockback;
            uiObjects = shockwaveAttackSO.uiObjects;
            base.Awake();
            powerUpgradeLevel = DeterminePlayerPower("shockwaveUpgrade");
            cooldownLevel = DeterminePlayerCooldown("shockwaveCooldown");
            transform.localPosition = Vector3.zero;
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    public void CreateShockwave(Vector2 firePoint)
    {
        if (isAttacking) return;

        isAttacking = true;

        shockwave = Instantiate(shockwaveAttackSO.shockwaveEffectPrefab, firePoint, Quaternion.identity);

        shockwave.GetComponent<ShockwaveAbility>().Initialize(power, level, attack, knockback, attacker);

        Collider2D[] characterColliders = GetComponentsInHierarchy<Collider2D>((attatchedCharacter as MonoBehaviour).transform);
        Collider2D shockwaveCollider = shockwave.GetComponentInChildren<Collider2D>();

        foreach (Collider2D c in characterColliders)
        {
            Physics2D.IgnoreCollision(c, shockwaveCollider);
        }

        cooldownTimer = cooldown;

        Invoke("Reset", 0.2f);
    }

    private void Reset()
    {
        if (shockwave == null)
        {
            isAttacking = false;
            FinishAttack();
        }
        else
        {
            Invoke("Reset", 0.01f);
        }
    }

    protected override void Update()
    {
        base.Update();
    }
}
