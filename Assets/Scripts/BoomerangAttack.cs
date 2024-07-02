using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangAttack : BasicAttack
{
    private BoomerangAttackSO boomerangAttackSO;
    [HideInInspector] public float knockback;

    protected override void Awake()
    {
        if (basicAttackSO != null)
        {
            boomerangAttackSO = (BoomerangAttackSO)basicAttackSO;
            uiObjects = boomerangAttackSO.uiObjects;
            knockback = boomerangAttackSO.knockback;
            base.Awake();
            powerUpgradeLevel = DeterminePlayerPower("boomerangDamage");
            cooldownLevel = DeterminePlayerCooldown("boomerangCooldown");
            transform.localPosition = Vector3.zero;
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    public GameObject Throw(Vector2 direction, Transform firePoint)
    {
        if (cooldownTimer > 0)
        {
            return null;
        }
        AudioManager.instance.PlayOneShot("ProjectileShoot");
        cooldownTimer = cooldown;
        Boomerang b = Instantiate(boomerangAttackSO.boomerang, firePoint.position, Quaternion.identity).GetComponent<Boomerang>();
        b.Initialize(boomerangAttackSO, this, direction, firePoint);
        FinishAttack();
        return b.gameObject;
    }

    protected override void Update()
    {
        base.Update();
    }
}
