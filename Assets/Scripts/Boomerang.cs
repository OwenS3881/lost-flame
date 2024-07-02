using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class Boomerang : MonoBehaviour
{
    public BoomerangAttackSO boomerangAttackSO;
    [HideInInspector] public BasicAttack basicAttack;
    [ReadOnly] public Vector2 direction;
    private Rigidbody2D rb;
    private Collider2D[] myColliders;
    private float currentTime;
    private Transform firePoint;
    private float currentShrinkTime;
    private Vector2 defaultSize;
    private bool isShrinking;

    private void Awake()
    {
        if (boomerangAttackSO != null)
        {
            Instantiate(boomerangAttackSO.graphicsObject, transform);
            rb = GetComponent<Rigidbody2D>();
            myColliders = GetComponentsInHierarchy<Collider2D>(transform);
            defaultSize = transform.localScale;
            rb.AddForce(direction.normalized * boomerangAttackSO.speed * rb.mass, ForceMode2D.Impulse);
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    public void Initialize(BoomerangAttackSO so, BoomerangAttack parent, Vector2 dir, Transform point)
    {
        boomerangAttackSO = so;
        basicAttack = parent;
        direction = dir.normalized;
        firePoint = point;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true) != null)
        {
            IAttackable character = GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true);
            if (character.characterType == boomerangAttackSO.attacker)
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

    private void IgnoreCollision(GameObject g)
    {
        Collider2D[] otherColliders = GetComponentsInHierarchy<Collider2D>(g.transform);
        foreach (Collider2D myC in myColliders)
        {
            foreach (Collider2D gC in otherColliders)
            {
                Physics2D.IgnoreCollision(myC, gC);
            }
        }
    }

    private void Collided()
    {
        if (currentTime < boomerangAttackSO.returnTime) return;

        currentTime = boomerangAttackSO.returnTime;
    }

    private void Shrink()
    {
        if (!isShrinking) return;

        transform.localScale = Vector2.Lerp(defaultSize, Vector2.zero, currentShrinkTime / boomerangAttackSO.shrinkTime);
        currentShrinkTime += Time.deltaTime;

        if (currentShrinkTime > boomerangAttackSO.shrinkTime)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (rb == null || boomerangAttackSO == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, firePoint.position);

        if (currentTime < boomerangAttackSO.returnTime)
        {
            rb.AddForce(direction.normalized * boomerangAttackSO.speed * rb.mass, ForceMode2D.Force);
            Debug.DrawRay(transform.position, direction, Color.red, Time.deltaTime);
        }
        else if (currentTime < boomerangAttackSO.returnTime + boomerangAttackSO.turnTime)
        {
            Vector2 targetDir = (firePoint.position - transform.position).normalized;

            float t = (currentTime - boomerangAttackSO.returnTime) / (boomerangAttackSO.turnTime);

            Vector2 actualDir = Vector2.Lerp(direction, targetDir, t).normalized;

            rb.AddForce(actualDir.normalized * boomerangAttackSO.speed * rb.mass, ForceMode2D.Force);
        }
        else if (distanceToTarget > boomerangAttackSO.aggressiveReturnDistance) //Regular Returning
        {
            rb.AddForce((firePoint.position - transform.position) * boomerangAttackSO.returnSpeed * rb.mass, ForceMode2D.Force);           
        }
        else
        {
            if (distanceToTarget < boomerangAttackSO.hasReturnedDistance) //Shrinking/Has Returned
            {
                isShrinking = true;
                rb.velocity = (firePoint.position - transform.position) * boomerangAttackSO.returnSpeed / 5 * rb.mass;
            }
            else //Agressive Returning
            {
                rb.velocity = (firePoint.position - transform.position) * boomerangAttackSO.returnSpeed * rb.mass;
            }
        }

        if (currentTime > boomerangAttackSO.decayTime)
        {
            isShrinking = true;
        }
        
        if (isShrinking)
        {
            Shrink();
        }

        currentTime += Time.deltaTime;
    }
}
