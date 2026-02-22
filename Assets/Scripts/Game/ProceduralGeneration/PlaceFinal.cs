using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
public class PlaceFinal : IChoiceStrategy
{
    Room FinalRoom;
    public PlaceFinal(Room final)
    {
        FinalRoom = final;
    }
    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs)
    {
        int maxInd = 0;
        for(int i = 1; i < offs.Count; i++)
        {
            Offset current = offs[i];
            Offset max = offs[maxInd];
            if(max.x <= current.x && max.y <= current.y)
                maxInd = i;
            else if(current.x >= max.x)
                maxInd = i;
        }
        return maxInd;
    }
    public Room FindRoom(Direction _dir, Offset _off, in HashSet<Room> _createdRooms)
    {
        return FinalRoom;
    }
}
