using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinterStormGameManager : MonoBehaviour
{

	public GameObject playerObj, snowpiles;
	[SerializeField] Camera camera;
	Player2D player;
	[SerializeField] float step = 0f, wetTimer, wetCooldown, coldTimer, coldCooldown, windForce;
	[SerializeField] [Tooltip("Number of frames until player takes damage in cold")] float damageTickCounter;
	[SerializeField] [Tooltip("Number of seconds until events occur")] float eventCounter;
	public Canvas UI;
	Text health, heartRateText, eventText;
	int eventIndex;
	System.Action[] events;
	SnowManager snow;

	// Start is called before the first frame update
	void Start()
	{
		player = playerObj.GetComponent<Player2D>();
		health = UI.transform.GetChild(1).GetComponent<Text>();
		heartRateText = UI.transform.GetChild(2).GetComponent<Text>();
		eventText = UI.transform.GetChild(3).GetComponent<Text>();
		events = new System.Action[] { () => ColdSnap(), () => Sleet(), () => Snow(), () => HighWind(), () => Icy() };
		eventIndex = Random.Range(0, events.Length);
		snow = snowpiles.GetComponent<SnowManager>();
	}

	// Update is called once per frame
	void Update()
	{
		player.health = player.health > 100 ? 100 : Mathf.Max(0, player.health);
		heartRateText.text = (player.heartRate + 80).ToString();
		health.text = player.health.ToString();
		if (player.health <= 0)
			GameOver();

		UI.transform.GetChild(4).gameObject.SetActive(player.obstacle);
		if (player.hasNoSnow)
		{
			UI.transform.GetChild(4).gameObject.SetActive(false);
			player.hasNoSnow = false;
		}

		player.wet = player.cold ? false : player.wet;
		if (step - (player.inside ? coldCooldown / 2 : coldCooldown) > coldTimer)
			player.cold = false;
		if (step - (player.inside ? wetCooldown / 2 : wetCooldown) > wetTimer)
			player.wet = false;
	}

	void FixedUpdate()
	{
		//camera.transform.position = new Vector3(playerObj.transform.position.x, camera.transform.position.y, camera.transform.position.z);
		step++;

		if (player.inside)
		{
			if (step % (2 * damageTickCounter) == 0)
				player.health++;
			if (step % damageTickCounter == 0)
				player.heartRate--;
		}
		else
		{
			if (step % (player.cold ? damageTickCounter / 4 : (player.wet ? damageTickCounter / 2 : damageTickCounter)) == 0)
			{
				player.health--;
				player.heartRate++;
			}
			if (player.windy && step % 75 == 0 && player.hor <= 0)
				player.GetComponent<Rigidbody>().AddForce(Vector3.left * windForce, ForceMode.VelocityChange);
		}
		if (player.heartRate > 60 && step % 10 == 0)
			player.health--;

		if (step % (eventCounter * 60) == 0)
		{
			player.windy = false;
			player.icy = false;
			events[eventIndex].Invoke();
			eventIndex = Random.Range(0, events.Length - 1);
		}

	}

	void GameOver()
	{
		Destroy(playerObj);
	}

	#region weatherevents
	void ColdSnap()
	{
		eventText.text = "Cold Snap!";
		Debug.Log("cold " + step);
		player.cold = true;
		coldTimer = step;
	}

	void Sleet()
	{
		eventText.text = "Sleet Fall!";
		Debug.Log("sleet " + step);
		player.wet = true;
		wetTimer = step;
	}

	void Snow()
	{
		eventText.text = "Thick Snow!";
		Debug.Log("snow " + step);
		snow.Increase();
	}

	void HighWind()
	{
		eventText.text = "High Winds!";
		Debug.Log("wind " + step);
		player.windy = true;
		player.cold = true;
		coldTimer = step + coldCooldown * .75f;
	}

	void Icy()
	{
		eventText.text = "Ice Slicks!";
		Debug.Log("ice " + step);
		player.icy = true;
		player.wet = true;
		wetTimer = step + wetCooldown * .75f;
	}
	#endregion

}

