

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
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        GrappleHand gh = collision.GetComponent<GrappleHand>();
        if (gh != null)
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
        indicatorSr.enabled = true;
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
        pm.Jump();
        PInput.Instance.Jump.ConsumeBuffer();
        pm.Velocity.y += ExtraJumpBoost;
        pm.onGround?.Invoke();
        AbilityManager.Instance.GetAbility<Dash>().Recharge();

        rechargeTimer = RechargeDuration;
        canJumpNow = false;
        sr.color = Color.white * 0.5f;
        jumpParticles.Play();
        return true;
    }
    private void Recharge()
    {
        sr.color = Color.white;
    }
}