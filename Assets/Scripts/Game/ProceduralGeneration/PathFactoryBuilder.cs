using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Offset = UnityEngine.Vector2Int;
using static Direction;

public class PathFactoryBuilder
{
    private GenStack stack;
    private Grid grid;
    private Room start;
    private Room finish;
    public PathFactoryBuilder()
    {
        stack = new();
        grid = new();
        start = null;
        finish = null;
    }
    public PathFactoryBuilder WithStartRoom(Room room)
    {
        start = room;
        stack.extractAll(room, new Offset(0,0));
        grid.InsertRoom(room, new Offset(0,0));
        return this;
    }
    public PathFactoryBuilder WithFinishRoom(Room room)
    {
        finish = room;
        return this;
    }
    public PathFactoryBuilder GenerateWith(IRoomChoiceStrategy strategy, int pathLength)
    {
        // spots that were rejected by strategy
        GenStack rejectedStack = new();

        Direction dir; Offset off; Offset botleft;
        for(int i = 0; i < pathLength; i++)
        {
            // temporary logging of stack
            Debug.Log($"[GenerateWith] step {i+1}");
            grid.LogEntries();
            stack.LogEntries();
            (dir, off) = stack.PopRandom();
            Debug.Log($"[GenerateWith] dir: {dir}, off: {off}");
            Room possibleRoom = strategy.FindRoom(dir, off);
            if(possibleRoom == null)
            {
                rejectedStack.PutBack(dir, off);
                continue;
            }

            // only slot this room in when it can fit
            bool fitRoom = grid.CanFit2(possibleRoom, off, dir, out botleft);
            if(fitRoom)
            {
                Debug.Log($"[GenerateWith] room fit at coord {botleft}");
                bool x = grid.InsertRoom(possibleRoom, botleft);
                stack.extractAll(possibleRoom, botleft);
            }
            else
            {
                Debug.Log("[GenerateWith] room did not fit");
            }
        }

        // for future runs
        stack.Cannibalize(rejectedStack);
        return this;
    }

    public PathCreator Finalize()
    {
        return grid.ProduceCreator();
    }
}
