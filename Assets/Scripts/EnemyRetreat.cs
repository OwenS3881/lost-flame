using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRetreat : EnemyPathfinding
{
    private EnemyRetreatSO enemyRetreatSO;

    // Start is called before the first frame update
    public override void Start()
    {
        if (enemySO != null)
        {
            enemyRetreatSO = (EnemyRetreatSO)enemySO;
            graphicsObject = enemyRetreatSO.graphicsObject;
            base.Start();
        }
        else
        {
            Invoke("Start", 0.01f);
        }
    }

    private void Retreating()
    {
        anim.speed = enemyRetreatSO.retreatSpeed;
        if (rb.velocity.Equals(Vector2.zero))
        {
            rb.velocity = -directionToPlayer.normalized * enemyRetreatSO.retreatSpeed;
        }
        else if (rb.velocity.magnitude < speed)
        {
            rb.AddForce(-directionToPlayer.normalized * enemyRetreatSO.retreatSpeed * rb.mass, ForceMode2D.Impulse);
        }
    }

    private void StateLogic()
    {
        if (state == EnemyState.Idle)
        {
            Idle();
        }
        else if (state == EnemyState.Moving)
        {            
            if (distanceToPlayer >= enemyRetreatSO.lossInterestDistance)
            {
                state = EnemyState.Idle;
                return;
            }
            else if (distanceToPlayer >= enemyRetreatSO.stopMovingDistance)
            {
                Pathfinding();
            }
            else if (distanceToPlayer <= enemyRetreatSO.retreatDistance)
            {
                state = EnemyState.Retreating;
                return;
            }
            else
            {
                StopMoving();
            }
            if (attackObjectType != AttackType.Collision) Attack();
        }
        else if (state == EnemyState.Attacking)
        {
            if (GetComponentInChildren<BasicAttack>().cooldownTimer > 0 && !(GetComponentInChildren<BasicAttack>() is ShockwaveAttack))
            {
                state = EnemyState.Moving;
            }
        }
        else if (state == EnemyState.Retreating)
        {
            Retreating();
            if (distanceToPlayer >= enemyRetreatSO.stopMovingDistance)
            {
                state = EnemyState.Moving;
            }
        }
    }

    public override void OnAttacked(BasicAttack attack)
    {
        base.OnAttacked(attack);
    }

    public override void OnAttacked(int attackerLevel, int power, int attackerAttack, CharacterType attacker)
    {
        base.OnAttacked(attackerLevel, power, attackerAttack, attacker);
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        StateLogic();
    }

    protected override void Update()
    {
        base.Update();
    }
}
