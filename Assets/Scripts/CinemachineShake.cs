using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake instance { get; private set; }
    private CinemachineVirtualCamera cmVcam;
    private CinemachineBasicMultiChannelPerlin cmVcamNoise;
    private float shakeTimer;
    private float shakeTimerTotal;
    [Tooltip("By enabling this option, the shake will slowly decrease to 0 instead of going from max to 0 when the shake is done")]
    public bool smoothStop;
    private float startingIntensity;
    
    private void Awake()
    {
        instance = this;
        cmVcam = GetComponent<CinemachineVirtualCamera>();
        cmVcamNoise = cmVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float intensity, float time)
    {     
        cmVcamNoise.m_AmplitudeGain = intensity;

        startingIntensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }

    public void ShakeCamera(float intensity, float time, bool smooth)
    {
        smoothStop = smooth;
        ShakeCamera(intensity, time);
    }

    public void StopShaking()
    {
        shakeTimer = 0f;
        cmVcamNoise.m_AmplitudeGain = 0f;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (smoothStop)
            {
                cmVcamNoise.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
            }
            else if (shakeTimer <= 0f)
            {
                cmVcamNoise.m_AmplitudeGain = 0f;
                
            }
        }
    }
}
