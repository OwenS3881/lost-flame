using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavedDataManager : MonoBehaviour
{
    public static SavedDataManager instance { get; private set; }

    [ReadOnly, SerializeField]private int playerExperience;
    public int PlayerExperience
    {
        get
        {
            return playerExperience;
        }
        set
        {
            playerExperience = value;
            SaveData();
        }
    }


    [ReadOnly, SerializeField] private int playerHealthIV;
    public int PlayerHealthIV
    {
        get
        {
            return playerHealthIV;
        }
        set
        {
            playerHealthIV = value;
            SaveData();
        }
    }


    [ReadOnly, SerializeField] private int playerAttackIV;
    public int PlayerAttackIV
    {
        get
        {
            return playerAttackIV;
        }
        set
        {
            playerAttackIV = value;
            SaveData();
        }
    }


    [ReadOnly, SerializeField] private int playerDefenseIV;
    public int PlayerDefenseIV
    {
        get
        {
            return playerDefenseIV;
        }
        set
        {
            playerDefenseIV = value;
            SaveData();
        }
    }

    [ReadOnly, SerializeField] private int dungeonExperience;
    public int DungeonExperience
    {
        get
        {
            return dungeonExperience;
        }
        set
        {
            dungeonExperience = value;
            SaveData();
        }
    }

    [SerializeField] private string[] skills = new string[0];
    public string[] Skills
    {
        get
        {
            return skills;
        }
        set
        {
            skills = value;
            SaveData();
            InitializeSkillsDictionary();
        }
    }

    [SerializeField] private bool[] skillsPurchased = new bool[0];
    public bool[] SkillsPurchased
    {
        get
        {
            return skillsPurchased;
        }
        set
        {
            skillsPurchased = value;
            SaveData();
            InitializeSkillsDictionary();
        }
    }

    private Dictionary<string, bool> skillsDict = new Dictionary<string, bool>();

    [ReadOnly, SerializeField] private int skillPoints;
    public int SkillPoints
    {
        get
        {
            return skillPoints;
        }
        set
        {
            skillPoints = value;

            if (SkillTree.instance != null)
            {
                SkillTree.instance.UpdateSkillPoints();
                SkillTree.instance.BeginUpdateChain();
            }

            SaveData();
        }
    }

    [ReadOnly, SerializeField] private bool dungeonExperienceMaxedOut;
    public bool DungeonExperienceMaxedOut
    {
        get
        {
            return dungeonExperienceMaxedOut;
        }
        set
        {
            dungeonExperienceMaxedOut = value;
            SaveData();
        }
    }

    [ReadOnly, SerializeField] private bool finishedGame;
    public bool FinishedGame
    {
        get
        {
            return finishedGame;
        }
        set
        {
            finishedGame = value;
            SaveData();
        }
    }

    [ReadOnly, SerializeField] private int prestigeLevel;
    public int PrestigeLevel
    {
        get
        {
            return prestigeLevel;
        }
        set
        {
            prestigeLevel = value;
            SaveData();
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        LoadData();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadData();
    }

    [ContextMenu("Reset Experience")]
    public void ResetExperience()
    {
        PlayerExperience = 0;
        ClampData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void LoadData()
    {
        SavedData data = SaveSystem.Load();
        if (data == null)
        {
            ClampData();
            SaveData();
            return;
        }
        playerExperience = data.playerExperience;
        playerHealthIV = data.playerHealthIV;
        playerAttackIV = data.playerAttackIV;
        playerDefenseIV = data.playerDefenseIV;
        dungeonExperience = data.dungeonExperience;

        if (data.skills != null)
        {
            skills = new string[data.skills.Length];
            System.Array.Copy(data.skills, skills, data.skills.Length);
        }
        else
        {
            skills = new string[0];
        }

        if (data.skillsPurchased != null)
        {
            skillsPurchased = new bool[data.skillsPurchased.Length];
            System.Array.Copy(data.skillsPurchased, skillsPurchased, data.skillsPurchased.Length);
        }
        else
        {
            skillsPurchased = new bool[0];
        }

        InitializeSkillsDictionary();

        skillPoints = data.skillPoints;

        dungeonExperienceMaxedOut = data.dungeonExperienceMaxedOut;

        finishedGame = data.finishedGame;

        prestigeLevel = data.prestigeLevel;

        ClampData();
    }

    private void InitializeSkillsDictionary()
    {
        skillsDict.Clear();
        for (int i = 0; i < skills.Length && i < skillsPurchased.Length; i++)
        {
            if (skills[i] == null) continue;

            skillsDict.Add(skills[i], skillsPurchased[i]);
        }
    }

    private void ClampData()
    {
        playerExperience = Mathf.Clamp(playerExperience, 5000, 100000);
        playerHealthIV = Mathf.Clamp(playerHealthIV, 0, 15);
        playerAttackIV = Mathf.Clamp(playerAttackIV, 0, 15);
        playerDefenseIV = Mathf.Clamp(playerDefenseIV, 0, 15);
        dungeonExperience = Mathf.Clamp(dungeonExperience, 1, 1000000);

        SaveData();
    }

    [ContextMenu("Save Data")]
    public void SaveData()
    {
        SaveSystem.Save(this);
    }

    [ContextMenu("Delete Data")]
    public void DeleteData()
    {
        SaveSystem.Delete();
    }

    public bool IsSkillPurchased(string skillId)
    {
        if (skillsDict.ContainsKey(skillId))
        {
            return skillsDict[skillId];
        }
        else
        {
            if (Application.isEditor)
            {
                Debug.LogError("No skill found with id: " + skillId);
            }
            return false;
        }
    }

    [ContextMenu("Reset Dungeon Experience")]
    public void ResetDungeonExperience()
    {
        DungeonExperience = 0;
        ClampData();
    }

    [ContextMenu("Rank Up")]
    public void CompleteRankUpChallenge()
    {
        if (!DungeonExperienceMaxedOut) return;

        DungeonExperience++;
        DungeonExperienceMaxedOut = false;
    }

    public int GetCurrentDungeonRank()
    {
        return DungeonRankExperienceBar.GetCurrentDungeonRank(DungeonExperience);
    }

    [ContextMenu("Prestige Test")]
    public void TestPrestige()
    {
        finishedGame = true;
        PrestigeUp();
    }

    [ContextMenu("Reset Prestige")]
    public void ResetPrestige()
    {
        PrestigeLevel = 0;
    }

    public bool PrestigeUp()
    {
        if (!finishedGame) return false;

        finishedGame = false;
        prestigeLevel++;

        playerExperience = 0;
        playerAttackIV = 0;
        playerDefenseIV = 0;
        playerHealthIV = 0;

        dungeonExperience = 0;
        dungeonExperienceMaxedOut = false;

        ClampData();

        return true;
    }
}
