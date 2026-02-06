
using UnityEngine;
using System.Collections.Generic;

using RoomID = System.Int32;

public class RoomStorage : Singleton<RoomStorage>
{
    [SerializeField] private List<GameObject> roomPrefabs;

    // returns null if ID is invalid
    public GameObject Get(RoomID id)
    {
        if(id >= roomPrefabs.Count || id < 0)
        {
            return null;
        }
        return roomPrefabs[id];
    }
}
