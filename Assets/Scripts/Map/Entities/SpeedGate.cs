using UnityEngine;

public class SpeedGate : ActivatableEntity
{
    private bool isBroken;
    public override bool IsSolid => !CanBreak(1);
    private Collider2D col;
    [SerializeField] private SpriteRenderer gateSr, indicatorSr;
    [SerializeField] private bool isHorizontal;
    [SerializeField] ParticleSystem breakParticles;

    public float SpeedRequirement = 20;



    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<Collider2D>();

    }
    public override void OnActivate()
    {
        Break();
    }
    // the reverse of OnActivate
    public override void ResetEntity()
    {
        isBroken = false;
        gateSr.enabled = true;
        indicatorSr.enabled = true;
        col.enabled = true;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isBroken) return;

        if (CanBreak(1))
        {
            indicatorSr.color = Color.green;
        }
        else if (CanBreak(0.75f)) 
        {
            indicatorSr.color = Color.yellow;        
        }
        else
        {
            indicatorSr.color = Color.red;
        }
    }

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);
        if (de is not PlayerMovement pm) return;
        Vector2 vel = pm.PreCollisionVelocity;
        float spd = Mathf.Abs(isHorizontal ? vel.x : vel.y);
        if (spd >= SpeedRequirement) {
            Break();
            pm.Velocity = vel;
        }
    }

    public void Break()
    {
        isBroken = true;
        gateSr.enabled = false;
        indicatorSr.enabled = false;
        col.enabled = false;
        breakParticles.Play();
    }

    private bool CanBreak(float mult = 1f)
    {
        Vector2 vel = Player.Instance.Movement.Velocity;
        float spd = Mathf.Abs(isHorizontal ? vel.x : vel.y);
        return spd >= SpeedRequirement * mult;
    }

}
