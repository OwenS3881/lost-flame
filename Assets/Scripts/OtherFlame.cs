using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MyFunctions;

public class OtherFlame : MonoBehaviour
{
    [Header("Randomization")]
    [SerializeField] private Vector2 sizeRange;
    [SerializeField] private Gradient colorRange;

    private Animator anim;
    private SpriteRenderer sr;

    [SerializeField] private bool setDirectionInInspector;
    [SerializeField] private bool defaultDirection;

    private void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (anim != null) anim.Play("Idle", 0, UnityEngine.Random.Range(0f, 1f));

        sr.color = colorRange.Evaluate(UnityEngine.Random.Range(0f, 1f));

        float newSize = UnityEngine.Random.Range(sizeRange.x, sizeRange.y);
        transform.localScale = new Vector2(newSize, newSize);

        if (!setDirectionInInspector) defaultDirection = UnityEngine.Random.Range(0f, 1f) > 0.5f;
    }

    private void Update()
    {
        if (Player.instance != null)
        {
            if (Player.instance.transform.position.x - transform.position.x > 0) //Player is to the right
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else //Player is to the left
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }
        }
        else
        {
            if (defaultDirection)
            {
                SetScale(transform, 0, Mathf.Abs(transform.localScale.x));
            }
            else
            {
                SetScale(transform, 0, -Mathf.Abs(transform.localScale.x));
            }
        }
    }
}
