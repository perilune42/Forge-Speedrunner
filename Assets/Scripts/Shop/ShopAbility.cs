using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopAbility : MonoBehaviour, IPointerEnterHandler
{
    public static ShopAbility SelectedAbility;

    [SerializeField] Ability ability;
    [SerializeField] int abilityLevel;

    // Editor refs
    [SerializeField] Image abilityImage;
    [SerializeField] AbilityLevelUI levelUI;

    public void Init(Ability ability, int level)
    {
        this.ability = ability;
        abilityLevel = level;

        if (levelUI != null)
            levelUI.SetLevel(level);
        abilityImage.sprite = ability.Icon;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.ShowTooltipInfo(ability, abilityLevel, false);
        else
            StartingAbilityManager.Instance.ShowTooltipInfo(ability, abilityLevel, false);
    }

    public void SetSelected(bool isSelected) 
    {
        if (isSelected) abilityImage.color = Color.gray;
        else abilityImage.color = Color.white;
    }
}
