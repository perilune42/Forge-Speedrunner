using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using System;
using static Direction;

public class MainPathReal : IChoiceStrategy
{
    List<Room> LeftOnly;
    List<Room> DownOnly;
    public MainPathReal(Room[] roomPrefabs)
    {
        LeftOnly = new();
        DownOnly = new();
        for(int i = 0; i < roomPrefabs.Length; i++)
        {
            Room r = roomPrefabs[i];
            List<Doorway> downs = r.doorwaysDown;
            List<Doorway> lefts = r.doorwaysLeft;
            bool hasLeft = lefts.Any(x => x != null && x.IsEntrance());
            bool hasDown = downs.Any(x => x != null && x.IsEntrance());
            bool hasUp = r.doorwaysUp.Any(x => x != null && x.IsExit());
            bool hasRight = r.doorwaysRight.Any(x => x != null && x.IsExit());
            if(!hasUp && !hasRight) continue;
            if(hasLeft)
                LeftOnly.Add(r);
            if(hasDown)
                DownOnly.Add(r);
        }
        LeftOnly.Shuffle();
        DownOnly.Shuffle();
        Debug.Log($"[MainPathReal.Constructor] have {LeftOnly.Count} rooms with left openings and {DownOnly.Count} rooms with down openings.");
    }

    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs, in Grid grid)
    {
        Cell c = grid.uniqueCells[grid.uniqueCells.Count-1];
        List<Offset> all = allPossibleEntriesFor(c.offset, c.room);
        all.Shuffle();
        foreach(Offset o in all)
        {
            int ind = offs.IndexOf(o);
            if(ind > -1)
                return ind;
        }
        return 0;
    }

    public Room FindRoom(Direction dir, Offset off, Grid grid, in HashSet<Room> placedRooms)
    {
        Debug.Log($"[MainPathReal.FindRoom] looking at {dir}, {off}.");

        if(dir == LEFT || dir == DOWN)
            return null;

        List<Room> lst = dir == RIGHT ? LeftOnly : DownOnly;
        List<Room> otherLst = lst == LeftOnly ? DownOnly : LeftOnly;
        if(lst == null || lst.Count <= 0)
            return null;

        Room r = lst[0];
        lst[0] = lst[lst.Count-1];
        lst.RemoveAt(lst.Count-1);
        otherLst.Remove(r);

        return r;
    }
    private List<Offset> allPossibleEntriesFor(Offset botleft, Room r)
    {
        List<Offset> all = new();
        Offset yof = new(0,1);
        Offset xof = new(1,0);
        for(int i = 0; i < r.size.y; i++)
        {
            Offset checkRight = botleft + xof * r.size.x + yof * i;
            // Offset checkLeft = botleft + yof * i - xof;
            if(r.doorwaysRight[i] != null)
                all.Add(checkRight);
            // if(r.doorwaysLeft[i] != null)
            //     all.Add(checkLeft);
        }
        for(int i = 0; i < r.size.x; i++)
        {
            Offset checkUp = botleft + yof * r.size.y + xof * i;
            // Offset checkDown = botleft + xof * i - yof;
            if(r.doorwaysUp[i] != null)
                all.Add(checkUp);
            // if(r.doorwaysDown[i] != null)
            //     all.Add(checkDown);
        }
        return all;
    }
}
