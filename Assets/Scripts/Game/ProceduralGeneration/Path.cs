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

    public (List<Room>, List<Passage>) Create()
    {
        Dictionary<Cell, Room> createdRooms = new();
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
            Room roomSource = createdRooms[c.Source];
            Room roomSink = createdRooms[c.Sink];

            List<Doorway> sourceDoors; List<Doorway> sinkDoors;
            switch(c.ConnectionDir)
            {
                case LEFT:
                    sourceDoors = roomSource.doorwaysLeft;
                    sinkDoors = roomSource.doorwaysRight;
                    break;
                case RIGHT:
                    sourceDoors = roomSource.doorwaysRight;
                    sinkDoors = roomSource.doorwaysLeft;
                    break;
                case UP:
                    sourceDoors = roomSource.doorwaysUp;
                    sinkDoors = roomSource.doorwaysDown;
                    break;
                default: // DOWN
                    sourceDoors = roomSource.doorwaysUp;
                    sinkDoors = roomSource.doorwaysDown;
            }

            Doorway sourceDoor = sourceDoors[c.SourceInd];
            Doorway sinkDoor = sinkDoors[c.SinkInd];

            Passage pass = new();
            pass.door1 = sourceDoor;
            pass.door2 = sinkDoor;
            passages.Add(pass);
        }

        return (createdRooms.Values.ToList(), passages);
    }
}
