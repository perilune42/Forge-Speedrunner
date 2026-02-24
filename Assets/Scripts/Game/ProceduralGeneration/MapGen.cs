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
    public List<(PathCreator, Status)> FailedRunsDebug = new();
    public int NumFails = 0;
    public int CheckThis=0;

    [SerializeField] GameRegistry gameRegistry;

    void Awake()
    {

    }
    private PathFactoryBuilder defaultBuilder()
    {
        Room[] roomPrefabs = Array.ConvertAll(gameRegistry.RoomPrefabs, x => x.GetComponent<Room>());
        Room start = gameRegistry.StartRoom.GetComponent<Room>();
        Room finish = gameRegistry.FinishRoom.GetComponent<Room>();
        return new PathFactoryBuilder()
                .WithStartRoom(start)
                .WithMin(pathMin)
                .OnePath()
                // .WithAlgorithm(new MainPath(roomPrefabs), pathSize)
                .WithAlgorithm(new RandomChoice(roomPrefabs), pathSize)
                .WithAlgorithm(new BufferOption(roomPrefabs), 1)
                .WithAlgorithm(new PlaceFinal(finish), 1);

    }
    private PathCreator runAlg()
    {
        return defaultBuilder().Finalize();
    }
    private PathCreator runUntilCorrect()
    {
        return defaultBuilder().FinalizeUntilCorrect();
    }
    public void Test()
    {
        // test for placing final room
        FailedRunsDebug = new();
        for(int i = 0; i < 100; i++)
        {
            Debug.Log($"[TEST] test {i}:");
            PathCreator pc = runAlg();
            Status pcStatus = pc.Validate(finish, pathMin);
            if(pcStatus != Status.ALL_CLEAR)
                FailedRunsDebug.Add((pc, pcStatus));
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
        PathCreator pc; Status fail;
        (pc, fail) = FailedRunsDebug[ind];
        pc.PassPrefab = this.PassPrefab;
        pc.RegisterParent(transform);
        (createdRooms, createdPassages) = pc.Create();
        Debug.Log($"Fail type: {fail}");
    }
    public (List<Room>, List<Passage>) CreateMap()
    {
        Room[] roomPrefabs = Array.ConvertAll(gameRegistry.RoomPrefabs, x => x.GetComponent<Room>());
        Room start = gameRegistry.StartRoom.GetComponent<Room>();
        Room finish = gameRegistry.FinishRoom.GetComponent<Room>();

        pc = runUntilCorrect();

        (createdRooms, createdPassages) = pc.Create();

        Transform AllPassages = transform.GetChild(0);
        foreach(Passage p in createdPassages)
        {
            p.gameObject.transform.SetParent(AllPassages);
        }

        return (createdRooms, createdPassages);
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
