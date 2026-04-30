using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Passage passagePrefab;
    public Grid Grid = new();

    private List<Room> createdRooms = new();
    private List<Passage> createdPassages = new();
    public (List<Room>, List<Passage>) CreateMap()
    {
        createdRooms = new();
        createdPassages = new();

        PlaceRoomWithOrigin(GameRegistry.Instance.StartRoom, new Vector2Int(0, 0));

        const int numRooms = 5;

        for (int i = 0; i < numRooms; i++)
        {
            PlaceRoomRandomly();
        }

        return (createdRooms, createdPassages);
    }

    bool CanPlaceRoomWithOrigin(Room roomPrefab, Vector2Int origin)
    {
        RoomBounds bounds = roomPrefab.GetBounds();
        for (int i = 0; i < roomPrefab.size.x; i++)
        {
            for (int j = 0; j < roomPrefab.size.y; j++)
            {
                Vector2Int cellPos = origin + new Vector2Int(i, j);
                if (Grid.Cells.ContainsKey(cellPos))
                {
                    return false;
                }
            }
        }
        return true;
    }

    void PlaceRoomWithOrigin(Room roomPrefab, Vector2Int origin)
    {
        RoomBounds bounds = roomPrefab.GetBounds();
        var newRoom = Instantiate(roomPrefab);
        newRoom.gridPosition = origin;
        // pass 1: place all cells
        for (int i = 0; i < roomPrefab.size.x; i++)
        {
            for (int j = 0; j < roomPrefab.size.y; j++)
            {
                Vector2Int cellPos = origin + new Vector2Int(i, j);
                if (Grid.Cells.ContainsKey(cellPos))
                {
                    Debug.LogError("Room Overlap");
                    return;
                }
                Cell newCell = new Cell();
                newCell.Position = cellPos;
                newCell.OwnedRoom = newRoom;
                newCell.SetConnections(new Vector2Int(i, j));

                Grid.Cells[cellPos] = newCell;
            }
        }
        // pass 2: mark cell connections
        for (int i = 0; i < roomPrefab.size.x; i++)
        {
            for (int j = 0; j < roomPrefab.size.y; j++)
            {
                Vector2Int cellPos = origin + new Vector2Int(i, j);
                Cell cell = Grid.Cells[cellPos];
                foreach (Vector2Int dir in Grid.Directions)
                {
                    Cell neighbor;
                    if (Grid.Cells.TryGetValue(cellPos + dir, out neighbor))
                    {
                        if (neighbor.OwnedRoom != newRoom)
                        {
                            // set all connections if able
                            if (Connection.CanConnect(cell.Connections[dir].type, neighbor.Connections[-dir].type)) {
                                cell.Connections[dir].type = ConnType.Connected;
                                neighbor.Connections[-dir].type = ConnType.Connected;
                                Passage newPassage = Instantiate(passagePrefab);
                                newPassage.door1 = cell.Connections[dir].doorway;
                                newPassage.door2 = neighbor.Connections[-dir].doorway;
                                createdPassages.Add(newPassage);
                            }
                        }

                    }

                }
            }
        }
        createdRooms.Add(newRoom);
    }

    bool TryPlaceRoomWithDoorway(Room roomPrefab, Doorway door, Connection attachPoint)
    {
        // attach dir = which way the existing doorway (attachPoint) leads
        if (!attachPoint.CanConnectToDoor(door)) return false;
        Vector2Int origin = (attachPoint.cellRef.Position + attachPoint.direction) - door.GetRoomOffset();
        if (CanPlaceRoomWithOrigin(roomPrefab, origin))
        {
            PlaceRoomWithOrigin(roomPrefab, origin);
            return true;
        }
        return false;
    }

    Room PlaceRoomRandomly()
    {
        foreach (Room roomCandidate in GameRegistry.Instance.RoomPrefabs.Shuffled())
        {
            foreach (Cell cell in Grid.Cells.Values.Shuffled())
            {
                foreach (Connection conn in cell.Connections.Values.Shuffled())
                {
                    foreach (Doorway door in roomCandidate.GetAllDoorways().Shuffled())
                    {
                        if (TryPlaceRoomWithDoorway(roomCandidate, door, conn))
                        {
                            return createdRooms[^1];
                        }
                    }
                }
            }
        }
        Debug.LogError("failed to place room");
        return null;
    }
}

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
    SameRoom, Entrance, Exit, Both, Connected, Closed
}

public class Connection
{
    public ConnType type;
    public Doorway doorway;
    public Vector2Int direction;
    public Cell cellRef;
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





    public Room OwnedRoom;
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
            if (OwnedRoom.doorwaysLeft[roomOffset.y] != null)
            {
                Connections[Vector2Int.left].type = Connection.FromDoorway(OwnedRoom.doorwaysLeft[roomOffset.y]);
                Connections[Vector2Int.left].doorway = OwnedRoom.doorwaysLeft[roomOffset.y];
            }
            else
            {
                Connections[Vector2Int.left].type = ConnType.Closed;
            }
        }
        if (roomOffset.x == OwnedRoom.size.x - 1)
        {
            if (OwnedRoom.doorwaysRight[roomOffset.y] != null)
            {
                Connections[Vector2Int.right].type = Connection.FromDoorway(OwnedRoom.doorwaysRight[roomOffset.y]);
                Connections[Vector2Int.right].doorway = OwnedRoom.doorwaysRight[roomOffset.y];
            }
            else
            {
                Connections[Vector2Int.right].type = ConnType.Closed;
            }
        }
        if (roomOffset.y == 0)
        {
            if (OwnedRoom.doorwaysDown[roomOffset.x] != null)
            {
                Connections[Vector2Int.down].type = Connection.FromDoorway(OwnedRoom.doorwaysDown[roomOffset.x]);
                Connections[Vector2Int.down].doorway = OwnedRoom.doorwaysDown[roomOffset.x];
            }
            else
            {
                Connections[Vector2Int.down].type = ConnType.Closed;
            }
        }
        if (roomOffset.y == OwnedRoom.size.y - 1)
        {
            if (OwnedRoom.doorwaysUp[roomOffset.x] != null)
            {
                Connections[Vector2Int.up].type = Connection.FromDoorway(OwnedRoom.doorwaysUp[roomOffset.x]);
                Connections[Vector2Int.up].doorway = OwnedRoom.doorwaysUp[roomOffset.x];

            }
            else
            {
                Connections[Vector2Int.up].type = ConnType.Closed;
            }
        }
    }
}


