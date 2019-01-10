using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour {

	public int health = 100,  food = 100, battery = 100;
	public int curHealth, curFood, curBatt;

	public Slider healthSlider, foodSlider, battSlider;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		healthSlider.value = curHealth;
		foodSlider.value = curFood;
		battSlider.value = curBatt;
	}
}
