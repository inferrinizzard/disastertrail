using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCar : MonoBehaviour
{
	[Header("Car Parts")] [SerializeField] private GameObject carBody;
	[SerializeField] private List<GameObject> wheels;
	[SerializeField] private List<GameObject> frontWheelAxes;

	[Header("Movement")] [SerializeField] private float speed = 50f, rotSpeed = 90f, wheelTurnSpeed = 6f;
	[SerializeField] private float spinSpeedMultiplier = 1, turnSpeedMultiplier = 1;
	public Vector3 pos, rotAxis, trackPos;


	[Header("UI")] public Image radioMessage;
	[SerializeField] float randomCounter = 0, radioTime, randomDist = 30, messageTimer = 0;

	private float currentRot;
	private bool rotating = false, playedMessage = false;
	private Rigidbody rb;
	private Renderer rend;
	private float brakesPercent;

	int health = 3;
	float buffer = 0;

	// Use this for initialization
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rend = carBody.GetComponent<Renderer>();
		radioTime = Random.Range(0f, 1f);
		trackPos = transform.position;
	}

	// Update is called once per frame
	void Update()
	{
		if(health < 1)
			GameOver();

		pos = transform.position;

		randomCounter = Vector3.Distance(pos, trackPos);

		if (randomCounter / randomDist > radioTime && !playedMessage)
			RadioMessage();

		if (randomCounter > randomDist && playedMessage)
		{
			playedMessage = false;
			trackPos = pos;
			randomCounter = 0;
			radioTime = Random.Range(0f, 1f);
		}

		if (Time.time - messageTimer > 3)
			radioMessage.gameObject.SetActive(false);
	}

	void FixedUpdate()
	{
#if DEBUG
		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");
#else
		float hor = MobileJoyStick.Horizontal();
		float ver = MobileJoyStick.Vertical();
#endif

		// Activate brake lights when stationary
		brakesPercent += Time.fixedDeltaTime * 4;
		Color color = Color.Lerp(Color.black, Color.white, brakesPercent);

		float mult = ver > 0 ? 1f : .5f;
		if (Mathf.Abs(ver) > 0)
		{
			rb.AddForce(transform.forward * ver * speed * mult);
			wheels.ForEach(i => i.transform.Rotate(Vector3.right, ver * Time.fixedDeltaTime * speed * mult * spinSpeedMultiplier));

			if (color != Color.black)
			{
				brakesPercent = 0;
				rend.material.SetColor("_BrakesColor", color);
			}

			if (Mathf.Abs(hor) > 0)
			{
				transform.Rotate(Vector3.up, hor * rotSpeed * Time.fixedDeltaTime);
				transform.GetChild(1).Rotate(Vector3.up, hor * rotSpeed * Time.fixedDeltaTime/2);
			}
		}
		else
		{
			if (color != Color.white)
			{
				rend.material.SetColor("_BrakesColor", color);
			}
		}

		// Turn the front wheels left or right during turns based off of the direction of the movement input magnitudes
		Vector3 move = new Vector3(hor, 0, ver);
		FrontWheelTurnHandler(move);
	}

	private void FrontWheelTurnHandler(Vector3 move)
	{
		Quaternion rot = Quaternion.identity;
		if (move.magnitude > 0f)
		{
			rot = Quaternion.LookRotation(move);
		}

		float targetRot;
		if (rot.eulerAngles.y <= 90)
		{
			targetRot = rot.eulerAngles.y;
		}
		else if (rot.eulerAngles.y < 270)
		{
			targetRot = rot.eulerAngles.y - 180;
		}
		else
		{
			targetRot = rot.eulerAngles.y - 360;
		}

		// Clamp the front wheels' turn rotation between -60 & 60 degrees
		targetRot = Mathf.Clamp(targetRot, -60f, 60f);

		// Turns the wheels
		currentRot = Mathf.Lerp(currentRot, targetRot, wheelTurnSpeed * Time.fixedDeltaTime);
		frontWheelAxes.ForEach(i => i.transform.localEulerAngles = new Vector3(0, currentRot, 0));
	}

	void RadioMessage()
	{
		//do ui/sound
		if (!playedMessage)
		{
			Debug.Log("Play Radio message");
			radioMessage.gameObject.SetActive(true);
			messageTimer = Time.time;
			playedMessage = true;
		}
	}

	#region CollisionDetection
	void OnTriggerEnter(Collider other)
	{
		Debug.Log(other);
		if ((other.gameObject.tag == "Obstacle" || other.gameObject.tag == "BuildingCollider")&&Time.time-buffer>3)
			TakeDamage();
	}

	void TakeDamage(){
		health--;
		Debug.Log("oof, health: "+health);
		buffer = Time.time;
		speed*=.8f;
		rotSpeed*=.8f;
		wheelTurnSpeed*=.8f;
		//play crash sound, change to damaged model, slow car?
	}

	void GameOver()
	{
		Destroy(transform.GetChild(0).gameObject);
		//transition to gameover screen
	}

	#endregion

	#region CarRotation
	IEnumerator RotateAbout(Vector3 point, Vector3 axis, float angle, float time)
	{
		Quaternion rotation = Quaternion.AngleAxis(angle, axis);
		Vector3 startPos = pos - point,
						endPos = rotation * startPos;
		Quaternion startRot = transform.rotation;
		float step = 0,
					smoothStep = 0,
					rate = 1f / time;
		while (step < 1)
		{
			step += Time.deltaTime * rate;
			smoothStep = Mathf.SmoothStep(0, 1, step);
			transform.position = point + Vector3.Slerp(startPos, endPos, smoothStep);
			transform.rotation = startRot * Quaternion.Slerp(Quaternion.identity, rotation, smoothStep);
			yield return null;
		}
		if (step > 1)
		{
			transform.position = point + endPos;
			transform.rotation = startRot * rotation;
		}
	}

	IEnumerator RotateObject(Vector3 point, Vector3 axis, float angle, float time)
	{
		float step = 0,
					rate = 1f / time,
					smoothStep = 0,
					lastStep = 0;
		while (step < 1)
		{
			step += Time.deltaTime * rate;
			smoothStep = Mathf.SmoothStep(0, 1, step);
			transform.RotateAround(point, axis, angle * (smoothStep - lastStep));
			lastStep = smoothStep;
			yield return null;
		}

		if (step > 1)
			transform.RotateAround(point, axis, angle * (1 - lastStep));
	}
	#endregion
}
