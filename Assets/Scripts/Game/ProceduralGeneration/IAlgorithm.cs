using UnityEngine;
using Offset = UnityEngine.Vector2Int;
using System;
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
    protected bool TryAdd(Grid grid, Room room, Offset off, Direction dir, HashSet<Room> placedRooms, bool removeDup)
    {
        Offset botleft;

        if(removeDup && placedRooms.Contains(room))
            return false;
        if(grid.CanFit(room, off, dir, out botleft))
        {
            grid.InsertRoom(room, botleft);
            placedRooms.Add(room);
            return true;
        }
        return false;
    }
    protected bool TryAddFirst(Grid grid, List<Room> rooms, Offset off, Direction dir, HashSet<Room> placedRooms, bool removeDup)
    {
        foreach(Room room in rooms)
        {
            if(TryAdd(grid, room, off, dir, placedRooms, removeDup))
                return true;
        }
        return false;
    }
    protected bool GetFirstMatching(List<(Offset, Direction)> spotList, Func<Direction, bool> pred, out Offset off, out Direction dir)
    {
        off = default;
        dir = default;
        for(int i = 0; i < spotList.Count; i++)
        {
            (off, dir) = spotList[i];
            if(pred(dir))
                return true;
        }
        return false;
    }
}
