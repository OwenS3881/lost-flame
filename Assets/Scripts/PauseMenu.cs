using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance { get; private set; }

    private bool isPaused;
    public bool IsPaused
    {
        get
        {
            return isPaused;
        }
        set
        {
            isPaused = value;
            if (pauseParent != null) pauseParent.SetActive(value);
            if (pauseButton != null) pauseButton.SetActive(!value);
        }
    }

    [SerializeField] protected GameObject pauseParent;
    [SerializeField] protected GameObject pauseButton;
    [SerializeField] private Slider volumeSlider;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple " + GetType() + "s in the scene");
        }
        
        SetSlider();

        if (!PlayerPrefs.HasKey("PauseKey"))
        {
            PlayerPrefs.SetInt("PauseKey", (int)KeyCode.Escape);
        }
    }

    protected virtual void Start()
    {
        IsPaused = false;
    }

    public void PlayButtonSound()
    {
        AudioManager.instance.PlayOneShot("ButtonPress");
    }

    public void Pause()
    {
        if (Time.timeScale == 0f) return;

        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void OnValueChanged(float value)
    {
        PlayerPrefs.SetFloat("GlobalVolume", Mathf.Clamp(value, 0, 1));
        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);
    }

    private void SetSlider()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("GlobalVolume", 1);
        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1);
    }

    public void ExitScene()
    {
        LevelLoader.instance.LoadScene("SkillTree", "ReloadMain");
        Time.timeScale = 1f;
    }

    protected virtual void Update()
    {
        if (Application.isMobilePlatform) return;

        if (Input.GetKeyDown((KeyCode)PlayerPrefs.GetInt("PauseKey")))
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (pauseButton != null) pauseButton.SetActive(Time.timeScale != 0f);
    }
}
