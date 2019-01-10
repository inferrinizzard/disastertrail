using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class worldPlayer : MonoBehaviour {
	
	[SerializeField] float speed = 10f, rotSpeed;
	float hor, ver;
	[SerializeField] bool onMap;

	Vector3 lastPos;
	Vector3[] trail;
	float radius = .05f;
	[SerializeField] GameObject overworldManager;
	// Use this for initialization
	void Start(){
		OverworldManager om = overworldManager.GetComponent<OverworldManager>();
	}
	
	// Update is called once per frame
	void Update(){
		hor = Input.GetAxis("Horizontal");
		ver = Input.GetAxis("Vertical");

		onMap = Physics.CheckSphere(transform.position,radius,1<<12);
		if(onMap){
			lastPos = transform.position;
			transform.Translate(Vector3.up*ver*speed*Time.deltaTime,Space.Self);
			transform.Rotate(Vector3.back, hor*rotSpeed*Time.fixedDeltaTime);
		}
		else
			transform.position = lastPos;

	}

	void OnTriggerEnter(Collider other){
		if(GameManager.instance.TrackScene(other.gameObject.name)){
			trail = new Vector3[GetComponent<TrailRenderer>().positionCount];
			GetComponent<TrailRenderer>().GetPositions(trail);
			Vector3[] temp = new Vector3[trail.Length+GameManager.instance.trail.Length];
			System.Array.Copy(GameManager.instance.trail,temp,GameManager.instance.trail.Length);
			System.Array.Copy(trail,0,temp,GameManager.instance.trail.Length,trail.Length);
			GameManager.instance.trail = temp;
		}

		Debug.Log(other);
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		radius = 0;
		lastPos = transform.position;
		onMap = false;
		overworldManager.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
		overworldManager.transform.GetChild(0).GetChild(other.transform.GetSiblingIndex()+1).gameObject.SetActive(true);
	}
}
