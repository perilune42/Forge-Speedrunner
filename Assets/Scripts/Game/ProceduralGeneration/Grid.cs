using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System; // exceptions
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

    /* Attempt at writing a cleaner CanFit
     */
    public bool CanFit2(Room room, Offset offset, Direction dir, out Offset botleft)
    {
        // initial constants important for calculations
        // functional match on enum is fun
        Offset mask = dir switch
        {
            LEFT or RIGHT => new(0,1),
            _ => new(1,0),
        };

        List<Doorway> roomDoorsAtDir = dir switch
        {
            LEFT => room.doorwaysRight,
            RIGHT => room.doorwaysLeft,
            UP => room.doorwaysDown,
            DOWN => room.doorwaysUp,
            _ => throw new Exception("this is not going to happen"),
        };


        // find bottom left where offset refers to the first element of the doorway list
        // botleft = offset + mask * room.size - mask;
        botleft = dir switch
        {
            LEFT or DOWN => offset + mask * room.size - mask,
            _ => offset,
        };

        // subtract from bottom left the difference from first element to first non-null element
        int firstNonNull = -1;
        // for(; firstNonNull < roomDoorsAtDir roomDoorsAtDir[firstNonNull] == null; firstNonNull++);
        for(int i = 0; i < roomDoorsAtDir.Count; i++)
        {
            if(roomDoorsAtDir[i] != null)
            {
                firstNonNull = i;
                break;
            }
        }

        // early return if no doorways are non null (should not happen)
        if(firstNonNull < 0)
        {
            Debug.Log($"first non null for {room} direction {dir}");
            return false;
        }
        botleft -= mask * firstNonNull;

        // check if there are no obstructions from offset to offset+room.size
        // return early if there are none
        Offset obstruction = botleft;
        bool valid = true;
        for(int i = 0; valid && i < room.size.x; i++)
            for(int j = 0; valid && j < room.size.y; j++)
        {
            obstruction = botleft + new Offset(i,j);
            valid = !grid.ContainsKey(obstruction);
        }
        if(valid) return true;
        Debug.Log($"obstruction at {obstruction}");

        // if there are obstructions, recalculate offset so that obstruction is outside.
        // obstruction's x or y coordinate depending on direction - room.size.xory
        // subtract this from botleft
        botleft -= mask * (obstruction - room.size) + mask;

        // check botleft again. return early if there are no obstructions
        valid = true;
        for(int i = 0; valid && i < room.size.x; i++)
            for(int j = 0; valid && j < room.size.y; j++)
        {
            obstruction = botleft + new Offset(i,j);
            valid = !grid.ContainsKey(obstruction);
        }
        Debug.Log($"obstruction again at {obstruction}. giving up.");

        return valid;
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
        uniqueCells.Add(roomCell); // also log the unique cell
        for(int i = offset.x; i < offset.x + room.size.x; i++)
            for(int j = offset.y; j < offset.y + room.size.y; j++)
        {
            Debug.Log($"insert ({i},{j})");
            grid.Add(new(i,j), closedEverywhere);
            cellsByGrid.Add(new(i,j), roomCell);
        }

        // left walls
        Offset xMask = new(1,0);
        Offset yMask = new(0,1);
        Offset leftStart = offset;
        Offset downStart = offset;
        Offset rightStart = offset + xMask * room.size.x - xMask;
        Offset upStart = offset + yMask * room.size.y - yMask;
        for(int i = 0; i < room.size.y; i++)
        {
            if(room.doorwaysLeft[i] != null)
                OpenAt(leftStart, LEFT);
            if(room.doorwaysRight[i] != null)
                OpenAt(rightStart, RIGHT);
            leftStart += yMask;
            rightStart += yMask;
        }
        for(int i = 0; i < room.size.x; i++)
        {
            if(room.doorwaysUp[i] != null)
                OpenAt(upStart, UP);
            if(room.doorwaysDown[i] != null)
                OpenAt(downStart, DOWN);
            upStart += xMask;
            downStart += xMask;
        }

        return true;
    }
    public void WriteConnections(PathCreator pc)
    {
        HashSet<(Offset, Direction)> exists = new();
        List<Offset> allOffsets = grid.Keys.ToList();
        while(allOffsets.Count > 0)
        {
            // dequeue
            Offset current = allOffsets[allOffsets.Count-1];
            allOffsets.RemoveAt(allOffsets.Count-1);

            // process in all directions
            TryStep(current, UP, pc, exists);
            TryStep(current, DOWN, pc, exists);
            TryStep(current, LEFT, pc, exists);
            TryStep(current, RIGHT, pc, exists);
        }
    }
    // only useful in above function WriteConnections
    private bool TryStep(Offset currentOff, Direction dir, PathCreator pc, HashSet<(Offset, Direction)> exists)
    {
        // do not build duplicate connections
        if(exists.Contains((currentOff, dir)))
            return false;

        // direction data
        Offset dirOff = DirMethods.calcOffset(currentOff, dir);

        Openings dirOpens;
        if(!grid.TryGetValue(dirOff, out dirOpens)) // early end if not found
            return false;

        Openings currOpens;
        if(!grid.TryGetValue(currentOff, out currOpens)) // early end if not found (this one should not happen)
            return false;

        Cell currentCell = cellsByGrid[currentOff];
        Cell dirCell = cellsByGrid[dirOff];
        if(currentCell.room == dirCell.room)
            return false;

        bool valid = dir switch
        {
            DOWN => (dirOpens.up && currOpens.down),
            UP => (dirOpens.down && currOpens.up),
            LEFT => (dirOpens.right && currOpens.left),
            RIGHT => (dirOpens.left && currOpens.right),
            _ => false,
        };

        if(!valid)
        {
            Debug.Log($"no connection between {currentCell.room} and {dirCell.room}. {currentOff} and {dirOff} do not connect.");
            return false;
        }

        Offset relativeCurrOff = currentOff - currentCell.offset;
        Offset relativeDirOff = dirOff - dirCell.offset;

        int currentIndex; int dirIndex;
        (currentIndex, dirIndex) = dir switch
        {
            LEFT or RIGHT => (relativeCurrOff.y, relativeDirOff.y),
            _ => (relativeCurrOff.x, relativeDirOff.x),
        };

        Debug.Log($"Connection between ({currentCell.room}) and ({dirCell.room}): ({currentOff}) -> ({dirOff})");
        Debug.Log($"For current ({currentCell.room}), dir ({dirCell.room}): \n currentIndex: {currentIndex}, dirIndex: {dirIndex}\n relativeCurrOff: {relativeCurrOff}, relativeDirOff: {relativeDirOff}\n currentOff: {currentOff}, dirOff: {dirOff}\n currentCell.offset: {currentCell.offset}, dirCell.offset: {dirCell.offset}\ndirection: {dir}");

        pc.AddConnection(currentCell, dirCell, currentIndex, dirIndex, dir);
        exists.Add((dirOff, DirMethods.opposite(dir)));
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
