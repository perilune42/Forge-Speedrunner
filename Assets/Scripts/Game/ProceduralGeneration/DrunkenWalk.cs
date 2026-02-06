using UnityEngine;
using System.Collections.Generic;

public class DrunkenWalk : IPathGenerator
{

    private Random randState;

    public DrunkenWalk()
    {
        randState = new Random();
    }

    public DrunkenWalk(Random random)
    {
        randState = random;
    }

    public Path CreatePath(int pathLength)
    {
        HashSet<Vector2Int> existsSet = new();
        // 1. pick a random direction
        // 2. push Vector2Int roomCoords
        // 3. if it goes backwards to a place we've already gone, don't decrement the path length
        // 4. keep going until path length 0. that is where the end is
        int counter = pathLength;
        Vector2Int current = new(0,0);
        Path path;
        path.start = current;
        while (counter != 0)
        {
            current += randomDirection();
            if (!existsSet.Contains(current))
            {
                existsSet.Add(current);
                counter -= 1;
            }
        }
        path.end = current;
        path.rooms = existsSet;
        return path;
    }

    private Vector2Int randomDirection()
    {
        int dir = randState.Range(0, 4);
        if(dir == 0)
            return new(0,1);
        if(dir == 1)
            return new(1, 0);
        if(dir == 2)
            return new(0, -1);
        if(dir == 3)
            return new(-1, 0);
        Debug.Log($"ERR: dir is {dir}. It should only be 0,1,2,3!");
    }

}
