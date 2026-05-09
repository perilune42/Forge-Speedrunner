using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class StartingUpgrade : Upgrade, IPointerEnterHandler
{
    
    protected override bool CanBuy => true;
    protected override bool CanFitAbility()
    {
        return true;
    }
    public override void SelectUpgrade()
    {
        StartingAbilityManager.Instance.SelectUpgrade(this);
    }

    public override void BuyUpgrade()
    {
        return;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
         StartingAbilityManager.Instance.ShowTooltipInfo(Ability, levelToUpgrade, true);
    }

    protected override void Update()
    {
        // TODO - optimize
        if (StartingAbilityManager.Instance.SelectedUpgrade.Ability.ID != Ability.ID)
        {
            UpgradeImage.color = Color.white;
            if (!isTool) NameText.color = Color.white;
            button.interactable = true;
        }
        else
        {
            UpgradeImage.color = Color.gray;
            if (!isTool) NameText.color = Color.lightGreen;
            button.interactable = false;
        }

        if (!CanFitAbility() && CostText != null)
        {
            CostText.text = "FULL";
        }
    }

}
