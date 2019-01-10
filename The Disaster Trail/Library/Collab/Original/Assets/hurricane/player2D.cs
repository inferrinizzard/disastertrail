using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player2D : MonoBehaviour
{

	public float speed = 1;
	public float rotSpeed = 2;
	public float dist = 0;
	public float maxD = 10;

	[SerializeField] private Transform rotateWrapper;

	private Rigidbody2D rb;
	public GameObject safe;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		if(Input.GetKeyDown(KeyCode.RightArrow))
			transform.Translate(new Vector3(1,0,0));

	}

  void Update(){
		dist = Vector3.Distance(transform.position,safe.transform.position);
	}
}