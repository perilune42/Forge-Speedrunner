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
        Dictionary<Vector2Int, Room> createdRooms = new();
        Stack<Vector2Int> coords = new();
        coords.Push(new Vector2Int(0,0));
        // 2. while there are rooms left to spawn:
        //    1. spawn room at coordinate, making random choice
        //    2. check if there's anything in all directions
        //    3. if already a created room, or nothing there, ignore
        //    4. otherwise push 
        while(coords.Count > 0)
        {
            Vector2Int coord = coords.Pop();
            // TODO: spawn room

            // push to stack
            for(int i = -1; i <= 1; i+= 2)
            {
                Vector2Int c1 = coord;
                Vector2Int c2 = coord;
                c1.x += i;
                c2.y += i;
                if(path.coords.Contains(c1) && !createdRooms.ContainsKey(c1))
                    coords.Push(c1);
                if(path.coords.Contains(c2) && !createdRooms.ContainsKey(c2))
                    coords.Push(c2);
            }
        }
    }
}
