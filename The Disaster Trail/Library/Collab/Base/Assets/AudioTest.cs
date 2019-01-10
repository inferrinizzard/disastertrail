using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour {

	public AudioSource rain;
	public player2D player;
	public float vol = 0;
	// Use this for initialization
	void Start () {
		rain = GameObject.Find("Rain").GetComponent<AudioSource>();
		player = this.GetComponent<player2D>();
	}
	
	// Update is called once per frame
	void Update () {
		rain.volume = Mathf.Min(0.05,player.dist/player.maxD);
	}
}
