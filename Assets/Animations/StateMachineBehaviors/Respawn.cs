using FMODUnity;
using UnityEngine;

public class Respawn : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.IsDead = true;
        Player.Instance.Movement.Locked = true;
        RuntimeManager.PlayOneShot("event:/Death Sound Shortened");
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        RoomManager.Instance.Respawn();
    }
}
