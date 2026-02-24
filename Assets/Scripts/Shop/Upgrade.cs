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

    [SerializeField] private UnityEngine.UI.Button button;
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
        // can only buy if you already have the ability, or can fit a new one
        bool canFitAbility = isTool || AbilityManager.Instance.PlayerAbilities.ContainsKey(ability.ID)
                             || AbilityManager.Instance.PlayerAbilities.Count < 5;

        // TODO - optimize
        if (CanBuy && canFitAbility)
        {
            UpgradeImage.color = Color.white;
            if (!isTool) NameText.color = Color.white;
            CostText.color = Color.white;
            if (!isTool) ChargeText.color = Color.white;
            button.interactable = true;
        }
        else
        {
            UpgradeImage.color = Color.gray;
            if (!isTool) NameText.color = Color.lightGray;
            CostText.color = Color.lightGray;
            if (!isTool) ChargeText.color = Color.lightGray;
            button.interactable = false;
        }

        if (!canFitAbility)
        {
            CostText.text = "FULL";
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

            if (ability is Chronoshift) 
            {
                // if (AbilityManager.Instance.TotalChronoshiftCharges == 0) AbilityManager.Instance.GiveChronoshift();
                AbilityManager.Instance.ChronoshiftCharges++;
                AbilityManager.Instance.TotalChronoshiftCharges++;
                return;
            }

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
        if (!isTool) NameText.text = $"{ability.Name} (Lvl. {level+1})";
        CostText.text = $"${cost}";
        if (!isTool) ChargeText.text = usesCharges ? $"({ability.MaxCharges})" : "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Sprite icon = ability.Icon;
        string header;
        if (isTool) header = ability.Name;
        else if (levelToUpgrade > 0) header = $"{ability.Name} (Lvl. {levelToUpgrade} -> {levelToUpgrade + 1})";
        else header = $"{ability.name} (Lvl. 1)";
        string description = ability.AllLevels[levelToUpgrade].Description;

         ShopManager.Instance.ShowTooltipInfo(ability, levelToUpgrade, true);
    }
}
