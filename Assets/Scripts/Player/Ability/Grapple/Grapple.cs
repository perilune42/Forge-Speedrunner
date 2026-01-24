using System;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : Ability
{
    public float PullStrength;
    public float LaunchSpeed;
    [SerializeField] private GameObject GrappleHandPrefab;
    [SerializeField] private GameObject GrappleArrowPrefab;
    [SerializeField] private int cooldown, pullCooldown;
    private int curCooldown;
    private GrappleHand grappleHand;
    private GameObject grappleArrow;
    [HideInInspector] public bool GrappleHandActive;
    public GrappleState grappleState;

    public override void Start()
    {
        base.Start();
        grappleState = GrappleState.Idle;
    }

    private void FixedUpdate()
    {
        if (curCooldown > 0) curCooldown--;
        if (PInput.Instance.Grapple.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();
    }

    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool CanUseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam &&
            grappleState == GrappleState.Active) return false;
        return (grappleState != GrappleState.Launch);
    }

    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;
        if (grappleState == GrappleState.Idle)
        {
            grappleHand = Instantiate(GrappleHandPrefab, PlayerMovement.transform.position, Quaternion.identity)
                .GetComponent<GrappleHand>();
            grappleHand.Grapple = this;
            grappleHand.Velocity += Vector2.up * LaunchSpeed;

            grappleState = GrappleState.Launch;
            curCooldown = pullCooldown;
        }
        else if (grappleState == GrappleState.Active)
        {
            if (PlayerMovement.SpecialState == SpecialState.Dash)
            {
                AbilityManager.Instance.GetAbility<Dash>().CancelDash();
            }
            Vector2 direction = (grappleHand.transform.position - PlayerMovement.transform.position).normalized;
            PlayerMovement.Velocity = direction * PullStrength;

            grappleState = GrappleState.Idle;
            Destroy(grappleHand.gameObject);
            Destroy(grappleArrow);
            curCooldown = cooldown;
        }

        return true;
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