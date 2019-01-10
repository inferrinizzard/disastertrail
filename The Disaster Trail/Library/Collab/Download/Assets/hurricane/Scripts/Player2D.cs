using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player2D : MonoBehaviour
{
	[SerializeField] private float speed = 50f;
	[SerializeField] private float rotSpeed = 10f;
	[SerializeField] private Transform rotateWrapper;

	private bool swapFlashlight;
	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		swapFlashlight = false;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			swapFlashlight = true;
        }else if(Input.GetKeyDown(KeyCode.F)){

        }
	}

	void FixedUpdate()
	{
		float hor = Input.GetAxis("Horizontal");
		float ver = Input.GetAxis("Vertical");

		Vector3 move = new Vector3(hor, 0, ver);
		if (move.magnitude > 0)
		{
			rb.AddForce(move * speed);
		}

		Vector3 mousePos = Input.mousePosition;
		Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3 relativePos = targetPos - transform.position;
		float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;

		rotateWrapper.rotation = Quaternion.Lerp(rotateWrapper.rotation, Quaternion.Euler(new Vector3(0, 0, angle - 90f)), rotSpeed * Time.fixedDeltaTime);

		if (swapFlashlight)
		{
			rotateWrapper.GetChild(0).gameObject.SetActive(!rotateWrapper.GetChild(0).gameObject.activeSelf);
			rotateWrapper.GetChild(1).gameObject.SetActive(!rotateWrapper.GetChild(1).gameObject.activeSelf);
			swapFlashlight = false;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("PickUp"))
		{
			return;
		}

        //On trigger enter call inventory collect
        //InventoryManager.inventory.Add(other.gameObject.name, 1);
	}
}
