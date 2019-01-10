using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    //Array of room positions sorted by safty 
    [SerializeField] private PlayerTopDown dangerRooms;
    [SerializeField] private Transform[] greenDangerRooms;
    [SerializeField] private Transform[] yellowDangerRooms;
    [SerializeField] private Transform[] redDangerRooms;
    private float[] roomWeights = new float[4];

    //Controlling room weights 
    [Tooltip("Chance a positive item spawns in a dangerous room")]
    [SerializeField] [Range(0f, 1f)] private float dangerWeight = .5f;
    [SerializeField] [Range(0f, 1f)] private float greenRoomWeight = .5f;
    [SerializeField] [Range(0f, 1f)] private float yellowRoomWeight = .5f;
    [SerializeField] [Range(0f, 1f)] private float redRoomWeight = .5f;


    //Array of items (GameObjects) the player can pick up
    [SerializeField] private GameObject[] postiveItems;
    [SerializeField] private GameObject[] negativeItems;
    [SerializeField] private int numberOfItems;
    [SerializeField] [Range(0f, 1f)] private float positiveItemWeight = .5f;

    //Determine how much space around items
    [SerializeField] [Range(0f, 1f)] private float checkRadius = .5f;

    //Rooms that have an item
    private Transform[] selectedRooms;

    /* TODO:
     *  Optimize While loop in getRoomPosition
     *  
     */

    // Use this for initialization
    void Start()
    {

        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        //initializing green, yellow, and red danger rooms
        greenDangerRooms = dangerRooms.GetGreenDangerRooms();
        yellowDangerRooms = dangerRooms.GetYellowDangerRooms();
        redDangerRooms = dangerRooms.GetRedDangerRooms();

        //prevents duplicate rooms
        selectedRooms = new Transform[numberOfItems];

        GameObject item;
        Transform room;
        Vector3 position;

        for (int i = 0; i < numberOfItems; i++)
        {
            item = GetRandomItem();
            room = GetRandomRoom();
            position = GetRoomPosition(room);
            selectedRooms[i] = room;
            GameObject clone = Instantiate(item, position, room.localRotation);
            dangerRooms.hurricanePickups.Add(clone);
        }
    }

    #region Random Item Functions

    //Calculates total weight of all items and returns an item
    private GameObject GetRandomItem()
    {
        float weightedSumItems = CalculateWeightedSumItems();
        float randomWeight = UnityEngine.Random.Range(0, weightedSumItems);
        float weightSum = 0;
        int i = 0;
        int itemType = 0;


        while (weightSum < randomWeight)
        {
            itemType = UnityEngine.Random.Range(0, 2);
            weightSum += itemType == 0 ? positiveItemWeight : 1 - positiveItemWeight;
            i++;
        }

        CalaculateWeightedSumRooms(itemType);

        if (itemType == 0)
        {
            i = i % postiveItems.Length; 
            return postiveItems[i];
        }
        else
        {
            i = i % negativeItems.Length;
            return negativeItems[i];
        }

    }

    private float CalculateWeightedSumItems()
    {
        return (postiveItems.Length * positiveItemWeight) + (negativeItems.Length * (1 - positiveItemWeight));

    }
    #endregion

    #region Random Room Functions

    //Returns the transfrom of a random room 
    private Transform GetRandomRoom()
    {

        //Uses random number to simulate mixed array
        float randomWeight = UnityEngine.Random.Range(0, roomWeights[3]);
        float weightSum = 0;
        int roomType = 0;
        int i = 0;


        while (weightSum < randomWeight)
        {
            roomType = UnityEngine.Random.Range(0, 3); //0 - green, 1 - yellow, 2 - red
            weightSum += roomType == 0 ? roomWeights[0] : roomType == 1 ? roomWeights[1] : roomWeights[2];
            i++;
        }

        return UniqueRoom(roomType, i);
    }

    //Returns a room that has not been selected
    private Transform UniqueRoom(int roomType, int i)
    {
        Transform[] localArray = roomType == 0 ? greenDangerRooms : roomType == 1 ? yellowDangerRooms : redDangerRooms;

        Transform[] unusedRooms = localArray.Except(selectedRooms).ToArray();

        if (unusedRooms.Length < 1)
        {
            Debug.LogWarning("No unique rooms left");
            return null;
        }

        return unusedRooms[Mathf.Clamp(i, 0, unusedRooms.Length - 1)];
    }

    //Calculates room weights wrt itemType
    private void CalaculateWeightedSumRooms(int itemType)
    {
        if (itemType == 0)
        {
            roomWeights[0] = greenDangerRooms.Length * greenRoomWeight * dangerWeight;
            roomWeights[1] = yellowDangerRooms.Length * yellowRoomWeight;
            roomWeights[2] = redDangerRooms.Length * redRoomWeight;

            roomWeights[3] = roomWeights[0] + roomWeights[1] + roomWeights[2];
        }
        else
        {
            roomWeights[0] = greenDangerRooms.Length * greenRoomWeight;
            roomWeights[1] = yellowDangerRooms.Length * yellowRoomWeight * dangerWeight;
            roomWeights[2] = redDangerRooms.Length * redRoomWeight * dangerWeight;

            roomWeights[3] = roomWeights[0] + roomWeights[1] + roomWeights[2];
        }
    }




    #endregion

    #region Random Room Positions Functions

    //Use selected rooms bounds to return a position to spawn item 
    private Vector3 GetRoomPosition(Transform room)
    {
        Vector3 position = new Vector3(0,.25f,0);
        Renderer roomBounds = room.GetComponent<Renderer>();

        position.x = UnityEngine.Random.Range(roomBounds.bounds.min.x, roomBounds.bounds.max.x);
        position.z = UnityEngine.Random.Range(roomBounds.bounds.min.z, roomBounds.bounds.max.z);

        Collider[] collisionCheck = Physics.OverlapSphere(position, checkRadius);

        while(collisionCheck.Length > 1)
        {
            position.x = UnityEngine.Random.Range(roomBounds.bounds.min.x, roomBounds.bounds.max.x);
            position.z = UnityEngine.Random.Range(roomBounds.bounds.min.z, roomBounds.bounds.max.z);
            collisionCheck = Physics.OverlapSphere(position, checkRadius);
        }
        return position;
    }

    #endregion
}