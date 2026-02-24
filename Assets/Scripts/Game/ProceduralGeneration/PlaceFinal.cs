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
    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs)
    {
        int maxInd = -1;
        // find start
        for(int i = 0; i < offs.Count; i++)
            if(dirs[i] == RIGHT)
            {
                maxInd = i;
                break;
            }
        if(maxInd < 0)
            return 0; // HACK: have to ignore this later if it's not good.

        for(int i = 1; i < offs.Count; i++)
        {
            if(dirs[i] != RIGHT)
                continue;

            Offset current = offs[i];
            Offset max = offs[maxInd];
            if(max.x <= current.x && max.y <= current.y)
                maxInd = i;
            else if(current.x >= max.x)
                maxInd = i;
        }
        return maxInd;
    }
    public Room FindRoom(Direction dir, Offset _off, in HashSet<Room> _createdRooms)
    {
        Debug.Log("[PlaceFinal.FindRoom] here's the end!");
        if(dir == RIGHT)
            return FinalRoom;
        else
            return null;
    }
}
