using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class WallLatch : Ability
{

    private PInput.InputButton AbilityButton;
    PlayerMovement pm => Player.Instance.Movement;

    public override void Start()
    {
        base.Start();

        AbilityButton = PInput.Instance.Parry;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (AbilityButton.HasPressed && CanUseAbility())
        {
            UseAbility();
        }
        // Debug.Log($"Is touching L:{pm.IsTouching(Vector2.left)} R:{pm.IsTouching(Vector2.right)}");
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
    }


    public override bool CanUseAbility()
    {
        return (pm.IsTouching(Vector2.left) || pm.IsTouching(Vector2.right)) && base.CanUseAbility();
    }


}