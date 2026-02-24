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

        Cell lastCell = grid.uniqueCells[grid.uniqueCells.Count-1];
        Offset botleft = lastCell.offset;
        Offset size = lastCell.room.size;
        Offset rightOutside = botleft - new Offset(1,1);
        Room r = lastCell.room;
        List<Offset> allRoomOffsets = new();
        Offset yof = new(0,1);
        Offset xof = new(1,0);
        for(int i = 0; i < r.size.y; i++)
        {
            Offset rightCheck = botleft + i * yof + xof * room.size.x;
            Offset leftCheck = botleft + i * yof;
            if(r.doorwaysRight[i] != null)
                allRoomOffsets.add(rightCheck);
            if(r.doorwaysLeft[i] != null)
                allRoomOffsets.add(leftCheck);
        }
        for(int i = 0; i < r.size.x; i++)
        {
            Offset upCheck = botleft + i * xof + yof * room.size.y;
            Offset downCheck = botleft + i * xof;
            if(r.doorwaysUp[i] != null)
                allRoomOffsets.add(upCheck);
            if(r.doorwaysDown[i] != null)
                allRoomOffsets.add(downCheck);
        }
        for(int i = 0; i < offs.Count; i++)
        {
            if(allRoomOffsets.Contains(offs[i]))
                return i;
        }
        return 0; // HACK: this needs to be -1 and be handled
        // int maxInd = -1;
        // // find start
        // for(int i = 0; i < offs.Count; i++)
        //     if(dirs[i] == RIGHT)
        //     {
        //         maxInd = i;
        //         break;
        //     }
        // if(maxInd < 0)
        //     return 0; // HACK: have to ignore this later if it's not good.

        // for(int i = 1; i < offs.Count; i++)
        // {
        //     if(dirs[i] != RIGHT)
        //         continue;

        //     Offset current = offs[i];
        //     Offset max = offs[maxInd];
        //     if(max.x <= current.x && max.y <= current.y)
        //         maxInd = i;
        //     else if(current.x >= max.x)
        //         maxInd = i;
        // }
        // return maxInd;
    }
    public Room FindRoom(Direction dir, Offset _off, Grid grid, in HashSet<Room> _createdRooms)
    {
        Debug.Log("[PlaceFinal.FindRoom] here's the end!");
        return FinalRoom;
    }
}
