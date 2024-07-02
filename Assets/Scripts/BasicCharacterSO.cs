using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicCharacterSO : ScriptableObject
{
    public float speed;
    [Range(1, 255)]
    public int baseHealth;
    [Range(1, 255)]
    public int baseAttack;
    [Range(1, 255)]
    public int baseDefense;
    [ReadOnly, SerializeField] private float baseStatAverage = 0;

    [ReadOnly]
    public int adjustedBaseHealth;
    [ReadOnly]
    public int adjustedBaseAttack;
    [ReadOnly]
    public int adjustedBaseDefense;

    public GameObject graphicsObject;

    public CharacterType characterType;

    public BasicAttackSO attackSO;

    public GameObject healthBar;

    public GameObject deathEffect;

    protected virtual void OnValidate()
    {
        baseStatAverage = (baseHealth + baseAttack + baseDefense) / 3.0f;
    }
}