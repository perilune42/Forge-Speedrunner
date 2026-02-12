using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
