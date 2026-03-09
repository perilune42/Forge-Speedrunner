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
    // NOTE: i end up not really using Cell "properly" here.
    private DoorwayGrid doorwayGrid;
    private LowLevelGrid<Cell> cellsByGrid;
    public List<Cell> uniqueCells;

    public Grid()
    {
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
            TryStep(current, UP, pc, exists);
            TryStep(current, DOWN, pc, exists);
            TryStep(current, LEFT, pc, exists);
            TryStep(current, RIGHT, pc, exists);
        }
    }
    private bool TryStep(Offset currentOff, Direction dir, PathCreator pc, HashSet<(Offset, Direction)> exists)
    {
        Offset dirOff = DirMethods.calcOffset(currentOff, dir);

        // do not build duplicate connections
        if(exists.Contains((currentOff, dir)))
        {
            Debug.Log($"[WriteConnections] {currentOff} -> {dirOff}: duplicate");
            return false;
        }

        // get cells, and make sure they are not the same
        Cell currentCell; Cell dirCell;
        if(!cellsByGrid.TryGetValue(currentOff, out currentCell))
            return false;
        if(!cellsByGrid.TryGetValue(dirOff, out dirCell))
            return false;
        if(currentCell == dirCell)
            return false;

        // check if there's an opening here
        Direction oppositeDir = DirMethods.opposite(dir);
        DoorwayType currentType; DoorwayType dirType;
        bool currentValid = doorwayGrid.Get(currentOff, dir, out currentType);
        bool dirValid = doorwayGrid.Get(dirOff, oppositeDir, out dirType);
        if(!currentValid || !dirValid)
            return false;
        if(currentType == dirType && currentType != DoorwayType.BOTH)
            return false;

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

    public List<Cell> NeighborsOf(Cell c)
    {
        List<Offset> neighbors = doorwayGrid.NeighborsWithinRange(c.offset, c.room.size);
        HashSet<Cell> cells = new();

        foreach(Offset off in neighbors)
        {
            cells.Add(cellsByGrid.Get(off));
        }
        return cells.ToList();
    }

    public void LogEntries()
    {
        doorwayGrid.LogEntries();
    }
}
