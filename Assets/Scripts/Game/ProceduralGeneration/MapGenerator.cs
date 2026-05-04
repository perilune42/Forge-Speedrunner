using System.Collections.Generic;
using System.Linq;
using Unity.Content;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Passage passagePrefab;
    public Grid Grid;

    [SerializeField] List<Room> standardRoomPrefabs;
    [SerializeField] Room startRoomPrefab, finishRoomPrefab;
    [SerializeField] Room interchangePrefab;

    private List<VirtualRoom> plannedRooms = new();
    private List<Room> createdRooms = new();
    private List<Passage> createdPassages = new();

    private List<Room> roomPool;

    public (List<Room>, List<Passage>) CreateMap()
    {

        int attempts = 0;
        while (attempts++ < 20)
        {
            if (TryCreateMap()) break;
        }
        Debug.Log($"Created map in {attempts} attempts");

        BuildRooms();
        return (createdRooms, createdPassages);
    }

    bool TryCreateMap()
    {
        roomPool = standardRoomPrefabs.ToList();

        Grid = new();
        plannedRooms = new();
        createdRooms = new();
        createdPassages = new();

        for (int i = 0; i < roomPool.Count(); i++)
        {
            roomPool[i].RoomID = i;
        }

        // create 2 interchange rooms that are guaranteed to have 2 viable paths between them
        VirtualRoom startRoom = PlaceRoomWithOrigin(startRoomPrefab, new Vector2Int(0, 0), new());
        var path1 = CreatePath(roomPool, startRoom, Random.Range(1, 2));
        VirtualRoom interchange1 = AttachRandomRoom(path1[^1], new List<Room>() { interchangePrefab });
        var path2 = CreatePath(roomPool, interchange1, Random.Range(2, 3));
        VirtualRoom interchange2 = AttachRandomRoom(path2[^1], new List<Room>() { interchangePrefab });
        CreateEnding(roomPool, interchange2);

        List<Room> savedPool = new(roomPool);

        int bridgeAttempts = 20;
        bool success = false;
        for (int i = 0; i < bridgeAttempts; i++)
        {
            if (TryBridge(roomPool, interchange1, interchange2, 5, out _, true))
            {
                success = true;
                break;
            }
            else
            {
                roomPool = new(savedPool);
            }
        }
        if (!success) return false;

        // now try some random bullshit
        savedPool = new(roomPool);

        int attempts = 0;
        int createdPaths = 0;

        while (attempts++ < 100 && createdPaths < 3)
        {
            success = false;
            foreach (VirtualRoom room in plannedRooms.Shuffled())
            {
                foreach (VirtualRoom target in plannedRooms.Shuffled())
                {
                    if (TryBridge(roomPool, room, target, 7, out var pathRooms, true))
                    {
                        success = true;
                        savedPool = new(roomPool);
                        createdPaths++;
                        break;
                    }
                    else
                    {
                        roomPool = new(savedPool);
                    }
                }
                if (success) break;
            }
        }
        Debug.Log($"created {createdPaths} additional paths");
        return true;

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

    VirtualRoom PlaceRoomWithOrigin(Room roomPrefab, Vector2Int origin, List<Connection> allowedConnections)
    {
        RoomBounds bounds = roomPrefab.GetBounds();
        VirtualRoom newRoom = new VirtualRoom(roomPrefab);
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
                newRoom.occupiedCells.Add(newCell);
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
                            if (Connection.CanConnect(cell.Connections[dir].type, neighbor.Connections[-dir].type)
                                && allowedConnections.Contains(neighbor.Connections[-dir])) {
                                cell.Connections[dir].attachedConn = neighbor.Connections[-dir];
                                neighbor.Connections[-dir].attachedConn = cell.Connections[dir];
                            }
                        }

                    }

                }
            }
        }
        plannedRooms.Add(newRoom);
        return newRoom;
    }

    VirtualRoom TryPlaceRoomWithDoorway(Room roomPrefab, Doorway door, Connection attachPoint)
    {
        // attach dir = which way the existing doorway (attachPoint) leads
        if (!attachPoint.CanConnectToDoor(door)) return null ;
        Vector2Int origin = (attachPoint.cellRef.Position + attachPoint.direction) - door.GetRoomOffset();
        if (CanPlaceRoomWithOrigin(roomPrefab, origin))
        {
            
            return PlaceRoomWithOrigin(roomPrefab, origin, new() { attachPoint }); ;
        }
        return null;
    }

    bool TryBridge(List<Room> pool, VirtualRoom start, VirtualRoom end, int maxLength, out List<VirtualRoom> createdRooms, bool requireOpenings = false)
    {
        createdRooms = new();
        VirtualRoom currEnd = start;
        for (int i = 0; i < maxLength; i++)
        {
            var createdRoom = AttachRandomRoom(currEnd, pool);
            if (createdRoom != null)
            {
                currEnd = createdRoom;
                RemoveFromPool(pool, createdRoom);
                createdRooms.Add(createdRoom);
                if (TryConnectRooms(createdRoom, end) != null)
                {
                    if (requireOpenings)
                    {
                        foreach (VirtualRoom pathRoom in createdRooms)
                        {
                            foreach (Connection conn in pathRoom.externalConnections)
                            {
                                // if there is at least one spot to attach a new room to
                                if (conn.attachedConn == null && !Grid.Cells.ContainsKey(conn.GetConnectedPos()))
                                {
                                    Debug.Log("Bridge successful!");
                                    return true;
                                }
                            }
                        }
                        Debug.Log("Bridge failed");
                        foreach (var room in createdRooms)
                        {
                            RemoveRoom(room);
                        }
                        return false;
                    }
                    else
                    {
                        Debug.Log("Bridge successful!");
                        return true;
                    }
                }
            }
            else break;
        }
        Debug.Log("Bridge failed");
        foreach (var room in createdRooms)
        {
            RemoveRoom(room);
        }

        return false;
    }

    // room1 to room2 only
    Connection TryConnectRooms(VirtualRoom room1, VirtualRoom room2, bool oneWay = true)
    {
        foreach (var conn in room1.externalConnections)
        {
            if (conn.attachedConn != null) continue;
            if (conn.type == ConnType.Entrance) continue;
            Cell cell = conn.cellRef;
            Cell neighbor;
            if (Grid.Cells.TryGetValue(cell.Position + conn.direction, out neighbor)
                && neighbor.OwnedRoom == room2)
            {
                Connection otherConn = neighbor.Connections[-conn.direction];
                if (otherConn.type == ConnType.Exit) continue;
                if (Connection.CanConnect(conn.type,otherConn.type))
                {
                    conn.attachedConn = otherConn;
                    otherConn.attachedConn = conn;
                    return conn;
                }
            }
        }
        return null;
    }


    VirtualRoom PlaceRoomRandomly(IEnumerable<Room> pool = null)
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
                        VirtualRoom createdRoom = TryPlaceRoomWithDoorway(roomCandidate, door, conn);
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

    List<VirtualRoom> CreatePath(List<Room> pool, VirtualRoom startPoint, int numRooms)
    {
        List<VirtualRoom> roomsAdded = new List<VirtualRoom>();
        VirtualRoom currentEnd = startPoint;
        VirtualRoom createdRoom = null;

        for (int i = 0; i < numRooms; i++)
        {
            createdRoom = AttachRandomRoom(currentEnd, pool);
            if (createdRoom != null)
            {
                currentEnd = createdRoom;
                roomsAdded.Add(createdRoom);
                RemoveFromPool(pool, createdRoom);
            }
        }
        return roomsAdded;
    }

    void CreateEnding(List<Room> pool, VirtualRoom startPoint)
    {
        List<Room> preFinalPool = pool.Where(r => r.doorwaysRight.Count(d => d != null) > 0).ToList();
        VirtualRoom preFinal = AttachRandomRoom(startPoint, preFinalPool);
        RemoveFromPool(pool, preFinal);
        List<Room> finalPool = new() { finishRoomPrefab };
        VirtualRoom final = AttachRandomRoom(preFinal, finalPool);
    }

    // attach a random room to the specified room at a random doorway
    VirtualRoom AttachRandomRoom(VirtualRoom room, IEnumerable<Room> pool = null)
    {
        if (pool == null) pool = roomPool;
        VirtualRoom createdRoom;

        foreach (Room roomCandidate in pool.Shuffled())
        {
            foreach (Connection fromConn in room.externalConnections
                .Where(c => c.type == ConnType.Exit || c.type == ConnType.Both).Shuffled())
            {
                foreach (Doorway toDoorway in roomCandidate.GetAllDoorways()
                    .Where(d => d.Type == DoorwayType.ENTRANCE || d.Type == DoorwayType.BOTH).Shuffled())
                {
                    createdRoom = TryPlaceRoomWithDoorway(roomCandidate, toDoorway, fromConn);
                    if (createdRoom != null)
                    {
                        return createdRoom;
                    }
                }
            }
        }
        return null;
    }

    // attach a random room to the specified room at a specified connection
    VirtualRoom AttachRandomRoom(VirtualRoom room, Connection targetConn, IEnumerable<Room> pool = null)
    {
        if (pool == null) pool = roomPool;
        VirtualRoom createdRoom;
        foreach (Room roomCandidate in pool.Shuffled())
        {
            foreach (Doorway toDoorway in roomCandidate.GetAllDoorways()
                .Where(d => d.Type == DoorwayType.ENTRANCE || d.Type == DoorwayType.BOTH).Shuffled())
            {
                createdRoom = TryPlaceRoomWithDoorway(roomCandidate, toDoorway, targetConn);
                if (createdRoom != null)
                {
                    return createdRoom;
                }
            }
        }
        return null;
    }
    
    void RemoveFromPool(List<Room> pool, VirtualRoom toRemove)
    {
        pool.Remove(pool.First(r => r.RoomID == toRemove.RoomID));
    }

    void RemoveRoom(VirtualRoom room)
    {
        plannedRooms.Remove(room);
        foreach (Cell cell in room.occupiedCells)
        {
            Grid.Cells.Remove(cell.Position);
        }
        foreach (Connection conn in room.externalConnections)
        {
            if (conn.attachedConn != null)
            {
                conn.attachedConn.attachedConn = null;
            }
        }
    }


    void BuildRooms()
    {
        Dictionary<VirtualRoom, Room> roomMap = new();
        Dictionary<Doorway, Doorway> passageAttachments = new();
        foreach (VirtualRoom vRoom in plannedRooms)
        {
            var newRoom = vRoom.Instantiate();
            createdRooms.Add(newRoom);
            roomMap[vRoom] = newRoom;
        }
        foreach (Room room in createdRooms)
        {
            foreach (Doorway doorway in room.GetAllDoorways())
            {
                if (passageAttachments.ContainsKey(doorway)) continue;
                Connection conn = Grid.Cells[room.gridPosition + doorway.GetRoomOffset()].Connections[doorway.GetTransitionDirection().ToV2Int()];
                if (conn.attachedConn != null)
                {
                    Room connectedRoom = roomMap[conn.attachedConn.cellRef.OwnedRoom];
                    Doorway matchedDoorway = connectedRoom.GetAllDoorways()
                                                .First(d => d.GetRoomOffset() + connectedRoom.gridPosition == conn.attachedConn.cellRef.Position
                                                && d.GetTransitionDirection().ToV2Int() == conn.attachedConn.direction);
                    passageAttachments[matchedDoorway] = doorway;                    
                }
            }
        }
        foreach (var kvp in passageAttachments)
        {
            Passage newPassage = Instantiate(passagePrefab);
            newPassage.door1 = kvp.Key;
            newPassage.door2 = kvp.Value;
            createdPassages.Add(newPassage);
        }

    }
}

   



