using System;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSlam : Ability, IStatSource
{
    [SerializeField] private float initialVelocity;
    private int rampUpTime;
    [SerializeField] private int timeBeforeAcceleration;
    [SerializeField] private float rampUpVelocity, rampUpAcceleration;
    private float rampUpVelocityDefault;
    [SerializeField] private float terminalVelocitySlam;
    private float terminalVelocityDefault;
    [SerializeField] private float heightConversion;
    [SerializeField] private float minimumSpeedGain;

    public bool wasSlammingBeforeDash;

    private Vector2 preservedVelocity;

    private bool slammingUpwards;

    public override void Start()
    {
        base.Start();
        PlayerMovement.onGround += () =>
        {
            if (PlayerMovement.SpecialState == SpecialState.GroundSlam) OnGround();
        };
        PlayerMovement.onGroundTop += () =>
        {
            if (slammingUpwards && PlayerMovement.SpecialState == SpecialState.GroundSlam) OnGround();
        };
        rampUpVelocityDefault = rampUpVelocity;
    }

    public override void OnReset()
    {
        base.OnReset();
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
                PlayerMovement.Velocity += GetSlamDir() * rampUpVelocity;
                if (GetSlamDir().y > 0)
                {
                    if (rampUpTime >= timeBeforeAcceleration && PlayerMovement.Velocity.y > -initialVelocity) rampUpVelocity += rampUpAcceleration;

                }
                else
                {
                    if (rampUpTime >= timeBeforeAcceleration && PlayerMovement.Velocity.y < initialVelocity) rampUpVelocity += rampUpAcceleration;
                }
            }
        }
        if (inputButton.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();
    }

    // returns: whether dash was successfully interrupted
    public bool DashInterrupt()
    {
        // if groundslam is level 1 and we are groundslamming, we cannot dash
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam && CurrentLevel < 1)
        {
            return false;
        }
        // check if we are currently groundslamming and groundSlam is level 2
        // if so, we set the flag of wasSlammingBeforeDash to true
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam && CurrentLevel >= 1)
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
        if (CurrentLevel >= 2 && PInput.Instance.MoveVector.y > 0)
        {
            slammingUpwards = true;
            PlayerMovement.GravityMultiplier.Multipliers[this] = -1;
        }

        if (PlayerMovement.SpecialState == SpecialState.Dash) AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        PlayerMovement.Velocity = GetSlamDir() * initialVelocity;
        PlayerMovement.SpecialState = SpecialState.GroundSlam;
        terminalVelocityDefault = PlayerMovement.TerminalVelocity;
        PlayerMovement.TerminalVelocity = terminalVelocitySlam;
        rampUpVelocity = rampUpVelocityDefault;
        rampUpTime = 0;

        base.UseAbility();
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.purple);
        return true;
    }

    public override bool CanUseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam) return false;
        if (PlayerMovement.State != BodyState.InAir) return false;
        return base.CanUseAbility();
    }

    public void OnGround()
    {
        Debug.Log(rampUpTime * heightConversion);
        PlayerMovement.Velocity = PlayerMovement.FacingDir * (rampUpTime * heightConversion + minimumSpeedGain);
        PlayerMovement.SpecialState = SpecialState.Normal;
        PlayerMovement.TerminalVelocity = terminalVelocityDefault;
        slammingUpwards = false;
        PlayerMovement.GravityMultiplier.Multipliers[this] = 1;
        stopParticleAction?.Invoke();
    }

    private Vector2 GetSlamDir()
    {
        return slammingUpwards ? Vector2.up : Vector2.down;
    }

}