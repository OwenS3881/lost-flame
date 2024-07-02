using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static MyFunctions;
using UnityEngine.UI;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance { get; private set; }

    private Animator anim;
    private string animationToBePlayed;
    private Slider progressBar;
    private TMP_Text progressText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        anim = GetComponent<Animator>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #region LoadScene Overloads
    public void LoadScene(string sceneName)
    {
        StartCoroutine(Load(sceneName, "N/A", "N/A"));
    }

    public void LoadScene(string sceneName, string animation)
    {
        StartCoroutine(Load(sceneName, animation, animation));
    }

    public void LoadScene(string sceneName, string exitAnimation, string enterAnimation)
    {
        StartCoroutine(Load(sceneName, exitAnimation, enterAnimation));
    }
    #endregion

    IEnumerator Load(string sceneName, string exitAnimation, string enterAnimation)
    {
        if (!exitAnimation.Equals("N/A") && !exitAnimation.Equals(""))
        {
            anim.Play("LevelLoader-" + exitAnimation + "-Exit");
        }

        yield return null;
        yield return null;

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        animationToBePlayed = enterAnimation;

        progressBar = GetComponentInChildren<Slider>(false);

        if (progressBar == null)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            progressText = progressBar.GetComponentInChildren<TMP_Text>();

            progressBar.minValue = 0;
            progressBar.maxValue = 100;

            SetProgress(0);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                SetProgress(progress * 100);

                yield return null;
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (animationToBePlayed != null && !animationToBePlayed.Equals("") && !animationToBePlayed.Equals("N/A"))
        {
            anim.Play("LevelLoader-" + animationToBePlayed + "-Enter");
        }
    }

    private void SetProgress(float value)
    {
        progressBar.value = value;
        progressText.text = value + "%";
    }

    private void ResetSlider()
    {
        SetProgress(0);
    }
}
