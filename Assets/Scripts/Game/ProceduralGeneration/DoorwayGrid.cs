using UnityEngine;
using Offset = UnityEngine.Vector2Int;
using System.Collections.Generic;
using System.Text;
using static Direction;

public class DoorwayGrid
{
    HashSet<Vector2Int> upOpens = new();
    HashSet<Vector2Int> downOpens = new();
    HashSet<Vector2Int> leftOpens = new();
    HashSet<Vector2Int> rightOpens = new();

    public void InsertRoom(Room room, Offset off)
    {
        Offset xof = new(1,0);
        Offset yof = new(0,1);
        Offset leftStart = off - xof;
        Offset rightStart = off + xof * room.size;
        Offset upStart = off + yof * room.size;
        Offset downStart = off - yof;

        // up and down
        for(int i = 0; i < room.size.x; i++)
        {
            if(room.doorwaysUp[i] != null)
                upOpens.Add(upStart + xof * i);
            if(room.doorwaysDown[i] != null)
                downOpens.Add(downStart + xof * i);
        }

        // right and left
        for(int i = 0; i < room.size.y; i++)
        {
            if(room.doorwaysLeft[i] != null)
                leftOpens.Add(leftStart + yof * i);
            if(room.doorwaysRight[i] != null)
                rightOpens.Add(rightStart + yof * i);
        }
    }

    public bool Get(Offset off, Direction dir) => dir switch
    {
        UP => upOpens.Contains(off),
        DOWN => downOpens.Contains(off),
        LEFT => leftOpens.Contains(off),
        _ => rightOpens.Contains(off) // RIGHT
    };
    public void LogEntries()
    {
        StringBuilder sb = new("[DoorwayGrid.LogEntries] Grid contains the following:\n");
        foreach(Offset x in upOpens)
            sb.Append($"\t{x} opens UP.\n");
        foreach(Offset x in downOpens)
            sb.Append($"\t{x} opens DOWN.\n");
        foreach(Offset x in leftOpens)
            sb.Append($"\t{x} opens LEFT.\n");
        foreach(Offset x in rightOpens)
            sb.Append($"\t{x} opens RIGHT.\n");
        Debug.Log(sb.ToString());
    }
}
