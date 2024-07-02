using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using static MyFunctions;

public class EndDungeonManager : MonoBehaviour
{
    public static EndDungeonManager instance { get; private set; }

    [ReadOnly] public int dungeonLevel = 1;
    [SerializeField] private GameObject endDungeonItemPrefab;
    [SerializeField] private GameObject itemCanvas;
    [SerializeField] private Transform itemSpawnParent;
    [SerializeField] private int itemsToSpawn;
    [SerializeField] private EndDungeonItemData[] items;  

    private List<EndDungeonItem> activeItems = new List<EndDungeonItem>();
    private List<EndDungeonItemData> activeItemDatas = new List<EndDungeonItemData>();

    [ReadOnly] public BasicAttackSO attackToGive;
    [SerializeField] BasicAttackSO defaultAttack;

    [Header("Dungeon Rank Stat Values for Dictionary")]
    [SerializeField] private string[] dungeonRankStatNames;
    [SerializeField] private float[] dungeonRankStatWeights;
    public Dictionary<string, Vector2> dungeonRankStats { private set; get; }

    private int playerHealth;
    private int playerAttackIV;
    private int playerDefenseIV;
    private float playerSpeedBoost;

    [Header("Ability Booleans")]
    [SerializeField] private string[] abilityNames;
    public Dictionary<string, bool> abilityUnlocks;

    [Header("Boss Battles")]
    [SerializeField] private Vector2Int potentialBossFloorRange;
    [SerializeField] private string[] bossBattleSceneNames;
    [SerializeField] private string[] bossBattleTransitionNames;
    [SerializeField] private string[] validLevelScenes;

    private void OnValidate()
    {
        itemsToSpawn = (int)Mathf.Clamp(itemsToSpawn, 0, Mathf.Infinity);
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

        itemCanvas.SetActive(false);

        if (attackToGive != null || defaultAttack != null)
        {
            Invoke(nameof(GivePlayerAttack), 0.02f);
        }

        InitializeDungeonRankStatDictionary();

        InitializeAbilityUnlockDictionary();

        SceneManager.sceneLoaded += OnSceneLoaded;
        OnValidate();
    }

    private void SetDefaults()
    {
        dungeonLevel = 1;
        attackToGive = null;

        playerHealth = 0;
        playerAttackIV = 0;
        playerDefenseIV = 0;
        playerSpeedBoost = 0f;

        InitializeAbilityUnlockDictionary();
        InitializeDungeonRankStatDictionary();
    }

    private void OnApplicationQuit()
    {
        if (Player.instance != null ) Player.instance.playerSO.ResetIVs();
    }

    private void InitializeDungeonRankStatDictionary()
    {
        if (dungeonRankStatNames.Length != dungeonRankStatWeights.Length)
        {
            Debug.LogError("Weights and names of dungeon rank stats are not equal");
            return;
        }

        dungeonRankStats = new Dictionary<string, Vector2>();

        for (int i = 0; i < dungeonRankStatNames.Length; i++)
        {
            dungeonRankStats.Add(dungeonRankStatNames[i], new Vector2(0, dungeonRankStatWeights[i]));
        }
    }

    private void InitializeAbilityUnlockDictionary()
    {
        abilityUnlocks = new Dictionary<string, bool>();
        for (int i = 0; i < abilityNames.Length; i++)
        {
            abilityUnlocks.Add(abilityNames[i], false);
        }
    }

    public void AddDungeonRankStat(string key, float value)
    {
        dungeonRankStats[key] = new Vector2(dungeonRankStats[key].x + value, dungeonRankStats[key].y);
    }

    public void SetDungeonRankStat(string key, float value)
    {
        dungeonRankStats[key] = new Vector2(value, dungeonRankStats[key].y);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if ((attackToGive != null || defaultAttack != null) && InValidLevel(scene.name))
        {
            Invoke(nameof(GivePlayerAttack), 0.02f);
        }

        if (scene.name.Equals("SkillTree") || scene.name.Equals("MainMenu"))
        {
            SetDefaults();
        }
    }

    private bool InValidLevel(string currentScene)
    {
        for (int i = 0; i < validLevelScenes.Length; i++)
        {
            if (currentScene.Equals(validLevelScenes[i]))
            {
                return true;
            }
        }
        return false;
    }

    public void ReachedEndOfDungeon()
    {
        Player.instance.DisableInteractiveUI();

        itemCanvas.SetActive(true);

        while (BasicEnemy.enemies.Count > 0)
        {
            if (BasicEnemy.enemies[0] != null)
            {
                Destroy(BasicEnemy.enemies[0].gameObject);
            }
            BasicEnemy.enemies.Remove(BasicEnemy.enemies[0]);
        }

        for (int i = 0; i < itemsToSpawn; i++)
        {
            EndDungeonItemData d = GetRandomItem();

            if (d == null) continue;

            CreateItem(d);
        }

        if (activeItems.Count == 0)
        {
            ItemSelected();
        }
    }

    public void ItemSelected()
    {
        foreach (EndDungeonItem item in activeItems)
        {
            item.Clear();
        }

        StartCoroutine(StartNextDungeon());
    }

