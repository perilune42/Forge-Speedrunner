using UnityEngine;
using Offset = UnityEngine.Vector2Int;
using System.Collections.Generic;

public interface IRoomChoiceStrategy
{
    // returns null if no desire to place a room
    public Room FindRoom(Direction dir, Offset off, in HashSet<Room> placedRooms);
}
