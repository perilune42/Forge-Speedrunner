public class EntityActivatorHook : ActivatableEntity
{
    public Entity HookedEntity;

    public override bool IsSolid => false;

    private void Awake()
    {
        HookedEntity.enabled = false;
    }

    public override void OnActivate()
    {
        HookedEntity.enabled = true;
    }

    public override void ResetEntity()
    {
        HookedEntity.enabled = false;
    }
}