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
        // icon.sprite = Ability.Data.Icon;
        Ability.onRecharged += () =>
        {
            flashMask.color = Color.white;
            flashMask.DOColor(Color.clear, 0.5f);
        };
        
        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    if (device is Gamepad) Ability.UpdateBindingText();
                    break;

                case InputDeviceChange.Removed:
                    if (device is Gamepad) Ability.UpdateBindingText();
                    break;
            }
        };
    }

    public void SetAbility(Ability ability)
    {
        this.Ability = ability;
        icon.sprite = ability.Icon;
    }

    private void Update()
    {
        bool active = Ability.CanUseAbility();
        mask.fillAmount = 1f - Ability.GetCooldown();
        
        icon.color = active ? Color.white : Color.gray;
        chargeText.text = Ability.UsesCharges ? $"{Ability.CurCharges}/{Ability.MaxCharges}" : "";
        keybindText.SetText(Ability.BindingDisplayString);
    }


}