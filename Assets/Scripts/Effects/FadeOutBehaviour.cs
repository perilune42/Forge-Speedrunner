using UnityEngine;
using UnityEngine.UI;

public class FadeOutBehaviour : StateMachineBehaviour
{
    Image im = null;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        im = im == null ? animator.GetComponent<Image>() : im;
        im.color = new Vector4(0f,0f,0f,1f);
        // im.transform.rotation = Quaterion.EulerAngles(0f,0f,180f);
        im.transform.eulerAngles = new Vector3(0f,0f,180f);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    // override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     im = im == null ? animator.GetComponent<Image>() : im;
    //     im.color = new Vector4(0f,0f,0f,1f);
    // }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        im = im == null ? animator.GetComponent<Image>() : im;
        animator.GetComponent<Image>().color = new Vector4(0f,0f,0f,1f);
        // do not run if not transitioning 
        // if (!animator.IsInTransition(layerIndex)) return;

        // do not run if not going to FadeIn
        // var next = animator.GetNextAnimatorStateInfo(layerIndex);
        // if (!next.IsName("FadeIn")) return;

        // might want to make this atomic in the future!
        RoomManager.Instance.RunBetweenStates?.Invoke();
        RoomManager.Instance.RunBetweenStates = null;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
