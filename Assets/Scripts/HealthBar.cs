using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    private Slider slider;
    [SerializeField] private Image fill;
    [SerializeField] private Gradient gradient;
    [SerializeField] private float updateRate;
    private int targetHealth;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        targetHealth = health;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        targetHealth = health;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    private void Update()
    {
        if (slider.value != targetHealth)
        {
            if (Mathf.Abs(slider.value - targetHealth) <= 1f)
            {
                slider.value = targetHealth;
            }
            else if (slider.value < targetHealth)
            {
                slider.value += updateRate * Time.deltaTime;
            }
            else if (slider.value > targetHealth)
            {
                slider.value -= updateRate * Time.deltaTime;
            }
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
    }
}
