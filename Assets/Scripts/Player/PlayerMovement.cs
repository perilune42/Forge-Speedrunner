using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Timeline;
using static UnityEngine.InputSystem.InputAction;



public class PlayerMovement : DynamicEntity
{
    public MovementParams MovementParams;
    public delegate void OnGround();
    public OnGround onGround;
    public delegate void OnJump();
    public OnJump onJump;

    private Vector2 moveDir;

    public int MaxJumpFrames = 20;
    private int jumpFrames = 0;

    private bool hangTime = false;

    public float PlayerHeight => ((BoxCollider2D)SurfaceCollider).size.y;
    public float PlayerWidth => ((BoxCollider2D)SurfaceCollider).size.x;

    private bool isLedgeClimbing = false;

    [SerializeField] TMP_Text speedText;


    protected override void Update()
    {
        base.Update();
        speedText.SetText(Velocity.ToString());
    }

    protected override void FixedUpdate()
    {
        if (!isLedgeClimbing)
        {
            moveDir = PInput.Instance.MoveVector;
        }
        
        CheckInputs();
        base.FixedUpdate();

        if (Velocity.y < MovementParams.MinHangVelocity) EndHangTime();

        ApplyForces();

        TickTimers();

    }

    private void ApplyForces()
    {
        
        float moveSpeed, moveAccel, friction;
        if (State == BodyState.OnGround)
        {
            moveSpeed = MovementParams.WalkSpeed;
            moveAccel = MovementParams.GroundAcceleration;
            friction = MovementParams.GroundFriction;
        }
        else if (State == BodyState.InAir)
        {
            moveSpeed = MovementParams.AirSpeed;
            moveAccel = MovementParams.AirAcceleration;
            friction = MovementParams.AirFriction;
        }
        else
        {
            moveSpeed = 0;
            moveAccel = 0;
            friction = 0;
        }

        bool ignoreFriction = false;

        // air and ground movement

        
        if (moveDir.x != 0)
        {
            float targetXVel = moveSpeed * moveDir.x;
            // if current speed is below max speed, and the player's movement input helps accelerate
            // the player towards the target max speed
            if ((targetXVel < 0 && Velocity.x >= targetXVel) || (targetXVel > 0 && Velocity.x <= targetXVel))
            {


                float dV = moveAccel * moveDir.x * fdt;
                if (Mathf.Abs(Velocity.x + dV) < Mathf.Abs(targetXVel))
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
            
    }
    

    

    protected override void OnGrounded(RaycastHit2D groundHit)
    {
        base.OnGrounded(groundHit);
        onGround?.Invoke();
    }

    private void CheckInputs()
    {
        if (PInput.Instance.Jump.HasPressed )
        {
            if (CanJump())
            {
                Jump();
                PInput.Instance.Jump.ConsumeBuffer();
            }
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
    }

    private bool CanJump()
    {
        return State == BodyState.OnGround;
    }

    private bool CanLedgeClimb(Vector2 dir)
    {
        return IsTouching(dir) && (State == BodyState.InAir) && GetLedgeHeight(dir) < 0.75 && GetLedgeHeight(dir) > 0;
    }

    private void Jump()
    {
        Velocity = new Vector2(Velocity.x, MovementParams.JumpSpeed);
        jumpFrames = MaxJumpFrames;
        GravityMultiplier = MovementParams.JumpGravityMult;
        onJump?.Invoke();
    }

    private void EndJump(bool force = false)
    { 
        jumpFrames = 0;
        if (PInput.Instance.Jump.IsPressing)
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
        Vector2 offset = SurfaceCollider.offset + new Vector2(PlayerWidth * (dir.x), PlayerHeight);
        Vector2 origin = (Vector2)transform.position + offset;
        Vector2 size = new (PlayerWidth, PlayerHeight);
        RaycastHit2D groundHit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, PlayerHeight * 4, collisionLayer);


        // how much the ledge is above the player's foot
        float ledgeHeight = PlayerHeight - groundHit.distance;
        return ledgeHeight;
    }

    private IEnumerator LedgeClimb(Vector2 dir)
    {
        // ascend the wall at jump speed, do a small hop once the top is reached.
        isLedgeClimbing = true;
        EndJump(force: true);
        GravityMultiplier = 0f;
        Velocity = new Vector2(0, MovementParams.JumpSpeed);
        yield return new WaitForFixedUpdate();
        while (GetLedgeHeight(dir) > 0)
        {
            yield return new WaitForFixedUpdate();
        }
        Velocity = new(dir.x * 2, MovementParams.JumpSpeed / 2);
        GravityMultiplier = 1f;
        isLedgeClimbing = false;
    }

}

[Serializable]
public struct MovementParams
{
    public float WalkSpeed;
    public float AirSpeed;
    public float JumpSpeed;
    public float GroundAcceleration, AirAcceleration;
    public float GroundFriction, AirFriction;
    public float JumpGravityMult, HangGravityMult;
    public float MinHangVelocity;
}