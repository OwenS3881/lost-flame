using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAttack : BasicAttack
{
    private ProjectileAttackSO projectileAttackSO;
    //[HideInInspector]public Vector2 direction;
    [HideInInspector] public float knockback;
    protected float speed;

    protected override void Awake()
    {
        if (basicAttackSO != null)
        {
            projectileAttackSO = (ProjectileAttackSO)basicAttackSO;
            knockback = projectileAttackSO.knockback;
            uiObjects = projectileAttackSO.uiObjects;
            base.Awake();
            powerUpgradeLevel = DeterminePlayerPower("projectileDamage");
            cooldownLevel = DeterminePlayerCooldown("projectileCooldown");
            transform.localPosition = Vector3.zero;
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    public GameObject Shoot(Vector2 direction, Vector2 firePoint)
    {
        if (cooldownTimer > 0)
        {
            return null;
        }
        AudioManager.instance.PlayOneShot("ProjectileShoot");
        cooldownTimer = cooldown;
        Projectile p = Instantiate(projectileAttackSO.projectile, firePoint, Quaternion.identity).GetComponent<Projectile>();
        p.Initialize(projectileAttackSO, this, direction);
        FinishAttack();
        return p.gameObject;
    }

    protected override void Update()
    {
        base.Update();
    }


}
