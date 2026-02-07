using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomSpawner : MonoBehaviour
{
    public void SpawnRoom(GameObject roomPrefab, Vector2Int gridPosition)
    {

    }

    private static (List<Doorway>, List<Doorway>) chooseDoorways(Room room1, Room room2, Direction dir)
    {
        switch(dir)
        {
            case Direction.LEFT:
                return (room1.doorwaysLeft, room2.doorwaysRight);
            case Direction.RIGHT:
                return (room1.doorwaysRight, room2.doorwaysLeft);
            case Direction.UP:
                return (room1.doorwaysUp, room2.doorwaysDown);
            case Direction.DOWN:
                return (room1.doorwaysDown, room2.doorwaysUp);
        }
    }
    public Passage ConnectRooms(Room room1, Room room2, Direction dir)
    {
        // 1. get doorway lists
        List<Doorway> doorwaysRoom1;
        List<Doorway> doorwaysRoom2;
        (doorwaysRoom1, doorwaysRoom2) = chooseDoorways(room1, room2, dir);

        // 2. find the only matching doorways
        foreach(Doorway doorFrom1 in doorwaysRoom1)
            foreach(Doorway doorFrom2 in doorwaysRoom2)
        {
            float xDiff = doorFrom1.x - doorFrom2.x;
            float yDiff = doorFrom1.y - doorFrom2.y;
            // if(-0.1F < xDiff && xDiff < 0.1F)
            //     return 
        }

        // DEFAULT: return null if failed
        return null;
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
