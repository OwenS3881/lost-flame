using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHolder : MonoBehaviour
{
    public BasicEnemySO enemySO;

    // Start is called before the first frame update
    void Start()
    {
        if (enemySO != null)
        {
            Spawn();
        }
        else
        {
            Invoke("Start", 0.01f);
        }
    }

    void Spawn()
    {
        BasicEnemy e = Instantiate(enemySO.enemyScript.gameObject, transform).GetComponent<BasicEnemy>();
        e.AssignSO(enemySO);
    }

}
