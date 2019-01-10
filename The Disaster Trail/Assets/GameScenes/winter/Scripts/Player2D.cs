using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2D : MonoBehaviour{

	public float hor, ver, speed = 10f;
	[SerializeField] float jump = 3f, fall = 2.5f, low = 2f;
	[SerializeField] float fadeTime = .75f, shovelCooldown = 0, jumpTimer = 0;
	[SerializeField] GameObject shovel, shoveledSnow;

	public int health = 100, heartRate = 0;
	public bool grounded = true, inside = false, obstacle = false, arrier = false;
	public bool canShovel = false, wet = false, cold = false, icy = false, windy = false, canMove = true, hasNoSnow = false;
	Rigidbody rb;
	Animator animator;
	void Start(){
		shovel.SetActive(false);
		rb = GetComponent<Rigidbody>();
		animator = transform.GetChild(0).GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update() {
		hor = Input.GetAxis("Horizontal");
		ver = Input.GetAxis("Vertical");
		animator.SetBool("isHoldingPhone", false);
		animator.SetFloat("speedPercent", Mathf.Abs(hor / speed) * 3f);
		transform.rotation = arrier&&hor<=0 ? Quaternion.Euler(0,180,0) : Quaternion.Euler(0, 90 - Mathf.Sign(hor) * 90, 0);
		shovel.SetActive(animator.GetBool("showShovel"));
		arrier = obstacle ? arrier : false;
	}

	void FixedUpdate() {
		grounded = Physics.Raycast(transform.position-.05f * Vector3.down, Vector3.down, transform.GetChild(0).GetChild(0).GetComponent<Renderer>().bounds.extents.y+.05f, 1<<12);

		if(hor!=0 && canMove){
			animator.ResetTrigger("shovelTrigger");
			if(icy&&!inside)
				rb.AddForce(Vector3.right*hor*(grounded ? speed*5 : speed/2));
			else
				rb.velocity = (Vector3.right*hor*(grounded ? speed : speed/10)*(windy && hor>0 ? .5f : 1));

			if(rb.velocity.magnitude>speed)
				rb.velocity = Vector3.right*hor*speed;
		}
		if(ver>.3f && grounded && Time.time-jumpTimer>1){
			animator.ResetTrigger("shovelTrigger");
			animator.SetBool("isJumping", true);
			rb.velocity = Vector3.up * jump;
			jumpTimer = Time.time;
		} 
		else
			animator.SetBool("isJumping", false);
			
		if(rb.velocity.y<0)
			rb.velocity += Vector3.up * Physics.gravity.y * (fall-1) * Time.deltaTime;
		else if(rb.velocity.y>0 && !(ver>0))
			rb.velocity += Vector3.up * Physics.gravity.y * (low-1) * Time.deltaTime;

		if(canShovel){
			canMove = false;
			// Display shovel & trigger shovel animations
			shovel.SetActive(true);
			animator.SetTrigger("shovelTrigger");
			
			hasNoSnow = shoveledSnow.transform.parent.childCount==1;
			obstacle = !hasNoSnow;
			Destroy(shoveledSnow);
			shovelCooldown = Time.time;
			canShovel = false;
			heartRate+=10;
		}

		if(animator.GetBool("showShovel") == false)
			canMove = true;
	}

	void OnTriggerEnter(Collider other) {
		if(other.tag == "BuildingCollider" && !inside) {
			inside = true;
			if(other.transform.GetChild(0).GetComponent<MeshRenderer>().material.color.a == 1)
				StartCoroutine(FadeObj(other.transform.GetChild(0).gameObject, fadeTime, 1, 0));
		}
		if(other.tag == "Obstacle"){
			obstacle = true;
			arrier = other.transform.position.x<transform.position.x;
		}
	}

	void OnTriggerStay(Collider other) {
		if(obstacle)
			// if(Input.GetKeyDown(KeyCode.F) && Time.time-(cold ? shovelCooldown*2 : shovelCooldown) > 2){
			if(Input.GetKeyDown(KeyCode.F) && other.tag == "Obstacle" && Time.time-(cold ? shovelCooldown*2 : shovelCooldown) > 2){
				canShovel = true;
				shoveledSnow = other.transform.GetChild(0).gameObject;
			}
	}

	void OnTriggerExit(Collider other) {
		if(other.tag == "BuildingCollider" && inside) {
			inside = false;
			if(other.transform.GetChild(0).GetComponent<MeshRenderer>().material.color.a == 0)
				StartCoroutine(FadeObj(other.transform.GetChild(0).gameObject, fadeTime/2f, 0, 1));
		}
		if(other.tag == "Obstacle")
			obstacle = false;
	}

	IEnumerator FadeObj(GameObject obj, float fadeTime, float a, float b) {
		float start = Time.time, step;
		bool fading = true;

		while(fading){
			yield return new WaitForEndOfFrame();

			step = Time.time - start;
			Color color = obj.GetComponent<MeshRenderer>().material.color;
			color = new Color(color.r, color.g, color.b, Mathf.Lerp(a, b, step / fadeTime));
			obj.GetComponent<MeshRenderer>().material.color = color;

			if(step > fadeTime)
				fading = false;
		}
		yield break;
	}

}
