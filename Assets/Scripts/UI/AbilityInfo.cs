using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInfo : MonoBehaviour
{
    [HideInInspector] public Ability Ability;
    [SerializeField] private Image icon, mask;
    
    private void Start()
    {
        icon.sprite = Ability.Data.Upgrades[Ability.Level - 1].Icon;
    }

    private void Update()
    {
        mask.fillAmount = 1f - Ability.GetCooldown();
        icon.color = Ability.CanUseAbility() ? Color.white : Color.gray;
    }
}