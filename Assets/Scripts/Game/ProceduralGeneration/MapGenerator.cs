using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Passage passagePrefab;
    public Grid Grid = new();

    private List<Room> createdRooms = new();
    private List<Passage> createdPassages = new();

    private List<Room> roomPool;

    public (List<Room>, List<Passage>) CreateMap()
    { 
        roomPool = GameRegistry.Instance.RoomPrefabs.ToList();

        createdRooms = new();
        createdPassages = new();

        for (int i = 0; i < roomPool.Count(); i++)
        {
            roomPool[i].RoomID = i;
        }
        CreateMainPath(roomPool, 10);

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

    Room PlaceRoomWithOrigin(Room roomPrefab, Vector2Int origin)
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
                    return null;
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
        return newRoom;
    }

    Room TryPlaceRoomWithDoorway(Room roomPrefab, Doorway door, Connection attachPoint)
    {
        // attach dir = which way the existing doorway (attachPoint) leads
        if (!attachPoint.CanConnectToDoor(door)) return null ;
        Vector2Int origin = (attachPoint.cellRef.Position + attachPoint.direction) - door.GetRoomOffset();
        if (CanPlaceRoomWithOrigin(roomPrefab, origin))
        {
            
            return PlaceRoomWithOrigin(roomPrefab, origin); ;
        }
        return null;
    }

    Room TryPlaceRoomWithDoorway(Room roomPrefab, Doorway door, Doorway attachPoint)
    {
        Connection attachPointConn = Grid.Cells[attachPoint.GetRoomOffset() + attachPoint.enclosingRoom.gridPosition]
                                        .Connections[attachPoint.GetTransitionDirection().ToV2Int()];
        return TryPlaceRoomWithDoorway(roomPrefab, door, attachPointConn);
    }


    Room PlaceRoomRandomly(IEnumerable<Room> pool = null)
    {
        if (pool == null) pool = roomPool;
        foreach (Room roomCandidate in pool.Shuffled())
        {
            foreach (Cell cell in Grid.Cells.Values.Shuffled())
            {
                foreach (Connection conn in cell.Connections.Values.Shuffled())
                {
                    foreach (Doorway door in roomCandidate.GetAllDoorways().Shuffled())
                    {
                        Room createdRoom = TryPlaceRoomWithDoorway(roomCandidate, door, conn);
                        if (createdRoom != null)
                        {
                            return createdRoom;
                        }
                    }
                }
            }
        }
        Debug.LogError("failed to place room");
        return null;
    }

    void CreateMainPath(List<Room> pool, int numRooms)
    {
        Room currentEnd = PlaceRoomWithOrigin(GameRegistry.Instance.StartRoom, new Vector2Int(0, 0));
        Room createdRoom = null;

        for (int i = 0; i < numRooms - 3; i++)
        {
            createdRoom = AttachRandomRoom(currentEnd, pool);
            if (createdRoom != null)
            {
                currentEnd = createdRoom;
                RemoveFromPool(pool, createdRoom);
            }
        }

        List<Room> preFinalPool = pool.Where(r => r.doorwaysRight.Count(d => d != null) > 0).ToList();
        Room preFinal = AttachRandomRoom(currentEnd, preFinalPool);
        RemoveFromPool(pool, preFinal);
        List<Room> finalPool = new() { GameRegistry.Instance.FinishRoom };
        Room final = AttachRandomRoom(preFinal, finalPool);
    }

    // attach a random room to the specified room at a random doorway
    Room AttachRandomRoom(Room room, IEnumerable<Room> pool = null)
    {
        if (pool == null) pool = roomPool;
        Room createdRoom;

        foreach (Room roomCandidate in pool.Shuffled())
        {
            foreach (Doorway fromDoorway in room.GetAllDoorways()
                .Where(d => d.Type == DoorwayType.EXIT || d.Type == DoorwayType.BOTH).Shuffled())
            {
                foreach (Doorway toDoorway in roomCandidate.GetAllDoorways()
                    .Where(d => d.Type == DoorwayType.ENTRANCE || d.Type == DoorwayType.BOTH).Shuffled())
                {
                    createdRoom = TryPlaceRoomWithDoorway(roomCandidate, toDoorway, fromDoorway);
                    if (createdRoom != null)
                    {
                        return createdRoom;
                    }
                }
            }
        }
        Debug.LogError("Error attaching random room");
        return null;
    }

    // attach a random room to the specified room at a specified doorway
    Room AttachRandomRoom(Room room, Doorway targetDoorway, IEnumerable<Room> pool = null)
    {
        if (pool == null) pool = roomPool;
        Room createdRoom;
        foreach (Room roomCandidate in pool.Shuffled())
        {
            foreach (Doorway toDoorway in roomCandidate.GetAllDoorways()
                .Where(d => d.Type == DoorwayType.ENTRANCE || d.Type == DoorwayType.BOTH).Shuffled())
            {
                createdRoom = TryPlaceRoomWithDoorway(roomCandidate, toDoorway, targetDoorway);
                if (createdRoom != null)
                {
                    return createdRoom;
                }
            }
        }
        return null;
    }
    
    void RemoveFromPool(List<Room> pool, Room toRemove)
    {
        pool.Remove(pool.First(r => r.RoomID == toRemove.RoomID));
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


