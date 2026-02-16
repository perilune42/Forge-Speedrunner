using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    public int CurrentRound = 1;

    public void FinishRound()
    {
        Timer.RecordTime();
        CurrentRound++;
        GoToShop(true);
    }

    public void GoToShop(bool newRound)
    {
        ShopManager.Instance.LoadShop(newRound);
        // deactivate all the stuff in the world
        RoomManager.Instance.gameObject.SetActive(false);
        Player.Instance.gameObject.SetActive(false);

        // stop the count
        Timer.Instance.Pause(true);
    }

    public void ReturnToPlay(bool practiceMode)
    {
        ShopManager.Instance.CloseShop();
        // reset rooms and player
        RoomManager.Instance.gameObject.SetActive(true);
        RoomManager.Instance.ResetAllEntities();
        Player.Instance.gameObject.SetActive(true);

        // start the count at 0
        Timer.speedrunTime = 0F;
        Timer.Instance.Pause(false);

        RoomManager.Instance.SpawnAtStart();

        AbilityManager.Instance.ResetAbilites();
        AbilityManager.Instance.RechargeAbilities();

        Player.Instance.Movement.OnReset();
        PInput.Instance.OnReset();
    }



    void Update()
    {
        RoomManager rm = RoomManager.Instance;
        if(Input.GetKeyDown(KeyCode.R))
            rm.ReEnterRoom();
        else if(Input.GetKeyDown(KeyCode.Z))
            ReturnToPlay(false);
        else if(Input.GetKeyDown(KeyCode.X))
            GoToShop(true);
    }
}
