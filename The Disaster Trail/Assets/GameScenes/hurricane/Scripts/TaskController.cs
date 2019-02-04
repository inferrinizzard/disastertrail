using UnityEngine;
using UnityEngine.UI;

public class TaskController : MonoBehaviour
{
	[SerializeField] private GameObject _item;
	public GameObject Item
	{
		get
		{
			return _item;
		}
	}

	public Toggle toggle;
	[HideInInspector] public bool completed;

	void Start()
	{
		toggle.interactable = false;
	}
}
