using UnityEngine;
using System.Collections.Generic;

public class MapGen : MonoBehaviour
{
    public IPathGenerator pathGen = new DrunkenWalk();
    public List<Room> createdRooms = new();
    public int pathSize;
    
    void Awake()
    {

    }
    public void CreateMap()
    {
        pathGen = new DrunkenWalk();
        List<Cell> path = pathGen.Generate(pathSize);
        Debug.Log("here we are");

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
