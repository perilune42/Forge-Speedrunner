using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
using System;
using static Direction;
using static DoorwayType;

public class MainPath : IAlgorithm
{
    List<Room> LeftEntrances;
    List<Room> DownEntrances;
    public MainPath(Room[] roomPrefabs)
    {
        LeftEntrances = new();
        DownEntrances = new();
        for(int i = 0; i < roomPrefabs.Length; i++)
        {
            Room r = roomPrefabs[i];
            bool hasUp = -1 != hasAny(DirMethods.matchingDir(UP, r), EXIT);
            bool hasDown = -1 != hasAny(DirMethods.matchingDir(DOWN, r), EXIT);
            bool hasLeft = -1 != hasAny(DirMethods.matchingDir(LEFT, r), ENTRANCE);
            bool hasRight = -1 != hasAny(DirMethods.matchingDir(RIGHT, r), ENTRANCE);
            if(!hasUp && !hasRight) continue;
            if(hasLeft)
                LeftEntrances.Add(r);
            if(hasDown)
                DownEntrances.Add(r);
        }
        LeftEntrances.Shuffle();
        DownEntrances.Shuffle();
        Debug.Log($"[MainPath.Constructor] have {LeftEntrances.Count} rooms with left openings and {DownEntrances.Count} rooms with down openings.");
    }

    public override bool Run(Grid grid, HashSet<Room> placedRooms)
    {
        int lastPosition = grid.uniqueCells.Count-1;
        Cell lastCell = grid.uniqueCells[lastPosition];

        List<(Offset, Direction)> possibleOpens = grid.OpenSpots(lastCell);
        possibleOpens.Shuffle();

        Offset firstOff = default; Direction firstDir = default;
        bool success = false;
        for(int i = 0; !success && i < possibleOpens.Count; i++)
        {
            (firstOff, firstDir) = possibleOpens[i];
            if(firstDir == RIGHT || firstDir == UP)
                success = true;
        }
        if(!success)
            return false;

        List<Room> doorways = firstDir == RIGHT ? LeftEntrances : DownEntrances;

        return TryAddFirst(grid, doorways, firstOff, firstDir, placedRooms);
    }
}
