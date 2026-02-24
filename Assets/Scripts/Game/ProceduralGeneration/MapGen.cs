using UnityEngine;
using System;
using System.Collections.Generic;

public class MapGen : MonoBehaviour
{
    public IPathGenerator pathGen;
    public List<Room> createdRooms = new();
    public List<Passage> passagesDebug;
    public GameObject PassPrefab; 
    public int pathSize;
    public int pathMin;

    [SerializeField] GameRegistry gameRegistry;

    void Awake()
    {

    }
    public void CreateMap()
    {
        Room[] roomPrefabs = Array.ConvertAll(gameRegistry.RoomPrefabs, x => x.GetComponent<Room>());
        Room start = gameRegistry.StartRoom.GetComponent<Room>();
        Room finish = gameRegistry.FinishRoom.GetComponent<Room>();
        // RandomFromPoint pathGen = new RandomFromPoint(roomPrefabs, start, null); // end is kind of ignored for now
        // List<Cell> path = pathGen.Generate(pathSize);
        // passagesDebug = pathGen.RealizePath();
        // Debug.Log($"here we are. size: {path.Count}");
        // PathCreator pc = pathGen.Generate(pathSize);

        PathCreator pc = new PathFactoryBuilder()
            .WithStartRoom(start)
            .WithMin(pathMin)
            .WithAlgorithm(new MainPath(roomPrefabs), pathSize)
            .WithAlgorithm(new BufferOption(roomPrefabs), 1)
            .WithAlgorithm(new PlaceFinal(finish), 1)
            .Finalize();

        pc.PassPrefab = this.PassPrefab;
        pc.RegisterParent(transform);

        (createdRooms, passagesDebug) = pc.Create();
        // Debug.Log("[CreateMap] why create anything? i think we are just fine the way we are...");

        Transform AllPassages = transform.GetChild(0);
        foreach(Passage p in passagesDebug)
        {
            p.gameObject.transform.SetParent(AllPassages);
        }

        // foreach(Cell c in path)
        // {
        //     Vector3 screenPosition = new(c.offset.x, c.offset.y, 0F);
        //     screenPosition *= 100F;
        //     Room room = c.room;

        //     Room realRoom = (Room)Instantiate(room, screenPosition, Quaternion.identity);
        //     createdRooms.Add(realRoom);
        //     realRoom.gridPosition = c.offset;
        //     realRoom.transform.SetParent(transform);
        // }
    }
    public void DeleteMap()
    {
        foreach(Room r in createdRooms)
        {
            DestroyImmediate(r.gameObject);
        }
        foreach(Passage p in passagesDebug)
        {
            DestroyImmediate(p.gameObject);
        }
        createdRooms = new();
        passagesDebug = new();
    }
}
