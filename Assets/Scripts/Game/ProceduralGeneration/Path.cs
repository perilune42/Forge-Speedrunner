using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static Direction;

/* Construct path once the rooms actually exist.
 * This replaces List<Cell> as a return type in IPathGenerator
 */
public class PathCreator
{
    internal struct Connection
    {
        public Cell Source;
        public Cell Sink;
        public int SourceInd;
        public int SinkInd;
        public Direction ConnectionDir;
    }
    private List<Cell> Cells;
    private List<Connection> Connections;
    private Transform roomParent;

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

            Passage pass = new();
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
}
