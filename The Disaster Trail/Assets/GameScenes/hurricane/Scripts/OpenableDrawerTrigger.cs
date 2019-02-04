using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableDrawerTrigger : MonoBehaviour
{
	[SerializeField] private Transform hurricaneGameManager;
	private bool drawerToggle = false;
	private float drawer1z;
	private ItemSpawner spawner;
	private HurricaneSoundManager hsm;
	public bool inProgress = false;

	private void Awake()
	{
		spawner = hurricaneGameManager.GetComponent<ItemSpawner>();
		hsm = hurricaneGameManager.GetComponent<HurricaneSoundManager>();
	}


	private void OnTriggerStay(Collider other)
	{
		if (other.tag == "Player")
		{
			other.GetComponent<PlayerTopDown>().ShowDrawerText(transform);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			other.GetComponent<PlayerTopDown>().HideDrawerText();
		}
	}

	public void ToggleDrawer()
	{
		drawerToggle = !drawerToggle;
		if (drawerToggle)
		{
			spawner.CheckDrawer(transform.parent.gameObject);
			StartCoroutine(OpenDrawer());
		} 
		else
		{
			StartCoroutine(CloseDrawer());
		}
		inProgress = true;
		hsm.DrawerSound(drawerToggle);
	}

	IEnumerator OpenDrawer()
	{
		float x = transform.parent.GetChild(1).transform.localPosition.x;
		float y = transform.parent.GetChild(1).transform.localPosition.y;
		float z = transform.parent.GetChild(1).transform.localPosition.z;
		drawer1z = z;
		float elapsedTime = 0;
		float time = 0.5f;
		while (elapsedTime < time)
		{
				float newZ = Mathf.Lerp(z, 0.75f, elapsedTime / time);
				transform.parent.GetChild(1).transform.localPosition = new Vector3(x, y, newZ);
				elapsedTime += Time.deltaTime;
				yield return null;
		}
		if(elapsedTime>time)
			inProgress = false;
	}

	IEnumerator CloseDrawer()
	{
		float x = transform.parent.GetChild(1).transform.localPosition.x;
		float y = transform.parent.GetChild(1).transform.localPosition.y;
		float z = transform.parent.GetChild(1).transform.localPosition.z;
		float elapsedTime = 0;
		float time = 0.5f;
		while (elapsedTime < time)
		{
			float newZ = Mathf.Lerp(z, drawer1z, elapsedTime / time);
			transform.parent.GetChild(1).transform.localPosition = new Vector3(x, y, newZ);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if(elapsedTime>time)
			inProgress = false;
	}
}
