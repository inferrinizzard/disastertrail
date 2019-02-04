//using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ItemSpawner : MonoBehaviour
{
	[SerializeField] private Transform player;

    //Dictionaries for faster lookup
    private Dictionary<Transform, float> roomDict = new Dictionary<Transform, float>();
    private Dictionary<GameObject, float> itemDict = new Dictionary<GameObject, float>();
	[SerializeField] private List<Transform> noItemSpawnRooms;

	//Array of items (GameObjects) the player can pick up
	[SerializeField] private GameObject[] postiveItems;
    [SerializeField] private GameObject[] negativeItems;
    [SerializeField] private GameObject[] medKitItems;
    [SerializeField] private GameObject[] drawers;

    private int numberOfMedicalItemsFound = 0;
    private int itemSpawned = 0;

    //Rooms that have an item
    private HashSet<Transform> selectedRooms;
    private HashSet<GameObject> selectedDrawers;


	private PlayerTopDown dangerRooms;
	private HurricaneGameManager hgm;

	//Controlling room/item weights 
	[Tooltip("Chance a positive item spawns in a room")]
    [SerializeField] [Range(0f, 1f)] private float greenRoomWeight = .5f;
    [SerializeField] [Range(0f, 1f)] private float yellowRoomWeight = .5f;
    [SerializeField] [Range(0f, 1f)] private float redRoomWeight = .5f;
    [SerializeField] [Range(0f, 1f)] private float positiveItemWeight = .5f;

    [SerializeField] private int numberOfItemsToSpawn;
    

    //Determine how much space around items
    [SerializeField] [Range(0f, 1f)] private float checkRadius = .5f;

    // Use this for initialization

    private void Awake()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

		dangerRooms = player.GetComponent<PlayerTopDown>();
		hgm = GetComponent<HurricaneGameManager>();

        //Initializing room dictionary 
        InitializeDict(roomDict, dangerRooms.GetGreenDangerRooms(), greenRoomWeight);
        InitializeDict(roomDict, dangerRooms.GetYellowDangerRooms(), yellowRoomWeight);
        InitializeDict(roomDict, dangerRooms.GetRedDangerRooms(), redRoomWeight);

        //Initializing item dictionary 
        InitializeDict(itemDict, postiveItems, positiveItemWeight);
        InitializeDict(itemDict, negativeItems, 1 - positiveItemWeight);

        //prevents duplicate rooms
        selectedRooms = new HashSet<Transform>();
        selectedDrawers = new HashSet<GameObject>();
    }

    void Start()
    {
        //remove all specified rooms from the roomDict dictionary
        ShuffleArray<GameObject>(drawers);
        ShuffleArray<GameObject>(medKitItems);
        DistributeItems();
        //ShuffleMedKitItems();

		roomDict = roomDict.Where(i => !noItemSpawnRooms.Contains(i.Key)).ToDictionary(x => x.Key, x => x.Value);

        GameObject item;
        Transform room;
        Vector3 position;

        for (int i = 0; i < numberOfItemsToSpawn; i++)
        {
            item = GetRandomItem();
            room = GetRandomRoom();
            position = GetRoomPosition(room);
            GameObject clone = Instantiate(item, position, room.localRotation);
            dangerRooms.hurricanePickups.Add(clone);
        }
    }

    private void ShuffleArray<T>(T[] array)
    {
        int index = 0;
        for (int i = array.Length - 1; i > 0; i--)
        {
            index = Random.Range(0, i + 1);
            T a = array[index];
            array[index] = array[i];
            array[i] = a;

        }
    }

    private void InitializeDict<TKey, TValue>(Dictionary<TKey, TValue> temp, TKey[] list, TValue value)
    {
        foreach (TKey item in list)
        {
            temp.Add(item, value);
        }
    }

    private TKey RandomKeys<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
		return dict.Keys.ElementAt(Random.Range(0, dict.Count));
    }

    #region Random Item Functions

    //Calculates total weight of all items and returns an item
    private GameObject GetRandomItem()
    {
        float weightedSumItems = itemDict.Sum((item) => item.Value);
        float randomWeight = Random.Range(0, weightedSumItems);
        float weightSum = 0;
        GameObject ranItem = null;


        while (weightSum < randomWeight)
        {
            ranItem = RandomKeys(itemDict);
            weightSum += itemDict[ranItem];
        }

        return ranItem;
    }

	#endregion

	#region Random Room Functions

	//Returns the transfrom of a random room 
	private Transform GetRandomRoom()
	{
		Dictionary<Transform, float> availableRooms = roomDict.Where(i => !selectedRooms.Contains(i.Key)).ToDictionary(x => x.Key, x => x.Value);

		//Uses random number to simulate mixed array
		float randomWeight = Random.Range(0, availableRooms.Sum((room) => room.Value));
		float weightSum = 0;
		Transform ranRoom = null;

		while (weightSum < randomWeight)
		{
			ranRoom = RandomKeys(availableRooms);
			weightSum += availableRooms[ranRoom];
		}

        selectedRooms.Add(ranRoom);
        return ranRoom;
    }

    #endregion

    #region Random Room Positions Functions

    //Use selected rooms bounds to return a position to spawn item 
    private Vector3 GetRoomPosition(Transform room)
    {
        Vector3 position = new Vector3(0, .05f, 0);
        Renderer roomBounds = room.GetComponent<Renderer>();

        position.x = Random.Range(roomBounds.bounds.min.x, roomBounds.bounds.max.x);
        position.z = Random.Range(roomBounds.bounds.min.z, roomBounds.bounds.max.z);

        Collider[] collisionCheck = Physics.OverlapSphere(position, checkRadius);

        while (collisionCheck.Length > 1)
        {
            position.x = Random.Range(roomBounds.bounds.min.x, roomBounds.bounds.max.x);
            position.z = Random.Range(roomBounds.bounds.min.z, roomBounds.bounds.max.z);
            collisionCheck = Physics.OverlapSphere(position, checkRadius);
        }

        return position;
    }

    #endregion

    #region Random Drawer Functions



    private void DistributeItems()
    {
        int i = 0;

        while( i < medKitItems.Length)
        {
            int index = Random.Range(0, drawers.Length);
            if (!selectedDrawers.Contains(drawers[index]))
            {
                selectedDrawers.Add(drawers[index]);
                i++;
            }
        }
    }

    public void CheckDrawer(GameObject drawer)
    {
        if (selectedDrawers.Contains(drawer))
        {
			GameObject obj = medKitItems[numberOfMedicalItemsFound];
            numberOfMedicalItemsFound++;
            selectedDrawers.Add(drawer);
			hgm.HandleObjectPickup(obj);
            selectedDrawers.Remove(drawer);
        }

    }

    #endregion
}