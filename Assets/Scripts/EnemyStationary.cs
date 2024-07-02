using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class EnemyStationary : BasicEnemy
{
    private EnemyStationarySO enemyStationarySO;

    [SerializeField] bool showLineOfSight;

    private GameObject head;

    public override void Start()
    {
        if (enemySO != null)
        {
            enemyStationarySO = (EnemyStationarySO)enemySO;
            graphicsObject = enemyStationarySO.graphicsObject;

            base.Start();

            FindHead();
        }
        else
        {
            Invoke("Start", 0.01f);
        }
    }

    private bool FindHead()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("EnemyHead"))
            {
                head = children[i].gameObject;
                return true;
            }
        }
        return false;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        base.OnCollisionStay2D(collision);
    }

    public override void OnAttacked(BasicAttack attack)
    {
        base.OnAttacked(attack);
        if (state == EnemyState.Idle)
        {
            Notice();
        }
    }

    public override void OnAttacked(int attackerLevel, int power, int attackerAttack, CharacterType attacker)
    {
        base.OnAttacked(attackerLevel, power, attackerAttack, attacker);
        if (state == EnemyState.Idle)
        {
            Notice();
        }
    }

    public override void Die()
    {
        base.Die();
    }

    private void Idle()
    {
        Vector2 sightDir = Quaternion.AngleAxis(UnityEngine.Random.Range(-enemyStationarySO.noticeRange, enemyStationarySO.noticeRange), Vector3.forward) * Vector2.up;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, sightDir, enemyStationarySO.playerNoticeDistance, enemyStationarySO.playerLayer);

        if (showLineOfSight)
        {
            Debug.DrawLine(transform.position, (Vector2)transform.position + sightDir * enemyStationarySO.playerNoticeDistance, Color.red, 1);
        }

        if (hit.collider != null)
        {
            Notice();
        }

        anim.SetBool("isMoving", false);
    }

    private void Notice()
    {
        GameObject ne = Instantiate(enemyStationarySO.noticeEffect, transform);
        ne.transform.position = noticeEffectSpawnPoint.position;
        state = EnemyState.Moving;
    }

    private void Moving()
    {
        anim.SetBool("isMoving", true);
        if (head != null)
        {
            head.transform.right = shootDirection;
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
            Moving();
            Attack();
            if (distanceToPlayer >= enemyStationarySO.lossInterestDistance)
            {
                state = EnemyState.Idle;
            }
        }
        else if (state == EnemyState.Attacking)
        {
            if (GetComponentInChildren<BasicAttack>().cooldownTimer > 0)
            {
                state = EnemyState.Moving;
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        StateLogic();

        if (head != null)
        {
            SetGlobalScale(head.transform, new Vector3(Mathf.Abs(head.transform.lossyScale.x), head.transform.lossyScale.y, 1));
        }
    }

    protected override void Update()
    {
        base.Update();
    }
}
