using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using static Direction;
public class BufferOptionNew : IAlgorithm
{
    List<Room> RoomPrefabs;
    public BufferOptionNew(Room[] prefabs)
    {
        RoomPrefabs = prefabs.ToList()
            // .FindAll(x => x != null && x.size.x == 1 && x.size.y == 1 && x.doorwaysRight.Any(y => y != null && y.IsExit()));
            .FindAll(x => x != null && x.size.x == 1 && x.size.y == 1 && -1 != hasAny(x.doorwaysRight, DoorwayType.EXIT));
    }

    public override bool Run(Grid grid, HashSet<Room> placedRooms)
    {
        int lastPosition = grid.uniqueCells.Count-1;
        Cell lastCell = grid.uniqueCells[lastPosition];

        List<(Offset, Direction)> possibleOpens = grid.OpenSpots(lastCell);
        possibleOpens.Shuffle();

        Offset firstOff; Direction firstDir;
        if(!GetFirstMatching(possibleOpens, x => x == RIGHT || x == UP, out firstOff, out firstDir))
        {
            Debug.Log($"[BufferOptionNew] Failed: no openings matching a direction. Cell: {lastCell.room}, {lastCell.offset}.");
            return false;
        }

        return TryAddFirst(grid, RoomPrefabs, firstOff, firstDir, placedRooms);
    }
}
