using UnityEngine;

public class GrappleHand : DynamicEntity
{
    [HideInInspector] public Grapple Grapple;
    [SerializeField] private int lifetime;
    private bool attached = false;
    [SerializeField] private LineRenderer lineRenderer;
    private float width;
    private float alpha;
    private Color color;
    protected override void Awake()
    {
        base.Awake();
        OnHitWallAny += AttachToWall;
        GravityMultiplier = 0f;
        lineRenderer.enabled = false;
        width = lineRenderer.startWidth;
        alpha = lineRenderer.startColor.a;
        color = lineRenderer.startColor;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (attached) 
        {
            lineRenderer.SetPosition(0, Player.Instance.Movement.GetCenterPos());
            lineRenderer.SetPosition(1, transform.position);
            return;
        };
        lifetime--;
        if (lifetime <= 0)
        {
            Grapple.grappleState = GrappleState.Idle;
            Destroy(gameObject);
        }
    }

    public void ApplyChargeVFX(float charge)
    {
        lineRenderer.startWidth = width * charge;
        lineRenderer.endWidth = width * charge;
        lineRenderer.startColor = new Color(color.r, color.g, color.b, alpha * charge);
        lineRenderer.endColor = new Color(color.r, color.g, color.b, alpha * charge);
    }


    private void AttachToWall(Entity entity)
    {
        if (entity) transform.SetParent(entity.transform, true);
        lineRenderer.enabled = true;
        attached = true;
        Grapple.grappleState = GrappleState.Active;
        Velocity = Vector2.zero;
        CollisionsEnabled = false;
        Locked = true;
        //Grapple.CreateGrappleArrow();
    }
}
