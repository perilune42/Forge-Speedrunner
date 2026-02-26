using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopAbility : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
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

    private void Update()
    {
        abilityImage.color = this == SelectedAbility ? Color.gray : Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        ShopManager.Instance.ShowTooltipInfo(ability, abilityLevel, false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (SelectedAbility == null) SelectedAbility = this;
        else if (SelectedAbility == this) SelectedAbility = null;
        // else rebind to
    }
}
