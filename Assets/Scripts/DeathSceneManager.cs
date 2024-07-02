using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathSceneManager : MonoBehaviour
{
    public static DeathSceneManager instance { get; private set; }

    [Header("Components")]
    [SerializeField] private Animator anim;

    [Header("Performance Info")]
    [SerializeField] private GameObject performanceTextPrefab;
    [SerializeField] private Transform performanceInfoParent;

    [Header("Dungeon Rank/Score")]
    [SerializeField] private TMP_Text scoreText;
    [Tooltip("Length of the section of the score reveal animation that includes when the text is being revealed")]
    [SerializeField] private float scoreRevealLength;
    private bool beginScoreTextAnimation;
    private float currentTime;  
    private float currentScore;

    [SerializeField]private float scoreToAdd;
    [SerializeField] private float[] dungeonRankExperienceUpgrades;
    private float dungeonRankExperienceMultiplier = 1;

    [HideInInspector] public int newExperience;

    private bool displayingPerformance;
    [ReadOnly, SerializeField] private bool skipPerformance;
    private bool hasFingerEntered;
    private float fingerEneteredTime;
    [SerializeField] private float maximumClickTime;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple death scene managers in scene");
        }

        newExperience = SavedDataManager.instance.DungeonExperience;

        AudioManager.instance.StopAllSongs();
    }

    private void Start()
    {
        AudioManager.instance.Play("DeathSceneEnter");
        DetermineDungeonRankExperienceMultiplier();
        StartCoroutine(MainSequence());
    }

    private void DetermineDungeonRankExperienceMultiplier()
    {
        for (int i = 0; i < dungeonRankExperienceUpgrades.Length; i++)
        {
            if (SavedDataManager.instance.IsSkillPurchased("dungeonExperienceBoost" + (i + 1)))
            {
                dungeonRankExperienceMultiplier += dungeonRankExperienceUpgrades[i];
            }
            else
            {
                return;
            }
        }
    }

    [ContextMenu("Reset Dungeon Experience")]
    private void ResetDungeonExperience()
    {
        SavedDataManager.instance.DungeonExperience = 0;
    }

    IEnumerator MainSequence()
    {
        yield return null;
        anim.SetTrigger("Reveal");

        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("DeathSceneManager-postreveal"))
        {
            yield return null;
        }

        displayingPerformance = true;

        yield return new WaitForSeconds(1.5f);
       
        foreach (KeyValuePair<string, Vector2> kvp in EndDungeonManager.instance.dungeonRankStats)
        {
            if (skipPerformance) break;

            GameObject pt = Instantiate(performanceTextPrefab, performanceInfoParent);
            pt.GetComponent<PerformanceInfoDisplay>().Initialize(kvp.Key, kvp.Value.x);

            scoreToAdd += kvp.Value.x * kvp.Value.y;

            yield return null;

            yield return new WaitForSeconds(pt.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);           
        }
        displayingPerformance = false;

        yield return new WaitForSeconds(0.5f);

        anim.SetTrigger("ScoreReveal");
        beginScoreTextAnimation = true;

        newExperience += Mathf.RoundToInt(scoreToAdd * dungeonRankExperienceMultiplier);

        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("DeathSceneManager-barreveal"))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        DungeonRankExperienceBar.instance.UpdateExperience(newExperience);
        yield return null;
        DungeonRankExperienceBar.instance.waitingForEnd = true;
    }

    private void ScoreTextAnimation()
    {
        currentScore = Mathf.Lerp(0, scoreToAdd, currentTime / scoreRevealLength);
        scoreText.text = "Score: " + (int)currentScore;
        currentTime += Time.deltaTime;
        if (currentTime >= scoreRevealLength)
        {
            while ((int)currentScore != (int)scoreToAdd)
            {
                if ((int)currentScore > (int)scoreToAdd)
                {
                    currentScore--;
                }
                else
                {
                    currentScore++;
                }
            }
            scoreText.text = "Score: " + (int)currentScore;

            beginScoreTextAnimation = false;
            currentScore = 0;
            currentTime = 0;
            return;
        }
    }

    public void ExitScene()
    {
        LevelLoader.instance.LoadScene("SkillTree");
    }

    private void SkipPerformancePotential()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.fingerId == 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    hasFingerEntered = true;
                }
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    hasFingerEntered = false;
                    if (fingerEneteredTime <= maximumClickTime)
                    {
                        skipPerformance = true;
                        displayingPerformance = false;
                    }
                }
            }
        }

        if (hasFingerEntered)
        {
            fingerEneteredTime += Time.deltaTime;
        }
        else
        {
            fingerEneteredTime = 0f;
        }
    }

    private void Update()
    {
        if (beginScoreTextAnimation)
        {
            ScoreTextAnimation();
        }

        if (displayingPerformance)
        {
            SkipPerformancePotential();
        }
    }
}
