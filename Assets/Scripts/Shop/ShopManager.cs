using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum ShopTab
{
    Review, Upgrade, Continue
}

public class ShopManager : Singleton<ShopManager>
{
    public int Money;
    [SerializeField] public List<UpgradeData> Upgrades;

    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private Transform upgradeLayoutGroup;
    [SerializeField] private TMP_Text moneyText;

    [SerializeField] private GameObject[] tabs;

    private const float chargeChance = 0.5f;

    public override void Awake()
    {
        base.Awake();
        UpdateMoney();

        foreach (AbilityData abilityData in AbilitySceneTransfer.AbilityDataArray)
        {
            if (abilityData.Level >= abilityData.Upgrades.Length) continue; // upgrade is already max level

            GameObject newUpgrade = Instantiate(upgradePrefab, upgradeLayoutGroup);

            bool useCharges = false;
            if (abilityData.Level == 0)
            {
                // newly bought abilities have a chance to be charge based
                if (Random.value < chargeChance)
                {
                    useCharges = true;
                }
            }
            else
            {
                useCharges = abilityData.UsesCharges;
            }
            newUpgrade.GetComponent<Upgrade>().Init(abilityData.ID, useCharges);

        }
    }

    void Start()
    {
        SwitchTab((int)ShopTab.Review);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("10 more dollar"))
        {
            Money += 10;
            UpdateMoney();
        }
    }

    public void SwitchTab(int tabIdx)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(i == tabIdx);
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
