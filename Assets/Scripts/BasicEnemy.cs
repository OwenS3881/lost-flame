using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;
using Pathfinding;

public abstract class BasicEnemy : BasicCharacter
{
    protected Player player;
    protected Transform playerCenter;
    protected float distanceToPlayer;
    protected Vector2 directionToPlayer;
    protected Vector2 shootDirection;

    [ReadOnly] public BasicEnemySO enemySO;

    [ReadOnly] public EnemyState state = EnemyState.Idle;

    private Vector2 dashDirection;

    protected Transform firePoint;
    protected List<Transform> multiFirePoints = new List<Transform>();
    protected Transform noticeEffectSpawnPoint;

    public static List<BasicEnemy> enemies = new List<BasicEnemy>();

    public override void Start()
    {
        basicCharacterSO = enemySO;
        base.Start();
        player = Player.instance;
        playerCenter = player.firePoint;
        enemies.Add(this);
        OptimizerSingleton.instance.AddToDictionary("Enemies", gameObject);

        foreach (Transform t in GetComponentsInHierarchy<Transform>(transform))
        {
            if (t.CompareTag("NoticeEffectSpawnPoint"))
            {
                noticeEffectSpawnPoint = t;
            }
            else if (t.CompareTag("FirePoint"))
            {
                firePoint = t;
            }
            else if (t.CompareTag("MultiFirePoint"))
            {
                multiFirePoints.Add(t);
            }
        }
    }

    public void FinishAttack()
    {
        if (state == EnemyState.Attacking)
        {
            state = EnemyState.Moving;
            anim.SetTrigger("attackFinished");
        }
    }

