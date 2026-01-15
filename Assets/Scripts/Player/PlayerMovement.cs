using System;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Timeline;
using UnityEngine.Windows;
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

    // this is awful, remove later
    [SerializeField]
    private InputActionReference moveAction;

    [SerializeField] TMP_Text speedText;


    protected override void Update()
    {
        base.Update();
        speedText.SetText(Velocity.ToString());
    }

    protected override void FixedUpdate()
    {
        moveDir = moveAction.action.ReadValue<Vector2>();
        moveDir = new Vector2(Mathf.Round(moveDir.x), Mathf.Round(moveDir.y));
        base.FixedUpdate();

        

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
            if ((targetXVel < 0 && Velocity.x > targetXVel) || (targetXVel > 0 && Velocity.x < targetXVel))
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

        TickTimers();

    }

    private void TickTimers()
    {
        if (jumpFrames > 0)
        {
            jumpFrames--;
            if (jumpFrames == 0)
            {
                EndJump();
            }
        }
            
    }
    

    

    protected override void OnGrounded(RaycastHit2D groundHit)
    {
        base.OnGrounded(groundHit);
        onGround?.Invoke();
    }
    // MoveInput => (every real frame) bool holdingRight = true;
    // CheckMove => (every fixed frame) move right if holdingRight

    private bool CanJump()
    {
        return State == BodyState.OnGround;
    }

    public void MoveInput(CallbackContext context)
    {
        Vector2 moveInput = context.action.ReadValue<Vector2>();
        float xinput = Mathf.Sign(moveInput.x);
        moveDir = new Vector2(xinput, 0);  // potentially allow for vertical movement inputs later
    }

    public void JumpInput(CallbackContext context)
    {
        bool jumpInput = context.action.WasPressedThisFrame();
        if (jumpInput && CanJump())
        {
            Jump();
        }
        if (context.action.WasReleasedThisFrame())
        {
            jumpFrames = Mathf.Min(3, jumpFrames);
        }
    }

    private void Jump()
    {
        Velocity = new Vector2(Velocity.x, MovementParams.JumpSpeed);
        jumpFrames = MaxJumpFrames;
        GravityMultiplier = 0.3f;
        onJump?.Invoke();
    }

    private void EndJump()
    { 
        // a minimum jump lasts 4 frames
        jumpFrames = 0;
        GravityMultiplier = 1;
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
}