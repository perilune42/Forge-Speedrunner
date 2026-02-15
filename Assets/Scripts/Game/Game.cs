using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    // timer stuff all moved to Timer class

    public void GoToShop()
    {
        ShopManager.Instance.LoadShop();
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
            GoToShop();
    }
}
