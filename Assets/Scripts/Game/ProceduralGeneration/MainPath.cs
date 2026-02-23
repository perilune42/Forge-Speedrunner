using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using System;
using static Direction;

public class MainPath : IChoiceStrategy
{
    List<Room> RightOnly;
    List<Room> DownOnly;
    Offset current;
    public MainPath(Room[] roomPrefabs)
    {
        RightOnly = new();
        DownOnly = new();
        current = new(Int32.MinValue, Int32.MaxValue);
        for(int i = 0; i < roomPrefabs.Length; i++)
        {
            Room r = roomPrefabs[i];
            List<Doorway> downs = r.doorwaysDown;
            List<Doorway> rights = r.doorwaysRight;
            bool hasRight = rights.Any(x => x != null && !x.Entrance);
            bool hasDown = downs.Any(x => x != null && !x.Entrance);
            if(hasRight)
                RightOnly.Add(r);
            if(hasDown)
                DownOnly.Add(r);
        }
        RightOnly.Shuffle();
        DownOnly.Shuffle();
    }

    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs)
    {
        return UnityEngine.Random.Range(0, dirs.Count);

        // // access entire list in random order
        // List<int> inds = Enumerable<int>.Range(0, dirs.Count);
        // inds.Shuffle();

        // Direction dir; Offset off;
        // foreach(int i in inds)
        // {
        //     dir = dirs[i];
        //     off = offs[i];
        //     // TODO: find a reason to do this. none yet.
        // }
    }

    public Room FindRoom(Direction dir, Offset off, in HashSet<Room> placedRooms)
    {
        // if(off.y > current.y || off.x < current.x)
        //     return null;

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
            if(off.y - r.size.y > current.y || off.x < current.x)
                continue;
            List<Doorway> doors = DirMethods.matchingDir(opposite, r);
            if(doors.Any(x => x != null))
            {
                firstList.RemoveAt(i);
                current = off;
                return r;
            }
        }
        for(int i = 0; i < secondList.Count; i++)
        {
            Room r = secondList[i];
            if(off.y - r.size.y > current.y || off.x < current.x)
                continue;
            List<Doorway> doors = DirMethods.matchingDir(opposite, r);
            if(doors.Any(x => x != null))
            {
                secondList.RemoveAt(i);
                current = off;
                return r;
            }
        }

        return null;
    }
}
