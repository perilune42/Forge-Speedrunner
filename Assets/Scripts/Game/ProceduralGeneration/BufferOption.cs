using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using static Direction;
public class BufferOption : IChoiceStrategy
{
    List<Room> RoomPrefabs;
    public BufferOption(Room[] prefabs)
    {
        RoomPrefabs = prefabs.ToList()
            .FindAll(x => x != null && x.size.x == 1 && x.size.y == 1 && x.doorwaysRight.Any(y => y != null && y.IsExit()));
    }
    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs, in Grid grid)
    {
        // find the last room's rightmost offset of door
        Cell c = grid.uniqueCells[grid.uniqueCells.Count-1];
        Room r = c.room;
        Offset botleft = c.offset;
        Offset yof = new(0,1);
        Offset xof = new(1,0);
        List<Offset> all = allPossibleEntriesFor(botleft, r);
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
        Debug.Log("[BufferOption.FindRoom] begin!");
        // if(dir == RIGHT || dir == LEFT) return null;

        List<Room> fits = RoomPrefabs.FindAll(x => {
                List<Doorway> doors = dir switch
                {
                UP => x.doorwaysDown,
                DOWN => x.doorwaysUp,
                RIGHT => x.doorwaysLeft,
                LEFT => x.doorwaysRight,
                _ => null,
                };
                return doors != null && doors.Any(z => z != null);
            });
        if(fits.Count == 0)
            return null;
        foreach(Room r in fits)
            if(!placedRooms.Contains(r))
            {
                placedRooms.Add(r);
                return r;
            }
        return fits[0];
    }
    private int OffSize(Offset x)
    {
        Offset xx = x * x;
        return xx.x + xx.y;
    }
    private List<Offset> allPossibleEntriesFor(Offset botleft, Room r)
    {
        List<Offset> all = new();
        Offset yof = new(0,1);
        Offset xof = new(1,0);
        for(int i = 0; i < r.size.y; i++)
        {
            Offset checkRight = botleft + xof * r.size.x + yof * i;
            Offset checkLeft = botleft + yof * i - xof;
            if(r.doorwaysRight[i] != null)
                all.Add(checkRight);
            if(r.doorwaysLeft[i] != null)
                all.Add(checkLeft);
        }
        for(int i = 0; i < r.size.x; i++)
        {
            Offset checkUp = botleft + yof * r.size.y + xof * i;
            Offset checkDown = botleft + xof * i - yof;
            if(r.doorwaysUp[i] != null)
                all.Add(checkUp);
            if(r.doorwaysDown[i] != null)
                all.Add(checkDown);
        }
        return all;
    }
}
