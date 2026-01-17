using UnityEngine;
using UnityEngine.InputSystem;

public class PInput : Singleton<PInput>
{
    // Use in other classes
    public Vector2 MoveVector;
    public InputButton Jump, Dash, GroundSlam;

    // Internal vars
    private InputAction move;

    public class InputButton
    {
        public bool HasPressed, IsPressing, StoppedPressing;
        private InputAction action;
        private int bufferFrames, bufferFramesLeft;

        private bool queuePress, queueHold, queueRelease;

        public InputButton(InputAction act, int buf)
        {
            action = act;
            bufferFrames = buf;
        }

        public void Update()
        {
            if (action.WasPressedThisFrame())
            {
                queuePress = true;
            }
            queueHold = action.IsPressed();
            if (action.WasReleasedThisFrame())
            {
                queueRelease = true;
            }
        }

        public void FixedUpdate()
        {
            if (queuePress)
            {
                HasPressed = true;
                bufferFramesLeft = bufferFrames;
                queuePress = false;
            }
            else if (bufferFramesLeft <= 0)
            {
                HasPressed = false;
            }
            else
            {
                bufferFramesLeft--;
            }
            if (queueHold)
            {
                IsPressing = true;
                queueHold = false;
            }
            else
            {
                IsPressing = false;
            }
            if (queueRelease) {
                StoppedPressing = true;
                queueRelease = false;
            }
            else
            {
                StoppedPressing = false;
            }
        }

        public void ConsumeBuffer()
        {
            // a valid input was accepted, clear the input buffer
            bufferFramesLeft = 0;
        }
    }

    private void Start()
    {
        move = InputSystem.actions.FindAction("Move");
        Jump = new InputButton(InputSystem.actions.FindAction("Jump"), 8);
        Dash = new InputButton(InputSystem.actions.FindAction("Dash"), 8);
        GroundSlam = new InputButton(InputSystem.actions.FindAction("GroundSlam"), 8);
    }

    private void Update()
    {
        MoveVector = move.ReadValue<Vector2>();
        Jump.Update();
        Dash.Update();
        GroundSlam.Update();
    }

    private void FixedUpdate()
    {
        Jump.FixedUpdate();
        Dash.FixedUpdate();
        GroundSlam.FixedUpdate();
    }
}
