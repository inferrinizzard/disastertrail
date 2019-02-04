using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    // The time scale before paused.
    private float oldTimeScale = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        oldTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    void OnDisable()
    {
        Time.timeScale = oldTimeScale;
    }

    public void Quit()
    {
        GameManager.instance.FadeToBlack(.1f, () =>
		{
			GameManager.instance.LoadScene(Scenes.MainMenu);
		});
    }

    public void Resume()
    {
        gameObject.SetActive(false);
    }

    public void Pause()
    {
        gameObject.SetActive(true);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
