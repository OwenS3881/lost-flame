using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance { get; private set; }

    private Queue<string> sentences;
    private Queue<Sprite> icons;
    private Queue<string> names;
    private Queue<int> textSlownesses;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text dialogueText;

    public Dialogue CurrentDialogue { get; private set; }

    private Animator anim;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        sentences = new Queue<string>();
        names = new Queue<string>();
        icons = new Queue<Sprite>();
        textSlownesses = new Queue<int>();

        anim = GetComponent<Animator>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        sentences.Clear();
        names.Clear();
        icons.Clear();
        textSlownesses.Clear();

        CurrentDialogue = dialogue;

        anim.SetBool("active", true);

        foreach (Sentence sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence.sentence);
            names.Enqueue(sentence.name);
            icons.Enqueue(sentence.icon);
            textSlownesses.Enqueue(sentence.textSlowness);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0 || names.Count == 0 || icons.Count == 0 || textSlownesses.Count == 0)
        {
            EndDialogue();
            return;
        }

        Sentence sentence = new Sentence(names.Dequeue(), sentences.Dequeue(), icons.Dequeue(), textSlownesses.Dequeue());

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        
        nameText.text = sentence.name;

        if (sentence.icon != null)
        {
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = sentence.icon;
        }
        else
        {
            iconImage.gameObject.SetActive(false);
        }
    }

    IEnumerator TypeSentence(Sentence sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.sentence.ToCharArray())
        {
            dialogueText.text += letter;
            for (int i = 0; i < sentence.textSlowness; i++)
            {
                yield return null;
            }
        }
    }

    public void EndDialogue()
    {
        anim.SetBool("active", false);
        CurrentDialogue = null;
    }
}
