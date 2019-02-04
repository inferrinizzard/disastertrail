using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldManager : MonoBehaviour{
    /*
	private Scenes gameToLoad;

	[SerializeField] GameObject spotParent, player;
	GameObject[] spots;

	// Use this for initialization
	void Start(){
		// Fade in from black
		GameManager.instance.FadeFromBlack(GameManager.instance.FadeTransitionSpeedPerFrame, fadeInComplete);

		Debug.Log(GameManager.instance.currentScene);
		spots = new GameObject[spotParent.transform.childCount];
		for(int i=0;i<spots.Length;i++)
			spots[i] = spotParent.transform.GetChild(i).gameObject;
		for(int i=0;i<=GameManager.instance.currentScene;i++){
			spots[i].SetActive(true);
			if(i==GameManager.instance.currentScene){
				spots[i].GetComponent<SphereCollider>().enabled = true;
				if(i>0){
					player.transform.position = spots[i-1].transform.position;
					player.GetComponent<TrailRenderer>().Clear();
					foreach(Vector3 v in GameManager.instance.trail)
						player.GetComponent<TrailRenderer>().AddPosition(v);
				}
			}
		}
		
	}
	
	// Update is called once per frame
	void Update(){
		
	}

	void fadeInComplete(){
		// Play goofy overworld music
		SoundManager.instance.SwitchBGM(BackgroundMusic.OverworldBGM);
		if (SoundManager.instance.bgmState != SoundPlayerState.Playing){
			SoundManager.instance.PlayBGM();
		}
	}

	public void LoadGame(int game){
		player.GetComponent<TrailRenderer>().Clear();
		GameManager.instance.FadeToBlackAndLoad(GameManager.instance.FadeTransitionSpeedPerFrame, game);
	}
	*/
}
