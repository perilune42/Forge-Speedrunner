using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using static Direction;
public class PlaceFinal : IChoiceStrategy
{
    Room FinalRoom;
    public PlaceFinal(Room final)
    {
        FinalRoom = final;
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
        for(int i = 0; i < r.size.y; i++)
        {
            Offset check = botleft + xof * r.size.y + yof * i;
            if(r.doorwaysRight[i] != null)
                all.Add(check);
        }

        for(int i = 0; i < offs.Count; i++)
            if(all.Contains(offs[i]))
                return i;

        return 0;
    }
    public Room FindRoom(Direction dir, Offset _off, Grid grid, in HashSet<Room> _createdRooms)
    {
        Debug.Log("[PlaceFinal.FindRoom] here's the end!");
        return FinalRoom;
    }
}
