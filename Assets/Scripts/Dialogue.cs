using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public Sentence[] sentences;

    public bool TriggerDialogue()
    {
        if (DialogueManager.instance == null) return false;

        DialogueManager.instance.StartDialogue(this);
        return true;
    }
}
