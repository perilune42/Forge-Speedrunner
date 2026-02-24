using UnityEngine;
using System;
using System.Collections.Generic;

public class MapGen : MonoBehaviour
{
    public IPathGenerator pathGen;
    public List<Room> createdRooms = new();
    public List<Passage> createdPassages;
    public GameObject PassPrefab; 
    public int pathSize;
    public int pathMin;

    [SerializeField] GameRegistry gameRegistry;

    void Awake()
    {

    }
    public (List<Room>, List<Passage>) CreateMap()
    {
        Room[] roomPrefabs = Array.ConvertAll(gameRegistry.RoomPrefabs, x => x.GetComponent<Room>());
        Room start = gameRegistry.StartRoom.GetComponent<Room>();
        Room finish = gameRegistry.FinishRoom.GetComponent<Room>();
        // RandomFromPoint pathGen = new RandomFromPoint(roomPrefabs, start, null); // end is kind of ignored for now
        // List<Cell> path = pathGen.Generate(pathSize);
        // passagesDebug = pathGen.RealizePath();
        // Debug.Log($"here we are. size: {path.Count}");
        // PathCreator pc = pathGen.Generate(pathSize);

        // PathCreator pc = new PathFactoryBuilder()
        //     .WithStartRoom(start)
        //     .WithMin(pathMin)
        //     .OnePath()
        //     // .WithAlgorithm(new MainPath(roomPrefabs), pathSize)
        //     .WithAlgorithm(new RandomChoice(roomPrefabs), pathSize)
        //     .WithAlgorithm(new BufferOption(roomPrefabs), 1)
        //     .WithAlgorithm(new PlaceFinal(finish), 1)
        //     .Finalize();

        Debug.Log("[MapGen] Up to random choice:");
        PathFactoryBuilder bld = new PathFactoryBuilder()
            .WithStartRoom(start)
            .WithMin(pathMin)
            .OnePath()
            .WithAlgorithm(new RandomChoice(roomPrefabs), pathSize);

        Debug.Log("[MapGen] Buffer option:");
        bld = bld.WithAlgorithm(new BufferOption(roomPrefabs), 1);

        Debug.Log("[MapGen] Place final:");
        bld = bld.WithAlgorithm(new PlaceFinal(finish), 1);


        PathCreator pc = bld.Finalize();

        pc.PassPrefab = this.PassPrefab;
        pc.RegisterParent(transform);

        (createdRooms, createdPassages) = pc.Create();
        // Debug.Log("[CreateMap] why create anything? i think we are just fine the way we are...");

        Transform AllPassages = transform.GetChild(0);
        foreach(Passage p in createdPassages)
        {
            p.gameObject.transform.SetParent(AllPassages);
        }

        return (createdRooms, createdPassages);


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
        foreach(Passage p in createdPassages)
        {
            DestroyImmediate(p.gameObject);
        }
        createdRooms = new();
        createdPassages = new();
    }
}
