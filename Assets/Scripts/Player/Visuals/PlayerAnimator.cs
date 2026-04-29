using UnityEngine;

public class PlayerAnimator : Singleton<PlayerAnimator>
{
    private PlayerMovement playerMovement => Player.Instance.Movement;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (Player.Instance.IsDead) return;
        if (playerMovement.SpecialState == SpecialState.WallLatch)
        {
            Util.SetLocalScaleX(gameObject, AbilityManager.Instance.GetAbility<WallLatch>().latchedDirection == Vector2.right ? 1 : -1);
        }
        else if (playerMovement.SpecialState == SpecialState.WallLatch
            || playerMovement.SpecialState == SpecialState.WallClimb
            && playerMovement.Velocity.y == 0) {
            // scuffed reversing fix
            Util.SetLocalScaleX(gameObject, playerMovement.FacingDir == Vector2.left ? -1 : 1);

        }
        else
        {
            Util.SetLocalScaleX(gameObject, playerMovement.FacingDir == Vector2.left ? 1 : -1);
        }
            
    }

    private void Update()
    {
        if (Player.Instance.IsDead) return;
        if (playerMovement.SpecialState == SpecialState.Dash)
        {
            anim.Play("PlayerDash");
        }
        else if ((playerMovement.SpecialState == SpecialState.WallClimb 
            || playerMovement.SpecialState == SpecialState.LedgeClimb)
            && playerMovement.Velocity.y != 0)
        {
            anim.Play("PlayerClimb");
        }
        else if (playerMovement.SpecialState == SpecialState.WallLatch
            || playerMovement.SpecialState == SpecialState.WallClimb
            || playerMovement.SpecialState == SpecialState.LedgeClimb)
        {
            anim.Play("PlayerHang");
        }
        else if (playerMovement.State == BodyState.InAir)
        {
            if (playerMovement.jumpFrames > 0)
            {
                anim.Play("PlayerJump");
            }
            else
            {
                anim.Play("PlayerFall");
            }
        }
        else if (playerMovement.MoveDir.x != 0)
        {
            anim.Play("PlayerWalk");
        }
        else
        {
            anim.Play("PlayerIdle");
        }
    }

    [ContextMenu("Kill Player")]
    public void DieWithAnimation()
    {
        if (Player.Instance.IsDead) return;
        
        anim.Play("PlayerDeath");
        // Animations/StateMachineBehaviors/Respawn script happens when animation plays
    }
}
