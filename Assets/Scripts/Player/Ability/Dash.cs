using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : Ability, IStatSource
{
    private bool canDash;
    private bool dashing;
    [SerializeField] private int dashDuration;
    private int curDashDuration;
    [SerializeField] private float dashVelocity;
    
    private Vector2 dashVelocityVec;
   
    //private Vector2 moveSpeedSnapshot;
    const float diagDashAngle = 35;

    [SerializeField] bool enableVFX = true;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void OnReset()
    {
        CancelDash();
    }

    public override void Start()
    {
        base.Start();

        PlayerMovement.onJump += () =>
        {
            CancelDash();
        };
        PlayerMovement.onGround += () =>
        {
            RefillDash();
        };
    }

    


    public void RefillDash()
    {
        canDash = true;
    }

    private bool CanDiagonalDash => CurrentLevel >= 1;
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (PlayerMovement == null)
        {
            return;
        }
        if (PlayerMovement.State == BodyState.OnGround) canDash = true;
        if (PlayerMovement.SpecialState == SpecialState.Dash)
        {
            PlayerMovement.Velocity = dashVelocityVec;
            PlayerMovement.GravityMultiplier.Multipliers[this] = 0f;
            curDashDuration--;
            if (curDashDuration <= 0)
            {
                PlayerMovement.SpecialState = SpecialState.Normal;
                CancelDash();
            }
        }
        else if (dashing) CancelDash();
        if (inputButton.HasPressed)
        {
            UseAbility();

        }
    }

    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool CanUseAbility()
    {
        return base.CanUseAbility() && canDash;
    }

    public void CancelDash()
    {
        if (!dashing) return;

        // thanh new part
        // when we are done with dashing, check if we were groundslamming before the dash
        // if we were, go back to the groundslam state so we can keep slamming
        GroundSlam slam = AbilityManager.Instance.GetAbility<GroundSlam>();
        if (slam != null && slam.wasSlammingBeforeDash)
        {
            slam.ContinueSlam();
        }
        else
        {
            // if we weren't groundslamming, this is a normal dash, so go back to normal state
            PlayerMovement.SpecialState = SpecialState.Normal;
        }
        // thanh new part

        dashing = false;
        PlayerMovement.GravityMultiplier.Multipliers.Remove(this);
        curDashDuration = 0;
        stopParticleAction?.Invoke();
    }

    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;

        // thanh new part
        // Grabs the slam instance and check if we are currently groundslamming
        GroundSlam slam = AbilityManager.Instance.GetAbility<GroundSlam>();
        bool interrupted = false;
        if (slam != null && Player.Instance.Movement.SpecialState == SpecialState.GroundSlam)
        {
            interrupted = slam.DashInterrupt();
            if (!interrupted)
            {
                // failed to dash as slam was not interrupted
                return false;
            }
        }
        // thanh new part

        PInput.Instance.Dash.ConsumeBuffer();
        // dash overrides forcemove
        PlayerMovement.EndForceMove();

        Vector2 dashVec = PlayerMovement.MoveDir;
        if (dashVec == Vector2.zero)
        {
            dashVec = PlayerMovement.FacingDir;
        }
        // interpret up/down inputs as diagonal, if possible
        if (dashVec == Vector2.up || dashVec == Vector2.down)
        {
            dashVec.x = PlayerMovement.FacingDir.x;
        }
        if (!CanDiagonalDash) dashVec.y = 0;
        else if (dashVec.x == 0) return false;

        // level 2 dash preserves speed
        float finalDashSpeed;
        if (CurrentLevel >= 2)
        {
            float horzVel = Player.Instance.Movement.Velocity.x;
            if (Util.SignOr0(horzVel) == Util.SignOr0(dashVec.x))
            {
                if (dashVec.y != 0)
                {
                    finalDashSpeed = Mathf.Max(dashVelocity, Mathf.Abs(horzVel) / Mathf.Cos(Mathf.Deg2Rad * diagDashAngle));
                }
                else
                {
                    finalDashSpeed = Mathf.Max(dashVelocity, Mathf.Abs(horzVel));
                }
            }
            else finalDashSpeed = dashVelocity;

        }
        else
        {
            finalDashSpeed = dashVelocity;
        }

        if (dashVec.x != 0 && dashVec.y != 0) // is diagonal dash 
        {
            dashVelocityVec = new Vector2(Util.SignOr0(dashVec.x) * Mathf.Cos(Mathf.Deg2Rad * diagDashAngle),
                                          Util.SignOr0(dashVec.y) * Mathf.Sin(Mathf.Deg2Rad * diagDashAngle)) 
                                         * finalDashSpeed;
        }
        else
        {
            dashVelocityVec = dashVec.normalized * finalDashSpeed;
        }
        canDash = false;
        curCooldown = cooldown;
        curDashDuration = dashDuration;
        dashing = true;
        PlayerMovement.SpecialState = SpecialState.Dash;

        if (enableVFX && !interrupted)
        {
            // particle effects
            stopParticleAction += PlayerVFXTrail.PlayParticle(Color.black);

        }


        base.UseAbility();

        return true;
    }

    /*private IEnumerator SetDashDirection(InputAction.CallbackContext context)
    {
        Vector2 inputVec = context.action.ReadValue<Vector2>();
        // If input is diagonal (player presses up/down and left/right on same frame),
        // default to left/right for dash direction
        if (inputVec.x != 0 && inputVec.y != 0)
        {
            if (CanDiagonalDash) inputs.Add(new Vector2(0, inputVec.y));
            inputVec = new Vector2(inputVec.x, 0);
        }
        inputs.Add(inputVec);
        // Debug.Log(inputVec);
        yield return new WaitForSeconds(inputStickyDuration);
        inputs.Remove(inputVec);
    }*/
}
