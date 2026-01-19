using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : Singleton<ShopManager>
{
    public int Money;
    [SerializeField] public List<UpgradeData> Upgrades;

    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private Transform upgradeLayoutGroup;
    [SerializeField] private TMP_Text moneyText;

    void Start()
    {
        UpdateMoney();

        foreach (UpgradeData upgrade in Upgrades)
        {
            GameObject newUpgrade = Instantiate(upgradePrefab, upgradeLayoutGroup);
            newUpgrade.GetComponent<Upgrade>().Init(upgrade);
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("10 more dollar"))
        {
            Money += 10;
            UpdateMoney();
        }
    }

    public void UpdateMoney()
    {
        moneyText.text = Money.ToString();
    }

    public void ReturnToWorld()
    {
        SceneManager.LoadScene("World");
    }
}
