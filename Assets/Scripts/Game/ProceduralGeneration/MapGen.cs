using UnityEngine;
using System;
using System.Collections.Generic;
using FMOD;
using Debug = UnityEngine.Debug;

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
    private Room[] roomPrefabs;
    private Room start;
    private Room finish;

    [SerializeField] GameRegistry gameRegistry;

    private void OnValidate()
    {
        roomPrefabs = Array.ConvertAll(gameRegistry.RoomPrefabs, x => x.GetComponent<Room>());
        start = gameRegistry.StartRoom.GetComponent<Room>();
        finish = gameRegistry.FinishRoom.GetComponent<Room>();
    }

    void Awake()
    {
        // OnValidate();
 
    }
    private PathFactoryBuilder defaultBuilder()
    {
        Debug.Log($"gr roobprefabs is null: {gameRegistry.RoomPrefabs == null}");
        var rooms = new List<Room>();
        foreach (var room in gameRegistry.RoomPrefabs)
        {
            rooms.Add(room.GetComponent<Room>());
        }
        roomPrefabs = rooms.ToArray();
        start = gameRegistry.StartRoom.GetComponent<Room>();
        finish = gameRegistry.FinishRoom.GetComponent<Room>();
        return new PathFactoryBuilder()
                .WithStartRoom(start)
                .WithMin(pathMin)
                .OnePath()
                .WithAlgorithmV2(new MainPath(roomPrefabs), pathSize)
                // .WithAlgorithm(new RandomChoice(roomPrefabs), pathSize)
                .WithAlgorithmV2(new BufferOptionNew(roomPrefabs), 1)
                .WithAlgorithmV2(new PlaceFinalNew(finish), 1);

    }
    private PathCreator runAlg()
    {
        PathCreator pc = defaultBuilder().FinalizeV2();
        pc.PassPrefab = this.PassPrefab;
        pc.RegisterParent(transform);
        return pc;
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
        (createdRooms, createdPassages) = pc.Create();
        Debug.Log($"Fail type: {fail}");
    }
    public (List<Room>, List<Passage>) CreateMap()
    {
        Room[] roomPrefabs = Array.ConvertAll(gameRegistry.RoomPrefabs, x => x.GetComponent<Room>());
        Room start = gameRegistry.StartRoom.GetComponent<Room>();
        Room finish = gameRegistry.FinishRoom.GetComponent<Room>();

        // inlined code from FinalizeUntilComplete(). Working here, not working in PathFactoryBuilder.
        PathCreator pc = null;
        int i = 1;
        Status pcStatus;
        do
        {
            Debug.Log($"[CreateMap] Finalize call {i++}");
            pc = runAlg();
            pcStatus = pc.Validate(finish, pathMin);
            Debug.Log($"status: {pcStatus}");
        } while(pcStatus != Status.ALL_CLEAR && i < 100);
        Debug.Log($"Created result of status {pcStatus} after {i} tries.");

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
