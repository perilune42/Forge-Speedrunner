using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopAbility : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] Ability ability;
    [SerializeField] int abilityLevel;

    // Editor refs
    [SerializeField] Image abilityImage;
    [SerializeField] TextMeshProUGUI abilityName;
    [SerializeField] TextMeshProUGUI abilityCharge;

    public void Init(Ability ability, int level)
    {
        this.ability = ability;
        abilityLevel = level;

        abilityImage.sprite = ability.Icon;
        abilityName.text = $"{ability.Name} (Lvl. {level})";
        abilityCharge.text = ability.UsesCharges ? ability.MaxCharges.ToString() : "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Sprite icon = ability.Icon;
        string header = $"{ability.Name} (Lvl. {abilityLevel})";
        string description = ability.AllLevels[abilityLevel].Description;

        ShopManager.Instance.ShowTooltipInfo(icon, header, description);
    }
}
