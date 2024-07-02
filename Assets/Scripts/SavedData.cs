using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedData
{
    public int playerExperience;

    public int playerHealthIV;
    public int playerAttackIV;
    public int playerDefenseIV;

    public int dungeonExperience;

    public string[] skills;
    public bool[] skillsPurchased;

    public int skillPoints;

    public bool dungeonExperienceMaxedOut;

    public bool finishedGame;

    public int prestigeLevel;

    public SavedData(SavedDataManager sdm)
    {
        playerExperience = sdm.PlayerExperience;
        playerHealthIV = sdm.PlayerHealthIV;
        playerAttackIV = sdm.PlayerAttackIV;
        playerDefenseIV = sdm.PlayerDefenseIV;
        dungeonExperience = sdm.DungeonExperience;

        skills = new string[sdm.Skills.Length];
        skillsPurchased = new bool[sdm.SkillsPurchased.Length];

        System.Array.Copy(sdm.Skills, skills, sdm.Skills.Length);
        System.Array.Copy(sdm.SkillsPurchased, skillsPurchased, sdm.SkillsPurchased.Length);

        skillPoints = sdm.SkillPoints;

        dungeonExperienceMaxedOut = sdm.DungeonExperienceMaxedOut;

        finishedGame = sdm.FinishedGame;

        prestigeLevel = sdm.PrestigeLevel;
    }
}
