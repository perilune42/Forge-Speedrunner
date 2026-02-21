using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour, IPointerEnterHandler
{
    public bool IsBought;

    private Ability ability;
    private int levelToUpgrade;
    private bool usesCharges;
    
    [SerializeField] private Image UpgradeImage;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text CostText;
    [SerializeField] private TMP_Text ChargeText;

    private int cost => ability.AllLevels[levelToUpgrade].Cost;
    private bool HasEnoughMoney => ShopManager.Instance.Money - cost >= 0;
    private bool CanBuy => !IsBought && HasEnoughMoney;

    private void Update()
    {
        // TODO - optimize
        if (CanBuy)
        {
            UpgradeImage.color = Color.white;
            NameText.color = Color.white;
            CostText.color = Color.white;
            ChargeText.color = Color.white;
        }
        else
        {
            UpgradeImage.color = Color.gray;
            NameText.color = Color.lightGray;
            CostText.color = Color.lightGray;
            ChargeText.color = Color.lightGray;
        }
    }

    public void BuyUpgrade()
    {
        if (IsBought)
        {
            Debug.Log($"Already bought {ability.Name}");
        }
        else if (!HasEnoughMoney)
        {
            Debug.Log($"Missing {cost - ShopManager.Instance.Money} money");
        }
        else
        {
            ShopManager.Instance.Money -= cost;
            ShopManager.Instance.UpdateMoney();
            CostText.text = "Bought";
            IsBought = true;

            bool abilityExists = AbilityManager.Instance.PlayerAbilities.TryGetValue(ability.ID, out var existingAbility);
            if (abilityExists)
            {
                existingAbility.CurrentLevel++;
                existingAbility.UsesCharges = usesCharges;
            }
            else
            {
                AbilityManager.Instance.GivePlayerAbility(ability.ID);
            }
            ShopManager.Instance.UpdateShopAbilities();
        }
    }

    public void Init(Ability ability, int level, bool usesCharges = false)
    {
        // Set data
        this.ability = ability;
        levelToUpgrade = level;
        this.usesCharges = usesCharges;

        // Set UI elements
        UpgradeImage.sprite = ability.Icon;
        NameText.text = $"{ability.Name} (Lvl. {level})";
        CostText.text = $"${cost}";
        ChargeText.text = usesCharges ? $"({ability.MaxCharges})" : "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Sprite icon = ability.Icon;
        string header;
        if (levelToUpgrade > 0) header = $"{ability.Name} (Lvl. {levelToUpgrade - 1} -> {levelToUpgrade})";
        else header = $"{ability.name} (Lvl. 0)";
        string description = ability.AllLevels[levelToUpgrade].Description;

        ShopManager.Instance.ShowTooltipInfo(icon, header, description);
    }
}
