using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Offset = UnityEngine.Vector2Int;
using static Direction;

public class RandomFromPoint : IPathGenerator
{
    private Grid grid;
    private GenStack stack;
    private Room[] roomPrefabs;
    private Room start;
    private Room end;
    public RandomFromPoint(Room[] roomPrefabs, Room start, Room end)
    {
        this.start = start;
        this.end = end;
        this.roomPrefabs = roomPrefabs;
        this.grid = new();
        this.stack = new();
        stack.extractAll(start, new(0,0));
        grid.InsertRoom(start, new(0,0));
    }
    public List<Cell> Generate(int pathLength)
    {
        for(int i = 0; i < pathLength; i++)
        {
            Debug.Log($"step {i}");
            Step();
        }
        return grid.uniqueCells; // problem: pointer to internal state
    }
    public List<Passage> RealizePath()
    {
        return grid.RealizePath();
    }
    private void Step()
    {
        grid.LogEntries();
        stack.LogEntries();

        Direction dir; Offset off; Offset botleft;
        (dir, off) = stack.PopRandom();
        Room possibleRoom = findRoomWith(DirMethods.opposite(dir), in roomPrefabs);
        bool fitRoom = grid.CanFit(possibleRoom, off, dir, out botleft);
        if(fitRoom)
        {
            Debug.Log($"room fit at coord {botleft}");
            bool x = grid.InsertRoom(possibleRoom, botleft);
            stack.extractAll(possibleRoom, botleft);
        }
    }
    private Room findRoomWith(Direction entranceDir, in Room[] roomPrefabs)
    {
        // this kind of sucks...
        int numRooms = roomPrefabs.Length;
        for(int i = 0; i < 100; i++) // prevent infinite iteration
        {
            int ind = Random.Range(0, numRooms);
            Room current = roomPrefabs[ind];
            List<Doorway> currentDoors = DirMethods.matchingDir(in entranceDir, in current);
            bool hasDoorsThisWay = currentDoors.Any(x => x != null);
            if(hasDoorsThisWay)
                return current;
        }
        Debug.Log("Incredibly rare, could not find a door. TODO: find a sane solution.");
        return null;
    }
}
