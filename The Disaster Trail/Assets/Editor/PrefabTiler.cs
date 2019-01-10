using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabTiler : EditorWindow{
	GameObject prefab;
	Vector3Int num;
	Vector3 pos, rot, delta;
	GameObject clone;
	List<GameObject> clones = new List<GameObject>();

	bool spawned = false, update;

	[MenuItem("Window/Prefab Tiler")]
	public static void ShowWindow(){
		GetWindow<PrefabTiler>("Prefab Tiler");
	}

	void OnGUI(){
		EditorGUILayout.Space();
		GUILayout.Label("Place object in field and edit attributes", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Prefab:");
		if(GUILayout.Button("Clear"))
			prefab = null;
		prefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject),true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		pos = EditorGUILayout.Vector3Field("Starting Position", pos);
		rot = EditorGUILayout.Vector3Field("Per-Object Rotation", rot);
		delta = EditorGUILayout.Vector3Field("Distance between Objects", delta);
		num = EditorGUILayout.Vector3IntField("How many instances in each dimension?", num);

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Force Update"))
			update = true;

		if(GUILayout.Button("Clear All")){
			Destroy(prefab);
			prefab = null;
			pos = new Vector3();
			rot = new Vector3();
			delta = new Vector3();
			num = new Vector3Int();
		}
		EditorGUILayout.EndHorizontal();

		if(GUI.changed||update){
			update = false;
			if(prefab!=null){
				if(!spawned){
					clone = Object.Instantiate(prefab,pos,Quaternion.Euler(rot));
					spawned = true;
				}
				else{
					if(clone!=null){
						clone.transform.position = pos;
						clone.transform.rotation = Quaternion.Euler(rot);
					}
				}

				if(num.x>=1||num.y>=1||num.z>=1){
					foreach(GameObject obj in clones)
						DestroyImmediate(obj);
					for(int i=0;i<num.x;i++)
						for(int j=0;j<num.y;j++)
							for(int k=0;k<num.z;k++)
								clones.Add(Object.Instantiate(prefab,pos+new Vector3(delta.x*i,delta.y*j,delta.z*k),Quaternion.Euler(rot)));
					// for(int i=0;i<num.x;i++)
					// 	for(int j=0;j<num.y;j++)
					// 		for(int k=0;k<num.z;k++){
					// 			clones[(i+1)*(j+1)*(k+1)-1].transform.position = pos+new Vector3(delta.x*i,delta.y*j,delta.z*k);
					// 			clones[(i+1)*(j+1)*(k+1)-1].transform.rotation = Quaternion.Euler(rot);
					// 		}
					// if((i+1)*(j+1)*(k+1)>clones.Count)

				}
			}	
		}
	}
}
