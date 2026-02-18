using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
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
        grid.InsertRoom(start, new(0,0));
    }
    public List<Cell> Generate(int pathLength)
    {
        for(int i = 0; i < pathLength; i++)
        {
            Debug.Log($"step {i}");
            Step();
        }
        return grid.uniqueCells; // problem: pointer to internal state
    }
    private void Step()
    {
        grid.LogEntries();
        stack.LogEntries();

        Direction dir; Offset off; Offset botleft;
        (dir, off) = stack.PopRandom();
        Room possibleRoom = findRoomWith(dir, in roomPrefabs);
        bool fitRoom = grid.CanFit(possibleRoom, off, dir, out botleft);
        if(fitRoom)
        {
            Debug.Log($"room fit at coord {botleft}");
            bool x = grid.InsertRoom(possibleRoom, botleft);
            stack.extractAll(possibleRoom, botleft);
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

    public Grid()
    {
        grid = new();
        uniqueCells = new();
    }

    private bool ObstructionWithin(Offset botLeft, Offset topRight, out Offset obstruction)
    {
        Offset current = botLeft;
        obstruction = current; // prevent compiler error
        for(int i = botLeft.x; i < topRight.x; i++)
            for(int j = botLeft.y; j < topRight.y; j++)
        {
            current.x = i;
            current.y = j;
            if(grid.ContainsKey(current))
            {
                obstruction = current;
                return true;
            }
        }
        return false;
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
        Debug.Log($"Checking offset {offset} and direction {dir}.");
        // 1. increment by direction
        Offset increment;
        if(dir == LEFT || dir == RIGHT)
            increment = new(0, -1);
        else
            increment = new(-1, 0);

        // 2. doorway list by direction
        List<Doorway> doors;
        if(dir == RIGHT)
            doors = room.doorwaysLeft;
        else if(dir == LEFT)
            doors = room.doorwaysRight;
        else if(dir == DOWN)
            doors = room.doorwaysUp;
        else // dir == UP
            doors = room.doorwaysDown;

        // 3. true bottom left at current offset
        Offset startingBotLeft = offset;
        if(dir == DOWN)
            startingBotLeft.y -= room.size.y;
        if(dir == LEFT)
            startingBotLeft.x -= room.size.x;
        Offset trueBotLeft = startingBotLeft;
        Debug.Log($"Initially believe trueBotLeft to be ({trueBotLeft.x},{trueBotLeft.y})");

        // 3.1. line up with possible door
        for(int i = 0; i < doors.Count; i++)
        {
            if(doors[i] == null)
            {
                Debug.Log("incr");
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
        Debug.Log($"Now checking between {trueBotLeft} and {topRightBound}");
        bool obsExists = ObstructionWithin(trueBotLeft, topRightBound, out obstruction);
        if(!obsExists)
        {
            // end here if no problem
            botleft = trueBotLeft;
            Debug.Log("no obstruction.");
            return true;
        }
        Debug.Log($"obsExists: {obsExists}. obstruction: {obstruction}");


        // 5. find a possible room right below obstruction
        Offset obstructionBound = obstruction;
        if(dir == LEFT || dir == RIGHT)
        {
            obstructionBound.x = trueBotLeft.x + room.size.x;
            trueBotLeft.y = obstructionBound.y - room.size.y;
        }
        else
        {
            obstructionBound.y = trueBotLeft.y + room.size.y;
            trueBotLeft.x = obstructionBound.x - room.size.x;
        }
        Debug.Log($"let's try {trueBotLeft} and {obstructionBound}.");

        // 6. find corresponding doorway (if none, end here)
        int connectingDoorwayInd = (dir == LEFT || dir == RIGHT)
            ? startingBotLeft.y - obstructionBound.y
            : startingBotLeft.x - obstructionBound.x;
        Debug.Log($"connectingDoorwayInd: {connectingDoorwayInd}.");
        if(connectingDoorwayInd < doors.Count)
        {
            Offset finalObstruction;
            Offset finalBotLeft = trueBotLeft;
            bool finalObsExist = ObstructionWithin(finalBotLeft, obstructionBound, out finalObstruction);
            if(!finalObsExist)
            {
                botleft = finalBotLeft;
                return true;
            }
            else {
                Debug.Log($"obstruction at {finalObstruction}");
            }
            botleft = new(0,0);
            return false;
        }
        botleft = new(0,0);
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
        bool success = grid.TryGetValue(offset, out opens);
        Debug.Log($"Opening ({offset}), direction {dir}");
        if(success)
        {
            if(dir == LEFT)
                opens.left = true;
            else if(dir == RIGHT)
                opens.right = true;
            else if(dir == UP)
                opens.up = true;
            else // dir == DOWN
                opens.down = true;
            grid[offset] = opens;
        }
        else
            Debug.Log($"Called OpenAt on a nonexistent grid cell! ({offset.x},{offset.y}).");

    }
    public bool InsertRoom(Room room, Offset offset)
    {
        Openings closedEverywhere = new Openings(false,false,false,false);

        // insert all elements
        for(int i = offset.x; i < offset.x + room.size.x; i++)
            for(int j = offset.y; j < offset.y + room.size.y; j++)
        {
            Debug.Log($"insert ({i},{j})");
            grid.Add(new(i,j), closedEverywhere);
        }

        // update left and right walls
        // for(int j = offset.y; j < offset.y + room.size.y; j++)
        for(int j = 0; j < room.size.y; j++)
        {
            int offY = offset.y + j;
            if(room.doorwaysLeft[j] != null)
            {
                Offset leftOff = new(offset.x, offY);
                OpenAt(leftOff, LEFT);
            }
            if(room.doorwaysRight[j] != null)
            {
                Offset rightOff = new(offset.x+room.size.x-1, offY);
                OpenAt(rightOff, RIGHT);
            }
        }

        // up and down walls
        // for(int i = offset.x; i < offset.x + room.size.x - 1; i++)
        for(int i = 0; i < room.size.x; i++)
        {
            int offX = i + offset.x;
            if(room.doorwaysUp[i] != null)
            {
                Offset upOff = new(offX, offset.y);
                OpenAt(upOff, UP);
            }
            if(room.doorwaysDown[i] != null)
            {
                Offset downOff = new(offX, offset.y+room.size.y-1);
                OpenAt(downOff, DOWN);
            }
        }

        // create a cell
        Cell cell = new Cell(room, offset);
        uniqueCells.Add(cell);
        return true;
    }
    public List<Passage> RealizePath()
    {
        HashSet<Offset> visited = new();
        Stack<Offset> toVisit = new();
        List<Passage> passages = new();
        toVisit.Push(new(0,0));
        while(toVisit.Count > 0)
        {
            break;
        }
        // TODO: ACTUALLY FINISH THIS 
        return null;
    }
    public void LogEntries()
    {
        StringBuilder sb = new("Grid contains keys:");
        foreach(Offset x in grid.Keys)
        {
            sb.Append($"({x.x},{x.y}) ");
        }
        Debug.Log(sb.ToString());
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

            newOffset = DirMethods.calcOffset(newOffset, facingDir);

            dirs.Add(facingDir);
            offsets.Add(newOffset);
        }
        return this;
    }
    public GenStack extractAll(Room r, Offset startingOffset)
    {
        // calculate starting states here (extractFrom cannot know)
        Offset startUp = startingOffset;
        Offset startRight = startingOffset;
        startRight.x += r.size.x-1;
        startUp.y += r.size.y-1;

        // extract from all directions
        this.extractFrom(r.doorwaysLeft, LEFT, startingOffset)
            .extractFrom(r.doorwaysDown, DOWN, startingOffset)
            .extractFrom(r.doorwaysUp, UP, startUp)
            .extractFrom(r.doorwaysRight, RIGHT, startRight);
        return this;
    }
    public void LogEntries()
    {
        StringBuilder sb = new("GenStack contains:\n");
        for(int i = 0; i < dirs.Count; i++)
        {
            sb.Append($"\t({offsets[i].x},{offsets[i].y}), {dirs[i]}\n");
        }
        Debug.Log(sb.ToString());
    }
}
