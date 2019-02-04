using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PhoneScreen
{
	Map = 0,
	Tasks = 1
}

public class HurricaneGameManager : MonoBehaviour
{
	[SerializeField] private float SingleRoundTime = 90f;
	[SerializeField] private GameObject gameManagerCanvas;
	[SerializeField] private Transform player;
	[SerializeField] private Image phoneNotificationIcon;
	[SerializeField] private Image taskNotificationIcon;
	[SerializeField] private float flashIconTime;
	[SerializeField] private Transform windowParent;

	[Header("Survival")] [SerializeField] [Range(0f, 1f)] private float survivalThreshold;
	[SerializeField] [Range(0f, 1f)] private float windowWeight;
	[SerializeField] [Range(0f, 1f)] private float taskWeight;
	[SerializeField] [Range(0f, 1f)] private float dangerWeight;

	[Header("Phone UI")] [SerializeField] private RectTransform cellPhone;
	[SerializeField] private RectTransform cellPhoneIcon;
	[SerializeField] private float cellPhoneToggleTime;
	[SerializeField] private RectTransform hurricaneIcon;
	[SerializeField] private RectTransform houseIcon;
	[SerializeField] private RectTransform dottedLine;
	[SerializeField] private List<RectTransform> taskTextList;
	[SerializeField] private List<RectTransform> taskImageList;
	[SerializeField] private List<RectTransform> taskToggleList;
	[SerializeField] private Sprite phoneMap;
	[SerializeField] private Sprite phoneTasks;

	[Header("Mobile Controls")] [SerializeField] private GameObject mobileControls;

	private GameObject endMinigameTextObject;
	private bool gameEnded;
	private bool phoneOverlay;
	private float cellPhoneStartPosY;
	private PhoneScreen currentScreen;
	private Vector2 hurricaneImageStartPos;
	private bool taskNotification;
	private bool notificationFlashingBrighter;
	private float notificationFlashingProgress;
	private Dictionary<PhoneScreen, Action> screenCleanupFunctionMapping;
	private HurricaneSoundManager hsm;

	// Use this for initialization
	void Start()
	{
#if UNITY_IOS || UNITY_ANDROID
		mobileControls.SetActive(true);
#elif UNITY_STANDALONE
		Cursor.visible = false;
		cellPhoneIcon.GetComponent<Button>().interactable = false;
#endif

		hsm = GetComponent<HurricaneSoundManager>();
		endMinigameTextObject = null;
		gameEnded = false;
		phoneOverlay = false;
		cellPhoneStartPosY = cellPhone.position.y;
		hurricaneImageStartPos = hurricaneIcon.localPosition;
		taskNotification = false;
		notificationFlashingBrighter = true;
		notificationFlashingProgress = 0f;
		currentScreen = PhoneScreen.Map;
		SetupMap();

		screenCleanupFunctionMapping = new Dictionary<PhoneScreen, Action>
		{
			{ PhoneScreen.Map, HideMap },
			{ PhoneScreen.Tasks, HideTasksList }
		};

		// Fade in to start game
		GameManager.instance.FadeFromBlack(GameManager.instance.FadeTransitionSpeedPerFrame);

		StartCoroutine(CountdownTime());
	}

	void Update()
	{
#if UNITY_STANDALONE
		if (Input.GetKeyDown(KeyCode.Space))
		{
			TogglePhoneOverlay();
		}
#endif

		if (gameEnded && Input.GetKeyDown(KeyCode.Return))
		{
			Destroy(endMinigameTextObject);

			//This will error for now. Should be replaced by string passed in via inspector after scene order finalized.
			player.gameObject.GetComponent<PlayerInventory>().SaveInventory();
			GameManager.instance.LoadScene("Flood");
		}

		if (taskNotification)
		{
			Color c = phoneOverlay ? taskNotificationIcon.color : phoneNotificationIcon.color;
			if (notificationFlashingProgress > 1f)
			{
				notificationFlashingProgress = 0f;
				notificationFlashingBrighter = !notificationFlashingBrighter;
			}

			notificationFlashingProgress += Time.deltaTime / flashIconTime;

			if (notificationFlashingBrighter)
			{
				c.a = Mathf.Lerp(0f, 1f, notificationFlashingProgress);
			}
			else
			{
				c.a = Mathf.Lerp(1f, 0f, notificationFlashingProgress);
			}

			if (phoneOverlay)
			{
				taskNotificationIcon.color = c;
			}
			else
			{
				phoneNotificationIcon.color = c;
			}
		}
	}

	private IEnumerator CountdownTime()
	{
		float timeRemaining = SingleRoundTime;

		while (timeRemaining > 0)
		{
			hurricaneIcon.localPosition = Vector2.Lerp(hurricaneImageStartPos, houseIcon.localPosition, 1 - (timeRemaining / SingleRoundTime));

			yield return null;
			timeRemaining -= Time.deltaTime;
		}

		//Time has ran out, process minigame scoring
		SoundManager.instance.PlaySFX(SoundEffect.GlassBreak);
		GameManager.instance.FadeToBlack(GameManager.instance.FadeTransitionSpeedPerFrame, EndGameCallback);
	}

	private void EndGameCallback()
	{
		bool survived = ScoreHurricane();

		endMinigameTextObject = new GameObject("EndHurricaneText", typeof(RectTransform));
		RectTransform rt = endMinigameTextObject.GetComponent<RectTransform>();
		RectTransform canvasRT = gameManagerCanvas.GetComponent<RectTransform>();
		rt.SetParent(gameManagerCanvas.transform);
		rt.localPosition = Vector3.zero;
		rt.sizeDelta = canvasRT.sizeDelta;

		Text textComponent = endMinigameTextObject.AddComponent<Text>();
		textComponent.text = survived ? "You survived!" : "You were injured!\nHint: avoid rooms with windows!";
		textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		textComponent.fontSize = 28;
		textComponent.alignment = TextAnchor.MiddleCenter;

		gameEnded = true;
	}

