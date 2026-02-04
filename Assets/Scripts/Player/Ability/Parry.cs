using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class Parry : Ability
{

    [SerializeField] int hitstopFrames, parryPrimedFrames;
    private int hitstopRemaining, parryPrimedRemaining;
    private PInput.InputButton AbilityButton;
    private float storedSpeed;

    PlayerMovement pm => Player.Instance.Movement;

    [SerializeField]SpriteRenderer circle;

    public override void Start()
    {
        base.Start();

        AbilityButton = PInput.Instance.Parry;
        pm.OnHitWallAny += (e, dir) =>
        {
            if (parryPrimedRemaining > 0)
            {
                StartParry(dir);
            }
        };
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (AbilityButton.HasPressed)
        {
            if (CanUseAbility()) UseAbility();
        }

        if (parryPrimedRemaining > 0)
        {
            parryPrimedRemaining--;
            if (parryPrimedRemaining == 0)
            {
                stopParticleAction?.Invoke();
            }
        }
        if (hitstopRemaining > 0)
        {
            hitstopRemaining--;
            if (hitstopRemaining == 0)
            {
                ReleaseParry();
            }
        }
    }


    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool UseAbility()
    {
        base.UseAbility();
        
        parryPrimedRemaining = parryPrimedFrames;
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.white);
        return true;
    }

    private void StartParry(Vector2 surfaceDir)
    {
        Debug.Log("bonk");
        storedSpeed = pm.PreCollisionVelocity.magnitude;
        pm.Locked = true;
        hitstopRemaining = hitstopFrames;
        parryPrimedRemaining = 0;
        circle.enabled = true;
        circle.transform.localScale = Vector3.one * 5;
        circle.transform.DOScale(0f, hitstopFrames * Time.fixedDeltaTime).SetEase(Ease.InCubic);
        pm.SpecialState = SpecialState.Normal;


    }

    private void ReleaseParry()
    {
        Vector2 inputDir = PInput.Instance.MoveVector.normalized;
        pm.Locked = false;
        pm.Velocity = inputDir * (storedSpeed * 1.5f + 5f);
        StartCoroutine(Util.FDelayedCall(30, stopParticleAction));
        circle.enabled = false;
    }

    public override bool CanUseAbility()
    {
        return base.CanUseAbility();
    }


}