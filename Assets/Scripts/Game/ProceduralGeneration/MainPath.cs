using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using System;
using static Direction;

public class MainPath : IChoiceStrategy
{
    List<Room> LeftOnly;
    List<Room> DownOnly;
    Offset current;
    public MainPath(Room[] roomPrefabs)
    {
        LeftOnly = new();
        DownOnly = new();
        current = new(Int32.MinValue, Int32.MinValue);
        for(int i = 0; i < roomPrefabs.Length; i++)
        {
            Room r = roomPrefabs[i];
            List<Doorway> downs = r.doorwaysDown;
            List<Doorway> lefts = r.doorwaysLeft;
            bool hasLeft = lefts.Any(x => x != null && x.IsEntrance());
            bool hasDown = downs.Any(x => x != null && x.IsEntrance());
            if(hasLeft)
                LeftOnly.Add(r);
            if(hasDown)
                DownOnly.Add(r);
        }
        LeftOnly.Shuffle();
        DownOnly.Shuffle();
        Debug.Log($"[MainPath.Constructor] have {LeftOnly.Count} rooms with left openings and {DownOnly.Count} rooms with down openings.");
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
        //
        Debug.Log($"[MainPath.FindRoom] looking at {dir}, {off}.");

        if(dir == LEFT || dir == DOWN)
            return null;

        List<Room> firstList; List<Room> secondList;
        int ind = UnityEngine.Random.Range(0, 2);
        (firstList, secondList) = ind == 1
            ? (LeftOnly, DownOnly)
            : (DownOnly, LeftOnly);

        if(firstList.Count <= 0 && secondList.Count <= 0)
            return null;

        Direction opposite = DirMethods.opposite(dir);

        for(int i = 0; i < firstList.Count; i++)
        {
            Room r = firstList[i];
            if(off.y < current.y || off.x < current.x)
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
            if(off.y < current.y || off.x < current.x)
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
