using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum DangerLevel
{
	Green = 33,
	Yellow = 67,
	Red = 100
};

[RequireComponent(typeof(PlayerInventory))]
public class PlayerTopDown : MonoBehaviour
{
	[HideInInspector] public List<GameObject> hurricanePickups;

	public GameObject charModel;

	[SerializeField] private GameObject hammer, phone;
	[SerializeField] private float speed = 50f, rotSpeed = 10f, hor, ver;
	[SerializeField] private float wallCheckDistance, upperFlashlightFadeDuration;
	[SerializeField] [Tooltip("How far from the edge of a wall to light")] private float minDistanceFromEdge;
	[SerializeField] private float upperFlashlightPositionOffsetZ;
	[SerializeField] private LayerMask wallLayer, pickupLayer;
	[SerializeField] private Transform flashlight, upperFlashlight;
	[SerializeField] private RectTransform pickupText;
	[SerializeField] private RectTransform repairText;
	[SerializeField] private RectTransform drawerText;
	[SerializeField] private float searchRadius;
	[SerializeField] private float itemScoreModifier;

	[SerializeField] private Transform[] greenDangerRooms, yellowDangerRooms, redDangerRooms;

	[Header("Controls")]
	[SerializeField] private MobileJoyStick joyStickMove;

	private Animator animator;

	private bool swapFlashlight;
	private Rigidbody rb;
	private Plane ground;
	private IEnumerator flashlightTransition;
	private float upperFlashlightIntensity;
	private PlayerInventory inventory;
	private bool repair;
	private Vector3 repairPosition;
	private bool drawer;
	private Vector3 drawerPosition;
	private Transform drawerObject;

    //repair variables 
    public bool repairReady = false;
    private GameObject currWindow;
    [SerializeField] private float repairTime = 5.0f;

    public bool CanMove { get; private set; }

	private const int MAX_DANGER = 50, RED_DANGER = 30, YELLOW_DANGER = 20, GREEN_DANGER = 10;

