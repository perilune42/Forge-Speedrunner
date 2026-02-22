using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum SpecialState
{
    Normal, Dash, LedgeClimb, GroundSlam, WallClimb, Zipline, WallLatch, Rocket, Teleport, Chronoshift
}

public class PlayerMovement : DynamicEntity, IStatSource
{
    public Collider2D Hurtbox;


    public MovementParams MovementParams;
    public Action onGround;
    public Action onGroundTop;
    public Action onJump;

    public Vector2 MoveDir;
    public Vector2 FacingDir = Vector2.right;

    public int MaxJumpFrames = 20;
    public int jumpFrames = 0;

    private int coyoteFrames = 0;
    [SerializeField]
    private int wallCoyoteFrames = 0;
    private Vector2 lastClimbDir = Vector2.right;
    private int maxCoyoteFrames = 6;
    private int maxWallCoyoteFrames = 8;

    private int forceMoveFrames = 0;
    private int maxForceMoveFrames = 10;

    private float retainedSpeed = 0;
    private bool hangTime = false;

    private bool isSprinting = false;

    public float PlayerHeight => ((BoxCollider2D)SurfaceCollider).size.y;
    public float PlayerWidth => ((BoxCollider2D)SurfaceCollider).size.x;

    public bool CanClimb = true;

    public bool CanJumpOverride;
    

    public SpecialState SpecialState { get => specialState; 
        set {
            OnSpecialStateChange?.Invoke(value);
            specialState = value;
        } }

    [SerializeField] private SpecialState specialState;

    [SerializeField] private List<AudioClip> audioClips;

    [HideInInspector] public Vector2 PreCollisionVelocity;

    private const float ledgeClimbHeight = 1f;
    [HideInInspector] public float LedgeClimbBonus = 0;

    public Action<SpecialState> OnSpecialStateChange;   // called before change

    private class JumpGravityMult : IStatSource { }
    JumpGravityMult jumpGravityMult = new();

    private class ClimbGravityMult : IStatSource { }
    ClimbGravityMult climbGravityMult = new();

    public VecStat RelativeVelocity = new VecStat(Vector2.zero);
    // apparent velocity of the surface this entity is resting on
    // for the purposes of friction

    protected override void Awake()
    {
        base.Awake();
        OnSpecialStateChange += (newState) =>
        {
            if (newState != SpecialState.WallClimb && newState != SpecialState.LedgeClimb)
            {
                GravityMultiplier.Multipliers[climbGravityMult] = 1f;
            }
        };
    }

    private void Start()
    {
        AbilityManager.Instance.GetAbility<Dash>().OnActivate += StartSprint;
        OnHitWallLeft += DoCollosionChecks;
        OnHitWallRight += DoCollosionChecks;

        FacingDir = Vector2.right;

    }

    public void OnReset()
    {
        Locked = false;
        State = BodyState.InAir;
        SpecialState = SpecialState.Normal;
        forceMoveFrames = 0;
        coyoteFrames = 0;
        jumpFrames = 0;
        GravityMultiplier.Reset();
        RelativeVelocity.Reset();
        CanJumpOverride = false;
        CanClimb = true;
        hangTime = false;
        retainedSpeed = 0;
        Velocity = Vector2.zero;
    }


