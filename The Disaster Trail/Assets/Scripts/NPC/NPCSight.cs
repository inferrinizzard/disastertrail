using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSight : MonoBehaviour
{
    public GameObject NPC;

    // The image element to billboard to the player view
    public GameObject DialogueBubble;

    // A text to prompt the player to interact with NPC
    public GameObject interactText;

    private bool isInteractTextActive = false;
    private bool isTextBubbleActive = false;

    void Update()
    {
        // Keeps the interact text on top of the NPC's head as long the player is in its sight zone
        if (isInteractTextActive)
        {
            // Billboards the interact text to the player view
            Vector3 interactTextPos = Camera.main.WorldToScreenPoint(NPC.transform.position + NPC.GetComponent<NPCController>().InteractTextOffset);
            interactText.transform.position = interactTextPos;
        }

        // Keeps the dialogue bubble text on top of the NPC's head as long the player is in its sight zone
        if (isTextBubbleActive)
        {
            // Billboards the text bubble to the player view
            Vector3 textPos = Camera.main.WorldToScreenPoint(NPC.transform.position + NPC.GetComponent<NPCController>().DialogueBubbleOffset);
            DialogueBubble.transform.position = textPos;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // If NPC sees player, prompt player to interact by displaying interact text
        if (other.tag == "Player")
        {
            Debug.Log(NPC.name);
            isInteractTextActive = true;
            if (isTextBubbleActive)
            {
                interactText.SetActive(false);
            } else
            {
                interactText.SetActive(true);
            }
        }

        // If the NPC "sees" the player and the player is holding down E
        // initiate dialogue with the player
        if (other.tag == "Player" && Input.GetKeyDown(KeyCode.E))
        {
            isTextBubbleActive = true;
            DialogueBubble.SetActive(true);
            DialogueTrigger dialogue = NPC.GetComponent<DialogueTrigger>();
            dialogue.triggerDialogue();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            // Deactivate all NPC texts when the player is no longer in its field of view
            interactText.SetActive(false);
            DialogueBubble.SetActive(false);
            isInteractTextActive = false;
            isTextBubbleActive = false;
        }
    }

}
