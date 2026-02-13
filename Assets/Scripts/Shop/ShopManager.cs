using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum ShopTab
{
    Review, Upgrade, Continue
}

public class ShopManager : Singleton<ShopManager>
{
    public int Money;
    public List<UpgradeData> Upgrades;

    [SerializeField] private Canvas screen;

    [Header("Prefabs")]
    [SerializeField] private GameObject upgradePrefab;

    [Header("Nav Panel Refs")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private GameObject[] tabs;

    [Header("Overview Tab Refs")]
    [SerializeField] private TMP_Text runTimeText;
    [SerializeField] private TMP_Text targetTimeText;
    [SerializeField] private TMP_Text moneyGainedText;

    [Header("Upgrade Tab Refs")]
    [SerializeField] private Transform upgradeLayoutGroup;
    [SerializeField] private Image upgradeInfoIcon;
    [SerializeField] private TMP_Text upgradeInfoNameText;
    [SerializeField] private TMP_Text upgradeInfoDescriptionText;

    [Header("Continue Tab Refs")]

    private const float chargeChance = 0.5f;

    public override void Awake()
    {
        base.Awake();

    }

    void Start()
    {
        
    }

    // pulls up the shop screen with the most updated information
    public void LoadShop()
    {
        screen.gameObject.SetActive(true);
        SwitchTab((int)ShopTab.Review);
        UpdateMoney();
        UpdateTimeTaken();

        foreach (AbilityData abilityData in ProgressionData.Instance.AbilityDataArray)
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

    public void ShowUpgradeInfo(UpgradeData upgradeData)
    {
        if (upgradeData == null) return;

        upgradeInfoIcon.sprite = upgradeData.Icon;
        upgradeInfoNameText.text = upgradeData.Name;
        upgradeInfoDescriptionText.text = upgradeData.Description;
    }

    public void CloseShop()
    {
        screen.gameObject.SetActive(false);
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

    private void GainReward()
    {
        int moneyGained = (int)(Timer.targetSpeedrunTime - Timer.speedrunTime);
        moneyGainedText.text = moneyGained + "";
        Money += moneyGained;
        UpdateMoney();
    }

    public void UpdateMoney()
    {
        moneyText.text = Money.ToString();
    }

    public void UpdateTimeTaken() {
        runTimeText.text = Util.SecondsToTime(Timer.speedrunTime);
        targetTimeText.text = Util.SecondsToTime(Timer.targetSpeedrunTime);
    }

    public void ReturnToWorld()
    {
        // SceneManager.LoadScene("World");
        Game.Instance.ReturnToPlay(false);
    }
}
