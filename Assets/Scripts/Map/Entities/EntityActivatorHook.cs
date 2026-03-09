public class EntityActivatorHook : ActivatableEntity
{
    public Entity HookedEntity;

    public override bool IsSolid => false;

    protected override void Awake()
    {
        base.Awake();
        HookedEntity.enabled = false;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        HookedEntity.enabled = true;
    }

    public override void ResetEntity()
    {
        HookedEntity.enabled = false;
    }
}