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
    [SerializeField] private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        flood.SetActive(false);
        hurricane.SetActive(false);
        back.SetActive(false);

    }

    public void OnPlay()
    {
        gm.FadeToBlackAndLoad(.1f, 1);
        gm.setStoryMode(true);
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
        gm.FadeToBlackAndLoad(.1f, 3);
        flood.SetActive(false);
        hurricane.SetActive(false);
    }

    public void OnHurricane()
    {
        gm.FadeToBlackAndLoad(.1f, 2);
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
