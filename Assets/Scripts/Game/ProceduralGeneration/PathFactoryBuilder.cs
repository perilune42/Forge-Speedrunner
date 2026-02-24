using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Offset = UnityEngine.Vector2Int;
using static Direction;

public class PathFactoryBuilder
{
    private Room start;
    private Room finish;
    private List<(IChoiceStrategy, int, bool)> actions;
    private int min;
    private bool onePath;
    public PathFactoryBuilder()
    {
        // TODO: initialize stack with strategy pattern class to pick doorway
        actions = new();
        start = null;
        finish = null;
        onePath = false;
    }
    public PathFactoryBuilder WithMin(int min)
    {
        this.min = min;
        return this;
    }
    public PathFactoryBuilder OnePath()
    {
        onePath = true;
        return this;
    }
    public PathFactoryBuilder ManyPath()
    {
        onePath = false;
        return this;
    }
    public PathFactoryBuilder WithStartRoom(Room room)
    {
        start = room;
        return this;
    }
    public PathFactoryBuilder WithFinishRoom(Room room)
    {
        finish = room;
        return this;
    }
    public PathFactoryBuilder WithAlgorithm(IChoiceStrategy strategy, int pathLength)
    {
        actions.Add((strategy, pathLength, onePath));
        return this;
    }
    private int GenerateWith(IChoiceStrategy strategy, bool shouldCannibalize, GenStack stack, Grid grid, int pathLength, HashSet<Room> placedRooms)
    {
        // spots that were rejected by strategy
        GenStack rejectedStack = new();

        int numCreated = 0;

        Direction dir; Offset off; Offset botleft;
        for(int i = 0; stack.NotEmpty() && i < pathLength; i++)
        {
            // temporary logging of stack
            Debug.Log($"[GenerateWith] step {i+1}");
            grid.LogEntries();
            stack.LogEntries();
            (dir, off) = stack.PopWith(strategy, grid);
            Debug.Log($"[GenerateWith] dir: {dir}, off: {off}");
            Room possibleRoom = strategy.FindRoom(dir, off, grid, in placedRooms);
            if(possibleRoom == null)
            {
                Debug.Log($"[GenerateWith] rejecting {dir}, {off}, due to strategy.");
                rejectedStack.PutBack(dir, off);
                continue;
            }

            // only slot this room in when it can fit
            bool fitRoom = grid.CanFit(possibleRoom, off, dir, out botleft);
            if(fitRoom)
            {
                Debug.Log($"[GenerateWith] room fit at coord {botleft}");
                bool x = grid.InsertRoom(possibleRoom, botleft);
                if(shouldCannibalize)
                    rejectedStack.Cannibalize(stack);
                stack.extractAll(possibleRoom, botleft);
                placedRooms.Add(possibleRoom);
                numCreated += 1;
            }
            else
            {
                Debug.Log("[GenerateWith] room did not fit");
            }
        }

        // for future runs
        stack.Cannibalize(rejectedStack);
        return numCreated;
    }

    public PathCreator Finalize()
    {
        int numTries = 1;

        Grid maxGrid = new();

        for(int j = 0; maxGrid.uniqueCells.Count < min && j < 5; j++)
        {
            Debug.Log($"[Finalize] Try {numTries++}:");
            int numSteps = 1;
            GenStack stack = new();
            Grid grid = new();
            stack.extractAll(start, new Offset(0,0));
            grid.InsertRoom(start, new Offset(0,0));
            foreach((IChoiceStrategy strategy, int pathSize, bool actionOnePath) in actions)
            {
                Debug.Log($"[Finalize] Algorithm {numSteps++}");
                HashSet<Room> placedRooms = new();
                int genSize = 0;
                int cnt = 1;
                int i;
                do
                {
                    Debug.Log($"[Finalize] algorithm run {cnt++}");
                    i = GenerateWith(strategy, actionOnePath, stack, grid, pathSize-genSize, placedRooms);
                    genSize += i;
                } while (i > 0 && genSize < pathSize);
            }
            if(maxGrid.uniqueCells.Count < grid.uniqueCells.Count)
                maxGrid = grid;
        }

        return maxGrid.ProduceCreator();
    }
}
