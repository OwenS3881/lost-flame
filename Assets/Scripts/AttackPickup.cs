using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPickup : MonoBehaviour
{
    [SerializeField] private BasicAttackSO newAttack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            foreach(BasicAttack b in player.gameObject.GetComponentsInChildren<BasicAttack>())
            {
                Destroy(b.gameObject);
            }
            player.playerSO.attackSO = newAttack;
            player.Start();
            Destroy(gameObject);
        }
    }
}
