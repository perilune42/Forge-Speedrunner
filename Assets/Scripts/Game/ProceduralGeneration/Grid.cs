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
    private DoorwayGrid doorwayGrid;
    private LowLevelGrid<Cell> cellsByGrid;
    // private Dictionary<Vector2Int, Cell> cellsByGrid;
    public List<Cell> uniqueCells;

    public Grid()
    {
        grid = new();
        uniqueCells = new();
        cellsByGrid = new();
        doorwayGrid = new();
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
        Debug.Log($"[CanFit] fitting room at {offset}, size {room.size}.");
        Offset mask = dir switch
        {
            LEFT or RIGHT => new(0,1),
            _ => new(1,0),
        };
        Offset otherMask = new(mask.y, mask.x);

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
            LEFT or DOWN => offset + otherMask * room.size - otherMask,
            _ => offset,
        };

        // subtract from bottom left the difference from first element to first non-null element
        int firstNonNull = -1;
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
            Debug.Log($"[CanFit] first non null for {room} direction {dir}");
            return false;
        }
        botleft -= mask * firstNonNull;

        // first obstruction check
        Offset obstruction;
        bool hasObstruction = cellsByGrid.FirstInRange(botleft, room.size, out obstruction);
        if(!hasObstruction)
            return true;

        // change botleft to be right below obstruction
        Offset change = mask * (obstruction - botleft + room.size) - mask;
        botleft -= change;

        // make sure there is still a door here
        int doorIndex = firstNonNull + dir switch
        {
            LEFT or RIGHT => change.y,
            _ => change.x,
        };
        if(doorIndex >= roomDoorsAtDir.Count || roomDoorsAtDir[doorIndex] == null)
        {
            Debug.Log("[CanFit] no available doorway at furthest possible point.");
            return false;
        }

        // final check
        hasObstruction = cellsByGrid.FirstInRange(botleft, room.size, out obstruction);
        if(hasObstruction)
        {
            Debug.Log($"Final obstruction at {obstruction}.");
            return false;
        }
        Debug.Log($"[CanFit] No further obstructions.");
        return true;
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
        Debug.Log($"[OpenAt] Opening ({offset}), direction {dir}");
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
            Debug.Log($"[OpenAt] Called on a nonexistent grid cell! {offset}.");

    }

    public PathCreator ProduceCreator()
    {
        PathCreator pc = new(uniqueCells);
        WriteConnections(pc);
        return pc;
    }

    public bool InsertRoom(Room room, Offset offset)
    {
        Cell roomCell = new Cell(room, offset);
        uniqueCells.Add(roomCell); // also log the unique cell
        doorwayGrid.InsertRoom(room, offset);
        cellsByGrid.InsertRange(roomCell, offset, room.size);
        return true;
    }
    public void WriteConnections(PathCreator pc)
    {
        HashSet<(Offset, Direction)> exists = new();
        List<Offset> allOffsets = uniqueCells.Select(x => x.offset).ToList();
        while(allOffsets.Count > 0)
        {
            // dequeue
            Offset current = allOffsets[allOffsets.Count-1];
            allOffsets.RemoveAt(allOffsets.Count-1);

            // process in all directions
            TryStep2(current, UP, pc, exists);
            TryStep2(current, DOWN, pc, exists);
            TryStep2(current, LEFT, pc, exists);
            TryStep2(current, RIGHT, pc, exists);
        }
    }
    private bool TryStep2(Offset currentOff, Direction dir, PathCreator pc, HashSet<(Offset, Direction)> exists)
    {
        Offset dirOff = DirMethods.calcOffset(currentOff, dir);

        // do not build duplicate connections
        if(exists.Contains((currentOff, dir)))
        {
            Debug.Log($"[WriteConnections] {currentOff} -> {dirOff}: duplicate");
            return false;
        }

        // get cells, and make sure they are not the same
        Cell currentCell = cellsByGrid.Get(currentOff);
        Cell dirCell = cellsByGrid.Get(dirOff);
        if(currentCell == dirCell) return false;

        // check if there's an opening here
        Direction oppositeDir = DirMethods.opposite(dir);
        bool valid = doorwayGrid.Get(currentOff, dir) && doorwayGrid.Get(dirOff, oppositeDir);
        if(!valid) return false;

        // indices into the relevant doorway list
        int currentIndex; int dirIndex;
        if(dir == LEFT || dir == RIGHT)
        {
            currentIndex = currentOff.y - currentCell.offset.y;
            dirIndex = dirOff.y - dirCell.offset.y;
        }
        else
        {
            currentIndex = currentOff.x - currentCell.offset.x;
            dirIndex = dirOff.x - dirCell.offset.x;
        }

        pc.AddConnection(currentCell, dirCell, currentIndex, dirIndex, dir);
        exists.Add((dirOff, DirMethods.opposite(dir)));
        return true;
    }
    // only useful in above function WriteConnections
    private bool TryStep(Offset currentOff, Direction dir, PathCreator pc, HashSet<(Offset, Direction)> exists)
    {
        // direction data
        Offset dirOff = DirMethods.calcOffset(currentOff, dir);

        // do not build duplicate connections
        if(exists.Contains((currentOff, dir)))
        {
            Debug.Log($"[WriteConnections] {currentOff} -> {dirOff}: duplicate");
            return false;
        }


        Openings dirOpens;
        if(!grid.TryGetValue(dirOff, out dirOpens)) // early end if not found
        {
            Debug.Log($"[WriteConnections] {currentOff} -> {dirOff}: {dirOff} is empty");
            return false;
        }

        Openings currOpens;
        if(!grid.TryGetValue(currentOff, out currOpens)) // early end if not found (this one should not happen)
            return false;

        Cell currentCell = cellsByGrid.Get(currentOff);
        Cell dirCell = cellsByGrid.Get(dirOff);
        if(currentCell == dirCell)
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
            Debug.Log($"[WriteConnections] {currentOff} -> {dirOff}: no possible opening");
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

        Debug.Log($"[WriteConnections] {currentOff} -> {dirOff}: Connection!");
        // Debug.Log($"For current ({currentCell.room}), dir ({dirCell.room}): \n currentIndex: {currentIndex}, dirIndex: {dirIndex}\n relativeCurrOff: {relativeCurrOff}, relativeDirOff: {relativeDirOff}\n currentOff: {currentOff}, dirOff: {dirOff}\n currentCell.offset: {currentCell.offset}, dirCell.offset: {dirCell.offset}\ndirection: {dir}");

        pc.AddConnection(currentCell, dirCell, currentIndex, dirIndex, dir);
        exists.Add((dirOff, DirMethods.opposite(dir)));
        return true;
    }

    public void LogEntries()
    {
        doorwayGrid.LogEntries();
        // StringBuilder sb = new("[Grid.LogEntries] Grid contains keys:");
        // foreach(Offset x in grid.Keys)
        // {
        //     sb.Append($"({x.x},{x.y}) ");
        // }
        // Debug.Log(sb.ToString());
    }
}
