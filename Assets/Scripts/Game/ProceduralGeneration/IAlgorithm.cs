using UnityEngine;
using Offset = UnityEngine.Vector2Int;
using System.Collections.Generic;

public abstract class IAlgorithm
{
    public abstract bool Run(Grid grid, HashSet<Room> placedRooms);


    // ROOM METHODS (you might find them useful)
    protected int hasAny(List<Doorway> doorList)
    {
        for(int i = 0; i < doorList.Count; i++)
            if(doorList[i] != null)
                return i;
        return -1;
    }
    protected int hasAny(List<Doorway> doorList, DoorwayType type)
    {
        for(int i = 0; i < doorList.Count; i++)
            if(doorList[i] != null && (doorList[i].Type == type || doorList[i].Type == DoorwayType.BOTH))
                return i;
        return -1;
    }
    protected bool TryAdd(Grid grid, Room room, Offset off, Direction dir, HashSet<Room> placedRooms)
    {
        Offset botleft;

        if(placedRooms.Contains(room))
            return false;
        if(grid.CanFit(room, off, dir, out botleft))
        {
            grid.InsertRoom(room, botleft);
            placedRooms.Add(room);
            return true;
        }
        return false;
    }
    protected bool TryAddFirst(Grid grid, List<Room> rooms, Offset off, Direction dir, HashSet<Room> placedRooms)
    {
        foreach(Room room in rooms)
        {
            if(TryAdd(grid, room, off, dir, placedRooms))
                return true;
        }
        return false;
    }
}
