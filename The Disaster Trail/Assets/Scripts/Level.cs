using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {

	public string lName, desc;
	public bool unlocked = true;
	public int num;
	public Scene scene;

	// Use this for initialization
	void Start () {
		num = transform.GetSiblingIndex();
	}

	// Update is called once per frame
	void Update () {
		
	}
}
