using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EndDungeonItemData
{
    public EndDungeonItemType type;

    public bool disabled;

    public string title;
    public Color titleColor = Color.white;

    public Sprite buttonSprite;
    public Color buttonSpriteColor = Color.white;
    public float buttonSpriteRotation;
    public Vector2 buttonImageDimensions;

    public Color sliceColor = Color.white;

    [Range(1, 99)]
    public int probability;

    [Tooltip("Leave empty if this item is not unlocked via the skill tree")]
    public string skillUnlockId;

    [Header("Type Specific Variables")]
    public BasicAttackSO attack;
    public string abilityUnlockId;
    public StatType statType;
    public int statBoost;
}
