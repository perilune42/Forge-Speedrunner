using UnityEngine;
public struct Cell
{
    public Room room{get; private set;}
    public Vector2Int offset {get; private set;} // from start
    bool up   ;
    bool down ;
    bool left ;
    bool right;
    public Cell(Room room, Vector2Int offset)
    {
        this.room = room;
        this.offset = offset;
        this.up    = false;
        this.down  = false;
        this.left  = false;
        this.right = false;
    }
}
