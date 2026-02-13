

using UnityEngine;

public class Drone : Entity
{
    public override bool IsSolid => false;
    PlayerMovement pm => Player.Instance.Movement;

    public int RechargeDuration = 60;
    private int rechargeTimer;

    [SerializeField] SpriteRenderer sr, indicatorSr;
    private bool active => rechargeTimer == 0;


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
            pm.CanJumpOverride = true;
            
        }
        pm.onJump += TryConsume;
        indicatorSr.enabled = true;
    }

    public override void OnPlayerExit()
    {
        base.OnPlayerExit();
        pm.CanJumpOverride = false;
        pm.onJump -= TryConsume;
        indicatorSr.enabled = false;
    }


    private void TryConsume()
    {
        if (!active || pm.CanJump(false) || pm.CanWallJump()) return;

        var grapple = AbilityManager.Instance.GetAbility<Grapple>();
        if (grapple != null && grapple.grappleState == GrappleState.Pulling) return;
        pm.onGround?.Invoke();


        rechargeTimer = RechargeDuration;
        pm.CanJumpOverride = false;
        sr.color = Color.white * 0.8f;
    }
    private void Recharge()
    {
        sr.color = Color.white;
    }
}