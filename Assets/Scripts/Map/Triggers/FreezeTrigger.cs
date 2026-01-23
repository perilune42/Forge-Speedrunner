using UnityEngine;
using UnityEngine.SceneManagement;

public class FreezeTrigger : Trigger 
{
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        Player.Instance.Movement.Locked = true;
    }

}
