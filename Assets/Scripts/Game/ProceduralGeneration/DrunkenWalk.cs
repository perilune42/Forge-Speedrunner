using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using static Direction;

public class DrunkenWalk : IPathGenerator
{
    public List<Cell> Generate(int pathLength)
    {
        Room[] roomPrefabs = GameRegistry.Instance.RoomPrefabs;
        int count = pathLength;

        GenState state = new GenState();

        Dictionary<Vector2Int, Cell> grid = new();
        List<Cell> uniqueCells = new();

        while(pathLength > 0 && state.NotEmpty())
        {
            Doorway door; Direction dir; Vector2Int offset;

            // take random
            (door, dir, offset) = state.PopRandom();

            // check if offset is clear. throw away if not
            // TODO: this code does not do what it's supposed to. find the bottom left instead.
            Vector2Int entryOffset = DirMethods.calcOffset(offset, dir);
            if(grid.ContainsKey(entryOffset)) continue;

            // find room
            Direction roomEntranceDir = DirMethods.opposite(dir);
            Room newRoom = findRoomWith(roomEntranceDir, roomPrefabs);
            List<Doorway> relevantDoorways = DirMethods.matchingDir(roomEntranceDir, newRoom);

            // we have
            // 1. doors in intended direction
            // 2. intended direction
            // 3. offset that we will enter into
            // 4. size of room
            // can calculate the distance from the bottom left with 1,2,4

            // furthest left possible bottom left point
            // NOTE: in the future, checkOffset will be updated to slot the rooms properly
            Vector2Int checkOffset = entryOffset;
            if(dir == LEFT)
                checkOffset.y -= i;
            if(dir == DOWN)
                checkOffset.x -= i;
            if(dir == RIGHT)
            {
                checkOffset.y -= i;
                checkOffset.x -= room.size.x;
            }
            if(dir == UP)
            {
                checkOffset.x -= i;
                checkOffset.y -= room.size.y;
            }

            bool valid = true;
            for(int i = checkOffset.x;
                    shouldContinue && i < checkOffset.x + newRoom.size.x;
                    i++)
            for(int j = checkOffset.y;
                    shouldContinue && j < checkOffset.y + newRoom.size.y;
                    j++)
            {
                if(grid.ContainsKey(new Vector2Int(i,j)))
                    valid = false;
            }


            // if room cannot be placed at this offset, pick a new room
            // TODO
            if(!valid) continue; // just ignore if impossible for now

            // add appropriate occupied slots
            // TODO: this code is not correct. find the bottom left correctly.
            Vector2Int newBotLeft = checkOffset;
            Vector2Int newTopRight = newBotLeft + newRoom.size;

            // add appropriate cells
            Cell newCell = new Cell(newRoom, newOffset);
            uniqueCells.Add(newCell);
            // NOTE: this does not properly set `up,down,left,right`.
            // might be useful to fix later
            for(int i = newOffset.x; i < newTopRight.x; i++)
                for(int j = newOffset.y; j < newTopRight.y; j++)
            {
                grid.Add(new Vector2Int(i, j), newCell);
            }

            // take from room one doorway list at a time
            // NOTE: a previous check will always ignore invalid options.
            state = state.extractAll(newRoom, newOffset);
        }
        return path;
    }

    private Passage findConnectingDoors(in Direction enteringFrom, in Doorway door, in Room room)
    {

    }

    private Room findRoomWith(Direction entranceDir, in Room[] roomPrefabs)
    {
        // this kind of sucks...
        int numRooms = roomPrefabs.Length;
        for(int i = 0; i < 100; i++) // prevent infinite iteration
        {
            int ind = Random.Range(0, numRooms);
            Room current = roomPrefabs[ind];
            List<Doorway> currentDoors = DirMethods.matchingDir(in entranceDir, in current);
            bool hasDoorsThisWay = currentDoors.Any(x => x != null);
            if(hasDoorsThisWay)
                return current;
        }
        Debug.Log("Incredibly rare, could not find a door. TODO: find a sane solution.");
        return null;
    }
}

internal enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

internal static class DirMethods
{
    internal static Vector2Int calcOffset(Vector2Int startOffset, Direction dir)
    {
        Vector2Int endOffset = startOffset;
        if(dir == LEFT)
            endOffset.x--;
        if(dir == RIGHT)
            endOffset.x++;
        if(dir == UP)
            endOffset.y++;
        if(dir == DOWN)
            endOffset.y--;
        return endOffset;
    }
    internal static List<Doorway> matchingDir(in Direction dir, in Room r)
    {
        if(dir == LEFT)
            return r.doorwaysLeft;
        if(dir == RIGHT)
            return r.doorwaysRight;
        if(dir == UP)
            return r.doorwaysUp;
        // if(dir == DOWN)
        return r.doorwaysDown;

    }
    internal static Direction opposite(in Direction dir)
    {
        if(dir == LEFT)
            return RIGHT;
        if(dir == RIGHT)
            return LEFT;
        if(dir == UP)
            return DOWN;
        // if(dir == DOWN)
        return UP;
    }
}

internal class GenState
{
    // these lists are always the same size. struct of arrays
    List<Doorway> doors;
    List<Direction> dirs;
    List<Vector2Int> offsets;

    public GenState()
    {
        doors = new();
        dirs = new();
        offsets = new();
    }
    public bool NotEmpty()
    {
        return doors.Count > 0;
    }
    public (Doorway, Direction, Vector2Int) PopRandom()
    {
        // desired values
        int ind = Random.Range(0, doors.Count);
        Doorway door = doors[ind];
        Direction dir = dirs[ind];
        Vector2Int offset = offsets[ind];

        // remove selected element (swap-remove array style)
        dirs[ind] = dirs[dirs.Count-1];
        doors[ind] = doors[doors.Count-1];
        offsets[ind] = offsets[offsets.Count-1];
        offsets.RemoveAt(offsets.Count-1);
        dirs.RemoveAt(dirs.Count-1);
        doors.RemoveAt(doors.Count-1);

        return (door, dir, offset);
    }
    // NOTE: startingOffset should be the bottom left corner!
    public GenState extractFrom(List<Doorway> roomDoors, Direction facingDir, Vector2Int startingOffset)
    {
        for(int i = 0; i < roomDoors.Count; i++)
        {
            Doorway door = roomDoors[i];
            if(door == null) continue;

            Vector2Int newOffset = startingOffset;
            if(facingDir == LEFT || facingDir == RIGHT)
                newOffset.y += i;
            else // UP or DOWN
                newOffset.x += i;

            doors.Add(door);
            dirs.Add(facingDir);
            offsets.Add(startingOffset);
        }
        return this;
    }
    public GenState extractAll(Room r, Vector2Int startingOffset)
    {
        // calculate starting states here (extractFrom cannot know)
        Vector2Int startUp = startingOffset;
        Vector2Int startRight = startingOffset;
        startRight.x += r.size.x;
        startUp.y += r.size.y;

        // extract from all directions
        this.extractFrom(r.doorwaysLeft, LEFT, startingOffset)
            .extractFrom(r.doorwaysDown, DOWN, startingOffset)
            .extractFrom(r.doorwaysUp, UP, startUp)
            .extractFrom(r.doorwaysRight, RIGHT, startRight);
        return this;
    }
}
