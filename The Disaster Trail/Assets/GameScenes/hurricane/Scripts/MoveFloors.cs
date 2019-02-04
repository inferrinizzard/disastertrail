using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFloors : MonoBehaviour
{
	[SerializeField] private Transform destinationObject;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.gameObject.CompareTag("Player"))
		{
			return;
		}

		GameManager.instance.FadeToBlack(GameManager.instance.FadeTransitionSpeedPerFrame, () =>
		{
			other.transform.position = destinationObject.position;
			GameManager.instance.FadeFromBlack(GameManager.instance.FadeTransitionSpeedPerFrame);
		});
	}
}
