using UnityEngine;
using System.Collections.Generic;
using static Direction;
public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public static class DirMethods
{
    public static Vector2Int calcOffset(Vector2Int startOffset, Direction dir)
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
    public static List<Doorway> matchingDir(in Direction dir, in Room r)
    {
        return dir switch
        {
            LEFT => r.doorwaysLeft,
            RIGHT => r.doorwaysRight,
            UP => r.doorwaysUp,
            _ => r.doorwaysDown,
        };

    }
    public static Direction opposite(in Direction dir)
    {
        return dir switch
        {
            LEFT => RIGHT,
            RIGHT => LEFT,
            UP => DOWN,
            _ => UP,
        };
    }
}
