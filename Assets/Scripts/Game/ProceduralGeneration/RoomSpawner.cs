using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomSpawner : MonoBehaviour
{
    public void SpawnRoom(GameObject roomPrefab, Vector2Int gridPosition)
    {

    }

    public void RealizePath(Path path)
    {
        // 1. start at 0,0
        Dictionary<Vector2Int, Room> createdRooms;
        Stack<Vector2Int> coords = new();
        coords.Push(new Vector2Int(0,0));
        // 2. while there are rooms left to spawn:
        //    1. spawn room at coordinate, making random choice
        //    2. check if there's anything in all directions
        //    3. if already a created room, or nothing there, ignore
        //    4. otherwise push 
    }
}
