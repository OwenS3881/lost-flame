using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoPostProcessingCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera noPPCamera;

    private void Update()
    {
        noPPCamera.orthographicSize = mainCamera.orthographicSize;
    }
}
