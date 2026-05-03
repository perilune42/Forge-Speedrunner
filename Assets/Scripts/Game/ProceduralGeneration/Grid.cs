using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public Dictionary<Vector2Int, Cell> Cells = new();
    public static readonly List<Vector2Int> Directions = new()
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };
}

public enum ConnType
{
    SameRoom, Entrance, Exit, Both, Closed
}

public class Connection
{
    public ConnType type;
    public Doorway doorway;
    public Vector2Int direction;
    public Cell cellRef;
    public Connection attachedConn = null;
    public static ConnType FromDoorway(Doorway doorway)
    {
        return doorway.Type switch
        {
            DoorwayType.ENTRANCE => ConnType.Entrance,
            DoorwayType.EXIT => ConnType.Exit,
            DoorwayType.BOTH => ConnType.Both,
            _ => ConnType.Closed
        };
    }


    public static bool CanConnect(ConnType conn1, ConnType conn2, bool recurse = true)
    {
        return (conn1 == ConnType.Entrance && conn2 == ConnType.Exit)
            || (conn1 == ConnType.Both && conn2 == ConnType.Entrance)
            || (conn1 == ConnType.Both && conn2 == ConnType.Exit)
            || (conn1 == ConnType.Both && conn2 == ConnType.Both)
            || (recurse && CanConnect(conn2, conn1, false));
    }

    public bool CanConnectToDoor(Doorway door)
    {
        if (door == null) return false;
        return (Vector2)direction == -door.GetTransitionDirection() && CanConnect(type, FromDoorway(door));
    }
}

public class Cell
{
    public VirtualRoom OwnedRoom;
    public Vector2Int Position;
    public Dictionary<Vector2Int, Connection> Connections = new();


    // only sets connections in isolation, ignores neighbors at this stage
    public void SetConnections(Vector2Int roomOffset)
    {
        // default to same room everything first
        foreach (var dir in Grid.Directions)
        {
            var conn = new Connection();
            conn.type = ConnType.SameRoom;
            conn.direction = dir;
            conn.doorway = null;
            conn.cellRef = this;
            Connections[dir] = conn;
        }
        if (roomOffset.x == 0)
        {
            Connection conn = Connections[Vector2Int.left];
            if (OwnedRoom.roomBase.doorwaysLeft[roomOffset.y] != null)
            {
                
                conn.type = Connection.FromDoorway(OwnedRoom.roomBase.doorwaysLeft[roomOffset.y]);
                conn.doorway = OwnedRoom.roomBase.doorwaysLeft[roomOffset.y];
                OwnedRoom.externalConnections.Add(conn);
            }
            else
            {
                conn.type = ConnType.Closed;
            }
            Connections[Vector2Int.left] = conn;
        }
        if (roomOffset.x == OwnedRoom.size.x - 1)
        {
            Connection conn = Connections[Vector2Int.right];
            if (OwnedRoom.roomBase.doorwaysRight[roomOffset.y] != null)
            {
                conn.type = Connection.FromDoorway(OwnedRoom.roomBase.doorwaysRight[roomOffset.y]);
                conn.doorway = OwnedRoom.roomBase.doorwaysRight[roomOffset.y];
                OwnedRoom.externalConnections.Add(conn);
            }
            else
            {
                conn.type = ConnType.Closed;
            }
            Connections[Vector2Int.right] = conn;
        }
        if (roomOffset.y == 0)
        {
            Connection conn = Connections[Vector2Int.down];
            if (OwnedRoom.roomBase.doorwaysDown[roomOffset.x] != null)
            {
                conn.type = Connection.FromDoorway(OwnedRoom.roomBase.doorwaysDown[roomOffset.x]);
                conn.doorway = OwnedRoom.roomBase.doorwaysDown[roomOffset.x];
                OwnedRoom.externalConnections.Add(conn);
            }
            else
            {
                conn.type = ConnType.Closed;
            }
            Connections[Vector2Int.down] = conn;
        }
        if (roomOffset.y == OwnedRoom.size.y - 1)
        {
            Connection conn = Connections[Vector2Int.up];
            if (OwnedRoom.roomBase.doorwaysUp[roomOffset.x] != null)
            {
                conn.type = Connection.FromDoorway(OwnedRoom.roomBase.doorwaysUp[roomOffset.x]);
                conn.doorway = OwnedRoom.roomBase.doorwaysUp[roomOffset.x];
                OwnedRoom.externalConnections.Add(conn);
            }
            else
            {
                conn.type = ConnType.Closed;
            }
            Connections[Vector2Int.up] = conn;
        }
    }
}