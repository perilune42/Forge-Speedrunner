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

    public void Init(AbilityData abilityData)
    {
        abilityImage.sprite = abilityData.Icon;
        abilityName.text = abilityData.name;
        abilityCharge.text = abilityData.MaxCharges.ToString();
    }
}
