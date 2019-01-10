using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public GameObject dialogueManager;

    [SerializeField] private Dialogue dialogue;

    public void triggerDialogue(){
        dialogueManager.GetComponent<DialogueManager>().StartDialogue(dialogue);
    }
}
