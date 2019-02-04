using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FloodSoundManager : MonoBehaviour
{

	private bool soundPlayed = false,
							 isPlaying = false;

	[SerializeField] private AudioClip[] directionAudio;
	[Header("Placeholder sounds")][SerializeField] private AudioClip[] obstacleSounds;

	public void Awake()
	{
		SoundManager.instance.StopBGM();
	}

	public void DirectionAudio(int direction)
	{

		if(SoundManager.instance.sfxState != SoundPlayerState.Playing)
		{
				SoundManager.instance.PlaySFX(directionAudio[direction]);
		}
		else if(SoundManager.instance.currClip != directionAudio[direction])
		{
				SoundManager.instance.StopSFX(SoundManager.instance.currSFX);
				SoundManager.instance.PlaySFX(directionAudio[direction]);
		}
	}

	public void ObstacleSound(int index){
		if(SoundManager.instance.sfxState != SoundPlayerState.Playing)
		{
			//TODO: delay until animation finishes
			SoundManager.instance.PlaySFX(obstacleSounds[index]);
		}
	}
}
