using UnityEngine;
using Offset = UnityEngine.Vector2Int;
using System.Collections.Generic;
using System.Text;
using System;
using static Direction;
using static DoorwayType;


/* Please forgive my bitpacking, Jacky. I had fun.
 */
public class DoorwayGrid
{
    /* Each element of this dictionary is using a packed data structure.
     * From least significant to most significant:
     * |UP[2]|DOWN[2]|LEFT[2]|RIGHT[2]|
     * At each point, there is one of 4 values:
     * 01: ENTRANCE
     * 10: EXIT
     * 11: BOTH
     * 00: NONE
     *
     * NOTE: many of the choices on how to handle this data comes from not modifying the original enums.
     *       There could be a better way to do this, should there be a DoorwayType.NONE.
     */
    Dictionary<Vector2Int, byte> opens = new();

    /* From direction, get the "index" into the byte.
     */
    private byte marshallDir(Direction dir) => dir switch
    {
        UP => 0,
        DOWN => 2,
        LEFT => 4,
        _ => 6,
    };

    /* Convert DoorwayType to one of 3 values. Realistically it's never going to be 0.
     */
    private byte marshallType(DoorwayType type) => type switch
    {
        ENTRANCE => 1,
        EXIT => 2,
        BOTH => 3,
        _ => 0
    };

    /* The full operation to turn (Direction, DoorwayType) into byte value.
     */
    private byte marshall(Direction dir, DoorwayType type) => Convert.ToByte(marshallType(type) << marshallDir(dir));

    /* The (more elaborate) way to figure out the DoorwayType at a particular Direction for a given byte value.
     * Returns false if there isn't a DoorwayType to give back (no DoorwayType.NONE yet).
     */
    private bool unmarshallType(byte val, Direction dir, out DoorwayType type)
    {
        type = BOTH;
        byte important = Convert.ToByte((val >> marshallDir(dir)) & 3);
        if(important == 0)
            return false;
        else if(important == 1)
            type = ENTRANCE;
        else if(important == 2)
            type = EXIT;
        return true;
    }

    public void InsertRoom(Room room, Offset off)
    {
        // TODO: allocate a buffer once, and never reallocate
        byte[,] BUFFER = new byte[room.size.x, room.size.y];

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
            {
                BUFFER[i, room.size.y-1] |= marshall(UP, room.doorwaysUp[i].Type);
            }
            if(room.doorwaysDown[i] != null)
            {
                BUFFER[i, 0] |= marshall(DOWN, room.doorwaysDown[i].Type);
            }
        }

        // right and left
        for(int i = 0; i < room.size.y; i++)
        {
            if(room.doorwaysLeft[i] != null)
            {
                BUFFER[0, i] |= marshall(LEFT, room.doorwaysLeft[i].Type);
            }
            if(room.doorwaysRight[i] != null)
            {
                BUFFER[room.size.x-1, i] |= marshall(RIGHT, room.doorwaysRight[i].Type);
            }
        }

        for(int i = 0; i < room.size.x; i++)
            for(int j = 0; j < room.size.y; j++)
        {
            Offset localOff = new(i,j);

            // this condition is true only when we are on the borders of the buffer.
            bool onlyBordersCondition = i < 1 || i == room.size.x-1 || j < 1 || j == room.size.y-1;

            // no point in storing literally nothing, when val == 0.
            byte val = BUFFER[i,j];
            if(onlyBordersCondition && val != 0)
                opens.Add(off+localOff, val);
        }
    }

    public bool Get(Offset off, Direction dir, out DoorwayType type)
    {
        type = BOTH;
        byte val;
        if(!opens.TryGetValue(off, out val))
            return false;
        return unmarshallType(val, dir, out type);
    }

    public bool GetEntrance(Offset off, Direction dir)
    {
        DoorwayType type;
        bool valid = Get(off, dir, out type);
        return valid && type != EXIT;
    }

    public bool GetExit(Offset off, Direction dir)
    {
        DoorwayType type;
        bool valid = Get(off, dir, out type);
        return valid && type != ENTRANCE;
    }

    public void LogEntries()
    {
        StringBuilder sb = new("[DoorwayGrid.LogEntries] Grid contains the following:\n");
        foreach((Offset off, byte val) in opens)
        {
            DoorwayType type;
            sb.Append($"\t{off} has: ");
            if(unmarshallType(val, UP, out type))
                sb.Append($"(UP, {type})");
            if(unmarshallType(val, DOWN, out type))
                sb.Append($"(DOWN, {type})");
            if(unmarshallType(val, RIGHT, out type))
                sb.Append($"(RIGHT, {type})");
            if(unmarshallType(val, LEFT, out type))
                sb.Append($"(LEFT, {type})");
        }
        Debug.Log(sb.ToString());
    }
}
