using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerMovement playerMovement => Player.Instance.Movement;
    private void FixedUpdate()
    {
        Util.SetLocalScaleX(gameObject, playerMovement.FacingDir == Vector2.left ? 1 : -1);
    }
}
