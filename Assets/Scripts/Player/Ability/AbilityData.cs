using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "Scriptable Objects/AbilityData")]
public class AbilityData : ScriptableObject
{
    [HideInInspector] public int ID = -1;
    [HideInInspector] public int Level = 0;
    public UpgradeData[] Upgrades;
    public int MaxCharges = 5;
    [HideInInspector] public bool UsesCharges = false;
}
