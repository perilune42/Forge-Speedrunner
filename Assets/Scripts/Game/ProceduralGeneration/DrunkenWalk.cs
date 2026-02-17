// using UnityEngine;
// using System.Collections.Generic;
// using System.Collections;
// using System.Linq;
// using static Direction;

// public class DrunkenWalk : IPathGenerator
// {
//     public List<Cell> Generate(int pathLength)
//     {
//         GameObject[] rawRoomPrefabs = GameRegistry.Instance.RoomPrefabs;
//         Room[] roomPrefabs = rawRoomPrefabs
//             .Select(go => go.GetComponent<Room>())
//             .ToArray();
//         Room startRoom = GameRegistry.Instance.StartRoom.GetComponent<Room>();
//         int count = pathLength;

//         GenState state = new GenState();

//         Dictionary<Vector2Int, Cell> grid = new();
//         List<Cell> uniqueCells = new();

//         // initialize data structures for start room
//         Debug.Log("entering init");
//         {
//             Cell startCell = new Cell(startRoom, new Vector2Int(0,0));
//             uniqueCells.Add(startCell);
//             for(int i = 0; i < startRoom.size.x; i++)
//                 for(int j = 0; j < startRoom.size.y; j++)
//             {
//                 grid.Add(new Vector2Int(i,j), startCell);
//             }
//             state.extractAll(startRoom, new Vector2Int(0,0));
//         }
//         Debug.Log("exiting init");

//         while(pathLength-- > 0 && state.NotEmpty())
//         {
//             // Debug.Log($"loop start. iteration {iteration_count}");
//             Doorway door; Direction dir; Vector2Int offset;

//             // take random
//             (door, dir, offset) = state.PopRandom();

//             // check if offset is clear. throw away if not
//             // TODO: this code does not do what it's supposed to. find the bottom left instead.
//             Debug.Log("before choice to skip");
//             Vector2Int entryOffset = DirMethods.calcOffset(offset, dir);
//             if(grid.ContainsKey(entryOffset)) {
//                 Debug.Log("choice to skip");
//                 continue;
//             }
//             Debug.Log("choice not to skip");

//             // find room
//             Direction roomEntranceDir = DirMethods.opposite(dir);
//             Room newRoom = findRoomWith(roomEntranceDir, roomPrefabs);
//             List<Doorway> relevantDoorways = DirMethods.matchingDir(roomEntranceDir, newRoom);

//             // furthest left possible bottom left point
//             // NOTE: in the future, checkOffset will be updated to slot the rooms properly
//             Vector2Int checkOffset = entryOffset;
//             if(dir == RIGHT)
//             {
//                 checkOffset.x -= newRoom.size.x;
//             }
//             if(dir == UP)
//             {
//                 checkOffset.y -= newRoom.size.y;
//             }
//             Debug.Log($"checkOffset = {checkOffset}");

//             bool valid = true;
//             for(int i = checkOffset.x;
//                     valid && i < checkOffset.x + newRoom.size.x;
//                     i++)
//             for(int j = checkOffset.y;
//                     valid && j < checkOffset.y + newRoom.size.y;
//                     j++)
//             {
//                 if(grid.ContainsKey(new Vector2Int(i,j)))
//                     valid = false;
//             }
//             Debug.Log($"valid: {valid}");


//             // if room cannot be placed at this offset, pick a new room
//             // TODO
//             if(!valid)
//             {
//                 // check every possible offset
//                 continue; // just ignore if impossible for now
//             }

//             // add appropriate occupied slots
//             // TODO: this code is not correct. find the bottom left correctly.
//             Vector2Int newBotLeft = checkOffset;
//             Vector2Int newTopRight = newBotLeft + newRoom.size;

//             // add appropriate cells
//             Cell newCell = new Cell(newRoom, newBotLeft);
//             uniqueCells.Add(newCell);
//             // NOTE: this does not properly set `up,down,left,right`.
//             // might be useful to fix later
//             for(int i = newBotLeft.x; i < newTopRight.x; i++)
//                 for(int j = newBotLeft.y; j < newTopRight.y; j++)
//             {
//                 grid.Add(new Vector2Int(i, j), newCell);
//             }
//             Debug.Log("added all");

//             // take from room one doorway list at a time
//             // NOTE: a previous check will always ignore invalid options.
//             state = state.extractAll(newRoom, newBotLeft);
//             Debug.Log("extracted all from newRoom.");
//         }
//         Debug.Log("end");
//         return uniqueCells;
//     }

//     private Room findRoomWith(Direction entranceDir, in Room[] roomPrefabs)
//     {
//         // this kind of sucks...
//         int numRooms = roomPrefabs.Length;
//         for(int i = 0; i < 100; i++) // prevent infinite iteration
//         {
//             int ind = Random.Range(0, numRooms);
//             Room current = roomPrefabs[ind];
//             List<Doorway> currentDoors = DirMethods.matchingDir(in entranceDir, in current);
//             bool hasDoorsThisWay = currentDoors.Any(x => x != null);
//             if(hasDoorsThisWay)
//                 return current;
//         }
//         Debug.Log("Incredibly rare, could not find a door. TODO: find a sane solution.");
//         return null;
//     }
// }
