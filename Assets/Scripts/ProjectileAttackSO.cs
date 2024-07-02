using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ProjectileAttackSO", menuName = "My Scriptable Objects/ProjectileAttackSO")]
public class ProjectileAttackSO : BasicAttackSO
{
    public float speed;
    public float knockback;
    public GameObject deathEffect;
    public GameObject spawnEffect;
    public GameObject projectile;
    public float playerProjectileDestroyTime;

    private void Awake()
    {
        attackType = AttackType.Projectile;
    }

    public override GameObject[] CreateUI(GameObject canvas)
    {
        //Clear
        foreach(Transform t in canvas.transform.GetComponentsInChildren<Transform>())
        {
            if (!t.Equals(canvas.transform))
            {
                Destroy(t.gameObject);
            }
        }

        //Create
        List<GameObject> tempList = new List<GameObject>();
        tempList.Add(Instantiate(uiObjects[0], canvas.transform));
        return tempList.ToArray();
    }

}
