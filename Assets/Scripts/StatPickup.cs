using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class StatPickup : MonoBehaviour
{
    [SerializeField] private StatType type;
    [Tooltip("For attack and defense: value between 1 - 15 \n" +
        "For health: add percent of max health (1% - 100%) \n" +
        "For Speed: add percent of additional speed")]
    [SerializeField] private float boostAmount;
    [SerializeField] private GameObject graphics;
    [SerializeField] private float rotateOffsetRange;
    Player player;

    private void Start()
    {
        SetRotation(graphics.transform, 2, UnityEngine.Random.Range(-rotateOffsetRange, rotateOffsetRange));
        Invoke("GetPlayer", 0.02f);
    }

    void GetPlayer()
    {
        player = Player.instance;
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void Boost()
    {
        GetComponent<Animator>().SetTrigger("Collect");
        if (type == StatType.Attack)
        {
            player.playerSO.attackIV += (int)boostAmount;
            player.playerSO.attackIV = (int)Mathf.Clamp(player.playerSO.attackIV, 0f, 15f);
            player.DetermineStats();
        }
        else if (type == StatType.Defense)
        {
            player.playerSO.defenseIV += (int)boostAmount;
            player.playerSO.defenseIV = (int)Mathf.Clamp(player.playerSO.defenseIV, 0f, 15f);
            player.DetermineStats();
        }
        else if (type == StatType.Health)
        {
            player.AddHealth((int)(boostAmount / 100 * player.maxHealth));
        }
        else if (type == StatType.Speed)
        {
            player.speedMultiplier += boostAmount / 100f;
        }
    }


    /// <summary>
    /// Boosts the player's attack or defense IV or the player's actual health
    /// Attack and Defense: 1-15, Health: percent value 1-100% of max health
    /// </summary>

    public static void Boost(StatType type, float boost)
    {
        if (type == StatType.Attack)
        {
            Player.instance.playerSO.attackIV += (int)boost;
            Player.instance.playerSO.attackIV = (int)Mathf.Clamp(Player.instance.playerSO.attackIV, 0f, 15f);
            Player.instance.DetermineStats();
        }
        else if (type == StatType.Defense)
        {
            Player.instance.playerSO.defenseIV += (int)boost;
            Player.instance.playerSO.defenseIV = (int)Mathf.Clamp(Player.instance.playerSO.defenseIV, 0f, 15f);
            Player.instance.DetermineStats();
        }
        else if (type == StatType.Health)
        {
            Player.instance.AddHealth((int)(boost / 100 * Player.instance.maxHealth));
        }
        else if (type == StatType.Speed)
        {
            Player.instance.speedMultiplier += boost / 100f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Boost();
            AudioManager.instance.PlayOneShot("Pickup");
            EndDungeonManager.instance.AddDungeonRankStat("Pickups Collected", 1);
        }
    }
}
