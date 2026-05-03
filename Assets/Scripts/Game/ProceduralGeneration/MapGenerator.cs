using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Passage passagePrefab;
    public Grid Grid = new();

    private List<VirtualRoom> plannedRooms = new();
    private List<Room> createdRooms = new();
    private List<Passage> createdPassages = new();

    private List<Room> roomPool;

    public (List<Room>, List<Passage>) CreateMap()
    { 
        roomPool = GameRegistry.Instance.RoomPrefabs.ToList();

        plannedRooms = new();
        createdRooms = new();
        createdPassages = new();

        for (int i = 0; i < roomPool.Count(); i++)
        {
            roomPool[i].RoomID = i;
        }
        CreateMainPath(roomPool, 10);

        BuildRooms();
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

    VirtualRoom PlaceRoomWithOrigin(Room roomPrefab, Vector2Int origin)
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
                                cell.Connections[dir].attachedConn = neighbor.Connections[-dir];
                                neighbor.Connections[-dir].type = ConnType.Connected;
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
            
            return PlaceRoomWithOrigin(roomPrefab, origin); ;
        }
        return null;
    }

    //VirtualRoom TryPlaceRoomWithDoorway(Room roomPrefab, Doorway door, Doorway attachPoint)
    //{
    //    Connection attachPointConn = Grid.Cells[attachPoint.GetRoomOffset() + attachPoint.enclosingRoom.gridPosition]
    //                                    .Connections[attachPoint.GetTransitionDirection().ToV2Int()];
    //    return TryPlaceRoomWithDoorway(roomPrefab, door, attachPointConn);
    //}


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

    void CreateMainPath(List<Room> pool, int numRooms)
    {
        VirtualRoom currentEnd = PlaceRoomWithOrigin(GameRegistry.Instance.StartRoom, new Vector2Int(0, 0));
        VirtualRoom createdRoom = null;

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
        VirtualRoom preFinal = AttachRandomRoom(currentEnd, preFinalPool);
        RemoveFromPool(pool, preFinal);
        List<Room> finalPool = new() { GameRegistry.Instance.FinishRoom };
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
        Debug.LogError("Error attaching random room");
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
                if (conn.type == ConnType.Connected)
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

   



