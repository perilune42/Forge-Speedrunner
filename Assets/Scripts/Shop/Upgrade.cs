using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Upgrade : MonoBehaviour, IPointerEnterHandler
{
    public bool IsBought;

    public Ability Ability;
    private int levelToUpgrade;
    public bool UsesCharges;

    [SerializeField] private UnityEngine.UI.Button button;
    [SerializeField] private Image UpgradeImage;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text CostText;
    [SerializeField] private TMP_Text ChargeText;
    [SerializeField] private AbilityLevelUI levelUI;
    [SerializeField] private bool isTool; // whether this shows up in the Tools group and thus doesn't show its name or charge count
    private int cost => Ability.AllLevels[levelToUpgrade].Cost;
    private bool HasEnoughMoney => ShopManager.Instance.Money - cost >= 0;
    private bool CanBuy => !IsBought && HasEnoughMoney;
    


    private void Update()
    {
        // can only buy if you already have the ability, or can fit a new one
        bool canFitAbility = isTool || AbilityManager.Instance.GetAbilityByID(Ability.ID) != null
                             || AbilityManager.Instance.GetAllAbilities().Count < 5;

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

    public void SelectUpgrade()
    {
        ShopManager.Instance.SelectUpgrade(this);
    }

    public void BuyUpgrade()
    {
        if (IsBought)
        {
            Debug.Log($"Already bought {Ability.Name}");
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
        }
    }

    public void Init(Ability ability, int level, bool usesCharges = false)
    {
        // Set data
        this.Ability = ability;
        levelToUpgrade = level;
        this.UsesCharges = usesCharges;

        // Set UI elements
        UpgradeImage.sprite = ability.Icon;
        if (!isTool) NameText.text = $"{ability.Name} (Lvl. {level+1})";
        CostText.text = $"<sprite name=\"computer_chip\">{cost}";
        if (!isTool) ChargeText.text = usesCharges ? $"({ability.MaxCharges})" : "";

        if (!isTool && levelUI != null)
        {
            levelUI.SetLevel(level);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
         ShopManager.Instance.ShowTooltipInfo(Ability, levelToUpgrade, true);
    }
}
