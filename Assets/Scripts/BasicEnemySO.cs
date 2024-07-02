using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicEnemySO : BasicCharacterSO
{
    public BasicEnemy enemyScript;
    public bool flipDeathEffect;

    [Header("Experience Particles")]
    [Tooltip("Recommended range is 36 - 635")]
    public int baseExperienceYield;
    public GameObject experienceParticle;
    [Tooltip("When spawning experience particles, the number of particles will be equal to a random number between the x and y values of this vector (Inclusive)")]
    public Vector2Int numberOfExperienceParticles;

    [Header("Buffs")]
    public GameObject[] statPickups;
    [Range(0, 100)]
    public float statPickupProbability;
    public int maxStatPickups;

    protected override void OnValidate()
    {
        base.OnValidate();
        characterType = CharacterType.Enemy;

        for (int i = 0; statPickups != null && i < statPickups.Length; i++)
        {
            if (statPickups[i] != null && statPickups[i].GetComponent<StatPickup>() == null)
            {
                Debug.LogError("statPickup #" + i + " has to be of type StatPickup smh");
                statPickups[i] = null;
            }
        }
    }

    public GameObject GetStatPickup()
    {
        return statPickups[UnityEngine.Random.Range(0, statPickups.Length)];
    }
}
