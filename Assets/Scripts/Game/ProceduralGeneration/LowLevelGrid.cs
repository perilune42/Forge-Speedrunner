using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Offset = UnityEngine.Vector2Int;
using static Direction;


// does not support deletion!
public class LowLevelGrid<T>
{
    /* To change this data structure for everyone,
     * 1. change _Grid
     * 2. change InsertRange
     * 3. change TryGet
     * The rest of the code should depend on these "primitives".
     */
    Dictionary<Offset, T> _Grid = new();

    /* Insert OBJ to every position from START to START+SIZE.
     * Not including the end.
     */
    public void InsertRange(T obj, Offset start, Offset size)
    {
        for(int i = 0; i < size.x; i++)
            for(int j = 0; j < size.y; j++)
        {
            _Grid.Add(start + new Offset(i,j), obj);
        }
    }

    /* Try to get a value from the grid.
     */
    public bool TryGetValue(Offset off, out T obj)
        => _Grid.TryGetValue(off, out obj);
    public T Get(Offset off)
        => _Grid[off];

    // oneliners using already defined methods
    public void Insert(T obj, Offset off) 
        => InsertRange(obj, off, new Offset(1,1));
    public bool Check(Offset off) => TryGet(off, out _);

    /*
     * Return FALSE if there's nothing here. 
     * If you're looking for an empty range, make sure you look for FALSE.
     */
    public bool FirstInRange(Offset off, Offset size, out Offset offOut)
    {
        offOut = new(INT_MIN,INT_MIN);
        // i feel like negative size should be possible but alas
        if(size.x <= 0 || size.y <= 0)
            return false;

        // iterate over range until find something
        for(int i = 0; i < size.x; i++)
            for(int j = 0; j < size.y; j++)
        {
            offOut = off + new Offset(i,j);
            if(TryGet(offOut, out _))
                return true;
        }
        return false;
    }
    public void LogEntries()
    {
        StringBuilder sb = new("[LowLevelGrid.LogEntries] Grid contains keys:");
        foreach(Offset x in _Grid.Keys)
        {
            sb.Append($"{x} ");
        }
        Debug.Log(sb.ToString());
    }
}
