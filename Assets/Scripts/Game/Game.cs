using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    // timer stuff all moved to Timer class

    public void GoToShop()
    {
        ShopManager.Instance.LoadShop();
        // deactivate all the stuff in the world
        RoomManager.Instance.gameObject.SetActive(false);
    }

    public void ReturnToPlay(bool practiceMode)
    {
        ShopManager.Instance.CloseShop();
        // reset rooms and player
        RoomManager.Instance.gameObject.SetActive(true);
        RoomManager.Instance.Reset();
    }

    void Update()
    {
        RoomManager rm = RoomManager.Instance;
        if(Input.GetKeyDown(KeyCode.R))
            rm.Respawn();
        if(Input.GetKeyDown(KeyCode.Z))
            rm.Reset();
    }
}
