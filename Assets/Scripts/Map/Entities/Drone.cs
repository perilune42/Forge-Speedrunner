

using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;

public class Drone : Entity
{
    public override bool IsSolid => false;
    PlayerMovement pm => Player.Instance.Movement;

    public int RechargeDuration = 60;
    public int ExtraJumpBoost = 4;
    private int rechargeTimer;

    [SerializeField] SpriteRenderer sr, indicatorSr;
    private bool active => rechargeTimer == 0;
    private bool canJumpNow = false;

    [SerializeField] ParticleSystem jumpParticles;
    Animator animator;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (AbilityManager.Instance.TryGetAbility<Parry>(out Parry parry)) parry.OnPrimeParry += () =>
        {
            if (canJumpNow && pm.State == BodyState.InAir) TryConsume();
        };

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (rechargeTimer > 0)
        {
            rechargeTimer--;
            if (rechargeTimer == 0)
            {
                Recharge();
            }
        }

        if (active && canJumpNow)
        {
            if (PInput.Instance.Jump.HasPressed)
            {
                TryConsume();
            }
        }

        if (active && canJumpNow)
        {
            animator.Play("DroneActive");
        }
        else if (active)
        {
            animator.Play("DroneIdle");
        }
        else
        {
            animator.Play("DroneInactive");
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        GrappleHand gh = collision.GetComponent<GrappleHand>();
        if (active && gh != null)
        {
            CaptureGrapple(gh);
        }
    }

    public void CaptureGrapple(GrappleHand gh)
    {
        gh.transform.position = transform.position;
        gh.AttachToWall(this, gh.Grapple.LastThrowDirection);
    }

    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (active)
        {
            canJumpNow = true;
        }

    }

    public override void OnPlayerExit()
    {
        base.OnPlayerExit();
        canJumpNow = false;
        indicatorSr.enabled = false;
    }


    private bool TryConsume()
    {
        if (!active) return false;

        var grapple = AbilityManager.Instance.GetAbility<Grapple>();
        if (grapple != null && grapple.grappleState == GrappleState.Pulling) return false;
        if (AbilityManager.Instance.TryGetAbility<Parry>(out Parry parry))
        {
            if (parry.ParryPrimed)
            {
                pm.PreCollisionVelocity = pm.Velocity;
                parry.CollideWithWall(this, Vector2.Normalize(-pm.Velocity));
                parry.SpecialEntity = this;
                return false;
            }
        }
        pm.Jump();
        pm.Velocity.y += ExtraJumpBoost;
        PInput.Instance.Jump.ConsumeBuffer();
        Consume();

        return true;
    }

    public void Consume()
    {
        
        pm.onGround?.Invoke();
        AbilityManager.Instance.GetAbility<Dash>().Recharge();

        rechargeTimer = RechargeDuration;
        canJumpNow = false;
        sr.color = Color.white * 0.8f;
        jumpParticles.Play();

        RuntimeManager.PlayOneShotAttached("event:/Drone Jumped on", gameObject);
    }
    private void Recharge()
    {
        sr.color = Color.white;
    }
}