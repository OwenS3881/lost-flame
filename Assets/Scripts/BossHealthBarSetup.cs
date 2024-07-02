using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossHealthBarSetup : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image background;
    [SerializeField] private Image border;

    public void Setup(string bossName, Color nameColor, Color backgroundColor, Color borderColor)
    {
        nameText.text = bossName;
        nameText.color = nameColor;
        background.color = backgroundColor;
        border.color = borderColor;
    }
}
