using System;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSlam : Ability
{
    [SerializeField] private int cooldown;
    private int curCooldown;
    [SerializeField] private float initialVelocity;
    private int rampUpTime;
    [SerializeField] private float rampUpAcceleration;

    [SerializeField] private float heightConversion;

    public bool wasSlammingBeforeDash;

    public override void Start()
    {
        base.Start();
        PlayerMovement.onGround += () =>
        {
            if (PlayerMovement.SpecialState == SpecialState.GroundSlam
                /* or the ground slam is level 2 and player recently dashed*/) OnGround();
        };
    }

    private void FixedUpdate()
    {
        // thanh new part
        if (curCooldown > 0) curCooldown--;

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
        // thanh new part
        if (PInput.Instance.GroundSlam.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();
    }

    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool UseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.Dash) AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        curCooldown = cooldown;
        PlayerMovement.Velocity = Vector2.down * initialVelocity;
        PlayerMovement.SpecialState = SpecialState.GroundSlam;
        rampUpTime = 0;
        return true;
    }

    public override bool CanUseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam) return false;
        if (PlayerMovement.State != BodyState.InAir) return false;
        return true;
    }

    private void OnGround()
    {
        Debug.Log(rampUpTime * heightConversion);
        PlayerMovement.Velocity = PlayerMovement.FacingDir * (rampUpTime * heightConversion);
        PlayerMovement.SpecialState = SpecialState.Normal;
    }


}