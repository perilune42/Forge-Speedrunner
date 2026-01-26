using System;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : Ability
{
    public float PullStrength;
    public float LaunchSpeed;
    [SerializeField] private GameObject GrappleHandPrefab;
    [SerializeField] private GameObject GrappleArrowPrefab;
    [SerializeField] private int pullCooldown;
    private GrappleHand grappleHand;
    private GameObject grappleArrow;
    [HideInInspector] public bool GrappleHandActive;
    public GrappleState grappleState;
    private bool charging = false;
    private int chargeTime = 0;
    [SerializeField] private float chargePerTick;
    [SerializeField] private int maxCharge;
    public override void Start()
    {
        base.Start();
        grappleState = GrappleState.Idle;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (curCooldown > 0) curCooldown--;
        if (PInput.Instance.Grapple.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();

        if (charging)
        {
            chargeTime++;
            grappleArrow.transform.localScale = Vector3.one * 4f * (1f + chargeTime * chargePerTick);
            if (chargeTime >= maxCharge || PInput.Instance.Grapple.StoppedPressing)
            {
                LaunchPlayer(PullStrength * (1f + chargeTime * chargePerTick));
                chargeTime = 0;
            }
        }
        
    }

    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool CanUseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam &&
            grappleState == GrappleState.Active) return false;
        return (grappleState == GrappleState.Active) || base.CanUseAbility() && (grappleState != GrappleState.Launch);
    }

    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;
        
        if (grappleState == GrappleState.Idle)
        {
            base.UseAbility();  // only use charge on initial throw
            grappleHand = Instantiate(GrappleHandPrefab, PlayerMovement.transform.position, Quaternion.identity)
                .GetComponent<GrappleHand>();
            grappleHand.Grapple = this;
            grappleHand.Velocity += Vector2.up * LaunchSpeed;

            grappleState = GrappleState.Launch;
            curCooldown = pullCooldown;
        }
        else if (grappleState == GrappleState.Active)
        {
            if (Level == 1) LaunchPlayer(PullStrength);
            else charging = true;
        }
        
        return true;
    }

    private void LaunchPlayer(float launchVelocity)
    {
        if (PlayerMovement.SpecialState == SpecialState.Dash)
        {
            AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        }
        Vector2 direction = (grappleHand.transform.position - PlayerMovement.transform.position).normalized;
        PlayerMovement.Velocity = direction * launchVelocity;
        charging = false;
        grappleState = GrappleState.Idle;
        Destroy(grappleHand.gameObject);
        Destroy(grappleArrow);
        if (!UsesCharges)
        {
            curCooldown = cooldown;
        }
        
    }
    
    public void CreateGrappleArrow()
    {
        grappleArrow = Instantiate(GrappleArrowPrefab, PlayerMovement.transform);
        grappleArrow.transform.position = PlayerMovement.GetComponent<BoxCollider2D>().bounds.center;
        grappleArrow.GetComponent<GrappleArrow>().grappleHand = grappleHand.gameObject;
    }
}

public enum GrappleState
{
    Idle,
    Launch,
    Active,
}