    IEnumerator StartNextDungeon()
    {
        yield return null;

        while (activeItems.Count > 0 && activeItems[0] != null)
        {
            yield return null;
        }

        activeItems.Clear();
        activeItemDatas.Clear();

        itemCanvas.SetActive(false);

        AddDungeonRankStat("Levels Completed", 1);

        playerHealth = Player.instance.GetHealth();
        playerAttackIV = Player.instance.playerSO.attackIV;
        playerDefenseIV = Player.instance.playerSO.defenseIV;
        playerSpeedBoost = Player.instance.speedMultiplier;

        bool loadBoss = false;

        if (SavedDataManager.instance.DungeonExperienceMaxedOut)
        {
            float chance = (1 - (float)(potentialBossFloorRange.y - dungeonLevel) / Math.Abs(potentialBossFloorRange.y - potentialBossFloorRange.x + 1)) * 100;
            chance = Mathf.Clamp(chance, 0f, 100f);
            loadBoss = UnityEngine.Random.Range(0f, 100f) <= chance;
        }

        dungeonLevel++;
        if (loadBoss)
        {
            LoadBossScene();
        }
        else
        {
            LoadMainLevel();
        }
    }

    private void LoadBossScene()
    {
        try
        {
            LevelLoader.instance.LoadScene(bossBattleSceneNames[SavedDataManager.instance.GetCurrentDungeonRank() - 1], bossBattleTransitionNames[SavedDataManager.instance.GetCurrentDungeonRank() - 1]);
        }
        catch (Exception e)
        {
            if (e.InnerException is IndexOutOfRangeException)
            {
                Debug.LogWarning("No boss fight implemented for the current dungeon rank");
                LoadMainLevel();
            }
            else
            {
                throw e;
            }
        }
    }

    private void LoadMainLevel()
    {
        LevelLoader.instance.LoadScene("MainLevel", "ReloadMain");
    }

    private int FindIndexOfData(EndDungeonItemData data)
    {
        return Array.IndexOf(items, data);
    }

    private EndDungeonItemData GetRandomItem()
    {
        int totalProbability = 0;
        foreach (EndDungeonItemData item in items)
        {
            totalProbability += item.probability;
        }

        float random = UnityEngine.Random.Range(0f, (float)totalProbability);

        int runningSum = 0;
        int nonValidItems = 0;
        foreach (EndDungeonItemData item in items)
        {
            if (item.disabled)
            {
                nonValidItems++;
                continue;
            }

            if (activeItemDatas.Contains(item))
            {
                nonValidItems++;
                continue;
            }

            if (item.attack != null && item.attack.name.Equals(Player.instance.playerSO.attackSO.name))
            {
                nonValidItems++;
                continue;
            }

            if (item.type == EndDungeonItemType.Ability && abilityUnlocks[item.abilityUnlockId])
            {
                nonValidItems++;
                continue;
            }

            if (item.type == EndDungeonItemType.StatBoost)
            {
                if (item.statType == StatType.Attack && Player.instance.playerSO.attackIV >= 15)
                {
                    nonValidItems++;
                    continue;
                }
                else if (item.statType == StatType.Defense && Player.instance.playerSO.defenseIV >= 15)
                {
                    nonValidItems++;
                    continue;
                }
                else if (item.statType == StatType.Health && Player.instance.GetHealth() >= Player.instance.maxHealth)
                {
                    nonValidItems++;
                    continue;
                }
            }

            if (!item.skillUnlockId.Equals("") && !SavedDataManager.instance.IsSkillPurchased(item.skillUnlockId))
            {
                nonValidItems++;
                continue;
            }

            runningSum += item.probability;
            if (runningSum > random)
            {
                return item;
            }
        }
        if (nonValidItems < items.Length)
        {
            return GetRandomItem();
        }
        else
        {
            return null;
        }
    }

    private GameObject CreateItem(EndDungeonItemData data, Transform parent, Vector3 localPosition)
    {
        GameObject item = null;

        item = Instantiate(endDungeonItemPrefab, parent);
        item.transform.localPosition = localPosition;
        
        item.GetComponent<EndDungeonItem>().Initialize(data);

        activeItems.Add(item.GetComponent<EndDungeonItem>());
        activeItemDatas.Add(data);

        return item;
    }

    private GameObject CreateItem(EndDungeonItemData data, Transform parent)
    {
        return CreateItem(data, parent, Vector3.zero);
    }

    private GameObject CreateItem(EndDungeonItemData data)
    {
        return CreateItem(data, itemSpawnParent, Vector3.zero);
    }

    private void GivePlayerAttack()
    {
        if (Player.instance != null)
        {
            if (dungeonLevel <= 1)
            {
                attackToGive = defaultAttack;
            }

            foreach (BasicAttack b in Player.instance.gameObject.GetComponentsInChildren<BasicAttack>())
            {
                Destroy(b.gameObject);
            }
            Player.instance.playerSO.attackSO = attackToGive;

            if (playerHealth > 0)
            {
                Player.instance.SetHealth(playerHealth);
            }

            Player.instance.playerSO.attackIV = playerAttackIV;
            Player.instance.playerSO.defenseIV = playerDefenseIV;
            Player.instance.speedMultiplier = playerSpeedBoost;

            Player.instance.Start();
            Player.instance.DetermineStats();
        }
        else
        {
            Invoke("GivePlayerAttack", 0.01f);
        }
    }
}
