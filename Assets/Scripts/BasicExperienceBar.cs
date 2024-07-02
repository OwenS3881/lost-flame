using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class BasicExperienceBar : MonoBehaviour
{
    [SerializeField] protected Slider slider;
    [SerializeField] protected TMP_Text currentLevelText, nextLevelText;
    [SerializeField] protected float updateRate;

    protected float currentExperiencePoints;
    protected int targetExperiencePoints;

    protected int currentLevel;
    protected int currentLevelExperiencePoints;

    protected int nextLevel;
    protected int nextLevelExperiencePoints;

    public bool updatingBar;

    protected virtual void Start()
    {
        currentLevel = ExperienceToLevel((int)currentExperiencePoints);
        currentLevelExperiencePoints = LevelToExperience(currentLevel);

        nextLevel = currentLevel + 1;
        nextLevelExperiencePoints = LevelToExperience(nextLevel);

        currentLevelText.text = currentLevel.ToString();
        nextLevelText.text = nextLevel.ToString();

        slider.minValue = currentLevelExperiencePoints;
        slider.maxValue = nextLevelExperiencePoints;
        slider.value = currentExperiencePoints;
    }

    protected abstract int ExperienceToLevel(int experience);

    protected abstract int LevelToExperience(int level);

    protected abstract void OnLevelUp();

    public void UpdateExperience(int newExperience)
    {
        targetExperiencePoints = newExperience;
    }

    protected void BarLogic()
    {
        if (currentExperiencePoints < targetExperiencePoints)
        {
            updatingBar = true;
            currentExperiencePoints += updateRate * Time.deltaTime;

            if (currentExperiencePoints > nextLevelExperiencePoints)
            {
                currentLevel++;
                nextLevel++;

                currentLevelExperiencePoints = LevelToExperience(currentLevel);
                nextLevelExperiencePoints = LevelToExperience(nextLevel);

                currentLevelText.text = currentLevel.ToString();
                nextLevelText.text = nextLevel.ToString();

                slider.minValue = currentLevelExperiencePoints;
                slider.maxValue = nextLevelExperiencePoints;

                OnLevelUp();
            }

            slider.value = currentExperiencePoints;
        }
        else if (currentExperiencePoints != targetExperiencePoints)
        {
            updatingBar = false;
            currentExperiencePoints = targetExperiencePoints;
        }
    }
}
