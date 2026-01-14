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

    

    public void MoveInput(CallbackContext context)
    {
        Vector2 inputVec = context.action.ReadValue<Vector2>();
        Debug.Log(inputVec);
        float xinput = inputVec.x;
        Velocity = new Vector2(xinput * Speed, Velocity.y);

    }

}