using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodGameManager : MonoBehaviour{
		public GameObject playerObj;
		PlayerCar player;
		int streets,
				avenues,
				horCount = 5;
		Vector3[] path = new Vector3[12];
    // Start is called before the first frame update
    void Awake(){
			player = playerObj.GetComponent<PlayerCar>();
			// streets = spawnPoints.transform.childCount;
			playerObj.transform.position = Vector3.right * 60 * Random.Range(-3,3);
			path[0] = Vector3.forward;
			for(int i=1;i<path.Length;i++){
				int dir = Random.Range(1,3);
				if(horCount>0)
					switch(dir){
						case 1:
							if(player.pos.x>-200)
								path[i] = Vector3.left;
							horCount--;
							break;
						case 2:
							path[i] = Vector3.forward;
							break;
						case 3:
							if(player.pos.x<200)
								path[i] =  Vector3.right;
							horCount--;
							break;
					}
			}
    }

    // Update is called once per frame
    void Update(){
        
    }
}
