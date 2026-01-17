using System;
using Unity.VisualScripting;
using UnityEngine;

public class GroundSlam : Ability
{
    [SerializeField] private int cooldown;
    private int curCooldown;
    [SerializeField] private float initialVelocity;
    private float startHeight;
    [SerializeField] private float heightConversion;

    public override void Start()
    {
        base.Start();
        PlayerMovement.onGround += () =>
        {
            if (PlayerMovement.SpecialState == SpecialState.GroundSlam) OnGround();
        };
    }

    private void FixedUpdate()
    {
        if (curCooldown > 0) curCooldown--;
        if (PInput.Instance.GroundSlam.HasPressed && CanSlam() && GetCooldown() >= 1f) UseAbility();
    }

    public override float GetCooldown()
    {
        if (!CanSlam()) return 0.0f;
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool UseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.Dash) FindFirstObjectByType<Dash>().CancelDash();
        curCooldown = cooldown;
        PlayerMovement.Velocity = Vector2.down * initialVelocity;
        PlayerMovement.SpecialState = SpecialState.GroundSlam;
        startHeight = transform.position.y;
        return true;
    }
    
    private bool CanSlam()
    {
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam) return false;
        if (PlayerMovement.State != BodyState.InAir) return false;
        return true;
    }

    private void OnGround()
    {
        Debug.Log(PlayerMovement.Velocity);
        PlayerMovement.Velocity = Vector2.right * ((startHeight - transform.position.y) * heightConversion);
        PlayerMovement.SpecialState = SpecialState.Normal;
    }
    
    
}