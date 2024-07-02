using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public abstract class BossSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnTrigger;
    [SerializeField] private Transform bossSpawnPoint;

    [SerializeField] private Animator gateAnim;
    [SerializeField] private Transform playerGateTrigger;

    [SerializeField] private Animator fightFinishedAnim;

    [SerializeField] private string sceneTransition;

    private bool gateClosed;
    private bool bossSpawned;

    private CinemachineTargetGroup targetGroup;

    private void Start()
    {
        targetGroup = FindObjectOfType<CinemachineTargetGroup>();
        AudioManager.instance.StopAllSongs();
    }

    private void CloseGate()
    {
        if (gateClosed) return;

        gateAnim.SetTrigger("Close");

        AudioManager.instance.Play("GateClose");

        gateClosed = true;
    }

    private void OpenGate()
    {
        gateAnim.SetTrigger("Open");
    }

    private void SpawnBoss()
    {
        if (bossSpawned) return;

        targetGroup.AddMember(bossSpawnPoint, 1, 6);

        Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);

        AudioManager.instance.PlaySong("BossTheme");

        Invoke("IntroAnimationDone", 1f);

        bossSpawned = true;
    }

    private void IntroAnimationDone()
    {
        targetGroup.RemoveMember(bossSpawnPoint);
    }

    public virtual void FightFinished()
    {
        Player.instance.DisableInteractiveUI();

        SavedDataManager.instance.CompleteRankUpChallenge();

        fightFinishedAnim.Play("FightFinishedCanvas-main");
    }

    public void ExitScene()
    {
        LevelLoader.instance.LoadScene("SkillTree", sceneTransition);
    }

    private void Update()
    {
        if (!gateClosed && Player.instance.transform.position.y > playerGateTrigger.position.y)
        {
            CloseGate();
        }

        if (!bossSpawned && Player.instance.transform.position.y > bossSpawnTrigger.position.y)
        {
            SpawnBoss();
        }
    }
}
