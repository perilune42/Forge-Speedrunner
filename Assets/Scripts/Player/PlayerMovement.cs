using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialState
{
    Normal, Dash, LedgeClimb, GroundSlam, WallClimb, Zipline
}

public class PlayerMovement : DynamicEntity
{

    public MovementParams MovementParams;
    public Action onGround;
    public Action onJump;

    public Vector2 MoveDir;
    public Vector2 FacingDir = Vector2.right;

    public int MaxJumpFrames = 20;
    private int jumpFrames = 0;

    private int coyoteFrames = 0;
    private int wallCoyoteFrames = 0;
    private Vector2 lastClimbDir = Vector2.right;
    private int maxCoyoteFrames = 5;

    private int forceMoveFrames = 0;
    private int maxForceMoveFrames = 10;

    private float retainedSpeed = 0;
    private bool hangTime = false;

    private bool isSprinting = false;

    public float PlayerHeight => ((BoxCollider2D)SurfaceCollider).size.y;
    public float PlayerWidth => ((BoxCollider2D)SurfaceCollider).size.x;

    public SpecialState SpecialState;

    [SerializeField] private List<AudioClip> audioClips;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        AbilityManager.Instance.GetAbility<Dash>().OnActivate += StartSprint;
        OnHitWallLeft += DoCollosionChecks;
        OnHitWallRight += DoCollosionChecks;

