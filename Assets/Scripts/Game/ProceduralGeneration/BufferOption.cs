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
    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs)
    {
        // find the largest offset
        int maxInd = 0;
        for(int i = 1; i < offs.Count; i++)
            maxInd = OffSize(offs[i]) > OffSize(offs[maxInd]) ? i : maxInd;
        return maxInd;
    }
    public Room FindRoom(Direction dir, Offset off, in HashSet<Room> placedRooms)
    {
        if(dir == RIGHT || dir == LEFT) return null;
        List<Room> fits = RoomPrefabs.FindAll(x => {
                List<Doorway> doors = dir switch
                {
                UP => x.doorwaysDown,
                DOWN => x.doorwaysUp,
                _ => null
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
