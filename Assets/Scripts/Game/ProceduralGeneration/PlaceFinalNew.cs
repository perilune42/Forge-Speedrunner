using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using static Direction;
public class PlaceFinalNew : IAlgorithm
{
    Room FinalRoom;
    public PlaceFinalNew(Room final)
    {
        FinalRoom = final;
    }

    public override bool Run(Grid grid, HashSet<Room> placedRooms)
    {
        int lastPosition = grid.uniqueCells.Count-1;
        Cell lastCell = grid.uniqueCells[lastPosition];

        List<(Offset, Direction)> possibleOpens = grid.OpenSpots(lastCell);
        possibleOpens.Shuffle();

        if(possibleOpens.Count == 0)
        {
            Debug.Log("[PlaceFinalNew] zero?");
            return false;
        }

        Offset firstOff = default; Direction firstDir = default;
        bool success = false;
        for(int i = 0; i < possibleOpens.Count; i++)
        {
            (firstOff, firstDir) = possibleOpens[i];
            if(firstDir == RIGHT)
            {
                success = true;
                break;
            }
        }
        if(!success)
            return false;

        if(placedRooms.Contains(FinalRoom))
            return false;

        return TryAdd(grid, FinalRoom, firstOff, firstDir, placedRooms);
    }
}
