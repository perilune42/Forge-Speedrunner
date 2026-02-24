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
            .FindAll(x => x != null && x.size.x == 1 && x.size.y == 1 && x.doorwaysRight.Any(y => y != null));
    }
    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs, in Grid grid)
    {
        // find the last room's rightmost offset of door
        Cell c = grid.uniqueCells[grid.uniqueCells.Count-1];
        Room r = c.room;
        Offset botleft = c.offset;
        Offset yof = new(0,1);
        Offset xof = new(1,0);
        List<Offset> all = new();
        for(int i = 0; i < room.size.y; i++)
        {
            Offset check = botleft + xof * room.size.y + yof * i;
            if(r.doorwaysRight[i] != null)
                all.Add(check);
        }

        for(int i = 0; i < offs.Count; i++)
            if(all.Contains(offs[i]))
                return i;

        return 0;
    }
    public Room FindRoom(Direction dir, Offset off, in HashSet<Room> placedRooms)
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
                return r;
        return fits[0];
    }
    private int OffSize(Offset x)
    {
        Offset xx = x * x;
        return xx.x + xx.y;
    }
}
