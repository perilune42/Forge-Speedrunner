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
}
