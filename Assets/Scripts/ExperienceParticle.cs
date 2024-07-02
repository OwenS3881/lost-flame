using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class ExperienceParticle : MonoBehaviour
{
    [ReadOnly] public int experiencePoints;
    [SerializeField] private float activateDelay;
    [SerializeField] private GameObject[] particleGraphics;
    private bool dead;
    private bool active;

    private void Start()
    {
        int index = UnityEngine.Random.Range(0, particleGraphics.Length);
        Instantiate(particleGraphics[index], transform);
        Invoke("Activate", activateDelay);
    }

    private void Activate()
    {
        active = true;
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerCode(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TriggerCode(collision);
    }

    void TriggerCode(Collider2D collision)
    {
        if (dead || !active) return;

        if (collision.CompareTag("Player"))
        {
            GetComponentInHierarchy<Player>(collision.transform).GainExperience(experiencePoints);
            dead = true;
            GetComponent<Animator>().SetTrigger("Die");
            AudioManager.instance.PlayOneShot("Pickup");
        }
    }
}
