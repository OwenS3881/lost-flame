using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiProjectileAttack : ProjectileAttack
{
    private MultiProjectileAttackSO multiProjectileAttackSO;

    protected override void Awake()
    {
        if (basicAttackSO != null)
        {
            multiProjectileAttackSO = (MultiProjectileAttackSO)basicAttackSO;
            base.Awake();
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    public GameObject[] ShootMulti(Vector2[] directions, Transform[] firePoints)
    {
        if (cooldownTimer > 0)
        {
            return null;
        }
        cooldownTimer = cooldown;

        GameObject[] projectiles = new GameObject[firePoints.Length];

        for (int i = 0; i < projectiles.Length; i++)
        {
            Projectile p = Instantiate(multiProjectileAttackSO.projectile, firePoints[i].position, Quaternion.identity).GetComponent<Projectile>();
            p.Initialize(multiProjectileAttackSO, this, directions[i]);

            projectiles[i] = p.gameObject;
        }
        FinishAttack();
        return projectiles;
    }

    protected override void Update()
    {
        base.Update();
    }
}
