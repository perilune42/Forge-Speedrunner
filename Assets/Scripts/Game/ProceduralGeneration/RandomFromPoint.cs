using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Offset = UnityEngine.Vector2Int;
using static Direction;

public class RandomFromPoint : IPathGenerator
{
    private Grid grid;
    private GenStack stack;
    private Room[] roomPrefabs;
    private Room start;
    private Room end;
    public RandomFromPoint(Room[] roomPrefabs, Room start, Room end)
    {
        this.start = start;
        this.end = end;
        this.roomPrefabs = roomPrefabs;
        this.grid = new();
        this.stack = new();
        stack.extractAll(start, new(0,0));
    }
    public List<Cell> Generate(int pathLength)
    {
        for(int i = 0; i < pathLength; i++)
        {
            Step();
        }
        return grid.uniqueCells; // problem: pointer to internal state
    }
    private void Step()
    {
        Direction dir; Offset off; Offset botleft;
        (dir, off) = stack.PopRandom();
        Room possibleRoom = findRoomWith(dir, in roomPrefabs);
        bool fitRoom = CanFit(possibleRoom, off, dir, out botleft);
        if(fitRoom)
        {
            InsertRoom(possibleRoom, botleft);
            stack.extractAll(possibleRoom);
        }
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

internal class Grid
{
    internal struct Openings
    {
        public bool up;
        public bool down;
        public bool left;
        public bool right;
        public Openings(bool up, bool down, bool left, bool right)
        {
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
        }
    }
    // NOTE: i end up not really using Cell "properly" here.
    private Dictionary<Vector2Int, Openings> grid;
    public List<Cell> uniqueCells;

    private bool ObstructionWithin(Offset botLeft, Offset topRight, out Offset obstruction)
    {
        Offset current = botLeft;
        obstruction = current; // prevent compiler error
        for(int i = botLeft.x; i < topRight.x; i++)
            for(int j = botLeft.y; j < topRight.y; j++)
        {
            if(grid.ContainsKey(current))
            {
                obstruction = current;
                return false;
            }
        }
        return true;
    }

    /* Given a ROOM, an entry point at OFFSET, and an entry direction DIR, fit the room. 
     *   Return TRUE if successful.
     * offset = a point of a particular doorway in `Room room`
     * dir = the direction you would enter the doorway from
     * room = the room we are trying to fit (useful: room.doors*, room.size)
     * botleft = the bottom left point that the room can go to
     */
    public bool CanFit(Room room, Offset offset, Direction dir, out Offset botleft)
    {
        // 1. increment by direction
        Offset increment;
        if(dir == LEFT || dir == RIGHT)
            increment = new(0, -1);
        else
            increment = new(-1, 0);

        // 2. doorway list by direction
        List<Doorway> doors;
        if(dir == LEFT)
            doors = room.doorwaysLeft;
        if(dir == RIGHT)
            doors = room.doorwaysRight;
        if(dir == UP)
            doors = room.doorwaysUp;
        if(dir == DOWN)
            doors = room.doorwaysDown;

        // 3. true bottom left at current offset
        Offset trueBotLeft = offset;
        if(dir == UP)
            trueBotLeft.y -= room.size.y+1;
        if(dir == RIGHT)
            trueBotLeft.x -= room.size.x+1;

        // 3.1. line up with possible door
        for(int i = 0; i < doors.Count; doorway++)
        {
            if(doors[i] == null)
            {
                trueBotLeft -= increment;
            }
            else
            {
                break;
            }
        }
        Offset topRightBound = trueBotLeft + room.size;

        // 4. check if there's any problem at the default placement
        Offset obstruction;
        bool obsExists = ObstructionWithin(trueBotLeft, topRightBound, out obstruction);
        if(!obsExists)
        {
            // end here if no problem
            botleft = trueBotLeft;
            return true;
        }


        // 5. find a possible room right below obstruction
        Offset obstructionBound = obstruction;
        if(dir == LEFT || dir == RIGHT)
        {
            obstructionBound.x = botLeft.x + room.size.x;
            trueBotLeft.y = obstructionBound.y - room.size.y;
        }
        else
        {
            obstructionBound.y = botLeft.y + room.size.y;
            trueBotLeft.x = obstructionBound.x - room.size.x;
        }

        // 6. find corresponding doorway (if none, end here)
        int connectingDoorwayInd = (dir == LEFT || dir == RIGHT)
            ? startingBotLeft.y - obstructionBound.y
            : startingBotLeft.x - obstructionBound.x;
        if(connectingDoorwayInd < doors.Count)
        {
            Offset finalObstruction;
            Offset finalBotLeft = startingBotLeft - increment * connectingDoorwayInd;
            bool finalObsExist = ObstructionWithin(finalBotLeft, obstructionBound, finalObstruction);
            if(!finalObsExist)
            {
                botleft = finalBotLeft;
                return true;
            }
            return false;
        }
        return false;
    }

    /* Place the ROOM at the OFFSET inside the internal grid. return TRUE if success.
     * This will update internal state.
     * room = room to insert at point.
     * offset = bottom left of room.
     */
    private void OpenAt(Offset offset, Direction dir)
    {
        Openings opens;
        bool success = grid.TryGetValue(offset, opens);
        if(success)
        {
            if(dir == LEFT)
                opens.left = true;
            else if(dir == RIGHT)
                opens.right = true;
            else if(dir == UP)
                opens.up = true;
            else if(dir == DOWN)
                opens.down = true;
            grid[offset] = opens;
        }
        else
            Debug.Log("Called OpenAt on a nonexistent grid cell!");

    }
    public bool InsertRoom(Room room, Offset offset)
    {
        Openings closedEverywhere = new Openings(false,false,false,false);

        // insert all elements
        for(int i = offset.x; i < offset.x + room.size.x; i++)
            for(int j = offset.y; j < offset.y + room.size.y; j++)
        {
            grid.Add(new(i,j), closedEverywhere);
        }

        // update left and right walls
        for(int j = offset.y; j < offset.y + room.size.y; j++)
        {
            if(room.doorwaysLeft[j] != null)
            {
                Offset leftOff = new(offset.x, j);
                OpenAt(leftOff, LEFT);
            }
            if(room.doorwaysRight[j] != null)
            {
                Offset rightOff = new(offset.x+roomSize.x-1, j);
                OpenAt(rightOff, RIGHT);
            }
        }

        // up and down walls
        for(int i = offset.x; i < offset.x + room.size.x - 1; i++)
        {
            if(room.doorwaysUp[i] != null)
            {
                Offset upOff = new(i, offset.y);
                OpenAt(upOff, UP);
            }
            if(room.doorwaysDown[j] != null)
            {
                Offset downOff = new(i, offset.y+room.size.y-1);
                OpenAt(downOff, DOWN);
            }
        }
        return true;
    }
    public List<Passage> RealizePath()
    {

    }
}

// like DrunkenWalk.GenState but different
internal class GenStack
{
    // these lists are always the same size. struct of arrays
    List<Direction> dirs;
    List<Offset> offsets;

    public GenStack()
    {
        dirs = new();
        offsets = new();
    }
    public bool NotEmpty()
    {
        return dirs.Count > 0;
    }
    public (Direction, Offset) PopRandom()
    {
        // desired values
        int ind = Random.Range(0, dirs.Count);
        Direction dir = dirs[ind];
        Offset offset = offsets[ind];

        // remove selected element (swap-remove array style)
        dirs[ind] = dirs[dirs.Count-1];
        offsets[ind] = offsets[offsets.Count-1];
        offsets.RemoveAt(offsets.Count-1);
        dirs.RemoveAt(dirs.Count-1);

        return (dir, offset);
    }
    public void PutBack(Direction dir, Offset offset)
    {
        dirs.Add(dir);
        offsets.Add(offset);
    }
    // NOTE: startingOffset should be the bottom left corner!
    public GenStack extractFrom(List<Doorway> roomDoors, Direction facingDir, Offset startingOffset)
    {
        for(int i = 0; i < roomDoors.Count; i++)
        {
            Doorway door = roomDoors[i];
            if(door == null) continue;

            Offset newOffset = startingOffset;
            if(facingDir == LEFT || facingDir == RIGHT)
                newOffset.y += i;
            else // UP or DOWN
                newOffset.x += i;

            newOffset += DirMethods.calcOffset(facingDir);

            dirs.Add(facingDir);
            offsets.Add(startingOffset);
        }
        return this;
    }
    public GenStack extractAll(Room r, Offset startingOffset)
    {
        // calculate starting states here (extractFrom cannot know)
        Offset startUp = startingOffset;
        Offset startRight = startingOffset;
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
