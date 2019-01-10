using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    /*
     * TODO:
     *  connect to UI 
     */

    // The NPC's corresponding name and diaglogue text to show to player
    // during a conversation
    public Text NPCName;
    public Text NPCDialogueText;

    // Holds every sentence in a dialogue as strings
    private Queue<string> sentences;

    void Start()
    {
        sentences = new Queue<string>();
    }

    // Enters each sentence from a dialogue class into the sentence queue
    // Then displays the first sentence in that queue
    public void StartDialogue(Dialogue dialogue){
        NPCName.GetComponent<Text>().text = dialogue.GetName();
        Debug.Log("Starting conversations with " + dialogue.GetName());

        sentences.Clear();

        foreach (string sentence in dialogue.GetSentence())
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence(){
        if(sentences.Count == 0){
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        NPCDialogueText.GetComponent<Text>().text = sentence;
        Debug.Log(sentence);
    } 

    void EndDialogue(){
        Debug.Log("Ending Dialogue");
    }
}
