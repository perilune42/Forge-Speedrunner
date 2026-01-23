using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    private int index;
    private UpgradeData data;
    public bool IsBought;

    public Image UpgradeImage;
    public TMP_Text CostText;

    public void BuyUpgrade()
    {
        if (IsBought)
        {
            Debug.Log($"Already bought {data.Name}");
        }
        else if (ShopManager.Instance.Money - data.Cost < 0)
        {
            Debug.Log($"Missing {data.Cost - ShopManager.Instance.Money} money");
        }
        else
        {
            ShopManager.Instance.Money -= data.Cost;
            ShopManager.Instance.UpdateMoney();
            CostText.text = "Bought";
            IsBought = true;

            AbilitySceneTransfer.AbilityDataArray[index].Level++;
        }
    }

    public void Init(int abilityIndex)
    {
        index = abilityIndex;
        var abilityData = AbilitySceneTransfer.AbilityDataArray[abilityIndex];
        this.data = abilityData.Upgrades[abilityData.Level];
        UpgradeImage.sprite = this.data.Icon;
        CostText.text = this.data.Name;
    }
}
