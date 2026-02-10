using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Direction;

public class DrunkenWalk : IPathGenerator
{
    public List<Cell> Generate(int pathLength)
    {
        Room[] roomPrefabs = GameRegistry.Instance.RoomPrefabs;
        int count = pathLength;

        List<Doorway> doors = new();
        List<Direction> dirs = new();
        List<Vector2Int> offsets = new();

        HashSet<Vector2Int> occupied = new();
        List<Cell> path = new();
        while(pathLength > 0 && doorStack.Count > 0)
        {
            Doorway door;
            Direction dir;
            Vector2Int offset;

            // take random
            int ind = Random.Range(0, doors.Count);
            door = doors[ind];
            dir = dirs[ind];
            offset = offsets[ind]

            // remove selected element (swap-remove array style)
            dirs[ind] = dirs[dirs.Count-1];
            doors[ind] = doors[doors.Count-1];
            offsets[ind] = offsets[offsets.Count-1]
            offsets.RemoveAt(offsets.Count-1);
            dirs.RemoveAt(dirs.Count-1);
            doors.RemoveAt(doors.Count-1);

            // calculate new offset


            // find room
            Room newRoom = findRoomWith(opposite(dir), roomPrefabs);

            // if room cannot be placed at this offset, pick a new room
            // TODO

            // add appropriate cells
            Cell newc = new Cell(newRoom, offset);

            // take from room one doorway list at a time
            extractList(doors, dirs, newRoom.doorwaysLeft, LEFT);
            extractList(doors, dirs, newRoom.doorwaysRight, RIGHT);
            extractList(doors, dirs, newRoom.doorwaysUp, UP);
            extractList(doors, dirs, newRoom.doorwaysDown, DOWN);
        }
        private Direction opposite(in Direction dir)
        {
            if(dir == LEFT)
                return RIGHT;
            if(dir == RIGHT)
                return LEFT;
            if(dir == UP)
                return DOWN;
            // if(dir == DOWN)
            return UP;
        }
        private List<Doorway> matchingDir(in Direction dir, in Room r)
        {
            if(dir == LEFT)
                return r.doorwaysLeft;
            if(dir == RIGHT)
                return r.doorwaysRight;
            if(dir == UP)
                return r.doorwaysUp;
            // if(dir == DOWN)
            return r.doorwaysDown;

        }
        private void extractList(out List<Doorway> doors, out List<Direction> dirs, in List<Doorway> roomDoors, in Direction dir)
        {
            foreach(Doorway door in roomDoors)
            {
                doors.Add(door);
                dirs.Add(dir);
            }
        }
        private Room findRoomWith(Direction entranceDir, in Room[] roomPrefabs)
        {
            // this kind of sucks...
            int numRooms = roomPrefabs.Count;
            for(int i = 0; i < 100; i++) // prevent infinite iteration
            {
                int ind = Random.Range(0, numRooms);
                Room current = roomPrefabs[ind];
                bool hasDoorsThisWay = current.Any(x => x != null);
                if(hasDoorsThisWay)
                    return current;

            }
            Debug.Log("Incredibly rare, could not find a door. TODO: find a sane solution.");
            return null;
        }

    }
}

private enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}
