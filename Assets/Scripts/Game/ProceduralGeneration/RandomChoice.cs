using UnityEngine;
using System.Collections.Generic;
using Offset = UnityEngine.Vector2Int;
using System.Linq;
public class RandomChoice : IRoomChoiceStrategy
{
    Room[] RoomPrefabs;
    public RandomChoice(Room[] roomPrefabs)
    {
        this.RoomPrefabs = roomPrefabs;
    }

    public Room FindRoom(Direction dir, Offset off)
    {
        // this kind of sucks...
        int numRooms = RoomPrefabs.Length;
        for(int i = 0; i < 100; i++) // prevent infinite iteration
        {
            int ind = Random.Range(0, numRooms);
            Room current = RoomPrefabs[ind];
            Direction entranceDir = DirMethods.opposite(dir);
            List<Doorway> currentDoors = DirMethods.matchingDir(in entranceDir, in current);
            bool hasDoorsThisWay = currentDoors.Any(x => x != null);
            if(hasDoorsThisWay)
                return current;
        }
        Debug.Log("Incredibly rare, could not find a door. TODO: find a sane solution.");
        return null;
    }
}
