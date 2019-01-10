using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    [SerializeField] private string name;
    //public string[] sentences;
    [Tooltip("Splits by new line")]
    [SerializeField] private TextAsset dialogueText;

    private string[] sentences;

    public string[] GetSentence(){

        return sentences = dialogueText.text.Split('\n', '\r');
    }

    public string GetName(){
        return name;
    }


}
