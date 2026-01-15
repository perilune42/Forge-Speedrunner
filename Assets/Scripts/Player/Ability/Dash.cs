using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : Ability
{
    private bool canDash;
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private int cooldown, dashDuration;
    private int curCooldown, curDashDuration;
    private bool dashing;
    [SerializeField] private float dashVelocity;
    private List<Vector2> inputs = new();
    [SerializeField] private float inputStickyDuration;
    public bool CanDiagonalDash;
    private Vector2 dashVelocityVec;
    //private Vector2 moveSpeedSnapshot;
    public override void Start()
    {
        base.Start();
        Debug.Log(PlayerMovement == null);
        PlayerMovement.onJump += CancelDash;
        moveActionReference.action.performed += ctx => StartCoroutine(SetDashDirection(ctx));
    }
    
    
    protected override void Update()
    {
        // Debug.Log(PlayerMovement == null);
        base.Update();
        if (dashing)
        {
            PlayerMovement.Velocity = dashVelocityVec;
        }
    }

    private void FixedUpdate()
    {
        if (PlayerMovement == null)
        {
            return;
        }
        if (PlayerMovement.State == BodyState.OnGround) canDash = true;
        if (dashing)
        {
            
            curCooldown--;
            curDashDuration--;
            if (curDashDuration <= 0)
            {
                dashing = false;
                //PlayerMovement.Velocity = moveSpeedSnapshot;
            }
        }
        if (PInput.Instance.Dash.HasPressed) UseAbility();
    }

    public override float GetCooldown()
    {
        if (!canDash) return 0.0f;
        return (float)(cooldown - curCooldown) / cooldown;
    }

    public void CancelDash()
    {
        dashing = false;
        curDashDuration = 0;
    }

    public override bool UseAbility()
    {
        if (inputs.Count == 0) return false;
        Vector2 dashVec = Vector2.zero;
        for (int i = 0; i < (CanDiagonalDash ? 2 : 1); i++)
        {
            Vector2 vec = inputs[inputs.Count - i - 1];
            if (dashVec - vec != Vector2.zero) dashVec += vec;
        }
        if (dashVec == Vector2.zero) return false;
        //moveSpeedSnapshot = PlayerMovement.Velocity;
        dashVelocityVec = dashVec.normalized * dashVelocity;
        canDash = false;
        curCooldown = cooldown;
        curDashDuration = dashDuration;
        dashing = true;
        return true;
    }

    private IEnumerator SetDashDirection(InputAction.CallbackContext context)
    {
        Vector2 inputVec = context.action.ReadValue<Vector2>();
        // If input is diagonal (player presses up/down and left/right on same frame),
        // default to left/right for dash direction
        if (inputVec.x != 0 && inputVec.y != 0)
        {
            if (CanDiagonalDash) inputs.Add(new Vector2(0, inputVec.y));
            inputVec = new Vector2(inputVec.x, 0);
        }
        inputs.Add(inputVec);
        // Debug.Log(inputVec);
        yield return new WaitForSeconds(inputStickyDuration);
        inputs.Remove(inputVec);
    }
}
