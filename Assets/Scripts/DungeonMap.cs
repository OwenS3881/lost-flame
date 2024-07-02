using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMap : MonoBehaviour
{
    public bool active;
    public static DungeonMap instance { get; private set; }
    [SerializeField] private GameObject room;
    [SerializeField] private GameObject hallway;
    [SerializeField] private float distanceBetweenRooms;
    [SerializeField] private GameObject mapObject;
    [SerializeField] private Transform mapParent;
    [SerializeField] private GameObject endIcon;
    [SerializeField] private GameObject startIcon;
    [SerializeField] private GameObject treausreIcon;

    private DungeonGenerator dg;
    private Vector2 playerStart;
    private Vector2 mapStart;
    private Transform playerTransform;

    private RectTransform mapObjectRect;

    private float xOffset;
    private float yOffset;

    private Vector2 mapOffset;

    private bool isExpanded;

    private Vector2 initialDimensions;

    [SerializeField] private Vector2 expandedDimensions;

   
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Multiple Dungeon Maps in scene");
        }

        active = true;
    }

    private void Start()
    {
        Invoke("GetInstances", 0.01f);
        Invoke("Initialize", 0.15f);

        mapObjectRect = mapObject.GetComponent<RectTransform>(); 

        if (!SavedDataManager.instance.IsSkillPurchased("map3"))
        {
            GetComponentInChildren<Button>().enabled = false;
        }
        else
        {
            initialDimensions = mapObjectRect.sizeDelta;
        }
    }

    public void OnClick()
    {
        isExpanded = !isExpanded;

        if (isExpanded)
        {
            mapObjectRect.sizeDelta = expandedDimensions;
        }
        else
        {
            mapObjectRect.sizeDelta = initialDimensions;
        }
    }

    /*
    private void SetAnchorToTopRight()
    {
        mapObjectRect.anchorMin = Vector2.one;
        mapObjectRect.anchorMax = Vector2.one;
        mapObjectRect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void SetAnchorToMiddle()
    {
        mapObjectRect.anchorMin = new Vector2(0.5f, 0.5f);
        mapObjectRect.anchorMax = new Vector2(0.5f, 0.5f);
        mapObjectRect.pivot = new Vector2(0.5f, 0.5f);
    }
    */

    void GetInstances()
    {
        dg = DungeonGenerator.instance;
        playerStart = StartPoint.instance.playerSpawn.position;
        mapStart = mapParent.transform.localPosition;
        playerTransform = Player.instance.transform;
    }

    private void Initialize()
    {
        if (dg.dungeonSpawned)
        {
            xOffset = (dg.dungeonArray.GetLength(0) / 4) * distanceBetweenRooms;
            yOffset = (dg.dungeonArray.GetLength(1) / 4) * distanceBetweenRooms;

            for (int x = 0; x < dg.dungeonArray.GetLength(0); x++)
            {
                for (int y = 0; y < dg.dungeonArray.GetLength(1); y++)
                {
                    GameObject newObject = null;
                    if (dg.dungeonArray[x,y] == 1)
                    {
                        newObject = Instantiate(room, mapParent);      
                    }
                    else if (dg.dungeonArray[x,y] == 3)
                    {
                        newObject = Instantiate(hallway, mapParent);
                    }
                    else
                    {
                        continue;
                    }

                    newObject.transform.localPosition = new Vector3((x * distanceBetweenRooms / 2) - xOffset, (y * distanceBetweenRooms / 2) - yOffset, 0);
                
                    if (SavedDataManager.instance.IsSkillPurchased("map2") && dg.endRoom.x == x && dg.endRoom.y == y)
                    {
                        Instantiate(endIcon, newObject.transform);
                    }

                    if (dg.startRoom.x == x && dg.startRoom.y == y)
                    {
                        Instantiate(startIcon, newObject.transform);
                    }

                    if (SavedDataManager.instance.IsSkillPurchased("mapTreasure") && dg.DoesObjectExist(x, y) && dg.FindObjectAtIndex(x, y).GetComponent<RoomData>().containsTreasure)
                    {
                        Instantiate(treausreIcon, newObject.transform);
                    }
                }
            }
        }
        else
        {
            Invoke("Initialize", 0.01f);
        }
    }

    private void Update()
    {
        if (active)
        {
            mapObject.SetActive(true);
        }
        else
        {
            mapObject.SetActive(false);

            active = EndDungeonManager.instance.abilityUnlocks["map"];

            return;
        }

        if (dg == null) return;

        mapOffset = (Vector2)Player.instance.rb.position - playerStart;
        mapOffset *= new Vector2 (distanceBetweenRooms / 3 / dg.transform.localScale.x, distanceBetweenRooms / 3 / dg.transform.localScale.y);
        mapParent.transform.localPosition = mapStart - mapOffset;
    }
}
