using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalBossSceneManager : BossSceneManager
{
    public static CrystalBossSceneManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple " + GetType() + "s in this scene");
        }
    }

    public override void FightFinished()
    {
        base.FightFinished();
    }
}
