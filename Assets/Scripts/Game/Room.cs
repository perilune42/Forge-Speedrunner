using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Vector2Int gridPosition;
    public Vector2Int size;
    public List<Doorway> doorwaysUp;
    public List<Doorway> doorwaysDown;
    public List<Doorway> doorwaysLeft;
    public List<Doorway> doorwaysRight;
    [SerializeField] private Transform entitiesContainer;
    [HideInInspector] public List<Entity> Entities;

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
        foreach (var entity in entitiesContainer.GetComponentsInChildren<Entity>())
        {
            Entities.Add(entity);
        }
    }

    public void AddEntity(Entity newEntity)
    {
        newEntity.transform.SetParent(entitiesContainer, true);
        Entities.Add(newEntity);
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
    }
}
#endif