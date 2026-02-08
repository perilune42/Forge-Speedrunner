using UnityEngine;
public abstract class ActivatableEntity : Entity 
{
    [SerializeField] protected SpriteRenderer colorIndicator;

    public abstract void OnActivate();
    public abstract void ResetEntity();

    public void SetColor(Color color)
    {
        colorIndicator.color = color;
    }
}
