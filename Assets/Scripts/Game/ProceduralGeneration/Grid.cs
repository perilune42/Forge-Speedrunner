using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Offset = UnityEngine.Vector2Int;
using static Direction;
public class Grid
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
    private Dictionary<Vector2Int, Cell> cellsByGrid;
    public List<Cell> uniqueCells;

    public Grid()
    {
        grid = new();
        uniqueCells = new();
        cellsByGrid = new();
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
            increment = new(0, 1);
        else
            increment = new(1, 0);

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

    public PathCreator ProduceCreator()
    {
        PathCreator pc = new(uniqueCells);
        WriteConnections(pc);
        return pc;
    }

    public bool InsertRoom(Room room, Offset offset)
    {
        Openings closedEverywhere = new Openings(false,false,false,false);

        // insert all elements
        Cell roomCell = new Cell(room, offset);
        for(int i = offset.x; i < offset.x + room.size.x; i++)
            for(int j = offset.y; j < offset.y + room.size.y; j++)
        {
            Debug.Log($"insert ({i},{j})");
            grid.Add(new(i,j), closedEverywhere);
            cellsByGrid.Add(new(i,j), roomCell);
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
    public void WriteConnections(PathCreator pc)
    {
        HashSet<Offset> visited = new();
        List<Offset> allOffsets = grid.Keys.ToList();
        while(allOffsets.Count > 0)
        {
            // dequeue
            Offset current = allOffsets[allOffsets.Count-1];
            allOffsets.RemoveAt(allOffsets.Count-1);

            // process in all directions
            TryStep(current, UP, allOffsets, pc);
            TryStep(current, DOWN, allOffsets, pc);
            TryStep(current, LEFT, allOffsets, pc);
            TryStep(current, RIGHT, allOffsets, pc);
        }
    }
    // only useful in above function WriteConnections
    private void TryStep(Offset current, Direction dir, List<Offset> allOffsets, PathCreator pc)
    {
        if(TryAddConnection(current, dir, pc, out Offset neighbor))
        {
            Debug.Log($"Connection! {current} -> {neighbor}");
            allOffsets.Remove(neighbor);
        }
    }

    private bool TryAddConnection(Offset currentOff,
            Direction dir,
            PathCreator pc,
            out Offset dirOff)
    {
        // direction data
        dirOff = DirMethods.calcOffset(currentOff, dir);

        Openings dirOpens;
        bool success = grid.TryGetValue(dirOff, out dirOpens);
        if(!success) // early end if not found
            return false;

        Cell currentCell = cellsByGrid[currentOff];
        Cell dirCell = cellsByGrid[dirOff];

        if(currentCell.room == dirCell.room)
            return false;

        Offset relativeCurrOff = currentOff - currentCell.offset;
        Offset relativeDirOff = dirOff - dirCell.offset;

        int currentIndex; int dirIndex;
        if(dir == LEFT || dir == RIGHT)
        {
            currentIndex = relativeCurrOff.y;
            dirIndex = relativeDirOff.y;
        }
        else // UP OR DOWN
        {
            currentIndex = relativeCurrOff.x;
            dirIndex = relativeDirOff.x;
        }
        Debug.Log($"currentIndex for ({currentCell.room}): {currentIndex}, dirIndex for ({dirCell.room}): {dirIndex}");

        pc.AddConnection(currentCell, dirCell, currentIndex, dirIndex, dir);
        return true;
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
