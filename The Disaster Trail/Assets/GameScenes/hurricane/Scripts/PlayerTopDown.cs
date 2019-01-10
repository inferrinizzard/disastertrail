using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerTopDown : MonoBehaviour{
	[HideInInspector] public List<GameObject> hurricanePickups;

    public GameObject charModel;

	[SerializeField] private float speed = 50f, rotSpeed = 10f, hor, ver;
	[SerializeField] private float wallCheckDistance, upperFlashlightFadeDuration;
	[SerializeField] [Tooltip("How far from the edge of a wall to light")] private float minDistanceFromEdge;
	[SerializeField] private float upperFlashlightPositionOffsetZ;
	[SerializeField] private LayerMask wallLayer, pickupLayer;
	[SerializeField] private Transform flashlight, upperFlashlight;
	[SerializeField] private RectTransform pickupText;
	[SerializeField] private float searchRadius;
	[SerializeField] private float itemScoreModifier;

	[SerializeField] private Transform[] greenDangerRooms, yellowDangerRooms, redDangerRooms;

	private Animator animator;

	private bool swapFlashlight;
	private Rigidbody rb;
	private Plane ground;
	private IEnumerator flashlightTransition;
	private float upperFlashlightIntensity;
	private PlayerInventory inventory;

	private const int MAX_DANGER = 50, RED_DANGER = 30, YELLOW_DANGER = 20, GREEN_DANGER = 10;

	void Start()
	{
		animator = charModel.GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		swapFlashlight = false;
		ground = new Plane(Vector3.up, Vector3.zero);
		upperFlashlightIntensity = upperFlashlight.GetComponent<Light>().intensity;
		inventory = GetComponent<PlayerInventory>();

		StartCoroutine(UpperFlashlightCheck(.1f));
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			swapFlashlight = true;
		}

		PickupCheck(Input.GetKeyDown(KeyCode.E), searchRadius, pickupLayer);

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			PlaceHeldItem(1.5f);
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			inventory.DebugInventory();
		}
	}

	void FixedUpdate()
	{
#if UNITY_EDITOR
			hor = Input.GetAxis("Horizontal");
			ver = Input.GetAxis("Vertical");
#else
			hor = MobileJoyStick.Horizontal();
			ver = MobileJoyStick.Vertical();
#endif

		Vector3 move = new Vector3(hor, 0, ver);
		if (move.magnitude > 0)
		{
			rb.AddForce(move * speed);
		}

        // Sync movement animation with player's movement speed
        animator.SetFloat("speedPercent", move.magnitude - 0.5f);

		//adjust player rotation to stay looking towards the mouse
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter;
		if (ground.Raycast(ray, out enter))
		{
			Vector3 point = ray.GetPoint(enter);
			Vector3 dir = transform.position - point;
			float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(90f, 0f, angle + 90f), rotSpeed * Time.fixedDeltaTime);
		}

		if (swapFlashlight)
		{
			ToggleFlashlights();
			swapFlashlight = false;
		}
	}

	void PlaceHeldItem(float distance)
	{
		if (inventory.GetCurrentItem() == null)
		{
			return;
		}

		if (Physics.Raycast(transform.position, transform.up, distance * 1.5f))
		{
			return;
		}

		GameObject clone = Instantiate(inventory.GetCurrentItem().itemPrefab, transform.position + transform.up * distance, Quaternion.Euler(90f, 0f, -transform.eulerAngles.y));
		inventory.RemoveFromInventory(inventory.GetCurrentItem());
		hurricanePickups.Add(clone);
	}

	void PickupCheck(bool pickup, float radius, LayerMask layer)
	{
		Collider[] hits = Physics.OverlapSphere(transform.position, radius, layer);
		if (hits.Length > 0)
		{
			pickupText.gameObject.SetActive(true);
			Collider hit = hits.First();
			Vector3 screenPos = Camera.main.WorldToScreenPoint(hit.transform.position);
			pickupText.position = screenPos;

			if (pickup)
			{
				GameItem item = hit.gameObject.GetComponent<PickupableObject>().item;
				inventory.AddToInventory(item);
				hurricanePickups.Remove(hit.gameObject);
				Destroy(hit.gameObject);
			}
		}
		else
		{
			pickupText.gameObject.SetActive(false);
		}
	}

	private IEnumerator UpperFlashlightCheck(float delay)
	{
		while (true)
		{
			bool onOrOff = false;

			//forward raycast to properly adjust wall lighting from the back
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.up, out hit, wallCheckDistance, wallLayer))
			{
				//if hitting a side wall. Note, this only works with walls at cardinal orientations.
				if (Mathf.Abs(hit.normal.x) > 0.001f)
				{
					float zEdge = hit.collider.bounds.center.z - hit.collider.bounds.extents.z;

					if (hit.point.z - zEdge >= minDistanceFromEdge)
					{
						onOrOff = true;
					}
				}
				else if (hit.normal.z > 0)
				{
					onOrOff = true;
				}
			}

			LightTransitionHandler(onOrOff, upperFlashlightFadeDuration);

			yield return new WaitForSeconds(delay);
		}
	}

	private void LightTransitionHandler(bool onOrOff, float duration)
	{
		if (flashlightTransition != null)
		{
			StopCoroutine(flashlightTransition);
		}

		flashlightTransition = DoLightTransition(onOrOff, duration);
		StartCoroutine(flashlightTransition);
	}

	private IEnumerator DoLightTransition(bool onOrOff, float duration)
	{
		Light light = upperFlashlight.GetComponent<Light>();
		float from = light.intensity;
		float to = onOrOff ? upperFlashlightIntensity : 0f;
		float progress = 0.0f;

		while (progress < 1)
		{
			progress += Time.unscaledDeltaTime / duration;
			light.intensity = Mathf.Lerp(from, to, progress);
			yield return null;
		}
	}

	private void ToggleFlashlights()
	{
		Light light = flashlight.gameObject.GetComponent<Light>();
		Light upperLight = upperFlashlight.gameObject.GetComponent<Light>();
		light.intensity = light.intensity == 8 ? 5 : 8;
		light.range = light.intensity;
		upperLight.intensity = upperLight.intensity == 12 ? 8 : 12;
		upperLight.range = upperLight.range == 5 ? 4 : 5;
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

	public float GetDangerLevel()
	{
		Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);

		Predicate<Transform> inRoom = i =>
				pos2d.x > i.position.x - i.localScale.x * 5
				 && pos2d.x < i.position.x + i.localScale.x * 5
				 && pos2d.y > i.position.z - i.localScale.z * 5
				 && pos2d.y < i.position.z + i.localScale.z * 5;

		float dangerLevel;
		int roomIdx = -1;
		Transform[] local;
		if ((roomIdx = Array.FindIndex(greenDangerRooms, inRoom)) >= 0)
		{
			local = greenDangerRooms;
			dangerLevel = GREEN_DANGER;
		}
		else if ((roomIdx = Array.FindIndex(yellowDangerRooms, inRoom)) >= 0)
		{
			local = yellowDangerRooms;
			dangerLevel = YELLOW_DANGER;
		}
		else
		{
			local = redDangerRooms;
			roomIdx = Array.FindIndex(redDangerRooms, inRoom);
			dangerLevel = RED_DANGER;
		}

		dangerLevel += AdjustDangerForItems(local[roomIdx]);
		return dangerLevel;
	}

	private bool InRoom(Transform room, float posX, float posY)
	{
		return posX > room.position.x - room.localScale.x * 5
				 && posX < room.position.x + room.localScale.x * 5
				 && posY > room.position.z - room.localScale.z * 5
				 && posY < room.position.z + room.localScale.z * 5;
	}

	private float AdjustDangerForItems(Transform room)
	{
		float adjust = 0f;
		hurricanePickups.Where(i => InRoom(room, i.transform.position.x, i.transform.position.z))
			.ToList()
			.ForEach(i => adjust += i.GetComponent<PickupableObject>().positive ? -itemScoreModifier : itemScoreModifier);

		return adjust;
	}

	public float GetMaxDanger()
	{
		return MAX_DANGER;
	}

    public Transform[] GetGreenDangerRooms()
    {
        return greenDangerRooms;
    }

    public Transform[] GetYellowDangerRooms()
    {
        return yellowDangerRooms;
    }

    public Transform[] GetRedDangerRooms()
    {
        return redDangerRooms;
    }
}
