using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
	private InventoryStats localInventory;

	//This is only used when a scene has been loaded without going through the overworld. (Debugging hopefully).
	private int maxInventoryItems = 10;


	/*
	 * TODO:
	 *	Add some form of UI for the player's inventory.
	 *	This will utilize the GameItem's sprite icon for the visual.
	 *	Also use the displayName and description (optionally via tooltip).
	 *	The quantity should be displayed as well, probably in the corner of the icon.
	 *  The UI can be always visible, or pop up with a button/key press ("I" most likely)
	 */ 


	void Start()
	{
		if (GameManager.instance == null)
		{
			Debug.LogWarning("Cannot find GameManager instance! Creating new one.");
			new GameObject("GameManager", typeof(GameManager));
			localInventory = new InventoryStats() { inventory = new List<GameItem>(), currentItem = -1, maxItems = maxInventoryItems };
			SaveInventory();
		}
		else
		{
			localInventory = new InventoryStats(GameManager.instance.inventoryStats);
		}
	}

	public void AddToInventory(GameItem item)
	{
		//Lookup item by name. 
		//If already in inventory, increase quantity. 
		//Otherwise add to inventory.
		int idx = localInventory.inventory.FindIndex(i => string.Equals(i.displayName, item.displayName));

		if (idx >= 0)
		{
			localInventory.inventory[idx].quantity += item.quantity;
		}
		else
		{
			GameItem newItem = ScriptableObject.CreateInstance<GameItem>().Init(item);
			localInventory.inventory.Add(newItem);
			
			//Set current item to 0 when picking up FIRST item. 
			//Can be changed to swap to new item always if desired.
			if (localInventory.currentItem < 0)
			{
				localInventory.currentItem = 0;
			}
		}
	}

	public void Purchase(GameItem item)
	{
		localInventory.money -= item.value;
		AddToInventory(item);
	}

	public void RemoveFromInventory(GameItem item)
	{
		//Remove item quantity from inventory item.
		//If that brings quantity to <= 0, remove item from inventory.
		int idx = localInventory.inventory.FindIndex(i => string.Equals(i.displayName, item.displayName));

		if (idx < 0)
		{
			return;
		}

		localInventory.inventory[idx].quantity -= item.quantity;
		if (localInventory.inventory[idx].quantity <= 0)
		{
			localInventory.inventory.RemoveAt(idx);

			//If the item removed from inventory is the LAST item, put current item to the previous item.
			//In all other cases the current item will point to the NEXT item in the inventory.
			if (idx == localInventory.inventory.Count)
			{
				localInventory.currentItem--;
			}
		}
	}

	public GameItem GetCurrentItem()
	{
		if (localInventory.currentItem < 0)
		{
			Debug.LogWarning("Trying to get an item from the inventory before anything has been added to the inventory!");
			return null;
		}
		return localInventory.inventory[localInventory.currentItem];
	}

	public float GetCurrentMoney()
	{
		return localInventory.money;
	}

	public void CycleInventory(int direction)
	{
		if (localInventory.currentItem < 0)
		{
			Debug.LogWarning("Trying to cycle inventory before any items are in inventory");
			return;
		}

		localInventory.currentItem += direction;

		if (localInventory.currentItem >= localInventory.inventory.Count)
		{
			localInventory.currentItem = 0;
		}
		else if (localInventory.currentItem < 0)
		{
			localInventory.currentItem = localInventory.inventory.Count - 1;
		}
	}

	public void SaveInventory()
	{
		//should not ever happen, just being safe.
		if (GameManager.instance == null)
		{
			Debug.LogWarning("No GameManager instance found. Inventory not saved!");
			return;
		}

		GameManager.instance.inventoryStats = new InventoryStats(localInventory);
	}

	public void DebugInventory()
	{
		Debug.Log(string.Format("Current Index: {0}", localInventory.currentItem));
		foreach (GameItem item in localInventory.inventory)
		{
			Debug.Log(string.Format("name = {0}, description = {1}, quantity = {2}, value = {3}", 
				item.displayName, item.description, item.quantity, item.value));
		}
	}
}
