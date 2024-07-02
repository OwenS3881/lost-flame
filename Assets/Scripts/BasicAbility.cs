using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasicAbility : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] protected Button button;
    [SerializeField] protected Image bg;
    [SerializeField] protected Image icon;

    [Header("Ids")]
    [SerializeField] protected string abilityUnlockId = "";
    [SerializeField] protected string skillUnlockId = "";
    [SerializeField] protected string cooldownUnlockId = "";

    [Header("Cooldown")]
    [SerializeField] protected int cooldownDamage;
    [SerializeField] protected int[] cooldownUpgrades;
    protected int currentCooldownDamage;

    public static List<BasicAbility> instances = new List<BasicAbility>();   

    protected virtual void Awake()
    {
        instances.Add(this);
    }

    protected virtual void Start()
    {
        if (!abilityUnlockId.Equals("") && !skillUnlockId.Equals("") && (!SavedDataManager.instance.IsSkillPurchased(skillUnlockId) || !EndDungeonManager.instance.abilityUnlocks[abilityUnlockId]))
        {
            Disable();
        }

        UpgradeCooldown();

        currentCooldownDamage = cooldownDamage;
    }

    private void OnDestroy()
    {
        instances.Remove(this);
    }

    public void Disable()
    {
        button.enabled = false;
        bg.enabled = false;
        icon.enabled = false;
    }

    public void Enable()
    {
        button.enabled = true;
        bg.enabled = true;
        icon.enabled = true;
    }

    public void ReceiveDamgeDealt(int damage)
    {
        currentCooldownDamage = Mathf.Clamp(currentCooldownDamage + damage, 0, cooldownDamage);
        UpdateCooldown((float)currentCooldownDamage / cooldownDamage);
    }

    private void UpgradeCooldown()
    {
        if (cooldownUnlockId.Equals("")) return;

        for (int i = 0; i < cooldownUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased(cooldownUnlockId + (i + 1)))
            {
                cooldownDamage -= cooldownUpgrades[i];
            }
            else
            {
                return;
            }
        }
    }

    public void UpdateCooldown(float normalizedValue)
    {
        bg.fillAmount = normalizedValue;

        button.interactable = normalizedValue >= 1;
    }

    public abstract void AbilityAction();

    public virtual void OnPlayerAttacked()
    {

    }
}
