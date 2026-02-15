using UnityEngine;
public struct Cell
{
    public Room room{get; private set;}
    public Vector2Int offset {get; private set;} // from start
    public Cell(Room room, Vector2Int offset)
    {
        this.room = room;
        this.offset = offset;
    }
}
