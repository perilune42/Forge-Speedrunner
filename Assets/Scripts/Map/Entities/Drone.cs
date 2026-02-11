

using UnityEngine;

public class Drone : Entity
{
    public override bool IsSolid => false;
    PlayerMovement pm => Player.Instance.Movement;

    public int RechargeDuration = 60;
    private int rechargeTimer;

    [SerializeField] SpriteRenderer sr;
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

    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (active)
        {
            pm.CanJumpOverride = true;
            
        }
        pm.onJump += TryConsume;
    }

    public override void OnPlayerExit()
    {
        base.OnPlayerExit();
        pm.CanJumpOverride = false;
        pm.onJump -= TryConsume;
    }


    private void TryConsume()
    {
        if (!active) return;
        rechargeTimer = RechargeDuration;
        pm.CanJumpOverride = false;
        sr.color = Color.white * 0.8f;
    }
    private void Recharge()
    {
        Debug.Log("Recharged");
        sr.color = Color.white;
    }
}