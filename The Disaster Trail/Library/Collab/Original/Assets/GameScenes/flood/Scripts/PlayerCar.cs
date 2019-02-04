using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCar : MonoBehaviour
{
	[Header("Car Parts")] [SerializeField] private GameObject carBody;
	[SerializeField] private List<GameObject> wheels, frontWheelAxes;

	[Header("Movement")] [SerializeField] private float maxSpeed = 90f;
	[SerializeField] private float wheelTurnSpeed = 6f;
	[SerializeField] private float maxReverseSpeed = 25f;
	[SerializeField] private float spinSpeedMultiplier = 1, turnSpeedMultiplier = 1;
	[SerializeField] private float accelerateSpeedIncreaseRate;
	[SerializeField] private float breakSpeedDecreaseRate;
	[SerializeField] private float idleSpeedDecreaseRate;
	[SerializeField] private float slowDownAngle = 5f;
	[SerializeField] private float slowDownMult = .8f;

	[Header("Camera Movement")] [SerializeField] private float maxCamZoomOut = 1f;
	[SerializeField] private float maxCamHorizontalTilt = .5f;
	[SerializeField] private float maxTurnAngle = 2f;
	[SerializeField] private float camWobbleThreshhold = .7f;
	[SerializeField] private float camWobbleIntensity = .2f;

	[Header("Hydroplaning")] [SerializeField] private float hydroplaneWobble;
	[SerializeField] private float hydroplaneSpeedLerpRate;
	[SerializeField] private float hydroplaneRotationLerpRate;
	[SerializeField] private float hydroplaneEndSpeed;
	[SerializeField] private float maxHydroplaneTurnAngle;

	[Header("Sound Effects")]
	[Tooltip("How fast the car needs to be traveling before a break sound is played.")]
	[SerializeField] private float screechSpeedThreshold = 25.0f;

	private float speed;
	private float currentRot;
	private Rigidbody rb;
	private Renderer rend;
	private SfxCar sfx;
	private float priorSpeed;
	private Vector3 priorDirection;
	private float speedMultiplier;
	private float maxTurnSpeed;
	private bool wasAccelerating; // To determine whether the user started accelerating again.
	private bool accelerating;
	private bool breaking;
	private bool hydroplaning;
	private Vector3 hydroplaneCurrentForward;
	private float baseCamZoomOut;
	private float baseCamYPosition;
	//used for turning on and off break lights
	private bool wasBreaking;
	private IEnumerator breakEnumerator;

	[Header("Gameplay Options")]
	public int health = 100;
	float buffer = 0;

	// Use this for initialization
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rend = carBody.GetComponent<Renderer>();
		sfx = GetComponent<SfxCar>();
		priorSpeed = 0f;
		priorDirection = transform.forward;
		speedMultiplier = 1f;
		maxTurnSpeed = maxSpeed * .75f;
		accelerating = false;
		breaking = false;
		hydroplaning = false;
		wasBreaking = breaking;
		baseCamZoomOut = Camera.main.transform.localPosition.z;
		baseCamYPosition = Camera.main.transform.localPosition.y;

		sfx.StartEngine();
	}

	// Update is called once per frame
	void Update()
	{
		if (health < 0)
			GameOver();
	}

	void FixedUpdate()
	{
#if UNITY_IOS || UNITY_ANDROID
		float hor = MobileJoyStick.Horizontal();
		float ver = MobileJoyStick.Vertical();
#elif UNITY_STANDALONE
		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");
		accelerating = ver > 0;
		breaking = ver < 0;
#endif

		speed = CalculateSpeed(hor, accelerating, breaking);

		Vector3 dir = transform.forward;

		if (accelerating && !wasAccelerating)
		{
			sfx.Rev();
		}
		sfx.Move(rb.velocity.magnitude);

		if (hydroplaning)
		{
			speed = Mathf.Lerp(priorSpeed, speed, hydroplaneSpeedLerpRate);
			dir = Vector3.Lerp(hydroplaneCurrentForward, transform.forward, hydroplaneRotationLerpRate);
			hydroplaneCurrentForward = dir;
			dir += new Vector3(dir.z, 0f, -dir.x) * Random.Range(-hydroplaneWobble, hydroplaneWobble) * (1 - Mathf.Abs((.5f - (speed / maxSpeed))));
		}

		rb.AddForce(dir * speed * speedMultiplier);
		wheels.ForEach(i => i.transform.Rotate(Vector3.right, Time.fixedDeltaTime * speed * speedMultiplier * spinSpeedMultiplier));

		if (breaking && !wasBreaking)
		{
			// Play a sound effect if the car was moving fast enough.
			if (Vector3.Dot(rb.velocity, transform.forward) > screechSpeedThreshold)
				sfx.StartBreaking();
			BreakLightHandler(true, .2f);
		}
		else if (!breaking && wasBreaking)
		{
			sfx.StopBreaking();
			BreakLightHandler(false, .2f);
		}

		if (Mathf.Abs(speed) > 0 && Mathf.Abs(hor) > 0)
		{
			transform.Rotate(Vector3.up, hor * speed * turnSpeedMultiplier * Time.fixedDeltaTime);
		}

		// Turn the front wheels left or right during turns based off of the direction of the movement input magnitudes
		FrontWheelTurnHandler(new Vector3(hor, 0, Mathf.Abs(ver)));

		AdjustCameraPosition(speed);


		priorSpeed = speed;
		wasBreaking = breaking;
		wasAccelerating = accelerating;
	}

	private void BreakLightHandler(bool on, float duration)
	{
		if (breakEnumerator != null)
		{
			StopCoroutine(breakEnumerator);
		}

		breakEnumerator = ToggleBreakLight(on, duration);
		StartCoroutine(breakEnumerator);
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
		return speed;
	}

	private void AdjustCameraPosition(float speed)
	{
		float speedPercent = Mathf.Abs(speed) / maxSpeed;
		float turnDir = -Mathf.Sign(speed) * Vector3.SignedAngle(transform.forward, priorDirection, Vector3.up);
		float x = (turnDir / maxTurnAngle) * maxCamHorizontalTilt;
		float y = baseCamYPosition;
		float z = -speedPercent * maxCamZoomOut + baseCamZoomOut;

		Vector3 camMove = new Vector3(x, y, z);
		if (speedPercent >= camWobbleThreshhold)
		{
			float wobblePercentNormalized = (speedPercent - camWobbleThreshhold) / (1f - camWobbleThreshhold);
			camMove += new Vector3(
				Random.Range(-camWobbleIntensity, camWobbleIntensity),
				Random.Range(-camWobbleIntensity, camWobbleIntensity),
				Random.Range(-camWobbleIntensity, camWobbleIntensity)
			) * wobblePercentNormalized;
		}

		//Might want to multiply Time.fixedDeltaTime by some variable so that hard breaking zooms in camera faster, etc.
		Camera.main.transform.localPosition = Vector3.Slerp(Camera.main.transform.localPosition, camMove, Time.fixedDeltaTime);
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

	void OnTriggerEnter(Collider other)
	{
		if ((other.gameObject.CompareTag("Obstacle") || other.gameObject.CompareTag("BuildingCollider")) && Time.time - buffer > 3)
		{
			TakeDamage();
		}
		else if (other.gameObject.CompareTag("Puddle"))
		{
			hydroplaning = true;
			hydroplaneCurrentForward = transform.forward;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Puddle"))
		{
			hydroplaning = false;
			if (Vector3.Angle(transform.forward, hydroplaneCurrentForward) > maxHydroplaneTurnAngle)
			{
				priorSpeed = Mathf.Min(priorSpeed, hydroplaneEndSpeed);
			}
		}
	}

	void TakeDamage()
	{
		float prevHealth = health;
		health -= (int)Mathf.Abs(speed) * 3 / 4;
		buffer = Time.time;
		if ((Mathf.Ceil(health / 10) * 10 - 20) % 30 == 0 && prevHealth - health > 10)
		{
			speedMultiplier *= .8f;
			turnSpeedMultiplier *= .8f;
			wheelTurnSpeed *= .8f;
		}

		sfx.Crash();

		// Adjusts the mesh body's blend shape weight based on the current health
		carBody.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, 100 - health);
	}

	void GameOver()
	{
		// GameManager.instance.FadeToBlack(GameManager.instance.FadeTransitionSpeedPerFrame, fadeOut);
		// GameManager.instance.FadeToBlackAndLoad(.1f, 0);
		// GameManager.instance.FadeFromBlack(GameManager.instance.FadeTransitionSpeedPerFrame, fadeOut);
		GameManager.instance.FadeToBlackAndLoad(.1f, 0);

		maxSpeed = 0;
	}

	void fadeOut()
	{
		Debug.Log("load callback");
		GameManager.instance.LoadScene(0);
	}

	#region MobileControlCallbacks

	public void GasPedalDown()
	{
		accelerating = true;
	}

	public void GasPedalUp()
	{
		accelerating = false;
	}

	public void BreakPedalDown()
	{
		breaking = true;
	}

	public void BreakPedalUp()
	{
		breaking = false;
	}

	#endregion
}
