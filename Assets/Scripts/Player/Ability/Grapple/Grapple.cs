using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Grapple : Ability, IStatSource
{
    public float LaunchSpeed;
    public float HandLaunchSpeed;
    [SerializeField] private GrappleHand GrappleHandPrefab;
    [SerializeField] private GameObject GrappleArrowPrefab;
    [SerializeField] private GrappleIndicator GrappleIndicatorPrefab;
    [SerializeField] private int pullCooldown;
    [SerializeField] private int throwCooldown;
    private GrappleHand grappleHand;
    private GrappleIndicator grappleIndicator;
    //private GameObject grappleArrow;
    [HideInInspector] public bool GrappleHandActive;
    public GrappleState grappleState;
    private bool charging = false;
    private int chargeTime = 0;
    [SerializeField] private float chargePerTick;
    [SerializeField] private int maxCharge;
    [SerializeField] private int lifetime;

    public Vector2 LastThrowDirection;
    [HideInInspector] public Vector2 AttachedDirection;

    [SerializeField] private float verticalBoost = 7f;
    [SerializeField] private float pullSpeed = 20f;
    [SerializeField] private float minLaunchDistance = 3;   // force launch when getting within 3 tiles

    private int minPullDuration = 10;    // cannot launch until this many frames has passed
    private int forcePullTimer = 0;


    private float throwOffset => Player.Instance.Movement.PlayerHeight / 2;

    public override void Start()
    {
        base.Start();
        grappleState = GrappleState.Idle;
        PlayerMovement.OnHitWallAny += (entity, direction) =>
        {
            if (AbilityManager.Instance.GetAbility<Ricochet>() 
            ? !AbilityManager.Instance.GetAbility<Ricochet>().active : true) 
                stopParticleAction?.Invoke();
        };
        PlayerMovement.OnSpecialStateChange += (newState) =>
        {
            if (grappleState == GrappleState.Pulling && newState != SpecialState.Normal) RemoveGrapple();
        };

        grappleIndicator = Instantiate(GrappleIndicatorPrefab, transform);
        grappleIndicator.gameObject.SetActive(false);

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (curCooldown > 0) curCooldown--;
        if (forcePullTimer > 0) forcePullTimer--;
        if (inputButton.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();

        if (grappleState == GrappleState.Pulling)
        {
            Vector2 vecToHand = grappleHand.transform.position - PlayerMovement.transform.position;
            float dist = vecToHand.magnitude;
            Vector2 direction = vecToHand.normalized;
            PlayerMovement.Velocity = direction * pullSpeed * (1 + chargeTime * chargePerTick);
            if (forcePullTimer == 0)
            {
                if (PInput.Instance.Jump.HasPressed)
                {
                    PInput.Instance.Jump.ConsumeBuffer();
                    LaunchPlayer(true);
                }
                else if (!inputButton.IsPressing)
                {
                    LaunchPlayer(false);
                }
            }
            if (grappleState == GrappleState.Pulling && dist <= minLaunchDistance)
            {
                LaunchPlayer(false);
            }
        }
        else if(grappleState == GrappleState.Active)
        {
            if (inputButton.IsPressing & curCooldown == 0)
            {
                StartPulling();
            }
        }

        if (charging)
        {
            if (chargeTime < maxCharge)
            {
                chargeTime++;
            }

            // grappleArrow.transform.localScale = Vector3.one * 4f * (1f + chargeTime * chargePerTick);
            grappleHand.ApplyChargeVFX(1f + chargeTime * chargePerTick, chargeTime == maxCharge);
        }



        UpdateIndicator();
        
    }

    private float GetExpectedRange()
    {
        return lifetime * Time.fixedDeltaTime * HandLaunchSpeed;
    }

    private void UpdateIndicator()
    {
        if (CanUseAbility() && grappleState == GrappleState.Idle)
        {
            Vector2 launchdir = GetThrowDir();
            foreach (var entityHit in PlayerMovement.CustomBoxCastAll((Vector2)PlayerMovement.transform.position + Vector2.up * throwOffset,
                            new Vector2(0.9f, 0.9f), 0f,
                            launchdir, GetExpectedRange(), LayerMask.GetMask("Entity")))
            {
                if (entityHit.collider.GetComponent<Drone>() != null)
                {
                    grappleIndicator.gameObject.SetActive(true);
                    grappleIndicator.transform.position = entityHit.collider.transform.position;
                    return;
                }
            }

            var hit = PlayerMovement.CustomBoxCast((Vector2)PlayerMovement.transform.position + Vector2.up * throwOffset, 
                                        new Vector2(0.1f,0.1f), 0f,
                                        launchdir, GetExpectedRange(), LayerMask.GetMask("Solid"));
            if (hit)
            {
                grappleIndicator.gameObject.SetActive(true);
                grappleIndicator.transform.position = hit.point;
            }
            else
            {
                grappleIndicator.gameObject.SetActive(false);
            }

        }
        else
        {
            grappleIndicator.gameObject.SetActive(false);
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


    private Vector2 GetThrowDir()
    {
        Vector2 throwDir;
        if (PInput.Instance.MoveVector.x != 0)
        {
            throwDir = new Vector2(PInput.Instance.MoveVector.x, 0);
        }
        else if (PInput.Instance.MoveVector.y != 0)
        {
            throwDir = new Vector2(0, PInput.Instance.MoveVector.y);
        }
        else
        {
            throwDir = PlayerMovement.FacingDir;
        }
        return throwDir;
    }
    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;

        inputButton.ConsumeBuffer();

        if (grappleState == GrappleState.Idle)
        {
            OnActivate?.Invoke(); // only use charge on attach
            grappleHand = Instantiate(GrappleHandPrefab, (Vector2)PlayerMovement.transform.position + Vector2.up * throwOffset, Quaternion.identity)
                .GetComponent<GrappleHand>();
            grappleHand.Grapple = this;
            grappleHand.SetLifetime(lifetime);
            Vector2 throwDir = GetThrowDir();
            LastThrowDirection = throwDir;

            grappleHand.Velocity += throwDir * HandLaunchSpeed;
            grappleHand.transform.eulerAngles = Vector3.forward * (Mathf.Atan2(throwDir.y, throwDir.x) * Mathf.Rad2Deg);
            grappleState = GrappleState.Launch;
            
        }
        return true;
    }

    private void StartPulling()
    {
        if (PlayerMovement.SpecialState == SpecialState.Dash)
        {
            AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        }
        grappleState = GrappleState.Pulling;
        PlayerMovement.GravityMultiplier.Multipliers[this] = 0f;
        Vector2 direction = (grappleHand.transform.position - PlayerMovement.transform.position).normalized;
        PlayerMovement.Velocity = direction * pullSpeed;
        forcePullTimer = minPullDuration;
        charging = false;
    }

    private void LaunchPlayer(bool jumpBoost)
    {
        float launchVelocity = LaunchSpeed * (1f + chargeTime * chargePerTick);
        if (PlayerMovement.SpecialState == SpecialState.Dash)
        {
            AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        }
        Vector2 direction = (grappleHand.transform.position - PlayerMovement.transform.position).normalized;
        if (jumpBoost)
        {
            PlayerMovement.Jump();
        }
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.orange);
        RemoveGrapple();
        chargeTime = 0;
        //Destroy(grappleArrow);

        // gain a temporary bonus to ledge climbing
        // this is jank as hell, come back to this later
        /*
        PlayerMovement.LedgeClimbBonus = 0.5f;
        StartCoroutine(Util.FDelayedCall(60, () => PlayerMovement.LedgeClimbBonus = 0));
        */

    }

    private void RemoveGrapple()
    {
        charging = false;
        grappleState = GrappleState.Idle;
        Destroy(grappleHand.gameObject);
        if (!UsesCharges)
        {
            curCooldown = cooldown;
        }
        PlayerMovement.GravityMultiplier.Multipliers.Remove(this);
    }

    public void Attach(Vector2 direction)
    {
        grappleState = GrappleState.Active;
        if (UsesCharges)
        {
            // only use charge on attach
            CurCharges--;
        }
        AttachedDirection = direction;
        curCooldown = pullCooldown;
        Player.Instance.Movement.Velocity.y = verticalBoost;

        // if (level 2...)
        charging = true;
    }

    public void Abort()
    {
        grappleState = GrappleState.Idle;
        curCooldown = throwCooldown;
    }
    
    [ContextMenu("Reset")]
    public void Reset()
    {
        if (grappleHand == null) return;
        Destroy(grappleHand.gameObject);
        grappleState = GrappleState.Idle;
    }

    /*public void CreateGrappleArrow()
    {
        grappleArrow = Instantiate(GrappleArrowPrefab, PlayerMovement.transform);
        grappleArrow.transform.position = PlayerMovement.GetComponent<BoxCollider2D>().bounds.center;
        grappleArrow.GetComponent<GrappleArrow>().grappleHand = grappleHand.gameObject;
    }*/
}

public enum GrappleState
{
    Idle,
    Launch,
    Active,
    Pulling
}