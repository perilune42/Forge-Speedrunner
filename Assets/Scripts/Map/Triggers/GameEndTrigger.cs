using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndTrigger : Trigger 
{
    [SerializeField] bool returnToMenu = false;
    [SerializeField] bool win = false;

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
                if (win)
                {
                    Game.Instance.WinGame();
                }
                else if (Game.Instance.GetDataCollected() < Game.Instance.DataToWin)
                {
                    Game.Instance.FinishRound();
                }
            }
            else
            {
                Game.Instance.GoToShop(false);
            }
        }

    }

}
