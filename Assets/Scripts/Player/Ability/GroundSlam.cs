using System;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSlam : Ability
{
    [SerializeField] private float initialVelocity;
    private int rampUpTime;
    [SerializeField] private float rampUpAcceleration;

    [SerializeField] private float heightConversion;

    public bool wasSlammingBeforeDash;

    private Vector2 preservedVelocity;

    public override void Start()
    {
        base.Start();
        PlayerMovement.onGround += () =>
        {
            if (PlayerMovement.SpecialState == SpecialState.GroundSlam
                /* or the ground slam is level 2 and player recently dashed*/) OnGround();
        };
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // check if we are mid dash while slamming so that we can keep adding rampUpTime when we are dashing
        bool isMidDashWhileSlamming = false;
        if (wasSlammingBeforeDash && PlayerMovement.SpecialState == SpecialState.Dash)
        {
            isMidDashWhileSlamming = true;
        }

        if (PlayerMovement.SpecialState == SpecialState.GroundSlam || isMidDashWhileSlamming)
        {
            rampUpTime++;
            // If we are currently dashing, then don't apply slam velocity down yet
            if (PlayerMovement.SpecialState == SpecialState.GroundSlam)
            {
                PlayerMovement.Velocity += Vector2.down * rampUpAcceleration;
            }
        }
        if (PInput.Instance.GroundSlam.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();
    }

    // returns: whether dash was successfully interrupted
    public bool DashInterrupt()
    {
        // if groundslam is level 1 and we are groundslamming, we cannot dash
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam && Level == 1)
        {
            return false;
        }
        // check if we are currently groundslamming and groundSlam is level 2
        // if so, we set the flag of wasSlammingBeforeDash to true
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam && Level >= 2)
        {
            wasSlammingBeforeDash = true;
            preservedVelocity = Player.Instance.Movement.Velocity;
            return true;
        }
        return false;
    }
    public void ContinueSlam()
    {
        PlayerMovement.SpecialState = SpecialState.GroundSlam;
        wasSlammingBeforeDash = false;
        Player.Instance.Movement.Velocity = preservedVelocity;
    }

    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool UseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.Dash) AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        PlayerMovement.Velocity = Vector2.down * initialVelocity;
        PlayerMovement.SpecialState = SpecialState.GroundSlam;
        rampUpTime = 0;
        base.UseAbility();
        return true;
    }

    public override bool CanUseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam) return false;
        if (PlayerMovement.State != BodyState.InAir) return false;
        return base.CanUseAbility();
    }

    private void OnGround()
    {
        Debug.Log(rampUpTime * heightConversion);
        PlayerMovement.Velocity = PlayerMovement.FacingDir * (rampUpTime * heightConversion);
        PlayerMovement.SpecialState = SpecialState.Normal;
    }


}