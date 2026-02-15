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
        if(dir == LEFT)
            return r.doorwaysLeft;
        if(dir == RIGHT)
            return r.doorwaysRight;
        if(dir == UP)
            return r.doorwaysUp;
        // if(dir == DOWN)
        return r.doorwaysDown;

    }
    public static Direction opposite(in Direction dir)
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
