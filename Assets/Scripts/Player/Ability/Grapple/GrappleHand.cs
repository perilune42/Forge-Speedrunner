using UnityEngine;

public class GrappleHand : DynamicEntity
{
    [HideInInspector] public Grapple Grapple;
    [SerializeField] private int lifetime;
    private bool attached = false;

    protected override void Awake()
    {
        base.Awake();
        OnHitWallAny += AttachToWall;
        GravityMultiplier = 0f;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (attached) return;
        lifetime--;
        if (lifetime <= 0)
        {
            Grapple.grappleState = GrappleState.Idle;
            Destroy(gameObject);
        }
    }


    private void AttachToWall(Entity entity)
    {
        if (entity) transform.SetParent(entity.transform, true);
        attached = true;
        Grapple.grappleState = GrappleState.Active;
        Velocity = Vector2.zero;
        CollisionsEnabled = false;
        Locked = true;
        Grapple.CreateGrappleArrow();
    }
}
