using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryStats
{
	public List<GameItem> inventory;
	public int currentItem;
	public int maxItems;
	public float money;

	public InventoryStats() { }

	//Copy constructor, using deep copy of GameItems in inventory.
	public InventoryStats(InventoryStats stats)
	{
		inventory = new List<GameItem>();
		stats.inventory
			.Where(i => i.transferable)
			.ToList()
			.ForEach(i => inventory.Add(ScriptableObject.CreateInstance<GameItem>().Init(i)));

		currentItem = stats.currentItem;
		maxItems = stats.maxItems;
		money = stats.money;
	}
}
