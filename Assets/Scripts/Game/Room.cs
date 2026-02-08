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
        doorwaysUp = keepValid(doorwaysUp);
        doorwaysDown = keepValid(doorwaysDown);
        doorwaysLeft = keepValid(doorwaysLeft);
        doorwaysRight = keepValid(doorwaysRight);
        for(int i = doorwaysUp.Count; i < size.x; i++)
            doorwaysUp.Add(null);
        for(int i = doorwaysDown.Count; i < size.x; i++)
            doorwaysDown.Add(null);
        for(int i = doorwaysLeft.Count; i < size.y; i++)
            doorwaysLeft.Add(null);
        for(int i = doorwaysRight.Count; i < size.y; i++)
            doorwaysRight.Add(null);
    }

    private List<Doorway> keepValid(List<Doorway> ld)
    {
        return ld.Where(i => i != null).ToList();
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
