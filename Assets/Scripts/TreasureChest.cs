using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    private Animator anim;
    private bool isOpening;

    [SerializeField] private Transform itemSpawnLocation;
    private bool hasItemSpawned;
    private GameObject spawnedItem;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Open()
    {
        if (isOpening) return;

        isOpening = true;

        EndDungeonManager.instance.AddDungeonRankStat("Chests Opened", 1f);
        AudioManager.instance.Play("TreasureOpen");

        anim.SetTrigger("Open");
    }

    private void OnCollected()
    {
        anim.SetTrigger("ItemCollected");
        hasItemSpawned = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Open();
        }
    }

    //Used by animator
    private void SpawnItem()
    {
        TreasureItem item = TreasureManager.instance.GetRandomTreasureItem();

        spawnedItem = Instantiate(item.objectToSpawn, itemSpawnLocation);

        hasItemSpawned = true;
    }

    private void Update()
    {
        if (hasItemSpawned && spawnedItem == null)
        {
            OnCollected();
        }
        else if (hasItemSpawned && spawnedItem != null)
        {
            spawnedItem.transform.position = itemSpawnLocation.position;
        }
    }
}
