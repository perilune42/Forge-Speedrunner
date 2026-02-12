using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilityInfo : MonoBehaviour
{
    [HideInInspector] public Ability Ability;
    [SerializeField] private Image icon, mask, flashMask;
    [SerializeField] private TMP_Text chargeText, keybindText;

    
    private void Start()
    {
        icon.sprite = Ability.Data.Upgrades[Ability.Level - 1].Icon;
        Ability.onRecharged += () =>
        {
            flashMask.color = Color.white;
            flashMask.DOColor(Color.clear, 0.5f);
        };
    }

    private void Update()
    {
        bool active = Ability.CanUseAbility();
        mask.fillAmount = 1f - Ability.GetCooldown();
        
        icon.color = active ? Color.white : Color.gray;
        chargeText.text = Ability.UsesCharges ? $"{Ability.CurCharges}/{Ability.MaxCharges}" : "";
        keybindText.SetText(Ability.Data.BindingDisplayString);
    }


}