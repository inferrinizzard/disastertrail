using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowManager : MonoBehaviour
{
	public Dictionary<string, Transform[]> snow;
	public Transform[] snowmen;
	public Transform[] snowpiles;
	// Start is called before the first frame update
	void Start()
	{
		snowmen = new Transform[transform.GetChild(0).childCount - 1];
		snowpiles = new Transform[transform.GetChild(1).childCount - 1];
		snow = new Dictionary<string, Transform[]>
		{
			{"men",snowmen},
			{"piles",snowpiles}
		};

		Debug.Log(snow["piles"]);

		foreach (string key in snow.Keys)
		{
			for (int i = 1; i < transform.GetChild(key == "men" ? 0 : 1).childCount; i++)
				snow[key][i - 1] = transform.GetChild(key == "men" ? 0 : 1).GetChild(i);

			foreach (Transform t in snow[key])
			{
				for (int i = 0; i < t.childCount; i++)
					t.GetChild(i).rotation = Quaternion.Euler(Vector3.up * (key == "men" ? (Random.Range(0, -60) - 150) : Random.Range(0, 360)));
				for (int i = Random.Range(0, t.childCount); i >= 0; i--)
					Destroy(t.GetChild(i).gameObject);
			}
		}

	}

	// Update is called once per frame
	void Update()
	{
		foreach (string key in snow.Keys)
		{
			foreach (Transform t in snow[key])
				if (t.childCount == 0)
					t.gameObject.SetActive(false);
		}
	}

	public void Increase()
	{
		foreach (string key in snow.Keys)
			foreach (Transform t in snow[key])
			{
				if (t.childCount < t.childCount && Random.Range(0, 3) > 0)
					Object.Instantiate(t.parent.GetChild(0).GetChild(t.parent.GetChild(0).childCount - 1 - t.childCount).gameObject, t).transform.SetAsFirstSibling();
				t.gameObject.SetActive(t.childCount > 0);
			}
	}

}
