using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Upgrade : MonoBehaviour, IPointerEnterHandler
{
    public bool IsBought;

    public Ability Ability;
    public int levelToUpgrade;
    public bool UsesCharges;

    [SerializeField] protected UnityEngine.UI.Button button;
    [SerializeField] protected Image UpgradeImage;
    [SerializeField] protected TMP_Text NameText;
    [SerializeField] protected TMP_Text CostText;
    [SerializeField] protected TMP_Text ChargeText;
    [SerializeField] protected AbilityLevelUI levelUI;
    [SerializeField] protected bool isTool; // whether this shows up in the Tools group and thus doesn't show its name or charge count
    private int cost => Ability.AllLevels[levelToUpgrade].Cost;
    private bool HasEnoughMoney => ShopManager.Instance.Money - cost >= 0;
    protected virtual bool CanBuy => !IsBought && HasEnoughMoney && CanFitAbility();
    protected virtual bool CanFitAbility()
    {
        return isTool || AbilityManager.Instance.GetAbilityByID(Ability.ID) != null
                             || AbilityManager.Instance.GetAllAbilities().Count < 5;
    }

    protected virtual void Update()
    {
        // TODO - optimize
        if (CanBuy)
        {
            UpgradeImage.color = Color.white;
            if (!isTool) NameText.color = Color.white;
            if (CostText != null)
                CostText.color = Color.white;
            if (!isTool) ChargeText.color = Color.white;
            button.interactable = true;
        }
        else
        {
            UpgradeImage.color = Color.gray;
            if (!isTool) NameText.color = Color.lightGray;
            if (CostText != null)
                CostText.color = Color.lightGray;
            if (!isTool) ChargeText.color = Color.lightGray;
            button.interactable = false;
        }

        if (!CanFitAbility() && CostText != null)
        {
            CostText.text = "FULL";
        }
    }



    public virtual void SelectUpgrade()
    {
        ShopManager.Instance.SelectUpgrade(this);
    }

    public virtual void BuyUpgrade()
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
        if (CostText != null)
        {
            CostText.text = $"<sprite name=\"computer_chip\">{cost}";
        }
        if (!isTool) ChargeText.text = usesCharges ? $"({ability.MaxCharges})" : "";

        if (!isTool && levelUI != null)
        {
            levelUI.SetLevel(level);
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
         ShopManager.Instance.ShowTooltipInfo(Ability, levelToUpgrade, true);
    }
}
