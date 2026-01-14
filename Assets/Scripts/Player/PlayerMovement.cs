using System;
using UnityEngine;
using UnityEngine.InputSystem.XInput;
using UnityEngine.Windows;
using static UnityEngine.InputSystem.InputAction;

public class PlayerMovement : DynamicEntity
{
    public float MoveSpeed = 5;
    public float JumpSpeed = 5;
    public delegate void OnGround();
    public OnGround onGround;
    public delegate void OnJump();
    public OnJump onJump;
    protected override void Update()
    {
        base.Update();
        
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        
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
        Vector2 inputVec = context.action.ReadValue<Vector2>();
        float xinput = inputVec.x;
        Velocity = new Vector2(xinput * MoveSpeed, Velocity.y);

    }

    public void JumpInput(CallbackContext context)
    {
        bool jumpInput = context.action.WasPressedThisFrame();
        if (jumpInput && CanJump())
        {
            Velocity = new Vector2(Velocity.x, JumpSpeed);
            onJump?.Invoke();
        }

    }

}