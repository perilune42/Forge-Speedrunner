using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : Ability
{
    private bool canDash;
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private float cooldown, dashDuration;
    private float curCooldown, curDashDuration;
    private bool dashing;
    [SerializeField] private float dashVelocity;
    private List<Vector2> inputs = new();
    [SerializeField] private float inputStickyDuration;
    [HideInInspector] public bool CanDiagonalDash;
    private Vector2 moveSpeedSnapshot;
    public override void Initialize()
    {
        base.Initialize();
        PlayerMovement.onGround += () => canDash = true;
        moveActionReference.action.performed += ctx => StartCoroutine(SetDashDirection(ctx));
    }
    
    private void Update()
    {
        if (dashing)
        {
            curCooldown -= Time.deltaTime;
            curDashDuration -= Time.deltaTime;
            if (curDashDuration <= 0f)
            {
                dashing = false;
                PlayerMovement.Velocity = moveSpeedSnapshot;
            }
        }
    }
    
    public override float GetCooldown()
    {
        if (!canDash) return 0.0f;
        return (cooldown - curCooldown) / cooldown;
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
        moveSpeedSnapshot = PlayerMovement.Velocity;
        PlayerMovement.Velocity = dashVec * dashVelocity;
        canDash = false;
        curCooldown = cooldown;
        curDashDuration = dashDuration;
        dashing = true;
        return true;
    }

    private IEnumerator SetDashDirection(InputAction.CallbackContext context)
    {
        Vector2 inputVec = context.action.ReadValue<Vector2>();
        inputs.Add(inputVec);
        Debug.Log(inputVec);
        yield return new WaitForSeconds(inputStickyDuration);
        inputs.Remove(inputVec);
    }
}
