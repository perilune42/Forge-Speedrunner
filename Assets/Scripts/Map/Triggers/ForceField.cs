using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceField : Trigger 
{
    private bool playerInside = false;

    [SerializeField] Vector2 force;
    [SerializeField] float maxSpeed;

    protected override void Awake()
    {
        base.Awake();
    }

    private void FixedUpdate()
    {
        if (playerInside)
        {
            Vector2 vel = Player.Instance.Movement.Velocity;
            Vector2 fh = force.normalized;
            if (Vector2.Dot(vel, fh) > maxSpeed) return;
            vel += force * Time.fixedDeltaTime;
            if (Vector2.Dot(vel, fh) > maxSpeed) {
                vel = Player.Instance.Movement.Velocity;
                Vector2 pvProj = Util.Vec2Proj(vel, fh);
                Vector2 pvPerp = vel - pvProj;
                vel = fh * maxSpeed + pvPerp;

            }
            Player.Instance.Movement.Velocity = vel;
        }
    }

    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        playerInside = true;
    }

    public override void OnPlayerExit()
    {
        base.OnPlayerExit();
        playerInside = false;
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
