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

        // sane random!
        List<int> inds = Enumerable.Range(0, RoomPrefabs.Count).ToList();
        inds.Shuffle();

        foreach(int ind in inds)
        {
            Room current = RoomPrefabs[ind];
            if(placedRooms.Contains(current))
            {
                RoomPrefabs.RemoveAt(ind);
                continue;
            }
            Direction entranceDir = DirMethods.opposite(dir);
            List<Doorway> currentDoors = DirMethods.matchingDir(in entranceDir, in current);
            bool hasDoorsThisWay = currentDoors.Any(x => x != null && x.IsEntrance());
            if(hasDoorsThisWay)
            {
                RoomPrefabs.RemoveAt(ind);
                return current;
            }
        }
        Debug.Log("[FindRoom] Could not find a door. There might not be any that fit!");
        return null;
    }
}
