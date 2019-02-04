using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurricaneCameraController : MonoBehaviour
{

	[SerializeField] private Transform target;
	[SerializeField] private float cameraRotateSpeed;
	[SerializeField] private MobileJoyStick joyStickTurn;

	private PlayerTopDown ptd;

	void Start()
	{
		ptd = target.GetComponent<PlayerTopDown>();
	}

	void LateUpdate()
	{
#if UNITY_IOS || UNITY_ANDROID
		if (ptd.CanMove)
		{
			float horizontal = joyStickTurn.Horizontal() * cameraRotateSpeed;
			target.transform.Rotate(0, 0, -horizontal);
		}
#elif UNITY_STANDALONE
		if (ptd.CanMove)
		{
			float horizontal = Input.GetAxis("Mouse X") * cameraRotateSpeed;
			target.transform.Rotate(0, 0, -horizontal);
		}
#endif

		transform.LookAt(target);
	}
}
