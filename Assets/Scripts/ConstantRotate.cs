using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class ConstantRotate : MonoBehaviour
{
    [SerializeField] private float speed;

    // Update is called once per frame
    void Update()
    {
        SetRotation(transform, 2, transform.eulerAngles.z + (speed * Time.deltaTime));
    }
}
