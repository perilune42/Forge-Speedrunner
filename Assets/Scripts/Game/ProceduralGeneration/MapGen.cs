using UnityEngine;
using System;
using System.Collections.Generic;

public enum FailType
{
    NO_FIN,
    UNDER_MIN,
    DEAD_ENDS,
}

public class MapGen : MonoBehaviour
{
    public IPathGenerator pathGen;
    public List<Room> createdRooms = new();
    public List<Passage> createdPassages;
    public GameObject PassPrefab; 
    public int pathSize;
    public int pathMin;
    public List<(PathCreator, FailType)> FailedRunsDebug = new();
    public int NumFails = 0;
    public int CheckThis=0;

    [SerializeField] GameRegistry gameRegistry;

    void Awake()
    {

    }
    public void Test()
    {
        // test for placing final room
        FailedRunsDebug = new();
        Room[] roomPrefabs = Array.ConvertAll(gameRegistry.RoomPrefabs, x => x.GetComponent<Room>());
        Room start = gameRegistry.StartRoom.GetComponent<Room>();
        Room finish = gameRegistry.FinishRoom.GetComponent<Room>();
        for(int i = 0; i < 100; i++)
        {
            Debug.Log($"[TEST] test {i}:");
            PathCreator pc = new PathFactoryBuilder()
                .WithStartRoom(start)
                .WithMin(pathMin)
                .OnePath()
                // .WithAlgorithm(new MainPath(roomPrefabs), pathSize)
                .WithAlgorithm(new RandomChoice(roomPrefabs), pathSize)
                .WithAlgorithm(new BufferOption(roomPrefabs), 1)
                .WithAlgorithm(new PlaceFinal(finish), 1)
                .Finalize();
            Room r = pc.Cells[pc.Cells.Count-1].room;
            if(r != finish)
                FailedRunsDebug.Add((pc, FailType.NO_FIN));
            if(pc.Cells.Count < pathMin)
                FailedRunsDebug.Add((pc, FailType.UNDER_MIN));

            Dictionary<Vector2Int, int> numNeighbors = new();
            foreach(Cell c in pc.Cells)
                numNeighbors.Add(c.offset, 0);

            foreach(Connection conn in pc.Connections)
            {
                numNeighbors[conn.Source.offset] += 1;
            }
            foreach(int num in numNeighbors.Values)
                if(num > 2)
                {
                    FailedRunsDebug.Add((pc, FailType.DEAD_ENDS));
                    break;
                }
        }
        NumFails = FailedRunsDebug.Count;
    }
    public void CreateFailedTest(int ind)
    {
        if(ind < 0 || ind >= FailedRunsDebug.Count)
        {
            Debug.Log("Can't create this one! out of bounds.");
            return;
        }
        PathCreator pc; FailType fail;
        (pc, fail) = FailedRunsDebug[ind];
        (createdRooms, createdPassages) = pc.Create();
        Debug.Log($"Fail type: {fail}");
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
