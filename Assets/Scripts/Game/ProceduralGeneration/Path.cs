using UnityEngine;
using System.Collections.Generic;

public struct Path
{
    // NOTE: the units here are not grid units.
    //       instead it's in # of rooms below/above
    //       rooms can vary in size!
    public HashSet<Vector2Int> coords;
    public Vector2Int end;
    public Vector2Int start;
}
