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
    /* A byte buffer holding intermediate values. Intended to be used between
     * calls to InsertRoom, and cleared every time.
     * Prevents some allocations? I think it's good practice.
     */
    internal class Buffer
    {
        List<byte> _buf;
        int rowDim;
        public Buffer(Offset roomSize)
        {
            _buf = new();
            Resize(roomSize);
        }
        public Buffer()
        {
            _buf = new();
        }
        public void Resize(Offset roomSize)
        {
            rowDim = roomSize.y;
            _buf.Capacity = Math.Max(_buf.Capacity, roomSize.x * roomSize.y);
            while(_buf.Count < _buf.Capacity)
                _buf.Add(default);
        }
        public void Clear()
        {
            _buf.Clear();
        }
        public byte this[int x, int y]
        {
            get => _buf[x * rowDim + y];
            set => _buf[x * rowDim + y] = value;
        }
    }



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

    // These are constants that are neat to have.
    private static Offset xof = new Offset(1,0);
    private static Offset yof = new Offset(0,1);

    // Holds intermediate values. Intended to be room.size.x*room.size.y sized, at least.
    static Buffer BUFFER = new();

    // In case this ever needs to be iterated over.
    public IEnumerable<Offset> AllOffsets => opens.Keys;



    /* From direction, get the "index" into the byte.
     */
    private byte PackDirection(Direction dir) => dir switch
    {
        UP => 0,
        DOWN => 2,
        LEFT => 4,
        _ => 6,
    };

    /* Convert DoorwayType to one of 3 values. Realistically it's never going to be 0.
     */
    private byte PackDoorwayType(DoorwayType type) => type switch
    {
        ENTRANCE => 1,
        EXIT => 2,
        BOTH => 3,
        _ => 0
    };

    /* The full operation to turn (Direction, DoorwayType) into byte value.
     */
    private byte Pack(Direction dir, DoorwayType type) => Convert.ToByte(PackDoorwayType(type) << PackDirection(dir));

    /* The (more elaborate) way to figure out the DoorwayType at a particular Direction for a given byte value.
     * Returns false if there isn't a DoorwayType to give back (no DoorwayType.NONE yet).
     */
    private bool UnpackTypeAtDirection(byte val, Direction dir, out DoorwayType type)
    {
        type = BOTH;
        byte important = Convert.ToByte((val >> PackDirection(dir)) & 3);
        if(important == 0)
            return false;
        else if(important == 1)
            type = ENTRANCE;
        else if(important == 2)
            type = EXIT;
        return true;
    }

    /* Predicate condition that decides whether there is a connection between two locations
     */
    private bool HasConnection(DoorwayType typeSrc, DoorwayType typeDst)
        => typeSrc != typeDst || typeSrc == BOTH;

    public void InsertRoom(Room room, Offset off)
    {
        // Resize makes sure we have *at least* enough room.
        BUFFER.Resize(room.size);

        // UP and DOWN doorways are along the x axis.
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

        // LEFT and RIGHT doorways are along the y axis.
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
            byte val = BUFFER[i,j];

            if(val != 0)
            {
                opens.Add(off+localOff, val);
            }
        }

        BUFFER.Clear();
    }

    public bool Get(Offset off, Direction dir, out DoorwayType type)
    {
        type = BOTH;
        byte val;
        if(!opens.TryGetValue(off, out val))
            return false;
        return UnpackTypeAtDirection(val, dir, out type);
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

    // NOTE: can be made private. NeighborsWithinRange gets all possible connections.
    public bool ConnectionGoingDir(Offset src, Offset dst, Direction dir)
    {
        DoorwayType typeSrc, typeDst;
        if(!Get(src, dir, out typeSrc))
            return false;
        if(!Get(dst, DirMethods.opposite(dir), out typeDst))
            return false;

        return HasConnection(typeSrc, typeDst);
    }

    // NOTE: this control flow can easily be extracted into a different function.
    public List<(Offset, Direction)> NeighborsWithinRange(Offset start, Offset size)
    {
        Offset leftInsideBase = start;
        Offset downInsidebase = start;
        Offset rightOutsideBase = start + xof * size.x;
        Offset upOutsideBase = start + yof * size.y;
        List<(Offset, Direction)> values = new();

        bool validSrc; DoorwayType typeSrc;
        bool validDst; DoorwayType typeDst;
        for(int i = 0; i < size.y; i++)
        {
            Offset leftInside = leftInsideBase + yof * i;
            Offset leftOutside = leftInside - xof;
            Offset rightOutside = rightOutsideBase + yof * i;
            Offset rightInside = rightOutside - xof;

            if(ConnectionGoingDir(leftInside, leftOutside, LEFT))
                values.Add((leftOutside, LEFT));

            if(ConnectionGoingDir(rightInside, rightOutside, RIGHT))
                values.Add((rightOutside, RIGHT));
        }

        for(int i = 0; i < size.x; i++)
        {
            Offset downInside = downInsidebase + xof * i;
            Offset downOutside = downInside - yof;
            Offset upOutside = upOutsideBase + xof * i;
            Offset upInside = upOutside - yof;

            if(ConnectionGoingDir(downInside, downOutside, DOWN))
                values.Add((downOutside, DOWN));

            if(ConnectionGoingDir(upInside, upOutside, UP))
                values.Add((upOutside, UP));
        }
        return values;
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
