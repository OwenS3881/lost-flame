using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class MeleeAttack : BasicAttack
{
    private MeleeAttackSO meleeAttackSO;
    private Animator anim;
    private Rigidbody2D attatchedCharacterRb;
    private int sizeLevel;

    private bool collisionCooldown;

    protected override void Awake()
    {
        if (basicAttackSO != null)
        {
            base.Awake();
            powerUpgradeLevel = DeterminePlayerPower("meleeDamage");
            cooldownLevel = DeterminePlayerCooldown("meleeCooldown");
            meleeAttackSO = (MeleeAttackSO)basicAttackSO;
            sizeLevel = DetermineSize();
            anim = GetComponentInChildren<Animator>();
            attatchedCharacterRb = (attatchedCharacter as MonoBehaviour).GetComponent<Rigidbody2D>();
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    private int DetermineSize()
    {
        if (attacker != CharacterType.Player) return -1;

        int sLevel = 0;

        for (int i = 0; i < meleeAttackSO.sizeUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("meleeSize" + (i + 1)))
            {
                instantiatedGraphicsObject.transform.localScale = meleeAttackSO.sizeUpgrades[i].size;
                instantiatedGraphicsObject.transform.localPosition = meleeAttackSO.sizeUpgrades[i].localOffset;
                sLevel++;
            }
            else
            {
                return sLevel;
            }
        }
        return sLevel;
    }

    public void Slash()
    {
        if (cooldownTimer > 0) return;

        anim.SetTrigger("Attack");
        AudioManager.instance.PlayOneShot("Melee");
        if ((attatchedCharacter as IAttackable).characterType == CharacterType.Player)
        {
            GetComponentInHierarchy<Animator>((attatchedCharacter as MonoBehaviour).transform).Play("Base Layer.Player-meleeAttack");
        }
        else if ((attatchedCharacter as IAttackable).characterType == CharacterType.Enemy)
        {
            //attatchedCharacter.anim.Play("Base Layer.Attack");
        }

        cooldownTimer = cooldown;
        collisionCooldown = false;
        FinishAttack();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollision(collision);
    }

    private void OnCollision(Collision2D collision)
    {
        HitEffect(collision);
        if (GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true) != null)
        {
            IAttackable character = GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true);
            if (character.characterType == meleeAttackSO.attacker)
            {
                return;
            }

            if (collisionCooldown)
            {
                return;
            }
            else
            {
                collisionCooldown = true;
            }

            Collided(collision);
            StartCoroutine(DamageOpponent(character));
        }
    }

    IEnumerator DamageOpponent(IAttackable opponent)
    {
        yield return null;
        opponent.OnAttacked(this);
    }

    private void HitEffect(Collision2D collision)
    {
        ContactPoint2D hitPoint = collision.GetContact(0);

        if (meleeAttackSO.hitEffect != null)
        {
            Instantiate(meleeAttackSO.hitEffect, hitPoint.point, Quaternion.identity);
        }
    }

    private void Collided(Collision2D collision)
    {
        ContactPoint2D hitPoint = collision.GetContact(0);

        Vector2 dir = attatchedCharacterRb.position - hitPoint.point;

        int selfFactor = 0;
        int opponentFactor = 0;
        if ((attatchedCharacter as IAttackable).characterType == CharacterType.Player)
        {
            selfFactor = 2;
            opponentFactor = 1;
        }
        else
        {
            selfFactor = 1;
            opponentFactor = 2;
        }

        attatchedCharacterRb.AddForce(dir.normalized * meleeAttackSO.knockback * selfFactor * attatchedCharacterRb.mass, ForceMode2D.Impulse);

        if (collision != null)
        {
            Rigidbody2D collisionRb = GetComponentInHierarchy<Rigidbody2D>(collision.transform, true);
            collisionRb.AddForce(-dir.normalized * meleeAttackSO.knockback * opponentFactor * collisionRb.mass, ForceMode2D.Impulse);
        }

        CinemachineShake.instance.ShakeCamera(2f, 0.25f, false);
    }

    protected override void Update()
    {
        if (cooldownTimer <= 0)
        {
            collisionCooldown = false;
        }
        base.Update();
    }
}
