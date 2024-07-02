using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class SpikeBoss : BasicBoss
{
    protected override void OnValidate()
    {
        base.OnValidate();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void StateLogic()
    {
        if (state == EnemyState.Moving)
        {
            if (distanceToPlayer >= stopMovingDistance)
            {
                Pathfinding();
            }
            else
            {
                StopMoving();
            }

            if (currentCooldown <= 0 && distanceToPlayer < tooFarFromPlayerDistance)
            {
                PlayAttackAnimation(GetRandomAnimation());
            }
        }
    }

    public override void Attack(string name)
    {
        currentAttack = GetAttackFromName(name);
        AttackType currentType = currentAttack.type;

        collisionAttackActive = false;

        if (currentType == AttackType.Projectile)
        {
            ProjectileAttack pa = currentAttack.attackObject as ProjectileAttack;
            StartCoroutine(Projectile(shootDirection, pa));
        }
        else if (currentType == AttackType.Dash)
        {
            if (directionToPlayer.x >= 0)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }
            StopMoving();

            dashDirection = directionToPlayer;
            Invoke("Dash", currentAttack.attackSO.attackStartDelay);
        }       
        else if (currentType == AttackType.Collision)
        {
            StartCoroutine(CollisionAttackCoroutine());
        }
        else if (currentType == AttackType.Melee)
        {
            MeleeAttack ma = currentAttack.attackObject as MeleeAttack;
            ma.Slash();
        }
        else if (currentType == AttackType.MultiProjectile)
        {
            MultiProjectileAttack ma = currentAttack.attackObject as MultiProjectileAttack;
            StartCoroutine(MultiProjectile(ma, true));
        }
        else if (currentType == AttackType.Shockwave)
        {
            Invoke("Shockwave", currentAttack.attackSO.attackStartDelay);
        }
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        base.OnCollisionStay2D(collision);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
    }

    protected override void Die()
    {
        if (dead) return;

        dead = true;
        state = EnemyState.Idle;
        AudioManager.instance.StopAllSongs();

        AudioManager.instance.Play("BossDeath");

        anim.Play("SpikeBoss-die");
    }

    protected override void DestroySelf()
    {
        SpikeBossSceneManager.instance.FightFinished();

        base.DestroySelf();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
