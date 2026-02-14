using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class WallLatch : Ability
{

    PlayerMovement pm => Player.Instance.Movement;

    Vector2 latchedDirection = Vector2.zero;
    [SerializeField] float inwardBoost = 6f, outwardBoost = 9f;
    [SerializeField] float verticalSpeed = 14f;

    protected override bool CanRecharge()
    {
        // cannot recharge while latched
        return latchedDirection == Vector2.zero;
    }

    public override void Start()
    {
        base.Start();


        pm.OnSpecialStateChange += (newState) =>
        {
            if (latchedDirection != Vector2.zero)
            {
                CancelLatch(false);
            }

        };
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (inputButton.HasPressed && CanUseAbility())
        {
            UseAbility();
        }
        if (PInput.Instance.Jump.HasPressed && latchedDirection != Vector2.zero)
        {
            LatchJump();
        }
    }


    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool UseAbility()
    {
        base.UseAbility();
        StartLatch();
        return true;
    }

    private void StartLatch()
    {
        pm.SpecialState = SpecialState.WallLatch;
        Vector2 dir;
        if (pm.IsTouching(pm.FacingDir))
        {
            dir = pm.FacingDir;
        }
        else
        {
            dir = -pm.FacingDir;
            if (!pm.IsTouching(dir)) Debug.LogWarning("Invalid latch activation");
        }
        pm.Locked = true;
        latchedDirection = dir;
        pm.onGround?.Invoke();
    }

    private void LatchJump()
    {
        bool instantRecharge = false;
        if (pm.CanWallClimb(latchedDirection, true))
        {
            instantRecharge = true;
        }
        pm.Jump();
        pm.Velocity.y = verticalSpeed;
        if (pm.FacingDir == latchedDirection)
        {
            pm.Velocity.x = -pm.FacingDir.x * inwardBoost;
            Debug.Log(pm.Velocity.x);
        }
        else
        {
            
            pm.Velocity.x = pm.FacingDir.x * outwardBoost;
        }
        CancelLatch();
        if (instantRecharge) Recharge();
    }

    private void CancelLatch(bool setState = true)
    {
        if (setState)
        {
            pm.SpecialState = SpecialState.Normal;
        }
        pm.Locked = false;
        latchedDirection = Vector2.zero;
    }

    public override bool CanUseAbility()
    {
        return (pm.SpecialState != SpecialState.WallLatch)
            && pm.State == BodyState.InAir && (pm.IsTouching(Vector2.left) || pm.IsTouching(Vector2.right)) && base.CanUseAbility();
    }


}