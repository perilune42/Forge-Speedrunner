using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndTrigger : Trigger 
{
    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        Timer.RecordTime();
        SceneManager.LoadScene("Shop");
    }

}
