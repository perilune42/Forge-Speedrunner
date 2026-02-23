using UnityEngine;
using Offset = UnityEngine.Vector2Int;
using System.Collections.Generic;

public interface IChoiceStrategy
{
    // returns null if no desire to place a room
    public Room FindRoom(Direction dir, Offset off, in HashSet<Room> placedRooms);
    public int SelectIndex(in List<Direction> dirs, in List<Offset> offsets);
}
