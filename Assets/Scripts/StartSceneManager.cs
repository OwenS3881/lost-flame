using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    private void Start()
    {
        if (LevelLoader.instance == null)
        {
            Invoke("Start", 0.01f);
            return;
        }

        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);

        if (!PlayerPrefs.HasKey("HasViewedCutscene"))
        {
            PlayerPrefs.SetInt("HasViewedCutscene", 0);
        }

        if (PlayerPrefs.GetInt("HasViewedCutscene", 0) == 1)
        {
            LevelLoader.instance.LoadScene("MainMenu", "", "Death");
        }
        else
        {
            LevelLoader.instance.LoadScene("OpeningCutscene", "", "Death");
        }
    }
}
