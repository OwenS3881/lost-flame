using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class Projectile : MonoBehaviour
{
    public ProjectileAttackSO projectileAttackSO;
    [HideInInspector] public ProjectileAttack basicAttack;
    [HideInInspector] public Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (projectileAttackSO != null)
        {
            Instantiate(projectileAttackSO.graphicsObject, transform);
            rb = GetComponent<Rigidbody2D>();

            if (projectileAttackSO.attacker == CharacterType.Player)
            {
                Invoke("Collided", projectileAttackSO.playerProjectileDestroyTime);
            }

        }
        else
        {
            Invoke("Awake", 0.01f);
        }       
    }

    public void Initialize(ProjectileAttackSO so, ProjectileAttack parent, Vector2 dir)
    {
        projectileAttackSO = so;
        basicAttack = parent;
        direction = dir;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true) != null)
        {
            IAttackable character = GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true);
            if (character.characterType == projectileAttackSO.attacker)
            {
                Collided();
                return;
            }
            character.OnAttacked(basicAttack);
            Collided();
        }
        else
        {
            Collided();
        }
    }

    private void Collided()
    {
        if (projectileAttackSO.deathEffect != null)
        {
            Instantiate(projectileAttackSO.deathEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        transform.right = direction;
        if (rb != null)
        {
            rb.velocity = direction.normalized * projectileAttackSO.speed;
        }
        else
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }
}
