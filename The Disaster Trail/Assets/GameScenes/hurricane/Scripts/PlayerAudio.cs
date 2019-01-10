using System.Collections;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
	[SerializeField] [Range(0f, 1f)] private float volumeMultiplier;
	[SerializeField] [Tooltip("Time to transition between volumes in seconds")] private float volumeChangeDuration;

	private float previousDanger;

	private PlayerTopDown player;
	private IEnumerator transition;

	void Start()
	{
		player = GetComponent<PlayerTopDown>();
		previousDanger = 0;

        // Start hurricane bgm
        SoundManager.instance.SwitchBGM(BackgroundMusic.HurricaneBGM);
        if(SoundManager.instance.bgmState != SoundPlayerState.Playing)
        {
            SoundManager.instance.PlayBGM();
        }

		//only check for volume changes 10 times per second instead of every frame
		StartCoroutine(VolumeCheckOnDelay(.1f));
	}

	private IEnumerator VolumeCheckOnDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);

			float danger = player.GetDangerLevel();

			//transition to new audio volume
			if (danger != previousDanger)
			{
				VolumeTransition(danger / player.GetMaxDanger() * volumeMultiplier, volumeChangeDuration);
			}
		}
	}

	//handle quick transitions properly
	private void VolumeTransition(float volume, float duration)
	{
		if (transition != null)
		{
			StopCoroutine(transition);
		}

		transition = DoVolumeTransition(volume, duration);
		StartCoroutine(transition);
	}

	private IEnumerator DoVolumeTransition(float volume, float duration)
	{
		float from = SoundManager.instance.bgmVolume;
		float progress = 0.0f;

		while (progress < 1)
		{
			progress += Time.unscaledDeltaTime / duration;
			SoundManager.instance.SetBGMVolume(Mathf.Lerp(from, volume, progress));
			yield return null;
		}
	}
}
