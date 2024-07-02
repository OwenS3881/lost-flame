using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class CollisionAttack : BasicAttack
{
    private CollisionAttackSO collisionAttackSO;

    protected override void Awake()
    {
        if (basicAttackSO != null)
        {
            collisionAttackSO = (CollisionAttackSO)basicAttackSO;
            base.Awake();
            powerUpgradeLevel = DeterminePlayerPower("collisionDamage");
            cooldownLevel = DeterminePlayerCooldown("collisionCooldown");
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    public void OnCollide(Collision2D collision)
    {
        if (cooldownTimer > 0) return;

        GetComponentInHierarchy<IAttackable>(collision.transform, true).OnAttacked(this);

        Rigidbody2D collisionRb = GetComponentInHierarchy<Rigidbody2D>(collision.transform, true);

        collisionRb.AddForce(-collision.GetContact(0).normal * collisionAttackSO.knockbackForce * collisionRb.mass, ForceMode2D.Impulse);
        
        FinishAttack();
        cooldownTimer = cooldown;
    }

    protected override void Update()
    {
        base.Update();
    }
}
