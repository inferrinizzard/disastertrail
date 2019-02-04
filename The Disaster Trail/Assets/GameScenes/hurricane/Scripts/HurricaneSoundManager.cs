using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurricaneSoundManager : MonoBehaviour
{
	bool soundPlayed = false,
			 isPlaying = false;

	[Header("Placeholder sounds")][SerializeField] AudioClip drawerOpen;
	[SerializeField] AudioClip drawerClose;
	[SerializeField] AudioClip glass;
	[SerializeField] AudioClip stairs;

	// Start is called before the first frame update
	public void Awake()
	{
		//SoundManager.instance.StopBGM();
	}

	public void DrawerSound(bool opening){
		SoundManager.instance.PlaySFX(opening?drawerOpen:drawerClose);
	}

	//alternative to sm
	public void GlassSound(){
		SoundManager.instance.PlaySFX(glass);
	}	

	public void StairSound(){
		SoundManager.instance.PlaySFX(stairs);
	}	
}
