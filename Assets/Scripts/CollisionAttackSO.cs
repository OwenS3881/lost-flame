using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CollisionAttackSO", menuName = "My Scriptable Objects/CollisionAttackSO")]
public class CollisionAttackSO : BasicAttackSO
{
    public float knockbackForce;

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

        return new GameObject[0];
    }
}
