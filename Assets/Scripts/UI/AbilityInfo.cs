using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInfo : MonoBehaviour
{
    [HideInInspector] public Ability Ability;
    [SerializeField] private Image icon, mask;
    
    private void Start()
    {
        icon.sprite = Ability.Data.Icon;
    }

    private void Update()
    {
        mask.fillAmount = 1f - Ability.GetCooldown();
        
    }
}