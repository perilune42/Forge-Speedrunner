using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : Ability, IStatSource
{
    public float LaunchSpeed;
    public float HandLaunchSpeed;
    [SerializeField] private GrappleHand GrappleHandPrefab;
    [SerializeField] private GameObject GrappleArrowPrefab;
    [SerializeField] private GrappleIndicator GrappleIndicatorPrefab;
    [SerializeField] private int pullCooldown;
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
        if (inputButton.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();

        if (grappleState == GrappleState.Pulling)
        {
            Vector2 vecToHand = grappleHand.transform.position - PlayerMovement.transform.position;
            float dist = vecToHand.magnitude;
            Vector2 direction = vecToHand.normalized;
            PlayerMovement.Velocity = direction * pullSpeed;
            if (PInput.Instance.Jump.HasPressed)
            {
                LaunchPlayer(LaunchSpeed, true);
            }
            else if (dist <= minLaunchDistance)
            {
                LaunchPlayer(LaunchSpeed, false);
            }
        }

        if (charging)
        {
            chargeTime++;
            //grappleArrow.transform.localScale = Vector3.one * 4f * (1f + chargeTime * chargePerTick);
            grappleHand.ApplyChargeVFX(1f + chargeTime * chargePerTick);
            /*
            if (chargeTime >= maxCharge || AbilityButton.StoppedPressing)
            {
                LaunchPlayer(LaunchSpeed * (1f + chargeTime * chargePerTick));
                chargeTime = 0;
            }
            */
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
                            new Vector2(0.1f, 0.1f), 0f,
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

            grappleState = GrappleState.Launch;
            curCooldown = pullCooldown;
        }
        else if (grappleState == GrappleState.Active)
        {
            StartPulling();
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
    }

    private void LaunchPlayer(float launchVelocity, bool jumpBoost)
    {
        if (PlayerMovement.SpecialState == SpecialState.Dash)
        {
            AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        }
        Vector2 direction = (grappleHand.transform.position - PlayerMovement.transform.position).normalized;
        Vector2 finalVel;
        // for quick (non-charged) launches:
        // mainly give velocity in desired axis
        /*
        if (chargeTime < 20)
        {
            if (AttachedDirection.x != 0)
            {
                finalVel = new Vector2(direction.x * launchVelocity, direction.y * launchVelocity * 0.5f);
            }
            else
            {
                finalVel = new Vector2(direction.x * launchVelocity * 0.5f, direction.y * launchVelocity);
            }
        }
        else
        {
            finalVel = direction * launchVelocity;
        }
        if (AttachedDirection.y == 0)
        {
            // give small vertical boost on horizontal grapples
            finalVel += Vector2.up * verticalBoost;
        }
        PlayerMovement.Velocity = finalVel;
        */
        if (jumpBoost)
        {
            PlayerMovement.Jump();
        }
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.turquoise);
        RemoveGrapple();
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