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
    }
    public List<Cell> Generate(int pathLength)
    {
        return null;
    }
    private void Update()
    {
        Direction dir; Offset off;
        (dir, off) = stack.PopRandom();
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
    private Dictionary<Vector2Int, Openings> grid;

    /* Given a ROOM, an entry point at OFFSET, and an entry direction DIR, fit the room. 
     *   Return TRUE if successful.
     * offset = a point of a particular doorway in `Room room`
     * dir = the direction you would enter the doorway from
     * room = the room we are trying to fit (useful: room.doors*, room.size)
     * botleft = the bottom left point that the room can go to
     */
    public bool CanFit(Room room, Offset offset, Direction dir, out Offset botleft)
    {

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
            if(room.doorwaysRight[j] != null)
            {
                Offset downOff = new(i, offset.y+room.size.y-1);
                OpenAt(downOff, UP);
            }
        }
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
