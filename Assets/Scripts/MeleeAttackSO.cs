using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MeleeAttackSO", menuName = "My Scriptable Objects/MeleeAttackSO")]
public class MeleeAttackSO : BasicAttackSO
{
    public float knockback;
    public GameObject hitEffect;
    public MeleeSizeUpgrade[] sizeUpgrades;

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
