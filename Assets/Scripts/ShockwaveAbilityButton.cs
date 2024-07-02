using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class ShockwaveAbilityButton : BasicAbility
{
    [SerializeField] private GameObject shockwaveEffectPrefab;

    [SerializeField] private int power;
    [SerializeField] private int[] powerUpgrades;
    [SerializeField] private float knockback;
    [SerializeField] private float camShakeIntensity;
    private int level;
    private int attack;

    GameObject shockwave = null;
    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        Invoke("GetStatsFromPlayer", 0.02f);
        UpgradePower();

        if (Application.isMobilePlatform)
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-466.9f, 311.9f);
            transform.localScale = Vector2.one;
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-500f, 200f);
            transform.localScale = new Vector2(1.5f, 1.5f);
        }
    }

    private void UpgradePower()
    {
        for (int i = 0; i < powerUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("shockwaveUpgrade" + (i + 1)))
            {
                power += powerUpgrades[i];
            }
            else
            {
                return;
            }
        }
    }

    private void GetStatsFromPlayer()
    {
        level = Player.instance.level;
        attack = Player.instance.attack;
    }

    public override void AbilityAction()
    {
        if (isAttacking) return;

        isAttacking = true;

        shockwave = Instantiate(shockwaveEffectPrefab, Player.instance.firePoint.position, Quaternion.identity);

        shockwave.GetComponent<ShockwaveAbility>().Initialize(power, level, attack, knockback, CharacterType.Player);

        Collider2D[] playerColliders = GetComponentsInHierarchy<Collider2D>(Player.instance.transform);
        Collider2D shockwaveCollider = shockwave.GetComponentInChildren<Collider2D>();

        foreach (Collider2D p in playerColliders)
        {
            Physics2D.IgnoreCollision(p, shockwaveCollider);
        }

        float duration = shockwave.GetComponentInChildren<ParticleSystem>().main.startLifetime.constant;

        CinemachineShake.instance.ShakeCamera(camShakeIntensity, duration, true);

        Invoke("Reset", 0.2f);
    }

    private void Reset()
    {
        if (shockwave == null)
        {
            currentCooldownDamage = 0;
            UpdateCooldown(0);
            isAttacking = false;
        }
        else
        {
            Invoke("Reset", 0.01f);
        }
    }

    public override void OnPlayerAttacked()
    {
        base.OnPlayerAttacked();
    }
}
