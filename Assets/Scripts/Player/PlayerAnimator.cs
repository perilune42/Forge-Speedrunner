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
        if (playerMovement.MoveDir != Vector2.zero)
        {
            anim.Play("PlayerWalk");
        }
        else
        {
            anim.Play("PlayerIdle");
        }
    }
}
