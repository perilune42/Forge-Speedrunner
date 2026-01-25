using System.Collections.Generic;
using UnityEngine;

public class Button : Entity 
{
    public override bool IsSolid => false;
    public List<ActivatableEntity> LinkedEntities = new();

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);
        foreach (var entity in LinkedEntities)
        {
            entity.OnActivate();
        }
    }
}