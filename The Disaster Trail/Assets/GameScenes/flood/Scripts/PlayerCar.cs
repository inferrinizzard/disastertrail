using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCar : MonoBehaviour
{
	[Header("Car Parts")] [SerializeField] private GameObject carBody;
	[SerializeField] private List<GameObject> wheels, frontWheelAxes;

	[Header("Movement")] [SerializeField] private float maxSpeed = 80f;
	[SerializeField] private float rotSpeed = 90f;
	[SerializeField] private float wheelTurnSpeed = 6f;
	[SerializeField] private float maxReverseSpeed = 25f;
	[SerializeField] private float spinSpeedMultiplier = 1, turnSpeedMultiplier = 1;
	[SerializeField] private float accelerateSpeedIncreaseRate;
	[SerializeField] private float breakSpeedDecreaseRate;
	[SerializeField] private float idleSpeedDecreaseRate;
	[SerializeField] private float slowDownAngle = 5f;
	[SerializeField] private float slowDownMult = .8f;
	public Vector3 pos, rotAxis, trackPos;

	[Header("UI")] public Image radioMessage;
	[SerializeField] float randomCounter = 0, radioTime, randomDist = 30, messageTimer = 0;

	private float currentRot;
	private bool rotating = false, playedMessage = false;
	private Rigidbody rb;
	private Renderer rend;
	private float priorSpeed;
	private Vector3 priorDirection;
	private float speedMultiplier;
	private float maxTurnSpeed;

	int health = 3;
	float buffer = 0;

	// Use this for initialization
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rend = carBody.GetComponent<Renderer>();
		priorSpeed = 0f;
		priorDirection = transform.forward;
		radioTime = Random.Range(0f, 1f);
		trackPos = transform.position;
		speedMultiplier = 1f;
		maxTurnSpeed = maxSpeed * .75f;
	}

	// Update is called once per frame
	void Update()
	{
		if (health < 1)
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
#if UNITY_EDITOR
		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");
#else
		float hor = MobileJoyStick.Horizontal();
		float ver = MobileJoyStick.Vertical();
#endif
		bool accelerating = ver > 0;
		bool breaking = ver < 0;
		float speed = CalculateSpeed(hor, accelerating, breaking);
		float wheelHor = hor;

		rb.AddForce(transform.forward * speed * speedMultiplier);
		wheels.ForEach(i => i.transform.Rotate(Vector3.right, Time.fixedDeltaTime * speed * speedMultiplier * spinSpeedMultiplier));

		if (breaking)
		{
			StartCoroutine(ToggleBreakLight(true, .2f));
		}
		else
		{
			StartCoroutine(ToggleBreakLight(false, .2f));
		}

		if (Mathf.Abs(speed) > 0)
		{
			if (Mathf.Abs(hor) > 0)
			{
				//if the vertical velocity is negative, then the horizontal should be flipped
				//because the front tires should always be pointing in the dir of the analog stick
				if (breaking)
				{
					wheelHor *= -1f;
				}
				if (speed < 0)
				{
					hor *= -1f;
				}

				transform.Rotate(Vector3.up, hor * Mathf.Abs(speed) * turnSpeedMultiplier * Time.fixedDeltaTime);
			}
		}

		// Turn the front wheels left or right during turns based off of the direction of the movement input magnitudes
		Vector3 move = new Vector3(wheelHor, 0, ver);
		FrontWheelTurnHandler(move);
	}

	private IEnumerator ToggleBreakLight(bool on, float duration)
	{
		Color to = on ? Color.white : Color.black;
		Color from = rend.material.GetColor("_BrakesColor");

		if (to.Equals(from))
			yield break;

		float progress = 0.0f;

		while (progress < 1f)
		{
			progress += Time.unscaledDeltaTime / duration;
			rend.material.SetColor("_BrakesColor", Color.Lerp(from, to, progress));
			yield return null;
		}
	}

	
	private float CalculateSpeed(float hor, bool accelerating, bool breaking)
	{
		if (accelerating && breaking)
		{
			return 0f;
		}

		float speed = priorSpeed;
		float turnAngle = Vector3.Angle(priorDirection, transform.forward);
		float turnPenalty = turnAngle <= slowDownAngle ? 0f : turnAngle - slowDownAngle;
		float increase = accelerateSpeedIncreaseRate * (1 - (speed / maxSpeed) + .5f);

		if (accelerating)
		{
			//if the player is turning, cap their max speed. Also slow acceleration while turning. 
			speed += -(turnPenalty * slowDownMult) + (turnPenalty > 0f && speed > maxTurnSpeed ? 0f : increase);
			speed = Mathf.Clamp(speed, -maxReverseSpeed, maxSpeed);
		}
		else if (breaking)
		{
			speed -= breakSpeedDecreaseRate;
			speed = Mathf.Clamp(speed, -maxReverseSpeed, maxSpeed);
		}
		else
		{
			speed += speed < 0 ? idleSpeedDecreaseRate : -idleSpeedDecreaseRate;
			if (Mathf.Abs(speed) < 2f)
			{
				speed = 0f;
			}
		}

		priorDirection = transform.forward;
		priorSpeed = speed;

		return speed;
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
			//Debug.Log("Play Radio message");
			radioMessage.gameObject.SetActive(true);
			messageTimer = Time.time;
			playedMessage = true;
		}
	}

	#region CollisionDetection
	void OnTriggerEnter(Collider other)
	{
		if ((other.gameObject.tag == "Obstacle" || other.gameObject.tag == "BuildingCollider") && Time.time - buffer > 3)
			TakeDamage();
	}

	void TakeDamage()
	{
		health--;
		Debug.Log("oof, health: " + health);
		buffer = Time.time;
		speedMultiplier *= .8f;
		rotSpeed *= .8f;
		wheelTurnSpeed *= .8f;
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
