using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPoint : MonoBehaviour
{
    public static StartPoint instance { get; private set; }
    public Transform playerSpawn;
    [Header("Leave this field blank in order to let another script spawn the player")]
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple start points in scene");
        }

        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            PlacePlayer();
        }
    }

    public void PlacePlayer()
    {
        Player.instance.transform.position = playerSpawn.position;
    }
}
