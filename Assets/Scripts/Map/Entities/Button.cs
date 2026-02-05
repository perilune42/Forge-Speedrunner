using System.Collections.Generic;
using UnityEngine;

public class Button : Entity 
{
    public override bool IsSolid => false;
    public List<ActivatableEntity> LinkedEntities = new();

    [SerializeField] Sprite unpressedSprite, pressedSprite;

    SpriteRenderer sr;

    protected override void Awake()
    {
        base.Awake();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);
        foreach (var entity in LinkedEntities)
        {
            entity.OnActivate();
        }
        sr.sprite = pressedSprite;
    }
}