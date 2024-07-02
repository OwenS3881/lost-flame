using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PerformanceInfoDisplay : MonoBehaviour
{
    private string stat;
    private float number;

    [SerializeField] private TMP_Text displayText;

    public void Initialize(string statString, float numberValue)
    {
        stat = statString;
        number = numberValue;

        if (number == (int)numberValue)
        {
            displayText.text = stat + ": " + (int)number;
        }
        else
        {
            displayText.text = stat + ": " + number;
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
