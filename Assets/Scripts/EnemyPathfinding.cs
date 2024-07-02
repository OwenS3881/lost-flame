using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using static MyFunctions;

public abstract class EnemyPathfinding : BasicEnemy
{
    private EnemyPathfindingSO enemyPathfindingSO;

    [SerializeField] bool showLineOfSight;

    protected float nextWaypointDistance;

    protected Path path;
    protected int currentWaypoint = 0;
    protected bool reachedEndOfPath = false;

    protected Seeker seeker;

    protected Vector2 nextIdlePoint;

    private int idleStopFrames;

    private int rightFrames, leftFrames;

    public override void Start()
    {
        base.Start();
        enemyPathfindingSO = (EnemyPathfindingSO)enemySO;
        nextWaypointDistance = enemyPathfindingSO.nextWaypointDistance;
        seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath", 0f, enemyPathfindingSO.pathUpdateRate);
        nextIdlePoint = Vector2.zero;
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

    protected void UpdatePath()
    {
        if (!seeker.IsDone() || state != EnemyState.Moving) return;

        seeker.StartPath(transform.position, playerCenter.transform.position, OnPathComplete);
    }

    protected void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    protected void Pathfinding()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }


        anim.SetBool("isMoving", true);
        anim.speed = enemyPathfindingSO.moveAnimSpeed;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        if (rb.velocity.Equals(Vector2.zero))
        {
            rb.velocity = direction.normalized * speed;
        }
        else if (rb.velocity.magnitude < speed)
        {
            rb.AddForce(direction.normalized * speed * rb.mass, ForceMode2D.Impulse);
        }

        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    protected void Idle()
    {
        if (nextIdlePoint.Equals(Vector2.zero) || idleStopFrames > 20)
        {
            nextIdlePoint = PickIdlePoint();
        }
        else
        {
            Vector2 dir = (nextIdlePoint - (Vector2)transform.position).normalized;
            rb.velocity = dir * enemyPathfindingSO.idleSpeed;
            anim.SetBool("isMoving", true);
            anim.speed = enemyPathfindingSO.idleAnimSpeed;

            Vector2 sightDir = Quaternion.AngleAxis(UnityEngine.Random.Range(-enemyPathfindingSO.noticeRange, enemyPathfindingSO.noticeRange), Vector3.forward) * dir;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, sightDir, enemyPathfindingSO.playerNoticeDistance, enemyPathfindingSO.playerLayer);

            if (showLineOfSight)
            {
                Debug.DrawLine(transform.position, (Vector2)transform.position + sightDir * enemyPathfindingSO.playerNoticeDistance, Color.red, 1);
            }

            if (hit.collider != null)
            {
                Notice();
            }

            if (Vector2.Distance(nextIdlePoint, transform.position) < 0.05f)
            {
                nextIdlePoint = Vector2.zero;
            }
        }
    }


    protected void Notice()
    {
        GameObject ne = Instantiate(enemyPathfindingSO.noticeEffect, transform);
        ne.transform.position = noticeEffectSpawnPoint.position;
        state = EnemyState.Moving;
    }

    protected Vector2 PickIdlePoint()
    {
        Vector2 point = new Vector2(transform.position.x + UnityEngine.Random.Range(-2f, 2f), transform.position.y + UnityEngine.Random.Range(-2f, 2f));
        
        if (IsPointInPathfindingGrid(point))
        {
            return point;
        }
        else
        {
            return Vector2.zero;
        }
    }

    [ContextMenu("Reset Idle")]
    protected void ResetIdle()
    {
        nextIdlePoint = Vector2.zero;
    }

    void LookLogic()
    {
        if (rb.velocity.x > 0)
        {
            rightFrames++;
            leftFrames = 0;
        }
        else if (rb.velocity.x < 0)
        {
            leftFrames++;
            rightFrames = 0;
        }
        else //Not moving?, look at player
        {
            if (directionToPlayer.x >= 0)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }
        }

        if (rightFrames > 2)
        {
            SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
        }
        else if (leftFrames > 2)
        {
            SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();    
    }

    

    protected override void Update()
    {
        base.Update();

        if (state != EnemyState.Attacking && !anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            LookLogic();
        }

        if (state == EnemyState.Idle && rb.velocity.Equals(Vector2.zero))
        {
            idleStopFrames++;
        }
        else
        {
            idleStopFrames = 0;
        }
    }
}
