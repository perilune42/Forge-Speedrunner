using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "AbilityData", menuName = "Scriptable Objects/AbilityData")]
public class AbilityData : ScriptableObject
{
    [HideInInspector] public int ID = -1;
    [HideInInspector] public int Level = 0;
    public Sprite Icon;
    public UpgradeData[] Upgrades;
    public int MaxCharges = 5;
    public bool UsesCharges = false;
    [HideInInspector] public string BindingDisplayString;
}
