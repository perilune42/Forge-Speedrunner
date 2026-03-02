using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class Button : ActivatableEntity 
{
    public override bool IsSolid => false;
    public List<ActivatableEntity> LinkedEntities = new();

    [SerializeField] Sprite unpressedSprite, pressedSprite;

    SpriteRenderer sr;
    bool hasPressed = false;

    protected override void Awake()
    {
        base.Awake();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public override void OnValidate()
    {
        base.OnValidate();
        SetLinkedColors();
    }

    private void SetLinkedColors()
    {
        if (colorIndicator != null)
        {
            foreach (var entity in LinkedEntities)
            {
                if (entity != null)
                {
                    entity.SetColor(colorIndicator.color);
                }
            }
        }
    }

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);
        foreach (var entity in LinkedEntities)
        {
            entity.OnActivate();
        }
        if (!hasPressed)
        {
            sr.sprite = pressedSprite;
            RuntimeManager.PlayOneShotAttached("event:/Button press", gameObject);
            hasPressed = true;
        }
        
    }

    public override void OnActivate()
    {
        base.OnActivate();
    }

    public override void ResetEntity()
    {
        sr.sprite = unpressedSprite;
        hasPressed = false;
    }
}