using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningSceneManager : MonoBehaviour
{
    [SerializeField] private SerializableList<GameObject>[] panelObjects;

    private void Start()
    {
        AudioManager.instance.StopAllSongs();
    }

    public void ActivatePanel(int index)
    {
        for (int i = 0; i < panelObjects.Length; i++)
        {
            for (int j = 0; j < panelObjects[i].collection.Count; j++)
            {
                panelObjects[i].collection[j].SetActive(i == index);
            }
        }
    }

    public void ExitScene()
    {
        PlayerPrefs.SetInt("HasViewedCutscene", 1);
        LevelLoader.instance.LoadScene("MainMenu", "Death");
    }
}
