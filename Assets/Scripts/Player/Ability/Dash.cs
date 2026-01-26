using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dash : Ability
{
    private bool canDash;
    [SerializeField] private int dashDuration;
    private int curDashDuration;
    [SerializeField] private float dashVelocity;
    public bool CanDiagonalDash; // set to false, when you upgrade, it becomes true
    private Vector2 dashVelocityVec;

    [SerializeField] private ParticleSystemRenderer particleRenderer;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private List<Material> particleMaterials;
    private SpriteRenderer playerSpriteRenderer;
    //private Vector2 moveSpeedSnapshot;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();

        PlayerMovement.onJump += () =>
        {
            CancelDash();
        };
        playerSpriteRenderer = PlayerMovement.GetComponentInChildren<SpriteRenderer>();
    }

    private bool dashing;


    

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (PlayerMovement == null)
        {
            return;
        }
        if (PlayerMovement.State == BodyState.OnGround) canDash = true;
        
        if (PlayerMovement.SpecialState == SpecialState.Dash)
        {
            PlayerMovement.Velocity = dashVelocityVec;
            PlayerMovement.GravityMultiplier = 0f;
            curDashDuration--;
            if (curDashDuration <= 0)
            {
                PlayerMovement.SpecialState = SpecialState.Normal;
                CancelDash();
            }
        }
        else if (dashing) CancelDash();
        if (PInput.Instance.Dash.HasPressed)
        {
            UseAbility();
            
        }
    }

    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool CanUseAbility()
    {
        return base.CanUseAbility() && canDash;
    }

    public void CancelDash()
    {
        if (!dashing) return;
        PlayerMovement.SpecialState = SpecialState.Normal;
        dashing = false;
        PlayerMovement.GravityMultiplier = 1f;
        curDashDuration = 0;
        particle.Stop();
    }
     
    public override bool UseAbility()
    {
        if (!CanUseAbility()) return false;
        PInput.Instance.Dash.ConsumeBuffer();
        // dash overrides forcemove
        PlayerMovement.EndForceMove();

        Vector2 dashVec = PlayerMovement.MoveDir;
        if (dashVec == Vector2.zero)
        {
            dashVec = PlayerMovement.FacingDir;
        }
        // interpret up/down inputs as diagonal, if possible
        if (dashVec == Vector2.up || dashVec == Vector2.down) 
        {
            dashVec.x = PlayerMovement.FacingDir.x;
        }
        if (!CanDiagonalDash) dashVec.y = 0;
        else if (dashVec.x == 0) return false; // no up-dash or down-dash
        dashVelocityVec = dashVec.normalized * dashVelocity;
        canDash = false;
        curCooldown = cooldown;
        curDashDuration = dashDuration;
        dashing = true;
        // particle effects
        
        PlayerMovement.SpecialState = SpecialState.Dash;
        particle.Play();
        particleMaterials[0].mainTexture = playerSpriteRenderer.sprite.texture;
        particleRenderer.SetMaterials(particleMaterials);
        particleRenderer.flip = Vector3.right * (PlayerMovement.FacingDir.x < 0 ? 1 : 0);

        base.UseAbility();
        
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