	void Start()
	{
		animator = charModel.GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		swapFlashlight = false;
		ground = new Plane(Vector3.up, Vector3.zero);
		upperFlashlightIntensity = upperFlashlight.GetComponent<Light>().intensity;
		inventory = GetComponent<PlayerInventory>();
		CanMove = true;
		repair = false;
		repairPosition = Vector3.zero;
		drawer = false;
		drawerPosition = Vector3.zero;
		drawerObject = null;

		hammer.SetActive(false);
		StartCoroutine(UpperFlashlightCheck(.1f));
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F))
		{
			swapFlashlight = true;
		}

		if(Input.GetKeyDown(KeyCode.E))
		{
			Interact();
		}

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			PlaceHeldItem();
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			inventory.DebugInventory();
		}

		if (drawer)
		{
			drawerText.position = Camera.main.WorldToScreenPoint(drawerPosition);
		}

		// Toggles phone & hammer visibilities based on whether the hammer animation is playing or not
		phone.SetActive(!animator.GetBool("showHammer"));
		hammer.SetActive(animator.GetBool("showHammer"));

        // Allow repairing only after the repair animation is completed
        if (Input.GetKey(KeyCode.R) && repairReady)
        {
            float startTime = Time.time;
            hammer.SetActive(true);
            animator.SetTrigger("hammerTrigger");
            if (startTime + repairTime >= Time.time)
            {
                if(!currWindow.GetComponent<WindowTriggers>().broken) inventory.RemoveFromInventory("Tarp");
                currWindow.GetComponent<WindowTriggers>().Repair();
            }

        }
        else
        {
            animator.ResetTrigger("hammerTrigger");
        }
    }

	// Interaction triggered by mobile controls or pressing E.
	public void Interact()
	{
		PickupCheck(true, searchRadius, pickupLayer);

		if(drawer && !drawerObject.GetComponent<OpenableDrawerTrigger>().inProgress)
		{
			drawerObject.GetComponent<OpenableDrawerTrigger>().ToggleDrawer();
		}
	}

	void FixedUpdate()
	{
#if UNITY_IOS || UNITY_ANDROID
		hor = joyStickMove.Horizontal();
		ver = joyStickMove.Vertical();
#elif UNITY_STANDALONE
		hor = Input.GetAxis("Horizontal");
		ver = Input.GetAxis("Vertical");
#endif

		Vector3 move = Vector3.zero;

		if (CanMove)
		{
			// Apply gravity to the player
			// Prevents the player from floating the air when going down a set of stairs
			rb.AddForce(rb.velocity.x, Physics.gravity.y, rb.velocity.z);
			move = (hor * transform.right + ver * transform.up).normalized;
			if (move.magnitude > 0)
			{
				rb.AddForce(move * speed);
			}
		}

		if (swapFlashlight)
		{
			ToggleFlashlights();
			swapFlashlight = false;
		}

		// Sync movement animation with player's movement speed
		animator.SetFloat("speedPercent", move.magnitude - 0.5f);
	}
	
	void PlaceHeldItem()
	{
		PlaceHeldItem(1.5f);
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

	public void ShowRepairText(Vector3 pos)
	{
		repair = true;
		repairPosition = pos;
		repairText.gameObject.SetActive(true);
	}

	public void HideRepairText()
	{
		repair = false;
		repairText.gameObject.SetActive(false);
	}

	public void ShowDrawerText(Transform trans)
	{
		drawer = true;
		drawerPosition = trans.position;
		drawerObject = trans;
		drawerText.gameObject.SetActive(true);
	}

	public void HideDrawerText()
	{
		drawer = false;
		drawerText.gameObject.SetActive(false);
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
		if(other.gameObject.CompareTag("Stair")){
			hsm.StairSound();
		}
		if (!other.gameObject.CompareTag("PickUp"))
		{
			return;
		}

		//On trigger enter call inventory collect
		//InventoryManager.inventory.Add(other.gameObject.name, 1);
	}

	public DangerLevel GetDangerLevel()
	{
		Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);

		Predicate<Transform> inRoom = i =>
				pos2d.x > i.position.x - i.localScale.x * 5
				 && pos2d.x < i.position.x + i.localScale.x * 5
				 && pos2d.y > i.position.z - i.localScale.z * 5
				 && pos2d.y < i.position.z + i.localScale.z * 5;

		DangerLevel dangerLevel;
		if (Array.FindIndex(greenDangerRooms, inRoom) >= 0)
		{
			dangerLevel = DangerLevel.Green;
		}
		else if (Array.FindIndex(yellowDangerRooms, inRoom) >= 0)
		{
			dangerLevel = DangerLevel.Yellow;
		}
		else
		{
			dangerLevel = DangerLevel.Red;
		}

		return dangerLevel;
	}

	//private bool InRoom(Transform room, float posX, float posY)
	//{
	//	return posX > room.position.x - room.localScale.x * 5
	//			 && posX < room.position.x + room.localScale.x * 5
	//			 && posY > room.position.z - room.localScale.z * 5
	//			 && posY < room.position.z + room.localScale.z * 5;
	//}

	//private float AdjustDangerForItems(Transform room)
	//{
	//	float adjust = 0f;
	//	hurricanePickups.Where(i => InRoom(room, i.transform.position.x, i.transform.position.z))
	//		.ToList()
	//		.ForEach(i => adjust += i.GetComponent<PickupableObject>().positive ? -itemScoreModifier : itemScoreModifier);

	//	return adjust;
	//}

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

	public void ToggleMovement(bool on)
	{
		CanMove = !on;
	}

    #region Boarding up windows functions

    public bool CanRepair(GameObject window)
    {
        currWindow = window;
        bool isHit = Physics.Raycast(transform.position, transform.up, out RaycastHit hit, 15f);
        return isHit && hit.collider.gameObject == window && (inventory.CheckForItem("Wood Bundle") || inventory.CheckForItem("Tarp"));
    }


    #endregion
}