        FacingDir = Vector2.right;
    }


    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if ((SpecialState == SpecialState.Normal && forceMoveFrames == 0) || SpecialState == SpecialState.GroundSlam )
        {
            MoveDir = PInput.Instance.MoveVector.NormalizePerAxis();
        }
        if (SpecialState == SpecialState.Normal || SpecialState == SpecialState.GroundSlam)
        {
            if (MoveDir.x != 0)
            {
                FacingDir = new Vector2(MoveDir.x, 0);
            }
        }

        CheckInputs();

        if (SpecialState != SpecialState.LedgeClimb)
        {
            // keep retained speed even after collision
            retainedSpeed = Velocity.x;
        }

        if (State == BodyState.OnGround)
        {
            coyoteFrames = maxCoyoteFrames;
        }
        else if (coyoteFrames > 0)
        {
            coyoteFrames--;
        }

        if (SpecialState != SpecialState.WallClimb)
        {
            wallCoyoteFrames--;
        }

        if (Velocity.y < MovementParams.MinHangVelocity) EndHangTime();

        ApplyForces();

        if (SpecialState != SpecialState.LedgeClimb)
        {
            TryLedgeClimb();
        }
        
        if (SpecialState == SpecialState.WallClimb)
        {
            ContinueWallClimb(MoveDir);
        }

        TickTimers();

    }

    private void ApplyForces()
    {
        
        float moveSpeed, moveAccel, friction;
        moveSpeed = isSprinting ? MovementParams.SprintSpeed : MovementParams.WalkSpeed;

        if (State == BodyState.OnGround)
        {
            moveAccel = MovementParams.GroundAcceleration;
            friction = MovementParams.GroundFriction;
        }
        else if (State == BodyState.InAir)
        {
            moveAccel = MovementParams.AirAcceleration;
            friction = MovementParams.AirFriction;
        }
        else
        {
            return;
        }

        bool ignoreFriction = false;

        // air and ground movement

        
        if (MoveDir.x != 0 && SpecialState == SpecialState.Normal)
        {
            float targetXVel = moveSpeed * MoveDir.x;
            // if current speed is below max speed, and the player's movement input helps accelerate
            // the player towards the target max speed
            if ((targetXVel < 0 && Velocity.x >= targetXVel) || (targetXVel > 0 && Velocity.x <= targetXVel))
            {


                float dV = moveAccel * MoveDir.x * fdt;
                if ((targetXVel > 0 && Velocity.x + dV < targetXVel)
                    || (targetXVel < 0 && Velocity.x + dV > targetXVel))
                {
                    Velocity.x += dV;
                }
                else
                {
                    Velocity.x = targetXVel;
                }

                // do not apply friction if player is attempting to move in the same direction of acceleration
                ignoreFriction = Mathf.Sign(targetXVel) == Mathf.Sign(Velocity.x);
            }
            // otherwise walking in the same direction as movement does nothing
        }
        
        // friction

        if (!Mathf.Approximately(Velocity.x, 0) && !ignoreFriction)
        {
            float dV = -Util.SignOr0(Velocity.x) * friction * fdt;
            if ((Velocity.x > 0 && Velocity.x > Mathf.Abs(dV)) || (Velocity.x < 0 && Mathf.Abs(Velocity.x) > dV))
            {
                Velocity.x += dV;
            }
            else
            {
                Velocity.x = 0;
            }
        }
    }

    private void TickTimers()
    {
        if (jumpFrames > 0)
        {
            jumpFrames--;
            if (jumpFrames == 0)
            {
                EndJump();
                if (PInput.Instance.Jump.IsPressing)
                {
                    StartHangTime();
                }
            }
        }
        if (forceMoveFrames > 0)
        {
            forceMoveFrames--;
        }
            
    }
    
    public void EndForceMove()
    {
        forceMoveFrames = 0;
        MoveDir = PInput.Instance.MoveVector.NormalizePerAxis();
    }
    

    public override void OnGrounded(RaycastHit2D groundHit)
    {
        base.OnGrounded(groundHit);
        onGround?.Invoke();
    }

    public void CheckInputs()
    {
        if (PInput.Instance.Jump.HasPressed )
        {
            if (CanWallJump())
            {
                WallJump(lastClimbDir);
                PInput.Instance.Jump.ConsumeBuffer();
            }
            if (CanJump())
            {
                Jump();
                PInput.Instance.Jump.ConsumeBuffer();
            }
            /*
            var dir = Vector2.right;
            for (int i = 0; i < 2; i++)
            {
                if (CanLedgeClimb(dir))
                {
                    StartCoroutine(LedgeClimb(dir));
                    PInput.Instance.Jump.ConsumeBuffer();
                    break;
                }
                dir = Vector2.left;
            }
            */
        }
        else if (PInput.Instance.Jump.StoppedPressing)
        {
            // a minimum jump lasts 3 frames
            jumpFrames = Mathf.Min(3, jumpFrames);
            if (hangTime)
            {
                EndHangTime();
            }
        }

        if (PInput.Instance.Dash.StoppedPressing)
        {
            EndSprint();
        }

        var dir = Vector2.right;
        for (int i = 0; i < 2; i++)
        {
            if (MoveDir.x != -dir.x
                && IsInputtingWallClimb(dir)
                && !CanLedgeClimb(new Vector2(dir.x, 0))
                && CanWallClimb(new Vector2(dir.x, 0))
                && SpecialState != SpecialState.WallClimb)
            {
                StartWallClimb(new Vector2(dir.x, 0));
                break;
            }
            dir = Vector2.left;
        }


    }

    private bool CanJump()
    {
        return State == BodyState.OnGround || coyoteFrames > 0;
    }

    private bool CanWallJump()
    {
        return SpecialState == SpecialState.WallClimb || wallCoyoteFrames > 0;
    }

    private bool CanLedgeClimb(Vector2 dir)
    {
        return IsTouching(dir) && GetLedgeHeight(dir) < 0.75 && GetLedgeHeight(dir) > 0;
    }

    private bool CanWallClimb(Vector2 dir)
    {
        if (SpecialState != SpecialState.Normal && SpecialState != SpecialState.Dash && SpecialState != SpecialState.WallClimb) return false;
        Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.5f * PlayerHeight);
        Vector2 size = new(PlayerWidth, PlayerHeight);
        RaycastHit2D[] hits = new RaycastHit2D[8];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(collisionLayer);
        int hitCount = Physics2D.BoxCast(origin, size, 0f, dir, contactFilter, hits, COLLISION_CHECK_DISTANCE * 2);
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D wallHit = hits[i];
            if (wallHit.collider.GetComponent<ClimbableWall>() != null)
            {
                return IsTouching(dir);
            }
        }
        return false;
    }

    private bool IsInputtingWallClimb(Vector2 dir)
    {
        var inputMove = PInput.Instance.MoveVector.NormalizePerAxis();
        return (inputMove == Vector2.up && FacingDir.x == dir.x) || (inputMove.x == dir.x && inputMove.y >= 0);
    }
   

    public void Jump()
    {
        Velocity = new Vector2(Velocity.x, MovementParams.JumpSpeed);
        jumpFrames = MaxJumpFrames;
        coyoteFrames = 0;
        onJump?.Invoke();
        GravityMultiplier = MovementParams.JumpGravityMult;

        AudioManager.Instance?.PlaySoundEffect(audioClips[0], transform, 0.5f);
    }

    private void WallJump(Vector2 wallDir)
    {
        SpecialState = SpecialState.Normal;
        float xVel = -wallDir.x * MovementParams.JumpSpeed;
        Velocity = new Vector2(xVel, MovementParams.JumpSpeed);
        // player not allowed to turn around for 5 frames
        MoveDir = new Vector2(-wallDir.x, 0);
        forceMoveFrames = maxForceMoveFrames;
        jumpFrames = MaxJumpFrames;
        wallCoyoteFrames = 0;
        onJump?.Invoke();
        GravityMultiplier = MovementParams.JumpGravityMult;
    }

    public void EndJump(bool force = false)
    { 
        jumpFrames = 0;
        if (PInput.Instance.Jump.IsPressing && !force)
        {
            StartHangTime();
        }
        else
        {
            EndHangTime();
        }
        
    }

    private void StartHangTime()
    {
        GravityMultiplier = MovementParams.HangGravityMult;
        hangTime = true;
    }

    private void EndHangTime()
    {
        GravityMultiplier = 1;
        hangTime = false;
    }

    private float GetLedgeHeight(Vector2 dir)
    {
        // start a boxcast upwards and to either the left and right of the player, pointing downwards
        Vector2 offset = SurfaceCollider.offset + new Vector2(PlayerWidth * (dir.x), PlayerHeight - PlayerHeight * 0.45f) ;
        Vector2 origin = (Vector2)transform.position + offset;
        Vector2 size = new (PlayerWidth, PlayerHeight * 0.1f);
        RaycastHit2D groundHit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, PlayerHeight * 4, collisionLayer);


        // how much the ledge is above the player's foot
        float ledgeHeight = PlayerHeight - groundHit.distance;
        return ledgeHeight;
    }

    private IEnumerator LedgeClimb(Vector2 dir)
    {
        // ascend the wall at jump speed, do a small hop once the top is reached.
        SpecialState = SpecialState.LedgeClimb;
        EndJump(force: true);
        GravityMultiplier = 0f;
        Velocity = new Vector2(0, MovementParams.ClimbSpeed);
        lastClimbDir = MoveDir;
        yield return new WaitForFixedUpdate();
        while (GetLedgeHeight(dir) > 0)
        {
            yield return new WaitForFixedUpdate();
        }
        const float minLedgeBoost = 5f;

        float alignedRetainedSpeed = Util.SignOr0(retainedSpeed) == dir.x ? Mathf.Abs(retainedSpeed) : 0;
        float boost = Mathf.Max(alignedRetainedSpeed, minLedgeBoost);
        Velocity = new(boost * dir.x, MovementParams.JumpSpeed / 2);
        GravityMultiplier = 1f;
        SpecialState = SpecialState.Normal;
    }

    private void StartWallClimb(Vector2 dir)
    {
        // ascend the wall at jump speed while the input is being held.
        SpecialState = SpecialState.WallClimb;
        MoveDir = new Vector2(dir.x, 0);
        EndJump(force: true);
        GravityMultiplier = 0f;
        Velocity = new Vector2(0, MovementParams.ClimbSpeed);
        lastClimbDir = MoveDir;
        wallCoyoteFrames = maxCoyoteFrames;
    }
    private void ContinueWallClimb(Vector2 dir)
    {
        bool interrupt = false;
        if (CanWallClimb(dir))
        {
            Velocity = new(0, MovementParams.ClimbSpeed);
        }
        else
        {
            // likely reached top of wall
            Velocity = Vector2.zero;
        }

        if (!IsInputtingWallClimb(dir))
        {
            interrupt = true;
        }
        else
        {
            if (CanLedgeClimb(dir))
            {
                interrupt = true;
                StartCoroutine(LedgeClimb(dir));
                return;
            }
        }
        if (interrupt)
        {
            GravityMultiplier = 1f;
            SpecialState = SpecialState.Normal;
            return;
        }
        wallCoyoteFrames = maxCoyoteFrames;

    }

    private void DoCollosionChecks()
    {
        if (SpecialState == SpecialState.Dash)
        {
            AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        }
        TryLedgeClimb();
        // if did not successfully start a ledge climb on collision, interrupt spring
        if (SpecialState != SpecialState.LedgeClimb)
        {
            isSprinting = false;
        }
    }

    private void TryLedgeClimb()
    {
        if (SpecialState != SpecialState.LedgeClimb)
        {
            if (MoveDir.x > 0 && CanLedgeClimb(Vector2.right))
            {
                StartCoroutine(LedgeClimb(Vector2.right));
            }
            else if (MoveDir.x < 0 && CanLedgeClimb(Vector2.left))
            {
                StartCoroutine(LedgeClimb(Vector2.left));
            }

        }
    }

    private void StartSprint()
    {
        isSprinting = true;

        AudioManager.Instance?.PlaySoundEffect(audioClips[1], transform, 0.5f);
    }

    private void EndSprint()
    {
        isSprinting = false;
    }

}

[Serializable]
public struct MovementParams
{
    public float WalkSpeed;
    public float SprintSpeed;
    public float JumpSpeed;
    public float ClimbSpeed;
    public float GroundAcceleration, AirAcceleration;
    public float GroundFriction, AirFriction;
    public float JumpGravityMult, HangGravityMult;
    public float MinHangVelocity;
}
