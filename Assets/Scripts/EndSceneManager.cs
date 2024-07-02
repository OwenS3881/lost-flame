using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject[] objectsToDisable;
    [SerializeField] private GameObject[] otherActiveUIObjects;
    [SerializeField] private GameObject otherFlamePrefab;
    [SerializeField] private Transform otherFlameSpawnPointsParent;

    [SerializeField] private Transform firstDialogueTriggerPoint;
    private bool hasFirstDialogueStarted;
    private bool hasFirstDialogueEnded;
    [SerializeField] private Dialogue firstDialogue;

    private bool ended;

    private void Start()
    {
        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            objectsToDisable[i].SetActive(false);
        }

        foreach (Transform t in otherFlameSpawnPointsParent.GetComponentsInChildren<Transform>())
        {
            if (t.Equals(otherFlameSpawnPointsParent)) continue;

            Instantiate(otherFlamePrefab, t.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || ended) return;

        Player.instance.DisableInteractiveUI();

        foreach (GameObject g in otherActiveUIObjects)
        {
            g.SetActive(false);
        }

        GetComponent<Animator>().SetTrigger("End");
        ended = true;
    }

    public void ExitScene()
    {
        SavedDataManager.instance.FinishedGame = true;
        LevelLoader.instance.LoadScene("MainMenu", "Death");
    }

    private void Update()
    {
        if (!hasFirstDialogueStarted && Player.instance.transform.position.y > firstDialogueTriggerPoint.position.y)
        {
            firstDialogue.TriggerDialogue();
            hasFirstDialogueStarted = true;
            Player.instance.DisableInteractiveUI();
        }

        if (!hasFirstDialogueEnded && hasFirstDialogueStarted && DialogueManager.instance.CurrentDialogue == null)
        {
            Player.instance.EnableInteractiveUI();
            hasFirstDialogueEnded = true;
        }
    }
}
