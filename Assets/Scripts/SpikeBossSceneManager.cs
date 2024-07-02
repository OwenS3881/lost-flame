using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBossSceneManager : BossSceneManager
{
    public static SpikeBossSceneManager instance { get; private set; }

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
        Player.instance.DisableInteractiveUI();

        Invoke(nameof(Transition), 1f);
    }

    private void Transition()
    {
        LevelLoader.instance.LoadScene("EndScene", "SpikeBoss");
    }
}
