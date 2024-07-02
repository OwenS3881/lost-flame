using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BoomerangAttackSO", menuName = "My Scriptable Objects/BoomerangAttackSO")]
public class BoomerangAttackSO : BasicAttackSO
{
    public float speed;
    public float returnSpeed;
    public float knockback;
    [Tooltip("After this amount of time, the boomerang will begin to return")]
    public float returnTime;
    public float turnTime;
    public float decayTime;
    public float aggressiveReturnDistance;
    public float hasReturnedDistance;
    public float shrinkTime;
    public GameObject boomerang;

    private void Awake()
    {
        attackType = AttackType.Boomerang;
    }

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
