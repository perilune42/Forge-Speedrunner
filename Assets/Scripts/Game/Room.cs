using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Room : MonoBehaviour
{
    [HideInInspector] public int RoomID;
    public Vector2Int gridPosition;
    public Vector2Int size;
    public List<Doorway> doorwaysUp;
    public List<Doorway> doorwaysDown;
    public List<Doorway> doorwaysLeft;
    public List<Doorway> doorwaysRight;
    [SerializeField] private Transform entitiesContainer;
    [HideInInspector] public List<Entity> Entities;
    [HideInInspector] public bool visited = false;

    public bool isChallengeRoom = false;

    private void OnValidate()
    {
        doorwaysUp = keepValid(doorwaysUp, false);
        doorwaysDown = keepValid(doorwaysDown, false);
        doorwaysLeft = keepValid(doorwaysLeft, true);
        doorwaysRight = keepValid(doorwaysRight, true);
        for(int i = doorwaysUp.Count; i < size.x; i++)
            doorwaysUp.Add(null);
        for(int i = doorwaysDown.Count; i < size.x; i++)
            doorwaysDown.Add(null);
        for(int i = doorwaysLeft.Count; i < size.y; i++)
            doorwaysLeft.Add(null);
        for(int i = doorwaysRight.Count; i < size.y; i++)
            doorwaysRight.Add(null);
    }

    private List<Doorway> keepValid(List<Doorway> ld, bool isLR)
    {
        var list = new List<Doorway>();
        int maxSize = isLR ? size.y : size.x;
        for (int i = 0; i < maxSize; i++)
        {
            if (i >= ld.Count) break;
            list.Add(ld[i]);
        }
        return list;
    }

    private void Awake()
    {
        foreach (var entity in GetComponentsInChildren<Entity>())
        {
            Entities.Add(entity);
        }
    }

    public void AddEntity(Entity newEntity)
    {
        newEntity.transform.SetParent(entitiesContainer, true);
        Entities.Add(newEntity);
    }

    public RoomBounds GetBounds()
    {
        var bounds = new RoomBounds();
        bounds.min = gridPosition;
        bounds.max = gridPosition + size;
        return bounds;
    }

    public List<Doorway> GetAllDoorways()
    {
        List<Doorway> list = new List<Doorway>(doorwaysLeft);
        list.AddRange(doorwaysRight);
        list.AddRange(doorwaysUp);
        list.AddRange(doorwaysDown);
        return list.Where(d => d != null).ToList();
    }

}

public struct RoomBounds
{
    public Vector2Int min, max;
    public bool Contains(Vector2Int pos)
    {
        return pos.x >= min.x && pos.x < max.x && pos.y >= min.y && pos.y < max.y;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Room))]
public class Room_Inspector : Editor
{
    override public void OnInspectorGUI()
    {

        Room room = (Room)target;
        RoomManager rm = FindAnyObjectByType<RoomManager>();
        DrawDefaultInspector();

        if (GUILayout.Button("Place In Map"))
        {
            room.transform.SetParent(rm.transform, true);
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(room.transform.position.x / 1.2f / rm.BaseWidth),
                                                 Mathf.RoundToInt(room.transform.position.y / 1.2f / rm.BaseHeight));
            room.gridPosition = gridPos;
            Vector3 roomPos = room.transform.position;
            roomPos.x = room.gridPosition.x * rm.BaseWidth * 1.2f;
            roomPos.y = room.gridPosition.y * rm.BaseHeight * 1.2f;
            room.transform.position = roomPos;
            rm.AllRooms.Add(room);
            foreach (Entity e in room.Entities)
            {
                e.OnValidate();
            }
            EditorUtility.SetDirty(room);
        }

        if (GUILayout.Button("Assign Doorways"))
        {
            foreach (var door in room.GetComponentsInChildren<Doorway>())
            {
                door.enclosingRoom = room;
                EditorUtility.SetDirty(room);
            }
        }
        
    }
}
#endif