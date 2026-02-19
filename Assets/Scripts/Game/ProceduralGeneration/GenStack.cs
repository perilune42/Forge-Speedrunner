using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Offset = UnityEngine.Vector2Int;
using static Direction;
public class GenStack
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
