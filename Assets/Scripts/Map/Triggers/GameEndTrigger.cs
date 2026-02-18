using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndTrigger : Trigger 
{
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (!Game.Instance.IsPracticeMode)
        {
            Game.Instance.FinishRound();
        }
        else
        {
            Game.Instance.GoToShop(false);
        }
    }

}
