using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ShockwaveAttackSO", menuName = "My Scriptable Objects/ShockwaveAttackSO")]
public class ShockwaveAttackSO : BasicAttackSO
{
    public float knockback;
    public GameObject shockwaveEffectPrefab;

    public override GameObject[] CreateUI(GameObject canvas)
    {
        //Clear
        foreach (Transform t in canvas.transform.GetComponentsInChildren<Transform>())
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
