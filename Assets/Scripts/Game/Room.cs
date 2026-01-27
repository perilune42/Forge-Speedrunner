using UnityEngine;
using System.Collections.Generic;

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
