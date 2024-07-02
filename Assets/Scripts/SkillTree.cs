using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using TMPro;
using UnityEngine.UI;
using UnityEngine.Purchasing;

using static MyFunctions;

public class SkillTree : MonoBehaviour
{
    public static SkillTree instance { get; private set; }

    public GameObject defaultUILineRenderer;
    [SerializeField] private SkillTreeNode firstNode;
    public Transform lineRenderersParent;

    [Header("Colors")]
    public Color notAvailableColor = Color.white;
    public Color unAffordableColor = Color.white;
    public Color affordableColor = Color.white;
    public Color purchasedColor = Color.white;

    [Header("Display Info")]
    [SerializeField] private GameObject infoParent;
    [SerializeField] private TMP_Text infoTitleText;
    [SerializeField] private TMP_Text infoCostText;
    [SerializeField] private TMP_Text infoDescriptionText;
    [SerializeField] private Button infoBuyButton;
    [SerializeField] private TMP_Text skillPointsDisplayText;
    private SkillTreeNode selectedNode; 

    [Header("Panel Scaling")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private float panelScaleFactor;

    [Header("Debug")]
    [SerializeField] private GameObject debugParent;
    [SerializeField] private TMP_InputField levelInput;
    [SerializeField] private TMP_InputField dungeonRankInput;

    [Header("IAP")]
    [SerializeField] private GameObject buyButton;
    [SerializeField] private Animator buyMenuAnim;
    [SerializeField] private GameObject restorePurchasesButton;
    private const string skillPointsID = "com.owenszymanski.lostflame.skillpoints20";

    [Header("Prestige")]
    [SerializeField] private GameObject prestigeParent;
    [SerializeField] private Transform prestigeIconArea;
    [SerializeField] private GameObject prestigeIcon;
    [SerializeField] private GameObject prestigeButton;
    [SerializeField] private Animator prestigeMenuAnim;

    private SkillTreeNode[] nodes;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Multiple skill trees in scene");
        }

        //debugParent.SetActive(PlayerPrefs.GetInt("DebugMode") == 1);
        debugParent.SetActive(false);

