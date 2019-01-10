using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private int sceneNumber;
    [SerializeField] private GameObject guiObject;
    [SerializeField] private GameManager gm;
    [SerializeField] private float transitionTime = 1f;


    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.name == "Player")
        {
            guiObject.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                gm.FadeToBlack(transitionTime, () => gm.LoadScene(sceneNumber, transitionTime));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        guiObject.SetActive(false);
    }


}
