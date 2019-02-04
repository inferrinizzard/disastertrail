using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This component is responsible for playing and mixing car sound effects.
 */
public class SfxCar : MonoBehaviour
{
    [SerializeField] private AudioSource sfxEngineStart;
    [SerializeField] private float startFadeTimeEngineStart = 2.0f;
    [SerializeField] private float endFadeTimeEngineStart = 4.0f;

	[SerializeField] private AudioSource sfxRev;
	[SerializeField] private AudioSource sfxIdle;
	[SerializeField] private AudioSource sfxTurn;
	[SerializeField] private AudioSource sfxCrash;
	[SerializeField] private AudioSource sfxBrake;

    private float timeEngineStart = 0;
    private bool moving = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(sfxEngineStart.isPlaying)
        {
            if(timeEngineStart >= endFadeTimeEngineStart)
            {
                sfxIdle.volume = 1;
                sfxEngineStart.Stop();
            }
            else if(timeEngineStart >= startFadeTimeEngineStart)
            {
                float t = (timeEngineStart - startFadeTimeEngineStart)
                    / (endFadeTimeEngineStart - startFadeTimeEngineStart);
                sfxEngineStart.volume = Mathf.Lerp(1, 0, t);
                sfxIdle.volume = Mathf.Lerp(0, 1, t);
            }
            timeEngineStart += Time.deltaTime;
        }
    }

    public void StartEngine()
    {
        sfxEngineStart.volume = 1;
        sfxEngineStart.Play();
        
        sfxIdle.volume = 0;
        sfxIdle.loop = true;
        sfxIdle.Play();

        timeEngineStart = 0;
    }

    public void Move(float speed)
    {
        sfxIdle.pitch = 1.0f + speed / 100.0f;
        sfxRev.pitch = 1.0f + speed / 100.0f;
        if(!moving && speed >= 0.01f)
        {
            moving = true;
        }
        else if(speed < 0.01f)
        {
            moving = false;
            sfxIdle.pitch = 1.0f;
        }
    }

    public void Rev()
    {
        if(!moving)
        {
            sfxRev.volume = 1;
            sfxRev.Play();
        }
    }

    public void StartBraking()
    {
        sfxBrake.Play();
    }

    public void StopBraking()
    {
        sfxBrake.Stop();
    }

    public void Crash()
    {
        AudioSource sfx = GetRandomAudio(sfxCrash);
        sfx.volume = 1;
        sfx.Play();
    }

    /*
     * Returns a random AudioSource if the game object that holds
     * audioSource has more than one AudioSource component.
     */
    private AudioSource GetRandomAudio(AudioSource audioSource)
    {
        AudioSource[] sources = audioSource.GetComponents<AudioSource>();
        return sources[Random.Range(0, sources.Length)];
    }
}
