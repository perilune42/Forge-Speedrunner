using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string Name;
    public int Cost;
    public Sprite Icon;
}
