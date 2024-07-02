using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MyFunctions;
using TMPro;

public class EndDungeonItem : MonoBehaviour
{
    [SerializeField] private Image[] slices;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text titleText;
    private EndDungeonItemType type;
    private Sprite buttonSprite;
    private Color sliceColor;
    private BasicAttackSO attackToAssign;
    private string abilityUnlockId;
    private StatType statType;
    private float boost;
    private Animator anim;
    private Button button;

    public void Initialize(EndDungeonItemData data)
    {
        type = data.type;
        statType = data.statType;
        boost = data.statBoost;
        buttonSprite = data.buttonSprite;
        sliceColor = data.sliceColor;
        if (data.attack != null)
        {
            attackToAssign = data.attack;
        }
        abilityUnlockId = data.abilityUnlockId;

        anim = GetComponent<Animator>();
        buttonImage.sprite = buttonSprite;
        buttonImage.color = data.buttonSpriteColor;
        SetRotation(buttonImage.transform, 2, data.buttonSpriteRotation);
        buttonImage.SetNativeSize();
        buttonImage.GetComponent<RectTransform>().sizeDelta = data.buttonImageDimensions;
        button = buttonImage.GetComponent<Button>();

        foreach (Image s in slices)
        {
            s.color = sliceColor;
        }

        titleText.text = data.title;
        titleText.color = data.titleColor;
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void Clear()
    {
        if (!button.interactable) return;

        button.interactable = false;
        anim.SetTrigger("Disappear");
    }

    private void Clicked()
    {
        Clear();
        if (type == EndDungeonItemType.Attack)
        {
            EndDungeonManager.instance.attackToGive = attackToAssign;
        }
        else if (type == EndDungeonItemType.Ability)
        {
            EndDungeonManager.instance.abilityUnlocks[abilityUnlockId] = true;
        }
        else if (type == EndDungeonItemType.StatBoost)
        {
            StatPickup.Boost(statType, boost);
        }
        EndDungeonManager.instance.ItemSelected();
    }
}
