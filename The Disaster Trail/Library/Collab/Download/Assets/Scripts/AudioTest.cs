using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour {

	public AudioSource rain;
	public PlayerTopDown player;
	public float vol = 0;
	// Use this for initialization
	void Start () {
		rain = GameObject.Find("Rain").GetComponent<AudioSource>();
		player = this.GetComponent<PlayerTopDown>();
	}
	
	// Update is called once per frame
	void Update () {
		//rain.volume = Mathf.Min(0.05f,player.dist/player.maxD);
	}
}
