using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private GameObject play;
    [SerializeField] private GameObject miniGame;
    [SerializeField] private GameObject flood;
    [SerializeField] private GameObject hurricane;
    [SerializeField] private GameObject back;

    // Start is called before the first frame update
    void Start()
    {
        flood.SetActive(false);
        hurricane.SetActive(false);
        back.SetActive(false);
    }

    public void OnPlay()
    {
        GameManager.instance.FadeToBlackAndLoad(.1f, 2);
        GameManager.instance.storyMode = true;
    }

    public void OnMiniGame()
    {
        play.SetActive(false);
        miniGame.SetActive(false);
        flood.SetActive(true);
        hurricane.SetActive(true);
        back.SetActive(true);
    }

    public void OnFlood()
    {
        GameManager.instance.FadeToBlackAndLoad(.1f, 6);
        flood.SetActive(false);
        hurricane.SetActive(false);
    }

    public void OnHurricane()
    {
        GameManager.instance.FadeToBlackAndLoad(.1f, 7);
        flood.SetActive(false);
        hurricane.SetActive(false);
    }

    public void OnBack()
    {
        play.SetActive(true);
        miniGame.SetActive(true);
        flood.SetActive(false);
        hurricane.SetActive(false);
        back.SetActive(false);
    }
}
