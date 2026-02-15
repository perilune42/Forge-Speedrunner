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
    [SerializeField] private Canvas screen;

    [Header("Prefabs")]
    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private GameObject shopAbilityPrefab;

    [Header("Nav Panel Refs")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private GameObject[] tabs;

    [Header("Overview Tab Refs")]
    [SerializeField] private TMP_Text runTimeText;
    [SerializeField] private TMP_Text targetTimeText;
    [SerializeField] private TMP_Text moneyGainedText;

    [Header("Upgrade Tab Refs")]
    [SerializeField] private Transform abilityLayoutGroup;

    [SerializeField] private Transform upgradeLayoutGroup;
    [SerializeField] private Image upgradeInfoIcon;
    [SerializeField] private TMP_Text upgradeInfoNameText;
    [SerializeField] private TMP_Text upgradeInfoDescriptionText;

    [Header("Continue Tab Refs")]

    private const float chargeChance = 0f;

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

        for (int i = 0; i < upgradeLayoutGroup.childCount; i++)
        {
            Destroy(upgradeLayoutGroup.GetChild(i).gameObject);
        }

        var currentAbilities = AbilityManager.Instance.PlayerAbilities;

        foreach (Ability possibleAbility in GameRegistry.Instance.Abilities)
        {
            bool abilityExists = currentAbilities.TryGetValue(possibleAbility.ID, out var currentAbility);
            if (abilityExists && currentAbility.CurrentLevel >= currentAbility.AllLevels.Length - 1) continue; // upgrade is already max level

            GameObject newUpgrade = Instantiate(upgradePrefab, upgradeLayoutGroup);
            bool useCharges = false;
            if (!abilityExists)
            {
                // newly bought abilities have a chance to be charge based
                if (Random.value < chargeChance)
                {
                    useCharges = true;
                }
            }
            else
            {
                useCharges = currentAbility.UsesCharges;
            }
            int levelToUpgrade = abilityExists ? currentAbility.CurrentLevel + 1 : 0;
            newUpgrade.GetComponent<Upgrade>().Init(possibleAbility, levelToUpgrade, useCharges);

        }

        UpdateShopAbilities();
    }

    public void UpdateShopAbilities()
    {
        for (int i = 0; i < abilityLayoutGroup.childCount; i++)
        {
            Destroy(abilityLayoutGroup.GetChild(i).gameObject);
        }

        foreach (Ability ability in AbilityManager.Instance.GetAllAbilities())
        {
            GameObject shopAbility = Instantiate(shopAbilityPrefab, abilityLayoutGroup);
            shopAbility.GetComponent<ShopAbility>().Init(ability, ability.CurrentLevel);
        }
    }

    public void ShowUpgradeInfo(Ability ability, int level)
    {
        upgradeInfoIcon.sprite = ability.Icon;
        upgradeInfoNameText.text = $"{ability.Name} {level}";
        upgradeInfoDescriptionText.text = ability.AllLevels[level].Description;
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
