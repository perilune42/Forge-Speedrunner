using UnityEngine;
using System;
using System.Collections.Generic;

public class MapGen : MonoBehaviour
{
    public IPathGenerator pathGen;
    public List<Room> createdRooms = new();
    public List<Passage> passagesDebug;
    public int pathSize;
    
    void Awake()
    {

    }
    public void CreateMap()
    {
        Room[] roomPrefabs = Array.ConvertAll(GameRegistry.Instance.RoomPrefabs, x => x.GetComponent<Room>());
        Room start = GameRegistry.Instance.StartRoom.GetComponent<Room>();
        RandomFromPoint pathGen = new RandomFromPoint(roomPrefabs, start, null); // end is kind of ignored for now
        List<Cell> path = pathGen.Generate(pathSize);
        passagesDebug = pathGen.RealizePath();
        Debug.Log($"here we are. size: {path.Count}");

        foreach(Cell c in path)
        {
            Vector3 screenPosition = new(c.offset.x, c.offset.y, 0F);
            screenPosition *= 100F;
            Room room = c.room;

            Room realRoom = (Room)Instantiate(room, screenPosition, Quaternion.identity);
            createdRooms.Add(realRoom);
            realRoom.gridPosition = c.offset;
            realRoom.transform.SetParent(transform);
        }
    }
    public void DeleteMap()
    {
        foreach(Room r in createdRooms)
        {
            DestroyImmediate(r.gameObject);
        }
        createdRooms = new();
    }
}
