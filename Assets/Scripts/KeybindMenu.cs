using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeybindMenu : PauseMenu
{
    [SerializeField] private TMP_Text moveKeyText;
    [SerializeField] private TMP_Text attackKeyText;

    private string changingKey = "";

    protected override void Awake()
    {
        if (Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }
    }

    protected override void Start()
    {
        base.Start();
        moveKeyText.text = ((KeyCode)PlayerPrefs.GetInt("MovementKey")).ToString();
        attackKeyText.text = ((KeyCode)PlayerPrefs.GetInt("AttackKey")).ToString();
    }

    public void ModifyKey(string keyname)
    {
        changingKey = keyname;

        if (changingKey.Equals("MovementKey"))
        {
            moveKeyText.text = "Press any Key";
        }
        else if (changingKey.Equals("AttackKey"))
        {
            attackKeyText.text = "Press any Key";
        }
    }

    protected override void Update()
    {
        if (pauseButton != null) pauseButton.SetActive(Time.timeScale != 0f);

        if (!changingKey.Equals("") && Input.anyKey)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(key))
                {
                    PlayerPrefs.SetInt(changingKey, (int)key);

                    if (changingKey.Equals("MovementKey"))
                    {
                        moveKeyText.text = key.ToString();
                    }
                    else if (changingKey.Equals("AttackKey"))
                    {
                        attackKeyText.text = key.ToString();
                    }

                    changingKey = "";
                    break;
                }
            }
        }
    }
}
