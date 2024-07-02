using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class EnemyBouncer : BasicEnemy
{
    private EnemyBouncerSO enemyBouncerSO;

    private Vector2 currentDirection;

    public override void Start()
    {
        if (enemySO != null)
        {
            enemyBouncerSO = (EnemyBouncerSO)enemySO;
            graphicsObject = enemyBouncerSO.graphicsObject;
            base.Start();
            anim.SetBool("isMoving", true);
            state = EnemyState.Moving;
            anim.speed = enemyBouncerSO.rotateSpeed;
            currentDirection = GetRandomDirection();
        }
        else
        {
            Invoke("Start", 0.01f);
        }
    }

    private void OnEnable()
    {
        if (anim != null)
        {
            anim.SetBool("isMoving", true);
        }
    }

    private Vector2 GetRandomDirection()
    {
        Vector2 dir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        return dir.normalized;
    }

    private Vector2 BounceDirection(Collision2D collision)
    {
        Quaternion deflectRotation = Quaternion.FromToRotation(-currentDirection, collision.GetContact(0).normal);
        Vector2 deflectDirection = deflectRotation * collision.GetContact(0).normal;
        return deflectDirection.normalized;
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        base.OnCollisionStay2D(collision);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        currentDirection = BounceDirection(collision);
        base.OnCollisionEnter2D(collision);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        rb.velocity = (currentDirection + directionToPlayer * enemyBouncerSO.playerBias).normalized * speed;
    }

    protected override void Update()
    {
        base.Update();
        state = EnemyState.Moving;
        anim.speed = enemyBouncerSO.rotateSpeed;
    }
}
