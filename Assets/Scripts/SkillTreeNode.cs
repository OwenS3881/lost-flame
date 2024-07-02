using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class SkillTreeNode : MonoBehaviour
{
    [SerializeField] public bool connectorNode;

    public string title;
    public string id;
    public int cost;
    [TextArea]
    public string description;

    [ReadOnly]public bool isAvailable;
    [ReadOnly, SerializeField] private bool affordable;
    public bool purchased;
    [HideInInspector] public bool madeConnections;

    public SkillTreeNode[] connectedNodes;

    [HideInInspector] public SkillTree skillTree;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image bg;
    [SerializeField] private Button button;
    [SerializeField] private Image[] icons;
    private Color[] iconDefaults;

    [SerializeField] private UnityEvent onBuy;

    private void Awake()
    {
        titleText.text = title;

        iconDefaults = new Color[icons.Length];

        for (int i = 0; i < icons.Length; i++)
        {
            iconDefaults[i] = icons[i].color;

            if (icons[i].sprite == null)
            {
                icons[i].enabled = false;
            }
        }

        if (connectorNode)
        {
            titleText.gameObject.SetActive(false);

            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].gameObject.SetActive(false);
            }

            bg.enabled = false;
        }
    }

    private void OnValidate()
    {
        if (connectorNode)
        {
            if (bg != null) bg.color = new Color(1, 1, 1, 0.25f);
            for (int i = 0; i < icons.Length; i++) 
            {
                if (icons[i] != null)
                {
                    icons[i].enabled = false;
                }
            }
            if (titleText != null) titleText.text = "";
        }
        else
        {
            if (bg != null) bg.color = new Color(1, 1, 1, 1);
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                {
                    icons[i].enabled = true;
                }
            }
            if (titleText != null) titleText.text = "Name:";
        }
    }

    public void Select()
    {
        skillTree.SetSelectedNode(this);
    }

    [ContextMenu("Set To Defaults")]
    private void SetToDefaults()
    {
        title = "";
        cost = 0;
        description = "";
        connectedNodes = new SkillTreeNode[0];
    }

    public SkillTreeNodeBuyResult Buy()
    {
        if (SavedDataManager.instance.SkillPoints < cost)
        {
            return SkillTreeNodeBuyResult.TooExpensive;
        }
        else if (!isAvailable)
        {
            return SkillTreeNodeBuyResult.NotAvailable;
        }

        SavedDataManager.instance.SkillPoints -= cost;
        purchased = true;
        skillTree.BeginUpdateChain();
        onBuy.Invoke();

        return SkillTreeNodeBuyResult.Success;
    }

    public void UpdateNode()
    {
        affordable = SavedDataManager.instance.SkillPoints >= cost;

        if (isAvailable)
        {
            if (purchased || connectorNode)
            {
                Purchased();
            }
            else if (affordable)
            {
                Affordable();
            }
            else
            {
                NotAffordable();
            }
        }
        else
        {
            NotAvailable();
        }

        UpdateConnectedNodes();
    }

    private void NotAvailable()
    {
        bg.color = skillTree.notAvailableColor;
        button.interactable = true;
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].color = new Color(iconDefaults[i].r, iconDefaults[i].g, iconDefaults[i].b, skillTree.notAvailableColor.a);
        }
    }

    private void Purchased()
    {
        bg.color = skillTree.purchasedColor;

        button.interactable = false;
        
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].color = new Color(iconDefaults[i].r, iconDefaults[i].g, iconDefaults[i].b, 1);
        }
        
        if (!madeConnections)
        {
            for (int i = 0; i < connectedNodes.Length; i++)
            {
                UILineRenderer connection = Instantiate(skillTree.defaultUILineRenderer, skillTree.lineRenderersParent).GetComponent<UILineRenderer>();
                List<Vector2> positions = new List<Vector2>();
                positions.Add(GetComponent<RectTransform>().anchoredPosition);
                positions.Add(connectedNodes[i].GetComponent<RectTransform>().anchoredPosition);
                connection.SetPositions(positions);

                connectedNodes[i].isAvailable = true;
            }
            
            madeConnections = true;
        }
    }

    private void Affordable()
    {
        bg.color = skillTree.affordableColor;
        button.interactable = true;
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].color = new Color(iconDefaults[i].r, iconDefaults[i].g, iconDefaults[i].b, 1);
        }
    }

    private void NotAffordable()
    {
        bg.color = skillTree.unAffordableColor;
        button.interactable = true;
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].color = new Color(iconDefaults[i].r, iconDefaults[i].g, iconDefaults[i].b, 1);
        }
    }

    private void UpdateConnectedNodes()
    {
        for (int i = 0; i < connectedNodes.Length; i++)
        {
            connectedNodes[i].UpdateNode();
        }
    }
}
