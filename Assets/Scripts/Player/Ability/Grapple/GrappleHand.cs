using UnityEngine;

public class GrappleHand : DynamicEntity
{
    [HideInInspector] public Grapple Grapple;

    protected override void Awake()
    {
        OnHitWallAny += AttachToWall;
    }

    private void AttachToWall(Entity entity)
    {
        if (entity) transform.SetParent(entity.transform, true);
        Grapple.grappleState = GrappleState.Active;
        Velocity = Vector2.zero;
        CollisionsEnabled = false;
        Locked = true;
        Grapple.CreateGrappleArrow();
    }
}
