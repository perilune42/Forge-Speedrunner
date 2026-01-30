using UnityEngine;
using UnityEngine.InputSystem;

public class PInput : Singleton<PInput>
{
    // Use in other classes
    public Vector2 MoveVector;
    public InputButton Jump, Dash, GroundSlam, Grapple, Interact,Ricochet, Map;

    // Internal vars
    private InputAction move;

    public bool EnableControls = true;
    public Vector2 MoveInputOverrride = Vector2.zero;
    public class InputButton
    {
        private bool stoppedPressing;
        private InputAction action;
        private int bufferFrames, bufferFramesLeft;

        private bool queuePress, queueHold, queueRelease;
        private bool hasPressed;
        private bool isPressing;

        public bool HasPressed { get => hasPressed && PInput.Instance.EnableControls; set => hasPressed = value; }
        public bool IsPressing { get => isPressing && PInput.Instance.EnableControls; set => isPressing = value; }
        public bool StoppedPressing { get => stoppedPressing && PInput.Instance.EnableControls; set => stoppedPressing = value; }

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
        Grapple = new InputButton(InputSystem.actions.FindAction("Grapple"), 8);
        Interact = new InputButton(InputSystem.actions.FindAction("Interact"), 8);
        Ricochet = new InputButton(InputSystem.actions.FindAction("Ricochet"), 8);
        Map = new InputButton(InputSystem.actions.FindAction("Map"), 1);
    }

    private void Update()
    {
        if (MoveInputOverrride == Vector2.zero)
        {
            MoveVector = move.ReadValue<Vector2>();
        }
        else 
        { 
            MoveVector = MoveInputOverrride; 
        }
        Jump.Update();
        Dash.Update();
        GroundSlam.Update();
        Grapple.Update();
        Ricochet.Update();
        Map.Update();
        Interact.Update();
    }

    private void FixedUpdate()
    {
        Jump.FixedUpdate();
        Dash.FixedUpdate();
        GroundSlam.FixedUpdate();
        Grapple.FixedUpdate();
        Ricochet.FixedUpdate();
        Map.FixedUpdate();
        Interact.FixedUpdate();
    }
}
