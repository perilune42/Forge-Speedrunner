using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerMovement playerMovement => Player.Instance.Movement;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Util.SetLocalScaleX(gameObject, playerMovement.FacingDir == Vector2.left ? 1 : -1);
    }

    private void Update()
    {
        if (playerMovement.SpecialState == SpecialState.Dash)
        {
            anim.Play("PlayerDash");
        }
        else if (playerMovement.SpecialState == SpecialState.WallClimb)
        {
            anim.Play("PlayerClimb");
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
}
