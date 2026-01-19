using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndTrigger : Trigger 
{
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        SceneManager.LoadScene("Shop");
    }

}
