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
        if (curCooldown > 0) curCooldown--;
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam)
        {
            rampUpTime++;
            PlayerMovement.Velocity += Vector2.down * rampUpAcceleration;
        }
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