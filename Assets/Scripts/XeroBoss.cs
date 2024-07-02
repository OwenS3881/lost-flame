using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class XeroBoss : BasicBoss
{
    private float currentMoveTime;
    private float maxMoveTime;
    private Vector2 direction1;
    private Vector2 direction2;

    [Header("Xero Fields")]
    [SerializeField] private Vector2 randomMoveTimeRange;
    private XeroSword[] swords;
    private Transform[] defaultSwordPoints;
    private bool isAttacking;

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    protected override void Start()
    {
        base.Start();
        direction1 = GetRandomDirection();
        direction2 = GetRandomDirection();
        swords = GetComponentsInChildren<XeroSword>();
        ChangeSwordsAttackSO(defaultCollision.attackSO as CollisionAttackSO);

        defaultSwordPoints = new Transform[swords.Length];

        int cur = 0;
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.CompareTag("XeroSwordDefaultPoint"))
            {
                defaultSwordPoints[cur] = t;
                cur++;
            }
        }

        for (int i = 0; i < swords.Length; i++)
        {
            swords[i].Initialize(this, defaultSwordPoints[i], i);
        }
    }

    private void ChangeSwordsAttackSO(CollisionAttackSO so)
    {
        if (so == null)
        {
            Debug.LogError("Failed to change Xero Swords Attack SO due to the so being null");
            return;
        }

        for (int i = 0; i < swords.Length; i++)
        {
            swords[i].SetAttackSO(so);
        }
    }


    protected override void StateLogic()
    {
        if (dead) return;

        if (state == EnemyState.Moving)
        {
            if (distanceToPlayer < tooFarFromPlayerDistance)
            {
                MoveRandom();
            }
            else
            {
                HeadToPlayer();
            }

            if (currentCooldown <= 0 && distanceToPlayer < tooFarFromPlayerDistance && !isAttacking)
            {
                Attack(GetRandomAnimation());
            }
        }
    }

    private void HeadToPlayer()
    {
        currentMoveTime = maxMoveTime + 1;

        Vector2 dir = (Player.instance.transform.position - firePoint.position).normalized;

        rb.AddForce(dir.normalized * speed * rb.mass, ForceMode2D.Force);
    }

    private void MoveRandom()
    {
        anim.SetBool("isMoving", true);

        if (currentMoveTime >= maxMoveTime || rb.velocity.magnitude < 0.01f)
        {
            currentMoveTime = 0f;
            maxMoveTime = UnityEngine.Random.Range(randomMoveTimeRange.x, randomMoveTimeRange.y);
            direction1 = direction2;
            direction2 = GetRandomDirection();
        }

        Vector2 newDir = Vector2.Lerp(direction1, direction2, currentMoveTime / maxMoveTime).normalized;

        rb.AddForce(newDir.normalized * speed * rb.mass, ForceMode2D.Force);

        currentMoveTime += Time.deltaTime;
    }

    private Vector2 GetRandomDirection()
    {
        return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
    }


    public override void Attack(string name)
    {
        isAttacking = true;
        foreach (BossAttackData ba in attacks)
        {
            if (ba.attackName.Equals(name))
            {
                SendAttack(ba);
                return;
            }
        }
    }

    private void SendAttack(BossAttackData ba)
    {
        ChangeSwordsAttackSO(ba.attackSO as CollisionAttackSO);
        for (int i = 0; i < swords.Length; i++)
        {
            swords[i].Attack(ba.attackName, ba.attackSO.attackStartDelay);
        }
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        base.OnCollisionStay2D(collision);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        //base.OnCollisionEnter2D(collision);
    }

    protected override void Die()
    {
        if (dead) return;

        dead = true;
        state = EnemyState.Idle;
        AudioManager.instance.StopAllSongs();

        AudioManager.instance.Play("BossDeath");

        anim.Play("XeroBoss-die");
    }

    protected override void DestroySelf()
    {
        XeroBossSceneManager.instance.FightFinished();

        base.DestroySelf();
    }

    public void UpdateIsAttacking()
    {
        for (int i = 0; i < swords.Length; i++)
        {
            if (swords[i].IsAttacking)
            {
                isAttacking = true;
                return;
            }
        }
        isAttacking = false;
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
