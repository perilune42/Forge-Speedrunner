using UnityEngine;
using UnityEngine.InputSystem.XInput;
using static UnityEngine.InputSystem.InputAction;

public class PlayerMovement : DynamicEntity
{
    public float Speed = 5;

    protected override void Update()
    {
        base.Update();
        
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        
    }

    // MoveInput => (every real frame) bool holdingRight = true;
    // CheckMove => (every fixed frame) move right if holdingRight

    public void MoveInput(CallbackContext context)
    {
        Vector2 inputVec = context.action.ReadValue<Vector2>();
        Debug.Log(inputVec);
        float xinput = inputVec.x;
        Velocity = new Vector2(xinput * Speed, Velocity.y);

    }

    public void JumpInput(CallbackContext context)
    {
        bool jumpInput = context.action.WasPressedThisFrame();
        Debug.Log(inputVec);
        float xinput = inputVec.x;
        Velocity = new Vector2(xinput * Speed, Velocity.y);

    }

}