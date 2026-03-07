using DG.Tweening;
using FMODUnity;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class Parry : Ability
{
    [SerializeField] int maxLeniencyFrames;
    [SerializeField] int hitstopFrames, parryPrimedFrames, minHitstopFrames;
    private int hitstopRemaining, parryPrimedRemaining, leniencyFramesRemaining;
    private float storedSpeed;
    private Vector2 storedVelocity;
    public bool ParryPrimed => parryPrimedRemaining > 0;

    [SerializeField] ParticleSystem shockwaveParticles;

    PlayerMovement pm => Player.Instance.Movement;

    [SerializeField]SpriteRenderer circle;

    Vector2 surfaceDir;
    [SerializeField] float baseReflectSpeed = 6;
    [SerializeField] float speedMultiplier = 1f;
    [SerializeField] private float verticalSpeedSoftcap;
    [SerializeField] private float softcapAmount;

    [SerializeField] float minVerticalBoost;
    [HideInInspector] public Entity SpecialEntity;
    public Action OnPrimeParry;
    public override void Start()
    {
        base.Start();

        pm.OnHitWallAny += (e, dir) =>
        {
            CollideWithWall(e, dir);
        };
    }

    public override void OnReset()
    {
        base.OnReset();
        hitstopRemaining = 0;
        parryPrimedRemaining = 0;
        leniencyFramesRemaining = 0;
        storedSpeed = 0;
        storedVelocity = Vector2.zero;
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
                pm.IsInvulnerable = false;
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

    public void CollideWithWall(Entity e, Vector2 dir)
    {
        if (!ParryPrimed)
        {
            leniencyFramesRemaining = maxLeniencyFrames;
            return;
        }
        surfaceDir = dir;
        Vector2 scaledVelocity = pm.PreCollisionVelocity;
        scaledVelocity.y = Mathf.Abs(scaledVelocity.y);
        if (scaledVelocity.y > verticalSpeedSoftcap)
        {
            float cappedSpeed = scaledVelocity.y - verticalSpeedSoftcap;
            scaledVelocity.y = verticalSpeedSoftcap + cappedSpeed * softcapAmount;
        }
        if (pm.PreCollisionVelocity.y < 0) scaledVelocity.y *= -1;
        Debug.Log(scaledVelocity);
        storedSpeed = Vector2.Dot(scaledVelocity, dir);
        Debug.Log(storedSpeed);
        storedVelocity = scaledVelocity;
        
        StartParry(dir);
        
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
        if (CurrentLevel >= 2)
        {
            pm.IsInvulnerable = true;
        }
        OnPrimeParry?.Invoke();
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
        RuntimeManager.PlayOneShot("event:/Parry Sound Stop");
    }

    private void ReleaseParry()
    {
        Vector2 inputDir = Util.NormalizePerAxis(PInput.Instance.MoveVector);

        Vector2 perpendicularDir = Vector2.zero;
        if (CurrentLevel >= 1)
        {
            perpendicularDir = inputDir * new Vector2(Mathf.Abs(surfaceDir.y), Mathf.Abs(surfaceDir.x));
        }
        

        pm.Locked = false;
        if (SpecialEntity is Drone drone)
        {
            if (CurrentLevel >= 1) storedSpeed = Mathf.Abs(storedSpeed);
            pm.Velocity = (CurrentLevel >= 1 && inputDir != Vector2.zero ? inputDir : Vector2.right) 
                * (storedSpeed * speedMultiplier);
            pm.Velocity += Vector2.up * pm.MovementParams.JumpSpeed;
            drone.Consume();
        }
        else
        {
            pm.Velocity = -surfaceDir * (storedSpeed * speedMultiplier + (surfaceDir.y == 0 ? baseReflectSpeed : 0));
            pm.Velocity += perpendicularDir * storedSpeed * 0.5f * speedMultiplier;
            pm.Velocity += new Vector2(Mathf.Abs(surfaceDir.y), Mathf.Abs(surfaceDir.x)) * storedVelocity;
            if (surfaceDir.y == 0 && perpendicularDir != Vector2.down)
            {
                pm.Velocity += Vector2.up * minVerticalBoost;
            }
        }
        
        Debug.Log("total velocity: " + pm.Velocity.ToString());
        StartCoroutine(Util.FDelayedCall(30, stopParticleAction));
        hitstopRemaining = 0;
        circle.enabled = false;
        

        pm.ForceMove(inputDir, 3);
        StartCoroutine(Util.FDelayedCall(3, () =>
        {
            pm.CanClimb = true;
            pm.IsInvulnerable = false;
        }));

        RuntimeManager.PlayOneShot("event:/Parry Sound Launch");

        var p = Instantiate(shockwaveParticles);
        p.transform.position = transform.position + (Vector3)(Vector2.up * pm.PlayerHeight * 0.5f);
        p.transform.position += new Vector3(surfaceDir.x * pm.PlayerWidth * 0.5f, surfaceDir.y * pm.PlayerHeight * 0.5f);

        p.transform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, surfaceDir - perpendicularDir));
        p.Play();

        if (SpecialEntity is Bouncer bouncer) bouncer.PlayBouncerEffects();
        SpecialEntity = null;
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