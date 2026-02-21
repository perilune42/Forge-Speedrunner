using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using System;
using static Direction;

public class MainPath : IRoomChoiceStrategy
{
    List<Room> RightOnly;
    List<Room> DownOnly;
    int minY = Int32.MaxValue;
    public MainPath(Room[] roomPrefabs)
    {
        RightOnly = new();
        DownOnly = new();
        for(int i = 0; i < roomPrefabs.Length; i++)
        {
            Room r = roomPrefabs[i];
            List<Doorway> downs = r.doorwaysDown;
            List<Doorway> rights = r.doorwaysRight;
            bool hasRight = rights.Any(x => x != null);
            bool hasDown = downs.Any(x => x != null);
            if(hasRight)
                RightOnly.Add(r);
            if(hasDown)
                DownOnly.Add(r);
        }
        RightOnly.Shuffle();
        DownOnly.Shuffle();
    }

    public Room FindRoom(Direction dir, Offset off, in HashSet<Room> placedRooms)
    {
        if(off.y > minY)
            return null;

        if(dir == LEFT || dir == UP)
            return null;

        List<Room> firstList; List<Room> secondList;
        int ind = UnityEngine.Random.Range(0, 2);
        (firstList, secondList) = ind == 1 
            ? (RightOnly, DownOnly) 
            : (DownOnly, RightOnly);

        if(firstList.Count <= 0 && secondList.Count <= 0)
            return null;

        Direction opposite = DirMethods.opposite(dir);

        for(int i = 0; i < firstList.Count; i++)
        {
            Room r = firstList[i];
            List<Doorway> doors = DirMethods.matchingDir(opposite, r);
            if(doors.Any(x => x != null))
            {
                firstList.RemoveAt(i);
                return r;
            }
        }
        for(int i = 0; i < secondList.Count; i++)
        {
            Room r = secondList[i];
            List<Doorway> doors = DirMethods.matchingDir(opposite, r);
            if(doors.Any(x => x != null))
            {
                secondList.RemoveAt(i);
                return r;
            }
        }

        return null;
    }
}
