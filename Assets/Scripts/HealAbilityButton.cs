using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealAbilityButton : BasicAbility
{
    [Tooltip("Percent")]
    [SerializeField] private int healthToRestore;
    private bool isHealing;

    [SerializeField] private GameObject healEffect;

    [SerializeField] private int[] healthUpgrades;

    [SerializeField] private float camShakeIntensity;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        UpgradeHealRestore();

        if (Application.isMobilePlatform)
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-290f, 455f);
            transform.localScale = Vector2.one;
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(-200f, 200f);
            transform.localScale = new Vector2(1.5f, 1.5f);
        }
    }

    private void UpgradeHealRestore()
    {
        for (int i = 0; i < healthUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("healUpgrade" + (i + 1)))
            {
                healthToRestore += healthUpgrades[i];
            }
            else
            {
                return;
            }
        }
    }

    public override void AbilityAction()
    {
        if (isHealing || currentCooldownDamage < cooldownDamage) return;

        isHealing = true;
        GameObject effect = Instantiate(healEffect, Player.instance.firePoint);
        StartCoroutine(Heal(effect));
        AudioManager.instance.Play("HealAbility");
    }

    IEnumerator Heal(GameObject effect)
    {
        float elapsed = 0f;
        float duration = effect.GetComponentInChildren<ParticleSystem>().main.duration;

        CinemachineShake.instance.ShakeCamera(camShakeIntensity, duration, true);

        while (elapsed < duration)
        {
            if (!isHealing)
            {
                Destroy(effect);
                CinemachineShake.instance.StopShaking();
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        Player.instance.AddHealth((int)(healthToRestore / 100f * Player.instance.maxHealth));
        isHealing = false;

        currentCooldownDamage = 0;
        UpdateCooldown(0);
    }

    public override void OnPlayerAttacked()
    {
        base.OnPlayerAttacked();
        isHealing = false;
        AudioManager.instance.StopSound("HealAbility");
    }
}
