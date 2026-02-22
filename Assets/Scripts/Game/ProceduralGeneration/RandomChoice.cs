using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
using System.Collections;
public class RandomChoice : IChoiceStrategy
{
    List<Room> RoomPrefabs;
    public RandomChoice(Room[] roomPrefabs)
    {
        this.RoomPrefabs = roomPrefabs.ToList();
    }

    public int SelectIndex(in List<Direction> dirs, in List<Offset> offs)
    {
        return Random.Range(0, dirs.Count);
    }

    public Room FindRoom(Direction dir, Offset off, in HashSet<Room> placedRooms)
    {
        if(RoomPrefabs.Count <= 0)
            return null;
        // this kind of sucks...
        for(int i = 0; i < 100; i++) // prevent infinite iteration
        {
            int numRooms = RoomPrefabs.Count;
            int ind = Random.Range(0, numRooms);
            Room current = RoomPrefabs[ind];
            if(placedRooms.Contains(current))
            {
                RoomPrefabs.RemoveAt(ind);
                continue;
            }
            Direction entranceDir = DirMethods.opposite(dir);
            List<Doorway> currentDoors = DirMethods.matchingDir(in entranceDir, in current);
            bool hasDoorsThisWay = currentDoors.Any(x => x != null && !x.Entrance);
            if(hasDoorsThisWay)
            {
                RoomPrefabs.RemoveAt(ind);
                return current;
            }
        }
        Debug.Log("Incredibly rare, could not find a door. TODO: find a sane solution.");
        return null;
    }
}
