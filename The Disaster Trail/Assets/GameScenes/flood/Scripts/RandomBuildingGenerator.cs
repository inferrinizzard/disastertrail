using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBuildingGenerator : MonoBehaviour{
	public GameObject CityBlocks;
	GameObject[] buildings;
	GameObject[] exempt;
	int rows, cols;
	float blockSize = 55f;
    // Start is called before the first frame update
	void Start(){
		Transform cbTransform = CityBlocks.transform;
		rows = cbTransform.childCount;
		cols = cbTransform.GetChild(0).childCount;
		
		for(int i=0;i<rows;i++)
			for(int j=0;j<cols;j++){
				Transform block = cbTransform.GetChild(i).GetChild(j);
				if(block.gameObject.tag!="Positive"){
					buildings = new GameObject[block.childCount];
					for(int k=0;k<block.childCount;k++)
						buildings[k] = block.GetChild(k).gameObject;
					PlaceBuildings(buildings,block);
				}
			}			
	}

	void PlaceBuildings(GameObject[] buildings, Transform block){
		for(int i=0;i<buildings.Length;i++){
			Vector3 newpos = RandomBox(block, buildings[i]);
			Bounds bounds = buildings[i].GetComponent<Renderer>().bounds;
			int tries = 0;
			while(IsOverlapping(buildings[i].transform,newpos,bounds)&&tries<100){
				newpos = RandomBox(block, buildings[i]);
				bounds = buildings[i].GetComponent<Renderer>().bounds;
				tries++;
			}
			buildings[i].transform.position = newpos;
		}
	}

	// bool IsOverlapping(Transform a, Vector3 pos){
	// 	Bounds bounds = a.GetComponent<Renderer>().bounds;
	// 	Collider[] hits = Physics.OverlapBox(pos,bounds.extents*1.2f,a.rotation,1<<9);
	// 	foreach(Collider c in hits)
	// 		Debug.Log(c);
	// 	return hits.Length!=0;
	// }

	bool IsOverlapping(Transform a, Vector3 pos, Bounds bounds){return Physics.OverlapBox(pos,bounds.extents*1.3f,a.rotation,1<<9).Length!=0;}

	Vector3 RandomBox(Transform block, GameObject box){
		float limit = blockSize/2 - Mathf.Max(box.GetComponent<Renderer>().bounds.extents.x,box.GetComponent<Renderer>().bounds.extents.z);
		Vector3 pos = new Vector3(Random.Range(-limit,limit),0,Random.Range(-limit,limit)) + block.position;
		if(pos.x>block.position.x&&pos.z>block.position.z)
			box.transform.Rotate(0*Vector3.up);
		if(pos.x<block.position.x&&pos.z>block.position.z)
			box.transform.Rotate(90*Vector3.up);
		if(pos.x<block.position.x&&pos.z<block.position.z)
			box.transform.Rotate(180*Vector3.up);
		if(pos.x>block.position.x&&pos.z<block.position.z)
			box.transform.Rotate(270*Vector3.up);
		//TODO: optimise into 1 line
		return pos;
	}
}