    protected void StopMoving()
    {
        if (anim.GetBool("isMoving"))
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("isMoving", false);
            anim.speed = 1;
        }
    }

    public void AssignSO(BasicEnemySO so)
    {
        enemySO = so;
    }

    public override void Attack()
    {
        if (GetComponentInChildren<BasicAttack>().cooldownTimer > 0)
        {
            return;
        }
        state = EnemyState.Attacking;
        Invoke("PlayAttackAnimation", enemySO.attackSO.attackStartDelay);
        anim.speed = 1;
        if (attackObjectType == AttackType.Projectile)
        {
            StartCoroutine(Projectile(enemySO.attackSO.attackStartDelay, shootDirection));
        }
        else if (attackObjectType == AttackType.Dash)
        {
            StopMoving();
            if (directionToPlayer.x >= 0)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }
            dashDirection = shootDirection;
            Invoke("Dash", enemySO.attackSO.attackStartDelay);
        }
        else if (attackObjectType == AttackType.Collision)
        {
            state = EnemyState.Moving;

            if (collisionAttackTarget == null) return;

            GetComponentInChildren<CollisionAttack>().OnCollide(collisionAttackTarget);
        }
        else if (attackObjectType == AttackType.Melee)
        {
            GetComponentInChildren<MeleeAttack>().Slash();
        }
        else if (attackObjectType == AttackType.MultiProjectile)
        {
            StartCoroutine(MultiProjectile(enemySO.attackSO.attackStartDelay));
        }
        else if (attackObjectType == AttackType.Shockwave)
        {
            Invoke("Shockwave", enemySO.attackSO.attackStartDelay);
        }
        else if (attackObjectType == AttackType.Boomerang)
        {
            throw new System.NotImplementedException("Ha Ha, I was lazy and didnt put in boomerang attack for enemies");
        }
    }

    void PlayAttackAnimation()
    {
        anim.Play("Base Layer.Attack");
    }

    void Dash()
    {
        GetComponentInChildren<DashAttack>().Dash(rb, dashDirection);
    }

    void Shockwave()
    {
        GetComponentInChildren<ShockwaveAttack>().CreateShockwave(firePoint.position);
    }

    IEnumerator MultiProjectile(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector2[] dirs = new Vector2[multiFirePoints.Count];

        for (int i = 0; i < multiFirePoints.Count; i++)
        {
            dirs[i] = (multiFirePoints[i].position - firePoint.position).normalized;
        }

        GameObject[] projectiles = GetComponentInChildren<MultiProjectileAttack>().ShootMulti(dirs, multiFirePoints.ToArray());

        if (projectiles == null) yield break;

        Collider2D[] myColliders = GetComponentsInHierarchy<Collider2D>(transform);

        for (int i = 0; i < projectiles.Length; i++)
        {
            if (projectiles[i] == null)
            {
                continue;
            }

            Collider2D[] attackColliders = projectiles[i].GetComponentsInChildren<Collider2D>();
            if (attackColliders.Length == 0)
            {
                yield return new WaitForSeconds(0.01f);
                i--;
                continue;
            }
            
            foreach (Collider2D myCollider in myColliders)
            {
                foreach (Collider2D c in attackColliders)
                {
                    Physics2D.IgnoreCollision(c, myCollider);
                    c.isTrigger = false;
                }
            }
        }
    }

    IEnumerator Projectile(float delay, Vector2 dir)
    {
        yield return new WaitForSeconds(delay);
        GameObject a = GetComponentInChildren<ProjectileAttack>().Shoot(dir.normalized, firePoint.position);
        if (a != null)
        {
            rb.AddForce(-dir.normalized * GetComponentInChildren<ProjectileAttack>().knockback * rb.mass, ForceMode2D.Impulse);

            if (dir.x > 0)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else if (dir.x < 0)
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }

            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                if (a == null)
                {
                    continue;
                }
                Collider2D[] attackColliders = a.GetComponentsInChildren<Collider2D>();
                if (attackColliders.Length == 0)
                {
                    yield return new WaitForSeconds(0.01f);
                    continue;
                }
                Collider2D[] myColliders = GetComponentsInHierarchy<Collider2D>(transform);
                foreach (Collider2D myCollider in myColliders)
                {
                    foreach (Collider2D c in attackColliders)
                    {
                        Physics2D.IgnoreCollision(c, myCollider);
                        c.isTrigger = false;
                    }
                }

                break;
            }
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if (attackObjectType == AttackType.Collision && collision.gameObject.CompareTag("Player"))
        {
            Attack();
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (attackObjectType == AttackType.Dash && GetComponentInChildren<DashAttack>().isDashing)
        {
            GetComponentInChildren<DashAttack>().DashCollision(collision);
        }
    }

    private void GenerateExperienceParticles()
    {
        int numberOfParticles = UnityEngine.Random.Range(enemySO.numberOfExperienceParticles.x, enemySO.numberOfExperienceParticles.y + 1);

        /*
        int totalExperience = 2*level + 10;
        totalExperience /= level + player.level + 10;
        totalExperience = (int)Mathf.Pow(totalExperience, 2.5f);
        totalExperience *= (enemySO.baseExperienceYield * level) / 5;
        */

        int totalExperience = enemySO.baseExperienceYield;

        int[] experienceValues = new int[numberOfParticles];
        int actualTotal = 0;
        for (int i = 0; i < experienceValues.Length; i++)
        {
            experienceValues[i] = totalExperience / numberOfParticles;
            actualTotal += experienceValues[i];
            if (experienceValues[i] < 1)
            {
                experienceValues[i] = 1;
            }
        }

        if (actualTotal != totalExperience)
        {
            experienceValues[0] += totalExperience - actualTotal;
        }

        bool foundSpot = false;
        Vector2 spawnPos = Vector2.zero;
        float spawnRange = 0f;
        int attempts = 0;
        while (!foundSpot)
        {
            spawnPos = new Vector2(transform.position.x + UnityEngine.Random.Range(-spawnRange, spawnRange), transform.position.y + UnityEngine.Random.Range(-spawnRange, spawnRange));
            if(IsPointInPathfindingGrid(spawnPos))
            {
                foundSpot = true;
            }
            else if (spawnRange == 0 || attempts > 10)
            {
                spawnRange += 0.01f;
            }
            else
            {
                attempts++;
            }
        }

        foreach (int value in experienceValues)
        {       
            GameObject p = Instantiate(enemySO.experienceParticle, spawnPos, Quaternion.identity);
            p.GetComponent<ExperienceParticle>().experiencePoints = value;
            Vector2 dir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            float speed = UnityEngine.Random.Range(1f, 5f);

            Rigidbody2D pRb = p.GetComponent<Rigidbody2D>();
            pRb.AddForce(dir * speed * pRb.mass, ForceMode2D.Impulse);
        }

        CreateBuff(spawnPos);
    }

    private void CreateBuff(Vector2 spawnPos)
    {
        GameObject statPickup = enemySO.GetStatPickup();

        if (statPickup == null) return;

        for (int i = 0; i < enemySO.maxStatPickups; i++)
        {
            if (UnityEngine.Random.Range(0f, 100f) >= enemySO.statPickupProbability) continue;

            GameObject sp = Instantiate(statPickup, spawnPos, Quaternion.identity);
            Vector2 dir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            float speed = UnityEngine.Random.Range(1f, 5f);

            Rigidbody2D spRb = sp.GetComponent<Rigidbody2D>();
            spRb.AddForce(dir * speed * spRb.mass, ForceMode2D.Impulse);
        }
    }

    public override void Die()
    {
        if (enemySO.deathEffect != null)
        {
            GameObject de = Instantiate(enemySO.deathEffect, transform.position, Quaternion.identity);
            if (enemySO.flipDeathEffect)
            {
                if (transform.localScale.x < 0)
                {
                    SetScale(de.transform, 0, Mathf.Abs(de.transform.localScale.x));
                }
                else
                {
                    SetScale(de.transform, 0, -Mathf.Abs(de.transform.localScale.x));
                }
            }
        }
        EndDungeonManager.instance.AddDungeonRankStat("Enemies Killed", 1);
        GenerateExperienceParticles();
        base.Die();
    }

    protected bool IsPointInPathfindingGrid(Vector2 point)
    {
        if (AstarPath.active.data.gridGraph.CountNodes() <= 1) return false;
        GraphNode nearestNode = AstarPath.active.GetNearest(point, NNConstraint.None).node;
        return nearestNode.Walkable;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        directionToPlayer = (playerCenter.position - transform.position).normalized;
        shootDirection = (playerCenter.position - firePoint.position).normalized;
        distanceToPlayer = Vector2.Distance(playerCenter.position, transform.position);
    }

    protected override void Update()
    {
        base.Update();
    }
}
