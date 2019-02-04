using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrailManager : MonoBehaviour
{

    [SerializeField] public Transform[] trail;

    //Players car
    [SerializeField] private GameObject car;



    private Transform destination;


    void OnEnable()
    {
        SceneManager.sceneLoaded += this.OnLoadCallback;
    }

    void OnDisable()
    {

        SceneManager.sceneLoaded -= this.OnLoadCallback;
    }


    void OnLoadCallback(Scene scene, LoadSceneMode sceneMode)
    {
        ProgressStory();
    }

    private void ProgressStory()
    {
        //tween car to next trail spot 
        StartCoroutine(waitToLoad(2f));
        LeanTween.move(car, trail[GameManager.instance.getNextTrail()], 2f);
    }

    private IEnumerator waitToLoad(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log(GameManager.instance.getLevel());
        GameManager.instance.FadeToBlackAndLoad(.1f, GameManager.instance.getLevel());
    }

}
