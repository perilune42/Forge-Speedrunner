using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class Parry : Ability
{
    [SerializeField] int maxLeniencyFrames;
    [SerializeField] int hitstopFrames, parryPrimedFrames, minHitstopFrames;
    private int hitstopRemaining, parryPrimedRemaining, leniencyFramesRemaining;
    private float storedSpeed;

    [SerializeField] ParticleSystem shockwaveParticles;

    PlayerMovement pm => Player.Instance.Movement;

    [SerializeField]SpriteRenderer circle;

    Vector2 surfaceDir;
    [SerializeField] float baseReflectSpeed = 6;
    [SerializeField] float speedMultiplier = 1f;

    public override void Start()
    {
        base.Start();

        pm.OnHitWallAny += (e, dir) =>
        {
            surfaceDir = dir;
            storedSpeed = Vector2.Dot(pm.PreCollisionVelocity, dir);
            if (parryPrimedRemaining > 0)
            {
                StartParry(dir);
            }
            else
            {
                leniencyFramesRemaining = maxLeniencyFrames;
            }
        };
    }

    public override void OnReset()
    {
        base.OnReset();
        hitstopRemaining = 0;
        parryPrimedRemaining = 0;
        leniencyFramesRemaining = 0;
        storedSpeed = 0;
        circle.enabled = false;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (inputButton.HasPressed)
        {
            if (CanUseAbility()) UseAbility();
        }

        if (parryPrimedRemaining > 0)
        {
            parryPrimedRemaining--;
            if (parryPrimedRemaining == 0)
            {
                stopParticleAction?.Invoke();
                pm.CanClimb = true;
            }
        }
        if (hitstopRemaining > 0)
        {
            hitstopRemaining--;
            int hitstopCurrent = hitstopFrames - hitstopRemaining;
            if (hitstopRemaining == 0 || hitstopCurrent > minHitstopFrames && PInput.Instance.MoveVector.normalized != Vector2.zero)
            {
                ReleaseParry();
            }
        }
        if (leniencyFramesRemaining > 0)
        {
            leniencyFramesRemaining--;
        }
    }


    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool UseAbility()
    {
        base.UseAbility();
        PrimeParry();
        if (IsTouchingAny() && leniencyFramesRemaining > 0)
        {
            StartParry(surfaceDir);
        }

        return true;
    }
    private void PrimeParry()
    {
        parryPrimedRemaining = parryPrimedFrames;
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.white);
        pm.CanClimb = false;
    }

    private void StartParry(Vector2 hitSurfaceDir)
    {
        if (PlayerMovement.SpecialState == SpecialState.GroundSlam &&
            AbilityManager.Instance.TryGetAbility<GroundSlam>(out GroundSlam gs))
        {
            gs.OnGround();
        }
        hitstopRemaining = hitstopFrames;
        parryPrimedRemaining = 0;
        circle.enabled = true;
        circle.transform.localScale = Vector3.one * 5;
        circle.transform.DOScale(0f, hitstopFrames * Time.fixedDeltaTime).SetEase(Ease.InCubic);
        pm.SpecialState = SpecialState.Normal;
        pm.Locked = true;

    }

    private void ReleaseParry()
    {
        Vector2 inputDir = PInput.Instance.MoveVector.normalized;
        pm.Locked = false;
        pm.Velocity = inputDir * (storedSpeed * speedMultiplier + baseReflectSpeed);
        if (Vector2.Dot(inputDir, -surfaceDir) <= 0)
        {
            pm.Velocity += -surfaceDir * baseReflectSpeed;
        } 
        StartCoroutine(Util.FDelayedCall(30, stopParticleAction));
        hitstopRemaining = 0;
        circle.enabled = false;
        

        pm.ForceMove(inputDir, 3);
        StartCoroutine(Util.FDelayedCall(3, () =>
        {
            pm.CanClimb = true;
        }));

        var p = Instantiate(shockwaveParticles);
        p.transform.position = transform.position + (Vector3)(Vector2.up * pm.PlayerHeight * 0.5f);
        p.transform.position += new Vector3(surfaceDir.x * pm.PlayerWidth * 0.5f, surfaceDir.y * pm.PlayerHeight * 0.5f);

        p.transform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, -inputDir));
        p.Play();
    }

    public override bool CanUseAbility()
    {
        if (!(leniencyFramesRemaining > 0))
        {
            if (IsTouchingAny()) return false;
        }
        return base.CanUseAbility();
    }

    private bool IsTouchingAny()
    {
        return pm.IsTouching(Vector2.up) || pm.IsTouching(Vector2.down)
            || pm.IsTouching(Vector2.left) || pm.IsTouching(Vector2.right);
    }


}