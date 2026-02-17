using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopAbility : MonoBehaviour
{
    public Ability associatedAbility;
    [SerializeField] Image abilityImage;
    [SerializeField] TextMeshProUGUI abilityName;
    [SerializeField] TextMeshProUGUI abilityCharge;

    public void Init(Ability ability, int level)
    {
        AbilityLevel levelInfo = ability.AllLevels[level];
        abilityImage.sprite = ability.Icon;
        abilityName.text = $"{ability.Name} {level}";
        abilityCharge.text = ability.MaxCharges.ToString();
    }
}
