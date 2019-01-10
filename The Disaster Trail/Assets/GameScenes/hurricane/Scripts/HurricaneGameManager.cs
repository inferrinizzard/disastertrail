using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// An enum for maintaining the different time remaining indicators
public enum TimeRemainingIndicator
{
	Low = 0,
	Medium = 1,
	High = 2,
	Death = 3
}

public class HurricaneGameManager : MonoBehaviour
{
	[SerializeField] private float SingleRoundTime = 90f;
	[SerializeField] private float clockWaterScrollSpeed = 10f;
	[SerializeField] private float percentTimeRemainingMediumDanger = .5f, percentTimeRemainingHighDanger = .2f, percentTimeRemainingDeathDanger = .05f;
	[SerializeField] private Image clock;
	[SerializeField] [Range(0f, 100f)] private float survivalThreshold;
	[SerializeField] private GameObject gameManagerCanvas;
	[SerializeField] private Transform player;

	private GameObject endMinigameTextObject;
	private bool gameEnded;

	// Use this for initialization
	void Start()
	{
		endMinigameTextObject = null;
		gameEnded = false;

		// Fade in to start game
		GameManager.instance.FadeFromBlack(GameManager.instance.FadeTransitionSpeedPerFrame, startGame);

		StartCoroutine(CountdownTime());
	}

	void Update()
	{
		if (gameEnded && Input.GetKeyDown(KeyCode.Return))
		{
			Destroy(endMinigameTextObject);

			//This will error for now. Should be replaced by string passed in via inspector after scene order finalized.
			player.gameObject.GetComponent<PlayerInventory>().SaveInventory();
			GameManager.instance.LoadScene("Flood");
		}
	}

	private IEnumerator CountdownTime()
	{
		float timeRemaining = SingleRoundTime;
		RectTransform clockRT = clock.GetComponent<RectTransform>();
		Transform clockWaterOverlay = clock.transform.GetChild(0);

		while (timeRemaining > 0)
		{
			clockWaterOverlay.position += new Vector3(clockWaterScrollSpeed, clockRT.sizeDelta.y / SingleRoundTime, 0f) * Time.deltaTime;

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
		PlayerTopDown ptd = player.GetComponent<PlayerTopDown>();
		float danger = ptd.GetDangerLevel();
		return danger <= survivalThreshold;
	}

	void startGame()
	{
		Debug.Log("Start hurricane game!");
	}
}
