using UnityEngine;

[CreateAssetMenu(fileName = "GameItem", menuName = "Items/GameItem")]
public class GameItem : ScriptableObject
{
	public GameObject itemPrefab;
	public Sprite icon;
	public int quantity;
	public float value;
	public string displayName, description;
	public bool transferable;

	public GameItem Init(GameItem item)
	{
		this.itemPrefab = item.itemPrefab;
		this.icon = item.icon;
		this.value = item.value;
		this.quantity = item.quantity;
		this.displayName = item.displayName;
		this.description = item.description;
		this.transferable = item.transferable;

		return this;
	}
}
