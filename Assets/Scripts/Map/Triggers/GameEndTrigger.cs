using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndTrigger : Trigger 
{
    [SerializeField] bool returnToMenu = false;

    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        if (returnToMenu)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
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

}
