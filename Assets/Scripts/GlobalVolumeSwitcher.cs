using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

[RequireComponent(typeof(Volume))]
public class GlobalVolumeSwitcher : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile[] profiles;

    private void Start()
    {
        if (volume == null) volume = GetComponent<Volume>();

        try
        {
            volume.profile = profiles[SavedDataManager.instance.GetCurrentDungeonRank() - 1];
        }
        catch (Exception e)
        {
            if (e is IndexOutOfRangeException)
            {
                volume.profile = profiles[0];
            }
            else
            {               
                throw e;
            }
        }
    }
}
