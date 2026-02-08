using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceField : Trigger 
{
    private bool playerInside = false;

    [SerializeField] Vector2 force;
    [SerializeField] float maxSpeed;
    [SerializeField] float gravityMultiplier = 1f;
    [SerializeField] float centering = 0;
    [SerializeField] float stabilization = 0;

    const float minCenteringDist = 0.25f;

    // if true, removes excess velocity that isn't in the direction of the boost
    protected override void Awake()
    {
        base.Awake();
    }

    private void FixedUpdate()
    {
        if (playerInside)
        {
            float fdt = Time.fixedDeltaTime;

            Vector2 vel = Player.Instance.Movement.Velocity;
            Vector2 fh = force.normalized;
            Vector2 pvProj = Util.Vec2Proj(vel, fh);
            Vector2 pvPerp = vel - pvProj;

            pvPerp -= pvPerp.normalized * (Mathf.Max(stabilization, pvPerp.magnitude)) * fdt;
            Player.Instance.Movement.Velocity = pvProj + pvPerp;
            vel = Player.Instance.Movement.Velocity;

            bool isVertical = force.y != 0;
            float midpoint;
            if (!isVertical)
            {
                midpoint = col.bounds.center.y;
                float d = midpoint - Player.Instance.Movement.transform.position.y;
                if (Mathf.Abs(d) > minCenteringDist) {
                    vel += Util.SignOr0(d) * centering * Vector2.up * fdt;
                    Debug.Log($"Applying centering force: {Util.SignOr0(d) * centering * Vector2.up * fdt}");
                }
                
            }
            else
            {
                midpoint = col.bounds.center.x;
                float d = midpoint - Player.Instance.Movement.transform.position.x;
                if (Mathf.Abs(d) > minCenteringDist)
                {
                    vel += Util.SignOr0(d) * centering * Vector2.right * fdt;
                }
            }

            Player.Instance.Movement.Velocity = vel;
            pvProj = Util.Vec2Proj(vel, fh);
            pvPerp = vel - pvProj;

            if (Vector2.Dot(vel, fh) > maxSpeed) return;
            vel += force * fdt;

            if (Vector2.Dot(vel, fh) > maxSpeed) {
                vel = Player.Instance.Movement.Velocity;
                vel = fh * maxSpeed + pvPerp;
            }


            Player.Instance.Movement.Velocity = vel;
        }
    }

    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        playerInside = true;
        Player.Instance.Movement.GravityMultiplier.Multipliers[StatSource.ForceFieldGravityMult] = gravityMultiplier;
    }

    public override void OnPlayerExit()
    {
        base.OnPlayerExit();
        playerInside = false;
        Player.Instance.Movement.GravityMultiplier.Multipliers[StatSource.ForceFieldGravityMult] = 1;
    }

    [ContextMenu("Set Visual")]
    public void SetVisual()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && col != null)
        {
            sr.transform.localScale = col.size;
        }

        ParticleSystem p = GetComponent<ParticleSystem>();
        
        if (p != null && col != null)
        {
            var shape = p.shape;
            shape.scale = col.size;

            var vol = p.velocityOverLifetime;
            vol.enabled = true;


            vol.y = new ParticleSystem.MinMaxCurve(force.y * 0.1f);
            vol.x = new ParticleSystem.MinMaxCurve(force.x * 0.1f);
        }
    }
}
