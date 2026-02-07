using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PInput : Singleton<PInput>
{
    // Use in other classes
    public Vector2 MoveVector;
    public InputButton Jump, Dash, Interact, Map;

    // Internal vars
    private InputAction move;

    public bool EnableControls = true;
    public Vector2 MoveInputOverrride = Vector2.zero;

    const float DEADZONE = 0.2f;

    public List<InputButton> AbilityButtons;


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

        public string GetBindingDisplayString()
        {
            string s = action.GetBindingDisplayString();
            
            return Util.FixControlString(s, action);
        }

        public InputAction GetAction()
        {
            return action;
        }
    }

    private void Start()
    {
        AbilityButtons = new();

        move = InputSystem.actions.FindAction("Move");
        Jump = new InputButton(InputSystem.actions.FindAction("Jump"), 8);
        Dash = new InputButton(InputSystem.actions.FindAction("Dash"), 8);
        Interact = new InputButton(InputSystem.actions.FindAction("Interact"), 8);
        Map = new InputButton(InputSystem.actions.FindAction("Map"), 1);
    }

    private void Update()
    {
        if (MoveInputOverrride == Vector2.zero)
        {
            Vector2 rawMove = move.ReadValue<Vector2>();
            MoveVector = GetDirection(rawMove, 0.2f, 22.5f);
        }
        else 
        { 
            MoveVector = MoveInputOverrride; 
        }
        Jump.Update();
        Dash.Update();
        Map.Update();
        Interact.Update();
        foreach (InputButton button in AbilityButtons) button.Update();
    }

    private void FixedUpdate()
    {
        Jump.FixedUpdate();
        Dash.FixedUpdate();
        Map.FixedUpdate();
        Interact.FixedUpdate();
        foreach (InputButton button in AbilityButtons) button.FixedUpdate();
    }

    public InputButton AddAbilityInputButton()
    {
        // Debug.Log("Ability" + (AbilityButtons.Count + 1));
        InputButton button = new InputButton(InputSystem.actions.FindAction("Ability" + (AbilityButtons.Count + 1)), 8);
        AbilityButtons.Add(button);
        return button;
    }

    // AI slop yippee
    private static Vector2 GetDirection(
      Vector2 v,
      float deadzone = 0.2f,
      float diagonalHalfAngle = 22.5f
  )
    {
        if (v.magnitude < deadzone)
            return Vector2Int.zero;

        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        // Cardinal angles
        const float RIGHT = 0f;
        const float UP = 90f;
        const float LEFT = 180f;
        const float DOWN = 270f;

        // Diagonal center angles
        const float UR = 45f;
        const float UL = 135f;
        const float DL = 225f;
        const float DR = 315f;

        if (IsWithin(angle, UR, diagonalHalfAngle)) return new Vector2(1, 1);
        if (IsWithin(angle, UL, diagonalHalfAngle)) return new Vector2(-1, 1);
        if (IsWithin(angle, DL, diagonalHalfAngle)) return new Vector2(-1, -1);
        if (IsWithin(angle, DR, diagonalHalfAngle)) return new Vector2(1, -1);

        // Otherwise cardinal
        if (IsWithin(angle, RIGHT, 45f - diagonalHalfAngle)) return Vector2.right;
        if (IsWithin(angle, UP, 45f - diagonalHalfAngle)) return Vector2.up;
        if (IsWithin(angle, LEFT, 45f - diagonalHalfAngle)) return Vector2.left;
        return Vector2.down;
    }

    private static bool IsWithin(float angle, float target, float halfRange)
    {
        float delta = Mathf.Abs(Mathf.DeltaAngle(angle, target));
        return delta <= halfRange;
    }

    
}
