using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour, IPointerEnterHandler
{
    public bool IsBought;

    private int index;
    private UpgradeData data;
    private bool usesCharges;
    
    [SerializeField] private Image UpgradeImage;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text CostText;
    [SerializeField] private TMP_Text ChargeText;

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

            ProgressionData.Instance.AbilityDataArray[index].Level++;
            ProgressionData.Instance.AbilityDataArray[index].UsesCharges = usesCharges;
        }
    }

    public void Init(int abilityIndex, bool usesCharges = false)
    {
        // Set data
        index = abilityIndex;
        AbilityData abilityData = ProgressionData.Instance.AbilityDataArray[abilityIndex];
        data = abilityData.Upgrades[abilityData.Level];
        this.usesCharges = usesCharges;

        // Set UI elements
        UpgradeImage.sprite = data.Icon;
        NameText.text = data.Name;
        CostText.text = $"${data.Cost}";
        ChargeText.text = usesCharges ? $"({abilityData.MaxCharges})" : "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShopManager.Instance.ShowUpgradeInfo(data);
    }
}
