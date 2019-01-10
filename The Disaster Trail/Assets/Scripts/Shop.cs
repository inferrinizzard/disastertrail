using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
	[SerializeField] private Transform player;
	[SerializeField] private List<GameItem> items;

	private PlayerInventory inv;

	/*
	 * TODO: 
	 *	Build UI out of items list when "Shop" button is clicked.
	 *	Probably use some form of grid for this -- currently waiting for UI to be designed.
	 *	Make "Buy" buttons inactive unless CanBuyItems() returns True for that GameItem.
	 *	Create GameItems for all items to go in store. 
	 *	Potentially add selling items from inventory to shop to get money back?
	 */ 


	void Start()
	{
		inv = player.gameObject.GetComponent<PlayerInventory>();
	}

	public bool CanBuyItem(GameItem item)
	{
		return inv.GetCurrentMoney() >= item.value;
	}

	public void BuyItem(GameItem item)
	{
		if (CanBuyItem(item))
		{
			inv.Purchase(item);
		}
		else
		{
			Debug.Log("Not enough money :(");
		}
	}
}
