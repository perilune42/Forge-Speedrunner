using UnityEngine;
using UnityEngine.InputSystem;

public class PInput : Singleton<PInput>
{
    // Use in other classes
    public Vector2 MoveVector;
    public InputButton Jump, Dash, Ability1;

    // Internal vars
    private InputAction move;

    public class InputButton
    {
        public bool HasPressed, IsPressing, StoppedPressing;
        private InputAction action;
        private int bufferFrames, bufferFramesLeft;

        public InputButton(InputAction act, int buf)
        {
            action = act;
            bufferFrames = buf;
        }

        public void Update()
        {
            if (action.WasPressedThisFrame())
            {
                HasPressed = true;
                bufferFramesLeft = bufferFrames;
            }
            else if (bufferFramesLeft <= 0)
            {
                HasPressed = false;
            }
            else
            {
                bufferFramesLeft--;
            }
            IsPressing = action.IsPressed();
            StoppedPressing = action.WasReleasedThisFrame();
        }
    }

    private void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        Jump = new InputButton(InputSystem.actions.FindAction("Jump"), 8);
        Dash = new InputButton(InputSystem.actions.FindAction("Dash"), 8);
        Ability1 = new InputButton(InputSystem.actions.FindAction("Ability1"), 8);
    }

    private void FixedUpdate()
    {
        MoveVector = move.ReadValue<Vector2>();
        Jump.Update();
        Dash.Update();
        Ability1.Update();
    }
}