	private bool ScoreHurricane()
	{
		List<WindowTriggers> windows = windowParent.GetComponentsInChildren<WindowTriggers>().ToList();

		float totalWindows = windows.Count;
		float brokenWindows = windows.Where(i => i.broken).Count();
		float percentBroken = brokenWindows / totalWindows;

		float totalTasks = taskTextList.Count;
		float failedTasks = taskTextList.Where(i => !i.GetComponent<TaskController>().completed).Count();
		float percentFailed = failedTasks / totalTasks;

		float danger = (float) player.GetComponent<PlayerTopDown>().GetDangerLevel();

		Debug.Log("Broken Score: " + (percentBroken * windowWeight));
		Debug.Log("Task Score: " + (percentFailed * taskWeight));
		Debug.Log("Danger Score: " + ((danger / 100) * dangerWeight));

		Debug.Log("Final Score: " + (percentBroken * windowWeight + percentFailed * taskWeight + (danger / 100) * dangerWeight));
		Debug.Log("Survival Threshold: " + survivalThreshold);

		return percentBroken * windowWeight + failedTasks * taskWeight + (danger / 100) * dangerWeight <= survivalThreshold;
	}

#region phone functions
	public void TogglePhoneOverlay()
	{
		if (LeanTween.isTweening(cellPhone.gameObject))
		{
			LeanTween.cancel(cellPhone.gameObject);
		}

		phoneOverlay = !phoneOverlay;
		LeanTween.moveY(cellPhone.gameObject, cellPhoneStartPosY * Convert.ToInt16(!phoneOverlay), cellPhoneToggleTime);
		player.GetComponent<PlayerTopDown>().ToggleMovement(phoneOverlay);

		if (taskNotification)
		{
			ResetNotificationFlashing();

			if (currentScreen == PhoneScreen.Tasks)
			{
				taskNotification = false;
			}
		}

#if UNITY_STANDALONE
		Cursor.visible = phoneOverlay;
#endif
	}

	private void ResetNotificationFlashing()
	{
		notificationFlashingBrighter = true;
		notificationFlashingProgress = 0f;

		Color c = phoneNotificationIcon.color;
		c.a = 0f;
		phoneNotificationIcon.color = c;

		c = taskNotificationIcon.color;
		c.a = 0f;
		taskNotificationIcon.color = c;
	}

	public void OnMapPress()
	{
		if (currentScreen == PhoneScreen.Map)
		{
			return;
		}

		screenCleanupFunctionMapping[currentScreen]();
		SetupMap();

		currentScreen = PhoneScreen.Map;
	}

	public void OnTasksPress()
	{
		if (currentScreen == PhoneScreen.Tasks)
		{
			return;
		}

		screenCleanupFunctionMapping[currentScreen]();
		SetupTasksList();

		if (taskNotification)
		{
			taskNotification = false;
			ResetNotificationFlashing();
		}

		currentScreen = PhoneScreen.Tasks;
	}
	
	//private IEnumerator FlashIcon(RectTransform rt, float flashRate)
	//{
	//	Image image = rt.GetComponent<Image>();
	//	Color c = image.color;
	//	float progress;

	//	while (taskNotification)
	//	{
	//		progress = 0f;
	//		while (progress < 1)
	//		{
	//			progress += Time.unscaledDeltaTime / flashRate * 2;
	//			c.a = Mathf.Lerp(0f, 1f, progress);
	//			image.color = c;
	//			yield return null;
	//		}

	//		progress = 0f;
	//		while (progress < 1)
	//		{
	//			progress += Time.unscaledDeltaTime / flashRate * 2;
	//			c.a = Mathf.Lerp(1f, 0f, progress);
	//			image.color = c;
	//			yield return null;
	//		}
	//	}
	//}
#endregion

	public void HandleObjectPickup(GameObject obj)
	{
		//find the first matching task that has not already been completed
		RectTransform task = taskTextList
			.Where(i => (i?.GetComponent<TaskController>()?.completed).Value == false)
			.ToList()
			.Find(i => i?.GetComponent<TaskController>()?.Item == obj);

		if (task == null)
		{
			Debug.LogError(string.Format("Task {0} not found", obj), gameObject);
			return;
		}

		TaskController tc = task.GetComponent<TaskController>();
		tc.completed = true;
		tc.toggle.isOn = true;
		task.GetComponent<TextMeshProUGUI>().fontStyle |= FontStyles.Strikethrough; 
		taskNotification = true;
	}

	private void SetupMap()
	{
		cellPhone.GetComponent<Image>().sprite = phoneMap;
		hurricaneIcon.gameObject.SetActive(true);
		houseIcon.gameObject.SetActive(true);
		dottedLine.gameObject.SetActive(true);
	}


	private void HideMap()
	{
		hurricaneIcon.gameObject.SetActive(false);
		houseIcon.gameObject.SetActive(false);
		dottedLine.gameObject.SetActive(false);
	}

	private void SetupTasksList()
	{
		cellPhone.GetComponent<Image>().sprite = phoneTasks;
		taskTextList.ForEach(i => i.gameObject.SetActive(true));
		taskImageList.ForEach(i => i.gameObject.SetActive(true));
		taskToggleList.ForEach(i => i.gameObject.SetActive(true));
	}

	private void HideTasksList()
	{
		taskTextList.ForEach(i => i.gameObject.SetActive(false));
		taskImageList.ForEach(i => i.gameObject.SetActive(false));
		taskToggleList.ForEach(i => i.gameObject.SetActive(false));
	}
}
