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

    static Buffer BUFFER = new();

    // These are constants that are neat to have.
    private static Offset xof = new Offset(1,0);
    private static Offset yof = new Offset(0,1);

    public IEnumerable<Offset> AllOffsets => opens.Keys;

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
        Debug.Log($"Entering InsertRoom");
        // TODO: allocate a buffer once, and never reallocate
        BUFFER.Resize(room.size);

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

        StringBuilder sb = new($"For room {room}, adding the following to DoorwayGrid:\n");
        for(int i = 0; i < room.size.x; i++)
            for(int j = 0; j < room.size.y; j++)
        {
            Offset localOff = new(i,j);
            byte val = BUFFER[i,j];

            if(opens.ContainsKey(off+localOff))
                Debug.LogError("[DoorwayGrid] ERR: Trying to insert where something already exists!");
            if(val != 0)
            {
                sb.Append($"\t{off+localOff}, {Convert.ToString(val, 2)}");
                opens.Add(off+localOff, val);
            }
        }
        Debug.Log(sb.ToString());
        // TestInsertion(new Cell(room, off));
        BUFFER.Clear();
        // if(!TestInsertion(new Cell(room, off)))
        //     Debug.LogError($"ERR: Insertion of {room} at {off} failed!");
    }

    public bool TestInsertion(Cell c)
    {
        Offset leftStart = c.offset;
        Offset downStart = leftStart;
        Offset rightStart = leftStart + ((c.room.size.x - 1) * xof);
        Offset upStart = downStart + ((c.room.size.y - 1) * yof);

        bool opensHas; bool roomHas; Offset current;
        for(int i = 0; i < c.room.size.x; i++)
        {
            current = downStart + i * xof;
            opensHas = Get(current, DOWN, out _);
            roomHas = c.room.doorwaysDown[i] != null;
            if(opensHas != roomHas)
            {
                Debug.LogError($"At {current}, direction DOWN: stored opening? {opensHas}. room opening? {roomHas}.");
                // return false;
            }

            current = upStart + i * xof;
            opensHas = Get(current, UP, out _);
            roomHas = c.room.doorwaysUp[i] != null;
            if(opensHas != roomHas)
            {
                Debug.LogError($"At {current}, direction UP: stored opening? {opensHas}. room opening? {roomHas}.");
                // return false;
            }
        }
        for(int i = 0; i < c.room.size.y; i++)
        {
            current = leftStart + i * yof;
            opensHas = Get(current, LEFT, out _);
            roomHas = c.room.doorwaysLeft[i] != null;
            if(opensHas != roomHas)
            {
                Debug.LogError($"At {current}, direction LEFT: stored opening? {opensHas}. room opening? {roomHas}.");
                // return false;
            }

            current = rightStart + i * yof;
            opensHas = Get(current, RIGHT, out _);
            roomHas = c.room.doorwaysRight[i] != null;
            if(opensHas != roomHas)
            {
                Debug.LogError($"At {current}, direction RIGHT: stored opening? {opensHas}. room opening? {roomHas}.");
                // return false;
            }
        }
        return true;
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

    /* Predicate condition that decides whether there is a connection between two locations
    */
    private bool HasConnection(DoorwayType typeSrc, DoorwayType typeDst)
        => typeSrc != typeDst || typeSrc == BOTH;

    public bool ConnectionGoingDir(Offset src, Direction dir)
        => ConnectionGoingDir(src, DirMethods.calcOffset(src, dir), dir);

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

            validSrc = Get(leftInside, LEFT, out typeSrc);
            validDst = Get(leftOutside, RIGHT, out typeDst);
            if(validSrc && validDst && HasConnection(typeSrc, typeDst))
                values.Add((leftOutside, LEFT));

            validSrc = Get(rightInside, RIGHT, out typeSrc);
            validDst = Get(rightOutside, LEFT, out typeDst);
            if(validSrc && validDst && HasConnection(typeSrc, typeDst))
                values.Add((rightOutside, RIGHT));
        }

        for(int i = 0; i < size.x; i++)
        {
            Offset downInside = downInsidebase + xof * i;
            Offset downOutside = downInside - yof;
            Offset upOutside = upOutsideBase + xof * i;
            Offset upInside = upOutside - yof;

            validSrc = Get(downInside, DOWN, out typeSrc);
            validDst = Get(downOutside, UP, out typeDst);
            if(validSrc && validDst && HasConnection(typeSrc, typeDst))
                values.Add((downOutside, DOWN));

            validSrc = Get(upInside, UP, out typeSrc);
            validDst = Get(upOutside, DOWN, out typeDst);
            if(validSrc && validDst && HasConnection(typeSrc, typeDst))
                values.Add((upOutside, UP));
        }
        return values;
    }

    public void PrintAllDoorways(LowLevelGrid<Cell> cellsByGrid)
    {
        StringBuilder sb = new("All doorways in DoorwayGrid: \n");
        DoorwayType typeSrc; Cell cellSrc;
        DoorwayType typeDst; Cell cellDst;
        foreach(Offset o in opens.Keys)
        {
            if(Get(o, UP, out typeSrc) 
                    && Get(o+yof, DOWN, out typeDst) 
                    && cellsByGrid.TryGetValue(o, out cellSrc)
                    && cellsByGrid.TryGetValue(o+yof, out cellDst)
                    && cellSrc != cellDst
                    && HasConnection(typeSrc, typeDst))
                sb.Append($"\t{o} -> {o + yof}\n");
            if(Get(o, DOWN, out typeSrc)
                    && Get(o-yof, UP, out typeDst)
                    && cellsByGrid.TryGetValue(o, out cellSrc)
                    && cellsByGrid.TryGetValue(o-yof, out cellDst)
                    && cellSrc != cellDst
                    && HasConnection(typeSrc, typeDst))
                sb.Append($"\t{o} -> {o - yof}\n");
            if(Get(o, LEFT, out typeSrc)
                    && Get(o-xof, RIGHT, out typeDst)
                    && cellsByGrid.TryGetValue(o, out cellSrc)
                    && cellsByGrid.TryGetValue(o-xof, out cellDst)
                    && cellSrc != cellDst
                    && HasConnection(typeSrc, typeDst))
                sb.Append($"\t{o} -> {o - xof}\n");
            if(Get(o, RIGHT, out typeSrc)
                    && Get(o+xof, LEFT, out typeDst)
                    && cellsByGrid.TryGetValue(o, out cellSrc)
                    && cellsByGrid.TryGetValue(o+xof, out cellDst)
                    && cellSrc != cellDst
                    && HasConnection(typeSrc, typeDst))
                sb.Append($"\t{o} -> {o + xof}\n");
        }
        Debug.Log(sb.ToString());
    }

    // monstrosity that tests every possible combination
    public void Test()
    {
        List<Direction> allDirs = new List<Direction>{UP, DOWN, LEFT, RIGHT};
        List<DoorwayType> allTypes = new List<DoorwayType>{ENTRANCE, EXIT, BOTH};
        List<(DoorwayType, Direction)> allCombs = new();

        StringBuilder sb = new();
        int numErrors = 0;

        // generate every possible combination (81 iterations)
        foreach(DoorwayType upType in allTypes)
        foreach(DoorwayType downType in allTypes)
        foreach(DoorwayType leftType in allTypes)
        foreach(DoorwayType rightType in allTypes)
        for(int iUp = 0; iUp < 2; iUp++)
        for(int iDown = 0; iDown < 2; iDown++)
        for(int iLeft = 0; iLeft < 2; iLeft++)
        for(int iRight = 0; iRight < 2; iRight++)
        {
            bool hasUp = iUp == 1;
            bool hasDown = iDown == 1;
            bool hasLeft = iLeft == 1;
            bool hasRight = iRight == 1;
            byte packed = 0;
            sb.Append($"For case UP:{upType}, DOWN:{downType}, LEFT:{leftType}, RIGHT:{rightType} --\n");
            packed |= marshall(UP, upType);
            packed |= marshall(DOWN, downType);
            packed |= marshall(LEFT, leftType);
            packed |= marshall(RIGHT, rightType);

            DoorwayType typeUnmarshall;
            if(unmarshallType(packed, UP, out typeUnmarshall) == hasUp)
            {
                if(typeUnmarshall != upType)
                    sb.Append($"\tERR {numErrors++}: expected {upType} got {typeUnmarshall}.");
            }
            else
                sb.Append($"\tERR {numErrors++}: expected UP exists == {hasUp}. did not get this.");
            if(unmarshallType(packed, DOWN, out typeUnmarshall))
            {
                if(typeUnmarshall != downType)
                    sb.Append($"\tERR {numErrors++}: expected {downType} got {typeUnmarshall}.");
            }
            else
                sb.Append($"\tERR {numErrors++}: expected DOWN exists == {hasDown}. did not get this.");
            if(unmarshallType(packed, LEFT, out typeUnmarshall))
            {
                if(typeUnmarshall != leftType)
                    sb.Append($"\tERR {numErrors++}: expected {leftType} got {typeUnmarshall}.");
            }
            else
                sb.Append($"\tERR {numErrors++}: expected LEFT exists == {hasLeft}. did not get this.");
            if(unmarshallType(packed, RIGHT, out typeUnmarshall))
            {
                if(typeUnmarshall != rightType)
                    sb.Append($"\tERR {numErrors++}: expected {rightType} got {typeUnmarshall}.");
            }
            else
                sb.Append($"\tERR {numErrors++}: expected RIGHT exists == {hasRight}. did not get this.");
        }
    }
}