        nodes = GetComponentsInChildren<SkillTreeNode>();
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].skillTree = this;
        }

        infoParent.SetActive(selectedNode != null);

        LoadPurchasedData();

        firstNode.isAvailable = true;
        if (firstNode.connectorNode) firstNode.purchased = true;

        UpdateSkillPoints();

        Invoke("BeginUpdateChain", 0.01f);

        if (AudioManager.instance.activeSong.Equals(""))
        {
            AudioManager.instance.PlaySong("Song1");
        }

        restorePurchasesButton.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);

        prestigeParent.SetActive(SavedDataManager.instance.PrestigeLevel > 0);
        for (int i = 0; i < SavedDataManager.instance.PrestigeLevel; i++)
        {
            Instantiate(prestigeIcon, prestigeIconArea);
        }

        prestigeButton.SetActive(SavedDataManager.instance.FinishedGame);

        if (!Application.isMobilePlatform)
        {
            restorePurchasesButton.SetActive(false);
            buyButton.SetActive(false);
        }
    }

    public void UpdatePanelSize()
    {
        if (panel == null)
        {
            Debug.LogError("Assing panel first dumbass");
            return;
        }

        panelScaleFactor = Mathf.Clamp(panelScaleFactor, 0, Mathf.Infinity);

        nodes = GetComponentsInChildren<SkillTreeNode>();

        float left = 0f;
        float right = 0f;
        float top = 0f;
        float bottom = 0f;

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].GetComponent<RectTransform>().anchoredPosition.x > right)
            {
                right = nodes[i].GetComponent<RectTransform>().anchoredPosition.x;
            }
            else if (nodes[i].GetComponent<RectTransform>().anchoredPosition.x < left)
            {
                left = nodes[i].GetComponent<RectTransform>().anchoredPosition.x;
            }

            if (nodes[i].GetComponent<RectTransform>().anchoredPosition.y > top)
            {
                top = nodes[i].GetComponent<RectTransform>().anchoredPosition.y;
            }
            else if (nodes[i].GetComponent<RectTransform>().anchoredPosition.y < bottom)
            {
                bottom = nodes[i].GetComponent<RectTransform>().anchoredPosition.y;
            }
        }

        left *= panelScaleFactor;
        right *= panelScaleFactor;
        top *= panelScaleFactor;
        bottom *= panelScaleFactor;


        //Set left and bottom
        panel.offsetMin = new Vector2(left, bottom);

        //Set right and top
        panel.offsetMax = new Vector2(right, top);

        //Recenter panel
        Vector2 offset = panel.anchoredPosition;
        panel.offsetMin = new Vector2(left - offset.x, bottom - offset.y);
        panel.offsetMax = new Vector2(right - offset.x, top - offset.y);
        
    }

    [ContextMenu("Begin Update Chain")]
    public void BeginUpdateChain()
    {
        firstNode.UpdateNode();

        Invoke("SavePurchasedData", 0.01f);
    }

    public void SavePurchasedData()
    {
        List<string> skills = new List<string>();
        List<bool> skillsPurchased = new List<bool>();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].connectorNode || nodes[i].id.Equals("")) continue;

            skills.Add(nodes[i].id);
            skillsPurchased.Add(nodes[i].purchased);
        }

        SavedDataManager.instance.Skills = skills.ToArray();
        SavedDataManager.instance.SkillsPurchased = skillsPurchased.ToArray();
    }

    private void LoadPurchasedData()
    {
        Dictionary<string, bool> data = new Dictionary<string, bool>();

        string[] skills = SavedDataManager.instance.Skills;
        bool[] skillsPurchased = SavedDataManager.instance.SkillsPurchased;

        for (int i = 0; i < skills.Length && i < skillsPurchased.Length; i++)
        {
            if (skills[i] == null) continue;

            data.Add(skills[i], skillsPurchased[i]);
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].connectorNode) continue;

            if (data.ContainsKey(nodes[i].id))
            {
                nodes[i].purchased = data[nodes[i].id];
            }
        }
    }

    [ContextMenu("Reset")]
    public void ResetPurchasedData()
    {
        if (!Application.isPlaying) return;

        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i].purchased = false;
            nodes[i].isAvailable = false;
            nodes[i].madeConnections = false;
        }

        firstNode.isAvailable = true;
        if (firstNode.connectorNode) firstNode.purchased = true;

        UILineRenderer[] lines = lineRenderersParent.GetComponentsInChildren<UILineRenderer>();
        for (int i = 0; i < lines.Length; i++)
        {
            Destroy(lines[i].gameObject);
        }

        BeginUpdateChain();
    }

    public void SetSelectedNode(SkillTreeNode node)
    {
        selectedNode = node;

        if (selectedNode.purchased)
        {
            infoParent.SetActive(false);
            return;
        }

        infoParent.SetActive(true);

        infoTitleText.text = node.title;
        infoCostText.text = "Cost: " + node.cost;
        infoDescriptionText.text = node.description;

        infoBuyButton.interactable = SavedDataManager.instance.SkillPoints >= node.cost && node.isAvailable;
    }

    public void Buy()
    {
        if (selectedNode == null) return;

        SkillTreeNodeBuyResult result = selectedNode.Buy();

        if (result == SkillTreeNodeBuyResult.Success)
        {
            infoParent.SetActive(false);
        }
    }

    public void ExitScene()
    {
        SavePurchasedData();
        LevelLoader.instance.LoadScene("MainLevel", "ReloadMain");
    }

    public void LoadMainMenu()
    {
        LevelLoader.instance.LoadScene("MainMenu", "ReloadMain");
    }

    public void UpdateSkillPoints()
    {
        skillPointsDisplayText.text = "Skill Points: x" + SavedDataManager.instance.SkillPoints;
    }

    public void ShowBuyMenu()
    {
        if (buyMenuAnim.GetBool("isOpen")) return;

        buyMenuAnim.SetBool("isOpen", true);
    }

    public void HideBuyMenu()
    {
        if (!buyMenuAnim.GetBool("isOpen")) return;

        buyMenuAnim.SetBool("isOpen", false);
    }

    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id.Equals(skillPointsID))
        {
            SavedDataManager.instance.SkillPoints += 20;
            HideBuyMenu();
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning(product.definition.id + " failed because " + failureReason);
    }

    public void ShowPrestigeMenu()
    {
        if (prestigeMenuAnim.GetBool("isOpen")) return;

        prestigeMenuAnim.SetBool("isOpen", true);
    }

    public void HidePrestigeMenu()
    {
        if (!prestigeMenuAnim.GetBool("isOpen")) return;

        prestigeMenuAnim.SetBool("isOpen", false);
    }

    public void Prestige()
    {
        bool success = SavedDataManager.instance.PrestigeUp();

        if (success)
        {
            LevelLoader.instance.LoadScene("MainMenu", "ReloadMain");
        }
    }

    #region Debug Code

    public void ResetExperience()
    {
        SavedDataManager.instance.ResetExperience();
    }

    public void ResetSkillPoints()
    {
        SavedDataManager.instance.SkillPoints = 0;
    }

    public void LoadCrystalBossScene()
    {
        SavedDataManager.instance.DungeonExperience = DungeonLevelToExperience(Mathf.Clamp(2, 1, 100)) - 1;
        LevelLoader.instance.LoadScene("CrystalBossScene", "CrystalBoss");
    }

    public void LoadSpikeBossScene()
    {
        SavedDataManager.instance.DungeonExperience = DungeonLevelToExperience(Mathf.Clamp(4, 1, 100)) - 1;
        LevelLoader.instance.LoadScene("SpikeBossScene", "SpikeBoss");
    }

    public void LoadXeroBossScene()
    {
        SavedDataManager.instance.DungeonExperience = DungeonLevelToExperience(Mathf.Clamp(3, 1, 100)) - 1;
        LevelLoader.instance.LoadScene("XeroBossScene", "XeroBoss");
    }

    private int LevelToExperience(int level)
    {
        //return (int)Mathf.Pow(level, 3);
        return level * 1000;
    }

    public void SetLevel()
    {
        if (levelInput.text.Length <= 0) return;

        int newLevel = System.Int32.Parse(levelInput.text) + 4;

        SavedDataManager.instance.PlayerExperience = LevelToExperience(Mathf.Clamp(newLevel, 1, 100));
    }

    private int DungeonLevelToExperience(int level)
    {
        //return (int)(1000 * Mathf.Pow(level - 1, 2));
        return (level - 1) * 1500;
    }

    public void SetDungeonRank()
    {
        if (dungeonRankInput.text.Length <= 0) return;

        int newLevel = System.Int32.Parse(dungeonRankInput.text);

        SavedDataManager.instance.DungeonExperience = DungeonLevelToExperience(Mathf.Clamp(newLevel, 1, 100));
    }

    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(SkillTree))]
class SkillTreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SkillTree skillTree = (SkillTree)target;

        if (GUILayout.Button("Update Panel Size"))
        {
            skillTree.UpdatePanelSize();
        }
    }
}
#endif