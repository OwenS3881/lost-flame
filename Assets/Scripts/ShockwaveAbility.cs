using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class ShockwaveAbility : MonoBehaviour
{
    private int power;
    private int level;
    private int attack;
    private float knockback;
    private CharacterType characterType;

    private Collider2D myCollider;

    private void Awake()
    {
        AudioManager.instance.PlayOneShot("Shockwave");
    }

    public void Initialize(int p, int l, int a, float k, CharacterType t)
    {
        power = p;
        level = l;
        attack = a;
        knockback = k;
        characterType = t;
        myCollider = GetComponentInChildren<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Physics2D.IgnoreCollision(collision, myCollider);

        if (GetComponentInHierarchy<IAttackable>(collision.transform, true) != null)
        {
            IAttackable character = GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true);
            
            if (character.characterType == characterType) return;

            character.OnAttacked(level, power, attack, characterType);

            Rigidbody2D characterRb = GetComponentInHierarchy<Rigidbody2D>(collision.transform, true);

            ContactPoint2D[] hitPoints = new ContactPoint2D[1];
            collision.GetContacts(hitPoints);
            ContactPoint2D hitPoint = hitPoints[0];
            Vector2 dir = (hitPoint.point - characterRb.position).normalized;

            characterRb.AddForce(dir.normalized * characterRb.mass * knockback, ForceMode2D.Impulse);
        }
    }
}
