using System.Collections;
using UnityEngine;


public class Ricochet : Ability
{
    [HideInInspector] public bool active;
    [SerializeField] private int duration;
    [SerializeField] private float velocityMultiplierOnBounce;
    private int curDuration;
    public override void Start()
    {
        base.Start();
        PlayerMovement.onGround += OnGround;
        PlayerMovement.onGroundTop += OnGroundTop;
        PlayerMovement.OnHitWallLeft += OnHitWallLeft;
        PlayerMovement.OnHitWallRight += OnHitWallRight;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (active)
        {
            curDuration--;
            if (curDuration <= 0) 
            { 
                active = false;
                PlayerMovement.CanClimb = true;
            }
        }
        else if (inputButton.HasPressed) UseAbility();
    }

    public override bool CanUseAbility()
    {
        return base.CanUseAbility() && !active;
    }

    private void OnGround()
    {
        if (!active) return;
        PlayerMovement.Velocity.y = Mathf.Abs(PlayerMovement.PreCollisionVelocity.y) * velocityMultiplierOnBounce;
    }

    private void OnGroundTop()
    {
        if (!active) return;
        PlayerMovement.Velocity.y = -Mathf.Abs(PlayerMovement.PreCollisionVelocity.y) * velocityMultiplierOnBounce;
    }

    private void OnHitWallLeft()
    {
        if (!active) return;
        PlayerMovement.Velocity.x = Mathf.Abs(PlayerMovement.PreCollisionVelocity.x) * velocityMultiplierOnBounce;
    }

    private void OnHitWallRight()
    {
        if (!active) return;
        PlayerMovement.Velocity.x = -Mathf.Abs(PlayerMovement.PreCollisionVelocity.x) * velocityMultiplierOnBounce;
    }

    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;
        active = true;
        curDuration = duration;
        base.UseAbility();
        PlayerMovement.CanClimb = false;
        return true;
    }
}
