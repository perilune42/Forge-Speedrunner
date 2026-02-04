using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    // timer stuff all moved to Timer class

    public void GoToShop()
    {
        ShopManager.Instance.LoadShop();
        // deactivate all the stuff in the world
    }

    public void ReturnToPlay(bool practiceMode)
    {
        ShopManager.Instance.CloseShop();
        // reset rooms and player
    }
}
