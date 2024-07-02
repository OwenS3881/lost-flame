using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class DungeonRankExperienceBar : BasicExperienceBar
{
    public static DungeonRankExperienceBar instance { get; private set; }

    [SerializeField] private Animator barContentAnim;

    public bool waitingForEnd;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("There were multiple dungeon rank experience bars in the scene");
        }
    }

    protected override void Start()
    {
        currentExperiencePoints = SavedDataManager.instance.DungeonExperience;
        targetExperiencePoints = SavedDataManager.instance.DungeonExperience;

        base.Start();

        if (currentExperiencePoints == nextLevelExperiencePoints || nextLevelExperiencePoints - currentExperiencePoints == 1)
        {
            SavedDataManager.instance.DungeonExperienceMaxedOut = true;
        }
    }

    [ContextMenu("Enter Next Level")]
    public void EnterNextLevel()
    {
        SavedDataManager.instance.DungeonExperience++;
        Start();
    }

    protected override int ExperienceToLevel(int experience)
    {
        return GetCurrentDungeonRank(experience);
    }

    public static int GetCurrentDungeonRank(int experience)
    {
        //int newLevel = (int)(Mathf.Sqrt(experience / 1000) + 1);
        int newLevel = (int)(experience / 1500f) + 1;

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
        //return (int)(1000 * Mathf.Pow(level - 1, 2));
        return (level - 1) * 1500;
    }

    protected override void OnLevelUp()
    {
        Debug.Log("Implement level up effect");

        currentLevel--;
        nextLevel--;

        currentLevelExperiencePoints = LevelToExperience(currentLevel);
        nextLevelExperiencePoints = LevelToExperience(nextLevel);

        currentExperiencePoints = nextLevelExperiencePoints;
        targetExperiencePoints = (int)currentExperiencePoints;

        currentLevelText.text = currentLevel.ToString();
        nextLevelText.text = nextLevel.ToString();

        slider.minValue = currentLevelExperiencePoints;
        slider.maxValue = nextLevelExperiencePoints;

        SavedDataManager.instance.DungeonExperience = (int)currentExperiencePoints - 1;
        SavedDataManager.instance.DungeonExperienceMaxedOut = true;
    }

    IEnumerator End()
    {
        yield return new WaitForSeconds(0.5f);
        if (SavedDataManager.instance.DungeonExperienceMaxedOut)
        {
            barContentAnim.Play("BarContent-maxreveal");
        }
        else
        {
            SavedDataManager.instance.DungeonExperience = (int)currentExperiencePoints;
            barContentAnim.Play("BarContent-normalreveal");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!SavedDataManager.instance.DungeonExperienceMaxedOut)
        {
            BarLogic();
        }
        else
        {
            updatingBar = false;
        }

        if (waitingForEnd && !updatingBar)
        {
            waitingForEnd = false;
            StartCoroutine(End());
        }
    }
}
