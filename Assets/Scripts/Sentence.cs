using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sentence
{
    public string name;
    public Sprite icon;

    [TextArea(3, 10)]
    public string sentence;
    public int textSlowness = 3;

    public Sentence()
    {
        name = "";
        sentence = "";
        icon = null;
        textSlowness = 3;
    }

    public Sentence(string newName, string newSentence, Sprite newIcon, int newTextSlowness)
    {
        name = newName;
        sentence = newSentence;
        icon = newIcon;
        textSlowness = newTextSlowness;
    }

    public override string ToString()
    {
        return name + ": " + sentence;
    }
}
