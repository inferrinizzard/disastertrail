using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinterStormGameManager : MonoBehaviour{

	public GameObject playerObj, snowpiles;
	[SerializeField]Camera camera;
	Player2D player;
	[SerializeField]float step, damageTickCounter;
	public Canvas UI;
	Text health;

	// Start is called before the first frame update
	void Start(){
		player = playerObj.GetComponent<Player2D>();
		health = UI.transform.GetChild(1).GetComponent<Text>();

		for(int i=0;i<snowpiles.transform.childCount;i++)
			for(int j=Random.Range(0,4);j>=0;j--)
				Destroy(snowpiles.transform.GetChild(i).GetChild(j).gameObject);
	}

	// Update is called once per frame
	void Update(){
		for(int i=snowpiles.transform.childCount-1;i>=0;i--)
			if(snowpiles.transform.GetChild(i).childCount==0)
				Destroy(snowpiles.transform.GetChild(i).gameObject);
		health.text = player.health.ToString();

		player.health = Mathf.Min(100,player.health);
		
		if(player.health<=0)
			GameOver();
		UI.transform.GetChild(2).gameObject.SetActive(player.obstacle);

	}

	void FixedUpdate(){
		camera.transform.position = new Vector3(playerObj.transform.position.x,camera.transform.position.y,camera.transform.position.z);
		step++;
		
		if(player.inside){
			if(step%(2*damageTickCounter)==0)
				player.health++;
		}
		else{
			if(step%(player.wet?damageTickCounter/2:damageTickCounter)==0)
				player.health--;
		}
	}

	void GameOver(){
		Destroy(playerObj);
	}

	void ColdSnap(){
		Debug.Log("cold");
	}

	void Sleet(){
		Debug.Log("sleet");
	}

	void Snow(){
		Debug.Log("snow");
	}

	void HighWind(){
		Debug.Log("wind");
	}

	void Icy(){
		Debug.Log("ice");
	}
}
