using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static Direction;

/* Construct path once the rooms actually exist.
 * This replaces List<Cell> as a return type in IPathGenerator
 */
public struct Connection
{
    public Cell Source;
    public Cell Sink;
    public int SourceInd;
    public int SinkInd;
    public Direction ConnectionDir;
}
public enum Status
{
    NO_FIN,
    UNDER_MIN,
    DEAD_ENDS,
    ALL_CLEAR,
}
public class PathCreator
{
    public List<Cell> Cells;
    public List<Connection> Connections;
    private Transform roomParent;
    public GameObject PassPrefab;

    public PathCreator(List<Cell> cells)
    {
        Cells = cells;
        Connections = new();
    }

    public void AddConnection(Cell source, Cell sink, int sourceInd, int sinkInd, Direction dir)
    {
        Connection conn = new();
        conn.Source = source;
        conn.Sink = sink;
        conn.SourceInd = sourceInd;
        conn.SinkInd = sinkInd;
        conn.ConnectionDir = dir;
        Connections.Add(conn);
    }
    public void RegisterParent(Transform parent)
    {
        roomParent = parent;
    }

    public (List<Room>, List<Passage>) Create()
    {
        Dictionary<Vector2Int, Room> createdRooms = new();
        List<Passage> passages = new();
        // form rooms
        foreach(Cell c in Cells)
        {
            Vector3 screenPosition = new(c.offset.x, c.offset.y, 0F);
            screenPosition *= 100F;
            Room room = c.room;

            Room realRoom = (Room)GameObject.Instantiate(room, screenPosition, Quaternion.identity);
            createdRooms.Add(c.offset, realRoom);
            realRoom.gridPosition = c.offset;
            realRoom.transform.SetParent(roomParent);
        }

        // form paths
        foreach(Connection c in Connections)
        {
            Room roomSource; Room roomSink;
            if(!createdRooms.TryGetValue(c.Source.offset, out roomSource))
            {
                Debug.Log($"Can't find anything for cell at {c.Source.offset}");
                continue;
            }
            if(!createdRooms.TryGetValue(c.Sink.offset, out roomSink))
            {
                Debug.Log($"Can't find anything for cell at {c.Sink.offset}");
                continue;
            }

            List<Doorway> sourceDoors; List<Doorway> sinkDoors;
            switch(c.ConnectionDir)
            {
                case LEFT:
                    sourceDoors = roomSource.doorwaysLeft;
                    sinkDoors = roomSink.doorwaysRight;
                    break;
                case RIGHT:
                    sourceDoors = roomSource.doorwaysRight;
                    sinkDoors = roomSink.doorwaysLeft;
                    break;
                case UP:
                    sourceDoors = roomSource.doorwaysUp;
                    sinkDoors = roomSink.doorwaysDown;
                    break;
                default: // DOWN
                    sourceDoors = roomSource.doorwaysDown;
                    sinkDoors = roomSink.doorwaysUp;
                    break;
            }

            Doorway sourceDoor = sourceDoors[c.SourceInd];
            Doorway sinkDoor = sinkDoors[c.SinkInd];
            if(sourceDoor == null)
                Debug.Log($"sourceDoor null for room {roomSource} at index {c.SourceInd}, direction {c.ConnectionDir}");
            if(sinkDoor == null)
                Debug.Log($"sinkDoor null for room {roomSink} at index {c.SinkInd}, direction {c.ConnectionDir}");

            Passage pass = GameObject.Instantiate(PassPrefab).GetComponent<Passage>();
            pass.door1 = sourceDoor;
            pass.door2 = sinkDoor;
            passages.Add(pass);
        }

        // TODO replace when i have internet with something more sane
        List<Room> roomsTemp = new();
        foreach(Room r in createdRooms.Values)
            roomsTemp.Add(r);

        return (roomsTemp, passages);
    }
    public Status Validate(Room finishRoom, int pathMin)
    {
        // Room r = Cells[Cells.Count-1].room;
        // if(r != finishRoom)
        //     return Status.NO_FIN;
        bool hasFin = false;
        for(int i = 0; i < Cells.Count; i++)
        {
            Cell c = Cells[i];
            if(c.room == finishRoom)
            {
                hasFin = true;
                break;
            }
        }
        if(!hasFin) return Status.NO_FIN;

        if(Cells.Count < pathMin)
            return Status.UNDER_MIN;

        Dictionary<Vector2Int, int> numNeighbors = new();
        foreach(Cell c in Cells)
            numNeighbors.Add(c.offset, 0);

        foreach(Connection conn in Connections)
        {
            numNeighbors[conn.Source.offset] += 1;
            numNeighbors[conn.Sink.offset] += 1;
        }
        int numSourceSink = 0;
        foreach(int num in numNeighbors.Values)
            if(num > 2)
                return Status.DEAD_ENDS;
            else if(num == 1)
                numSourceSink++;
        if(numSourceSink != 2)
            return Status.DEAD_ENDS;
        return Status.ALL_CLEAR;
    }
}
