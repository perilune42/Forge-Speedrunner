using System;
using System.Security.Cryptography;
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
    [SerializeField] private AbilityLevelUI levelUI;
    
    private void Start()
    {
        // icon.sprite = Ability.Data.Icon;
        
        
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
        if (levelUI != null) levelUI.SetLevel(ability.CurrentLevel);
        Ability.onRecharged += RechargeFlash;
    }

    private void Update()
    {
        bool active = Ability.CanUseAbility();
        mask.fillAmount = 1f - Ability.GetCooldown();
        
        icon.color = active ? Color.white : Color.gray;
        if (Ability is Chronoshift) chargeText.text = $"{AbilityManager.Instance.ChronoshiftCharges}/{Ability.MaxCharges}";
        else chargeText.text = Ability.UsesCharges ? $"{Ability.CurCharges}/{Ability.MaxCharges}" : "";
        keybindText.SetText(Ability.BindingDisplayString);
    }

    private void RechargeFlash()
    {
        flashMask.color = Color.white;
        flashMask.DOColor(Color.clear, 0.5f);
    }

    private void OnDestroy()
    {
        flashMask.DOKill();
        Ability.onRecharged -= RechargeFlash;
    }


}