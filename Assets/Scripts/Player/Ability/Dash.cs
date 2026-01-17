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
    [SerializeField] private float dashVelocity;
    public bool CanDiagonalDash;
    private Vector2 dashVelocityVec;
    //private Vector2 moveSpeedSnapshot;
    public override void Start()
    {
        base.Start();
        PlayerMovement.onJump += CancelDash;
    }

    private bool dashing => Player.Instance.Movement.SpecialState == SpecialState.Dash;


    protected override void Update()
    {
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
        if (curCooldown > 0) curCooldown--;
        if (dashing)
        {
            curDashDuration--;
            if (curDashDuration <= 0)
            {
                Player.Instance.Movement.SpecialState = SpecialState.Normal;
            }
        }
        if (PInput.Instance.Dash.HasPressed) UseAbility();
    }

    public override float GetCooldown()
    {
        if (!canDash) return 0.0f;
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool CanUseAbility()
    {
        return base.CanUseAbility() && canDash && PInput.Instance.MoveVector != Vector2.zero;
    }

    public void CancelDash()
    {
        Player.Instance.Movement.SpecialState = SpecialState.Normal;
        curDashDuration = 0;
    }
     
    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;
        Vector2 dashVec = PInput.Instance.MoveVector;
        if (!CanDiagonalDash) dashVec.y = 0;
        else if (dashVec.x == 0) return false; // no up-dash or down-dash
        dashVelocityVec = dashVec.normalized * dashVelocity;
        canDash = false;
        curCooldown = cooldown;
        curDashDuration = dashDuration;
        Player.Instance.Movement.SpecialState = SpecialState.Dash;
        return true;
    }

    /*private IEnumerator SetDashDirection(InputAction.CallbackContext context)
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
    }*/
}
