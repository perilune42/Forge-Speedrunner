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
    [SerializeField] private bool isTool; // whether this shows up in the Tools group and thus doesn't show its name or charge count
    private int cost => ability.AllLevels[levelToUpgrade].Cost;
    private bool HasEnoughMoney => ShopManager.Instance.Money - cost >= 0;
    private bool CanBuy => !IsBought && HasEnoughMoney;

    private void Update()
    {
        // TODO - optimize
        if (CanBuy)
        {
            UpgradeImage.color = Color.white;
            if (!isTool) NameText.color = Color.white;
            CostText.color = Color.white;
            if (!isTool) ChargeText.color = Color.white;
        }
        else
        {
            UpgradeImage.color = Color.gray;
            if (!isTool) NameText.color = Color.lightGray;
            CostText.color = Color.lightGray;
            if (!isTool) ChargeText.color = Color.lightGray;
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
                if (existingAbility is not Chronoshift)
                {
                    existingAbility.CurrentLevel++;
                    existingAbility.UsesCharges = usesCharges;
                }
            }
            else
            {
                if (ability is Chronoshift) AbilityManager.Instance.GiveChronoshift();
                else AbilityManager.Instance.GivePlayerAbility(ability.ID);
            }
            if (ability is Chronoshift) AbilityManager.Instance.ChronoshiftCharges++;
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
        if (!isTool) NameText.text = $"{ability.Name} (Lvl. {level})";
        CostText.text = $"${cost}";
        if (!isTool) ChargeText.text = usesCharges ? $"({ability.MaxCharges})" : "";
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
