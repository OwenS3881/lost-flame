using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class DashAttack : BasicAttack
{
    private DashAttackSO dashAttackSO;
    protected float speed;
    [HideInInspector] public float dashTime;
    private ParticleSystem dashEffect;
    [ReadOnly]public bool isDashing;
    private Rigidbody2D characterRb;
    [HideInInspector] public float dashTimeCounter = -1;
    private Vector2 direction;
    private int stillFrames;
    private Sprite dashSprite;
    private Sprite defaultSprite;
    private SpriteRenderer playerRenderer;

    protected override void Awake()
    {
        if (basicAttackSO != null)
        {
            base.Awake();
            powerUpgradeLevel = DeterminePlayerPower("dashDamage");
            cooldownLevel = DeterminePlayerCooldown("dashCooldown");
            dashAttackSO = (DashAttackSO)basicAttackSO;
            speed = dashAttackSO.dashSpeed;
            dashTime = dashAttackSO.dashTime;
            dashTimeCounter = 0;
            if (graphicsObject != null)
            {
                dashEffect = Instantiate(graphicsObject, transform).GetComponent<ParticleSystem>();
                dashEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (dashAttackSO.dashSprite != null) dashSprite = dashAttackSO.dashSprite;
            transform.localPosition = Vector3.zero;
        }
        else
        {
            Invoke("Awake", 0.01f);
        }
    }

    /*
    public override GameObject[] CreateUI(GameObject canvas)
    {
        return dashAttackSO.CreateUI(canvas);
    }
    */

    public bool Dash(Rigidbody2D rb, Vector2 dir)
    {
        if (isDashing || cooldownTimer > 0)
        {
            return false;
        }

        characterRb = rb;
        playerRenderer = rb.GetComponentInChildren<SpriteRenderer>();
        defaultSprite = playerRenderer.sprite;
            
        if (dashSprite != null) playerRenderer.sprite = dashSprite;
        
        if (!dir.Equals(Vector2.zero))
        {
            direction = dir.normalized;
        }
        else
        {
            if (characterRb.transform.localScale.x > 0)
            {
                direction = Vector2.right;
            }
            else
            {
                direction = Vector2.left;
            }
        }

        if (dashEffect != null)
        {
            var rend = dashEffect.GetComponent<ParticleSystemRenderer>();
            if (direction.x > 0)
            {
                rend.flip = new Vector3(0f, rend.flip.y, rend.flip.z);
            }
            else
            {
                rend.flip = new Vector3(1f, rend.flip.y, rend.flip.z);
            }
            dashEffect.Play();
        }
        isDashing = true;

        AudioManager.instance.PlayOneShot("Dash");

        return true;
    }

    public void DashCollision(Collision2D collision)
    {
         EndDash();
         characterRb.AddForce(-direction.normalized * dashAttackSO.knockback * characterRb.mass, ForceMode2D.Impulse);
        
        //Collision with enemy
        if (GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true) != null)
        {
            IAttackable character = GetComponentInHierarchy<IAttackable>(collision.gameObject.transform, true);
            if (character.characterType == dashAttackSO.attacker)
            {
                return;
            }
            else
            {
                character.OnAttacked(this);
            }
        }
    }

    void EndDash()
    {
        if (playerRenderer != null) playerRenderer.sprite = defaultSprite;
        characterRb.velocity = Vector2.zero;
        dashTimeCounter = 0;
        isDashing = false;
        
        if (dashEffect != null) dashEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        
        cooldownTimer = cooldown;
        FinishAttack();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (isDashing && dashTimeCounter < dashTime)
        {
            characterRb.velocity = direction.normalized * speed;
            dashTimeCounter += Time.deltaTime;
        }
        else if (dashTimeCounter >= dashTime)
        {
            EndDash();
        }
    }
}
