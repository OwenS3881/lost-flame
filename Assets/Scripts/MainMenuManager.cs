using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance { get; private set; }

    [SerializeField] private GameObject otherFlamesParent;

    private int debugPresses;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple " + GetType() + "s found in the scene");
        }
    }

    private void Start()
    {
        if (!AudioManager.instance.activeSong.Equals("Song1"))
        {
            AudioManager.instance.PlaySong("Song1");
        }

        otherFlamesParent.SetActive(SavedDataManager.instance.FinishedGame);
    }

    public void OnStartButtonPressed()
    {
        LevelLoader.instance.LoadScene("MainLevel", "ReloadMain");
    }

    public void OnSkillTreeButtonPressed()
    {
        LevelLoader.instance.LoadScene("SkillTree", "ReloadMain");
    }

    public void OnViewCutsceneButtonPressed()
    {
        LevelLoader.instance.LoadScene("OpeningCutscene", "Death");
    }

    [ContextMenu("Reset Cutscene Pref")]
    public void ResetCutscenePref()
    {
        PlayerPrefs.SetInt("HasViewedCutscene", 0);
    }

    public void DebugPress()
    {
        debugPresses++;
        if (debugPresses == 5)
        {
            PlayerPrefs.SetInt("DebugMode", 1);
        }
        else
        {
            PlayerPrefs.SetInt("DebugMode", 0);
        }
    }
}
