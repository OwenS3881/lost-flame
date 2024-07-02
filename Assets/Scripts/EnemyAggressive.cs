using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using static MyFunctions;

public class EnemyAggressive : EnemyPathfinding
{
    private EnemyAggressiveSO enemyAggressiveSO;

    // Start is called before the first frame update
    public override void Start()
    {
        if (enemySO != null)
        {
            enemyAggressiveSO = (EnemyAggressiveSO)enemySO;
            graphicsObject = enemyAggressiveSO.graphicsObject;
            base.Start();
        }
        else
        {
            Invoke("Start", 0.01f);
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
            if (distanceToPlayer >= enemyAggressiveSO.lossInterestDistance)
            {
                state = EnemyState.Idle;
            }
            else if (distanceToPlayer >= enemyAggressiveSO.stopMovingDistance)
            {
                Pathfinding();             
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
