using UnityEngine;
public abstract class ActivatableEntity : Entity 
{
    [SerializeField] protected SpriteRenderer colorIndicator;

    public virtual void OnActivate()
    {
        if (AbilityManager.Instance.TryGetAbility<Chronoshift>(out Chronoshift chronoshift))
        {
            Debug.Log("plz");
            if (chronoshift.CanTeleport && !chronoshift.EntitiesToReset.Contains(this)) chronoshift.EntitiesToReset.Add(this);
        }
    }
    public abstract void ResetEntity();

    public void SetColor(Color color)
    {
        if (colorIndicator == null) return;
        colorIndicator.color = color;
    }
}
