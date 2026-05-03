using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirtualRoom
{
    public int RoomID;
    public Room roomBase;
    public Vector2Int gridPosition;
    public Vector2Int size;
    public List<Cell> occupiedCells = new();
    public List<Connection> externalConnections = new();

    public RoomBounds GetBounds()
    {
        var bounds = new RoomBounds();
        bounds.min = gridPosition;
        bounds.max = gridPosition + size;
        return bounds;
    }



    public VirtualRoom(Room roomBase)
    {
        this.roomBase = roomBase;
        RoomID = roomBase.RoomID;   
        gridPosition = roomBase.gridPosition;
        size = roomBase.size;
    }

    public Room Instantiate()
    {
        Room createdRoom = GameObject.Instantiate(roomBase);
        createdRoom.gridPosition = gridPosition;
        return createdRoom;
    }


}