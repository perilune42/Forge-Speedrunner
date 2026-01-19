using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    public UpgradeData Data;
    public bool IsBought;

    public Image UpgradeImage;
    public TMP_Text CostText;

    public void BuyUpgrade()
    {
        if (IsBought)
        {
            Debug.Log($"Already bought {Data.Name}");
        }
        else if (ShopManager.Instance.Money - Data.Cost < 0)
        {
            Debug.Log($"Missing {Data.Cost - ShopManager.Instance.Money} money");
        }
        else
        {
            ShopManager.Instance.Money -= Data.Cost;
            ShopManager.Instance.UpdateMoney();
            CostText.text = "Bought";
            IsBought = true;

            // TODO - Actual gameplay effects from buying upgrade (add player ability, modify player stats, etc.)
        }
    }

    public void Init(UpgradeData data)
    {
        Data = data;
        UpgradeImage.sprite = data.Icon;
        CostText.text = data.Name;
    }
}
