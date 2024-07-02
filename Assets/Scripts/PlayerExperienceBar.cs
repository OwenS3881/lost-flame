using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static MyFunctions;

public class PlayerExperienceBar : BasicExperienceBar
{
    public static PlayerExperienceBar instance { get; private set; }

    [Tooltip("Amount of potential skill points to gain (inclusive)")]
    [SerializeField] private Vector2Int skillPointRange;

    [SerializeField] private Transform levelUpEffectSpawnPoint;
    [SerializeField] private GameObject levelUpEffectPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("There were multiple player experience bars in the scene");
        }
    }

    protected override void Start()
    {
        currentExperiencePoints = SavedDataManager.instance.PlayerExperience;
        targetExperiencePoints = SavedDataManager.instance.PlayerExperience;

        base.Start();

        currentLevelText.text = (currentLevel - 4).ToString();
        nextLevelText.text = (nextLevel - 4).ToString();
    }

    protected override int ExperienceToLevel(int experience)
    {
        //int newLevel = (int)CubeRoot((float)experience);

        int newLevel = (int)(experience / 1000f);

        if (newLevel == 0)
        {
            return 1;
        }
        else
        {
            return newLevel;
        }
    }

    protected override int LevelToExperience(int level)
    {
        //return (int)Mathf.Pow(level, 3);
        return level * 1000;
    }

    protected override void OnLevelUp()
    {
        int skillPointsGained = UnityEngine.Random.Range(skillPointRange.x, skillPointRange.y + 1);
        SavedDataManager.instance.SkillPoints += skillPointsGained;
        GameObject effect = Instantiate(levelUpEffectPrefab, levelUpEffectSpawnPoint);
        effect.GetComponentInChildren<TMP_Text>().text = "+" + skillPointsGained;

        AudioManager.instance.Play("LevelUp");

        currentLevelText.text = (currentLevel - 4).ToString();
        nextLevelText.text = (nextLevel - 4).ToString();
    }

    private void Update()
    {
        BarLogic();
    }
}