    protected override void Update()
    {
        base.Update();

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (CanChangeMoveDir())
        {
            MoveDir = PInput.Instance.MoveVector.NormalizePerAxis();
        }
        if (CanChangeFacingDir())
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

        if (SpecialState == SpecialState.LedgeClimb)
        {
            ContinueLedgeClimb(lastClimbDir);
        }

        if (State == BodyState.OnGround)
        {
            coyoteFrames = maxCoyoteFrames;
        }
        else if (coyoteFrames > 0)
        {
            coyoteFrames--;
        }

        if (SpecialState != SpecialState.WallClimb && wallCoyoteFrames > 0)
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

    private bool CanChangeMoveDir()
    {
        return (SpecialState == SpecialState.Normal && forceMoveFrames == 0)
            || SpecialState == SpecialState.GroundSlam
            || SpecialState == SpecialState.WallLatch
            || SpecialState == SpecialState.Rocket;
    }

    private bool CanChangeFacingDir()
    {
        return SpecialState == SpecialState.Normal 
            || SpecialState == SpecialState.GroundSlam
            || SpecialState == SpecialState.WallLatch
            || SpecialState == SpecialState.Rocket;
    }

    public override void ApplyMovement(Vector2 move)
    {
        PreCollisionVelocity = Velocity;
        base.ApplyMovement(move);
    }

    private void ApplyForces()
    {
        Vector2 apparentVel = Velocity - RelativeVelocity.Get();
        float moveSpeed, moveAccel, friction;
        moveSpeed = isSprinting ? MovementParams.SprintSpeed : MovementParams.WalkSpeed;

        if (State == BodyState.OnGround)
        {
            moveAccel = MovementParams.GroundAcceleration;
            friction = Util.SignOr0(apparentVel.x) == Util.SignOr0(MoveDir.x) ? MovementParams.MovingFriciton : MovementParams.GroundFriction;
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
            if ((targetXVel < 0 && apparentVel.x >= targetXVel) || (targetXVel > 0 && apparentVel.x <= targetXVel))
            {


                float dV = moveAccel * MoveDir.x * fdt;
                if ((targetXVel > 0 && apparentVel.x + dV < targetXVel)
                    || (targetXVel < 0 && apparentVel.x + dV > targetXVel))
                {
                    apparentVel.x += dV;
                }
                else
                {
                    apparentVel.x = targetXVel;
                }

                // do not apply friction if player is attempting to move in the same direction of acceleration
                ignoreFriction = Mathf.Sign(targetXVel) == Mathf.Sign(apparentVel.x);
            }
            // otherwise walking in the same direction as movement does nothing
        }

        // friction

        if (!Mathf.Approximately(apparentVel.x, 0) && !ignoreFriction)
        {
            float dV = -Util.SignOr0(apparentVel.x) * friction * fdt;
            if ((apparentVel.x > 0 && apparentVel.x > Mathf.Abs(dV)) || (apparentVel.x < 0 && Mathf.Abs(apparentVel.x) > dV))
            {
                apparentVel.x += dV;
            }
            else
            {
                apparentVel.x = 0;
            }
        }

        Velocity = apparentVel + RelativeVelocity.Get();
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

    public void ForceMove(Vector2 dir, int frames)
    {
        MoveDir = dir;
        forceMoveFrames = frames;
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

    public override void OnGroundedTop(RaycastHit2D groundHit)
    {
        base.OnGroundedTop(groundHit);
        onGroundTop?.Invoke();
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
            bool canWallClimb = CanWallClimb(new Vector2(dir.x, 0));
            if (canWallClimb) wallCoyoteFrames = maxWallCoyoteFrames;
            if (MoveDir.x != -dir.x
                && IsInputtingWallClimb(dir)
                && !CanLedgeClimb(new Vector2(dir.x, 0))
                && canWallClimb
                && SpecialState != SpecialState.WallClimb)
            {
                StartWallClimb(new Vector2(dir.x, 0));
                break;
            }
            dir = Vector2.left;
        }


    }

    public bool CanJump(bool canOverride = true)
    {
        return (canOverride && CanJumpOverride) || (
            (State == BodyState.OnGround || coyoteFrames > 0)
            && (SpecialState == SpecialState.Normal || SpecialState == SpecialState.Dash)
            );
    }

    public bool CanWallJump()
    {
        return SpecialState == SpecialState.WallClimb || wallCoyoteFrames > 0;
    }

    private bool CanLedgeClimb(Vector2 dir)
    {
        if (SpecialState != SpecialState.Normal && SpecialState != SpecialState.Dash 
            && SpecialState != SpecialState.WallClimb && SpecialState != SpecialState.LedgeClimb) return false;
        return CanClimb && IsTouching(dir) 
            && GetLedgeHeight(dir) < (ledgeClimbHeight + LedgeClimbBonus) 
            && GetLedgeHeight(dir) > 0
            && !HazardOnLedge(dir);
    }

    public bool CanWallClimb(Vector2 dir, bool wallLatch = false)
    {
        if (!wallLatch)
        {
            if (SpecialState != SpecialState.Normal && SpecialState != SpecialState.Dash && SpecialState != SpecialState.WallClimb) return false;
            if (!CanClimb) return false;
        }
        if (HazardOnLedge(dir)) return false;
        Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.5f * PlayerHeight);
        Vector2 size = new(PlayerWidth, wallLatch ? PlayerHeight * 1.1f : PlayerHeight);
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
        GravityMultiplier.Multipliers[jumpGravityMult] = MovementParams.JumpGravityMult;

        AudioManager.Instance?.PlaySoundEffect(audioClips[0], transform, 0.5f);
    }

    private void WallJump(Vector2 wallDir)
    {
        SpecialState = SpecialState.Normal;
        GravityMultiplier.Multipliers.Remove(climbGravityMult);
        float xVel = -wallDir.x * MovementParams.JumpSpeed;
        Velocity = new Vector2(xVel, MovementParams.JumpSpeed);
        // player not allowed to turn around for 5 frames
        ForceMove(new Vector2(-wallDir.x, 0), maxForceMoveFrames);
        jumpFrames = MaxJumpFrames;
        wallCoyoteFrames = 0;
        onJump?.Invoke();
        GravityMultiplier.Multipliers[jumpGravityMult] = MovementParams.JumpGravityMult;
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
        GravityMultiplier.Multipliers[jumpGravityMult] = MovementParams.HangGravityMult;
        hangTime = true;
    }

    private void EndHangTime()
    {
        GravityMultiplier.Multipliers.Remove(jumpGravityMult);
        hangTime = false;
    }

    private float GetLedgeHeight(Vector2 dir)
    {
        // start a boxcast upwards and to either the left and right of the player, pointing downwards
        Vector2 offset = SurfaceCollider.offset + new Vector2(PlayerWidth * (dir.x), PlayerHeight - PlayerHeight * 0.45f) ;
        Vector2 origin = (Vector2)transform.position + offset;
        Vector2 size = new (PlayerWidth, PlayerHeight * 0.1f);
        RaycastHit2D groundHit = CustomBoxCast(origin, size, 0f, Vector2.down, PlayerHeight * 4, collisionLayer);


        // how much the ledge is above the player's foot
        float ledgeHeight = PlayerHeight - groundHit.distance;
        return ledgeHeight;
    }

    private bool HazardOnLedge(Vector2 dir)
    {
        // start a boxcast upwards and to either the left and right of the player, pointing downwards
        Vector2 offset = SurfaceCollider.offset + new Vector2(PlayerWidth * (dir.x), PlayerHeight - PlayerHeight * 0.45f);
        Vector2 origin = (Vector2)transform.position + offset;
        Vector2 size = new(PlayerWidth, PlayerHeight * 0.1f);
        var hitEntites = CustomBoxCastAll(origin, size, 0f, Vector2.down, PlayerHeight * 4, interactLayer);

        if (hitEntites.Where(e => e.collider != null && e.collider.GetComponent<Hazard>() != null).Count() > 0)
        {
            return true;
        }
        return false;
    }


    private void StartLedgeClimb(Vector2 dir)
    {
        // ascend the wall at jump speed, do a small hop once the top is reached.
        SpecialState = SpecialState.LedgeClimb;
        EndJump(force: true);
        GravityMultiplier.Multipliers[climbGravityMult] = 0f;
        Velocity = new Vector2(0, MovementParams.ClimbSpeed);
        lastClimbDir = MoveDir;
        if (State == BodyState.OnGround)
        {
            OnAirborne();
        }
    }

    private void ContinueLedgeClimb(Vector2 dir)
    {
        if (!CanLedgeClimb(dir))
        {
            GravityMultiplier.Multipliers.Remove(climbGravityMult);
            SpecialState = SpecialState.Normal;
        }
        if (GetLedgeHeight(dir) > 0)
        {
            Velocity = new Vector2(0, MovementParams.ClimbSpeed);
            return;
        }
        const float minLedgeBoost = 5f;

        float alignedRetainedSpeed = Util.SignOr0(retainedSpeed) == dir.x ? Mathf.Abs(retainedSpeed) : 0;
        float boost = Mathf.Max(alignedRetainedSpeed, minLedgeBoost);
        Velocity = new(boost * dir.x, MovementParams.JumpSpeed / 2);
        GravityMultiplier.Multipliers.Remove(climbGravityMult);
        SpecialState = SpecialState.Normal;
    }

    private void StartWallClimb(Vector2 dir)
    {
        // ascend the wall at jump speed while the input is being held.
        SpecialState = SpecialState.WallClimb;
        MoveDir = new Vector2(dir.x, 0);
        EndJump(force: true);
        GravityMultiplier.Multipliers[climbGravityMult] = 0f;
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
                wallCoyoteFrames = 0;
                StartLedgeClimb(dir);
                return;
            }
        }
        if (interrupt)
        {
            GravityMultiplier.Multipliers.Remove(climbGravityMult);
            SpecialState = SpecialState.Normal;
            return;
        }
        wallCoyoteFrames = maxWallCoyoteFrames;

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
                StartLedgeClimb(Vector2.right);
            }
            else if (MoveDir.x < 0 && CanLedgeClimb(Vector2.left))
            {
                StartLedgeClimb(Vector2.left);
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

    public Vector3 GetCenterPos()
    {
        return SurfaceCollider.bounds.center;
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
    public float MovingFriciton;    // friction when you are moving along a direction
    public float JumpGravityMult, HangGravityMult;
    public float MinHangVelocity;
}
