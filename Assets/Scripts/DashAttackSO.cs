using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DashAttackSO", menuName = "My Scriptable Objects/DashAttackSO")]
public class DashAttackSO : BasicAttackSO
{
    public float dashSpeed;
    public float dashTime;
    public float knockback;
    public Sprite dashSprite;  

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
