using UnityEngine;

public class GrappleHand : DynamicEntity
{
    [HideInInspector] public Grapple Grapple;
    private int lifetime;
    private bool attached = false;
    [SerializeField] private LineRenderer lineRenderer;
    private float width;
    private float alpha;
    private Color color;
    [SerializeField] Color fullyChargedColor;
    [SerializeField] private Sprite attachedSprite;

    public Collider col;
    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<Collider>();
        OnHitWallAny += AttachToWall;
        GravityEnabled = false;
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
            Grapple.Abort();
        }
    }

    public void SetLifetime(int lifetime)
    {
        this.lifetime = lifetime;
    }

    public void ApplyChargeVFX(float charge, bool fullyCharged)
    {
        lineRenderer.startWidth = width * charge;
        lineRenderer.endWidth = width * charge;

        Color color = this.color;
        if (fullyCharged) color = fullyChargedColor;

        lineRenderer.startColor = new Color(color.r, color.g, color.b, alpha * charge);
        lineRenderer.endColor = new Color(color.r, color.g, color.b, alpha * charge);
    }


    public void AttachToWall(Entity entity, Vector2 direction)
    {
        if (entity) transform.SetParent(entity.transform, true);
        lineRenderer.enabled = true;
        attached = true;
        Velocity = Vector2.zero;
        CollisionsEnabled = false;
        Locked = true;
        Grapple.Attach(direction);
        GetComponent<SpriteRenderer>().sprite = attachedSprite;
        //Grapple.CreateGrappleArrow();
    }
}
