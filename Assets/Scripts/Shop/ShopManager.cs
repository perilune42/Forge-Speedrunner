using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private GameObject upgradePrefab, toolPrefab;
    [SerializeField] private GameObject shopAbilityPrefab;

    [Header("Nav Panel Refs")]
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text moneyText, prevTargetText, newTargetText;

    [SerializeField] private GameObject[] tabs;

    [Header("Overview Tab Refs")]
    [SerializeField] private TMP_Text runTimeText;
    [SerializeField] private TMP_Text moneyGainedText;

    [Header("Upgrade Tab Refs")]
    [SerializeField] private Transform abilityLayoutGroup;
    [SerializeField] private Transform toolsLayoutGroup;
    [SerializeField] private List<ShopAbilitySlot> abilitySlots;
    private Dictionary<AbilitySlotID, ShopAbilitySlot> shopAbilitySlotDict = new();
    private Dictionary<AbilitySlotID, ShopAbility> shopAbilityDict = new();


    [SerializeField] private Transform upgradeLayoutGroup;

    [SerializeField] private Image tooltipInfoIcon;
    [SerializeField] private TMP_Text tooltipInfoNameText;
    [SerializeField] private TMP_Text tooltipInfoDescriptionText;
    [SerializeField] private TMP_Text tooltipInfoCooldownText;

    [SerializeField] private Transform tooltipInfoDescriptionParent;
    [SerializeField] private TMP_Text tooltipInfoDescriptionPrefab;

    [SerializeField] private UnityEngine.UI.Button rerollButton;
    [SerializeField] private TMP_Text rerollCostText;

    [Header("Continue Tab Refs")]

    private const float chargeChance = 0f;
    [SerializeField] private int shopOffers = 3;

    [SerializeField] private FullscreenMapUI shopMap;
    [SerializeField] private TMP_Text selectSpawnText;

    private ShopAbilitySlot selectedAbilitySlot;

    private bool goingToPracticeMode = false;
    public override void Awake()
    {
        base.Awake();
        foreach (var slot in abilitySlots)
        {
            shopAbilitySlotDict[slot.SlotID] = slot;
        }
    }

    public int baseRerollCost = 10;
    public float MultiplierPerReroll = 2f;
    private int currRerollCost;


    // pulls up the shop screen with the most updated information
    public void LoadShop(bool newRound)
    {
        screen.gameObject.SetActive(true);

        if (newRound)
        {
            currRerollCost = baseRerollCost;
            rerollCostText.text = $"<${currRerollCost}>";
            GainReward();
            RestockShop();
        }

        SwitchTab((int)ShopTab.Review);

        UpdateRoundInfo();

        

        UpdateShopAbilities();
        UpdateMoney();

        shopMap.clearImages();
        shopMap.produceImages();

        goingToPracticeMode = false;
        selectSpawnText.enabled = false;

        ClearTooltipInfo();

    }

    public void RestockShop()
    {
        for (int i = 0; i < upgradeLayoutGroup.childCount; i++)
        {
            Destroy(upgradeLayoutGroup.GetChild(i).gameObject);
        }
        for (int i = 0; i < toolsLayoutGroup.childCount; i++)
        {
            Destroy(toolsLayoutGroup.GetChild(i).gameObject);
        }

        var currentAbilities = AbilityManager.Instance.GetAllAbilities();
        List<Ability> abilityChoices;
        if (currentAbilities.Count >= 5)
        {
            // if full, only choose from existing abilities
            abilityChoices = GameRegistry.Instance.Abilities.Where(
                a => AbilityManager.Instance.GetAbilityByID(a.ID) != null
                ).ToList().Shuffled();
        }
        else abilityChoices = GameRegistry.Instance.Abilities.Shuffled();


        int count = 0, idx = 0;
        while (count < shopOffers && idx < abilityChoices.Count)
        {
            Ability possibleAbility = abilityChoices[idx];
            idx++;
            Ability currentAbility = AbilityManager.Instance.GetAbilityByID(possibleAbility.ID);
            bool abilityExists = currentAbility != null;
            if (abilityExists && currentAbility.CurrentLevel >= currentAbility.AllLevels.Length - 1) continue;
            // upgrade is already max level

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
            count++;
        }

        Chronoshift chronoshift = GameRegistry.Instance.Chronoshift;
        GameObject chronoshiftUpgrade = Instantiate(toolPrefab, toolsLayoutGroup);
        chronoshiftUpgrade.GetComponent<Upgrade>().Init(chronoshift, 0, true);
    }

    public void RerollShop()
    {
        Money -= currRerollCost;
        currRerollCost = Mathf.RoundToInt(currRerollCost * MultiplierPerReroll);
        RestockShop();
        UpdateMoney();
    }

    public void UpdateShopAbilities()
    {
        for (int i = 0; i < abilitySlots.Count; i++)
        {
            ShopAbilitySlot slot = abilitySlots[i];
            for (int j = 0; j < slot.transform.childCount; j++)
            {
                Transform child = slot.transform.GetChild(j);
                if (child != slot.bindingText.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }


        foreach (ShopAbilitySlot slot in abilitySlots)
        {
            if (AbilityManager.Instance.PlayerAbilities.TryGetValue(slot.SlotID, out Ability ability)
                && ability != null)
            {
                GameObject shopAbilityObj = Instantiate(shopAbilityPrefab, slot.transform);
                ShopAbility shopAbility = shopAbilityObj.GetComponent<ShopAbility>();
                shopAbility.Init(ability, ability.CurrentLevel);
                shopAbilityDict[slot.SlotID] = shopAbility;
            }
            else
            {
                shopAbilityDict[slot.SlotID] = null;
            }
        }

    }

    private void ClearTooltipInfo()
    {
        tooltipInfoIcon.enabled = false;
        tooltipInfoNameText.text = "";
        tooltipInfoCooldownText.text = "";
        for (int i = 0; i < tooltipInfoDescriptionParent.childCount; i++)
        {
            Destroy(tooltipInfoDescriptionParent.GetChild(i).gameObject);
        }
    }
    public void ShowTooltipInfo(Ability ability, int level, bool isUpgrade)
    {
        tooltipInfoIcon.enabled = true;
        tooltipInfoIcon.sprite = ability.Icon;
        if (isUpgrade)
        {
            tooltipInfoNameText.text = $"{ability.Name} (Lvl. {level} -> {level + 1})";
        }
        else
        {
            tooltipInfoNameText.text = $"{ability.Name} (Lvl. {level+1})";
        }

        tooltipInfoCooldownText.text = $"Cooldown: {(ability.cooldown / 60f):F1}s";

        for (int i = 0; i < tooltipInfoDescriptionParent.childCount; i++)
        {
            Destroy(tooltipInfoDescriptionParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < ability.AllLevels.Length; i++)
        {
            AbilityLevel abilityLevelDesc = ability.AllLevels[i];
            TMP_Text txt = Instantiate(tooltipInfoDescriptionPrefab, tooltipInfoDescriptionParent);
            txt.text = $"Lvl. {i+1}: {abilityLevelDesc.Description}";

            if (!isUpgrade)
            {
                if (i <= level) txt.color = Color.lightGreen;
                else txt.color = Color.gray8;
            }
            else
            {
                if (i < level) txt.color = Color.lightGreen;
                else if (i == level) txt.color = Color.green;
                else txt.color = Color.gray8;
            }
            
        }
        
        
    }

    public void CloseShop()
    {
        screen.gameObject.SetActive(false);
    }

    //private void OnGUI()
    //{
    //    // if (GUILayout.Button("10 more dollar"))
    //    //{
    //    //    Money += 10;
    //    //    UpdateMoney();
    //    //}
    //}

    public void SwitchTab(int tabIdx)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(i == tabIdx);
        }
    }

    private void GainReward()
    {
        int moneyGained = Game.Instance.GetRunReward();
        moneyGainedText.text = $"+<sprite name=\"computer_chip\">{moneyGained}";
        Money += moneyGained;
        UpdateMoney();
    }

    public void SelectUpgrade(Upgrade upgrade)
    {
        var selectedUpgrade = upgrade;
        selectedUpgrade.BuyUpgrade();
        Ability ability = selectedUpgrade.Ability;
        if (ability is Chronoshift)
        {
            // if (AbilityManager.Instance.TotalChronoshiftCharges == 0) AbilityManager.Instance.GiveChronoshift();
            AbilityManager.Instance.ChronoshiftCharges++;
            AbilityManager.Instance.TotalChronoshiftCharges++;
            return;
        }
        Ability existingAbility = AbilityManager.Instance.GetAbilityByID(ability.ID);
        bool abilityExists = existingAbility != null;
        if (abilityExists)
        {
            existingAbility.CurrentLevel++;
            existingAbility.UsesCharges = selectedUpgrade.UsesCharges;
        }
        else
        {
            foreach (var kvp in AbilityManager.Instance.PlayerAbilities)
            {
                if (kvp.Value == null && kvp.Key != AbilitySlotID.Dash)
                {
                    AbilityManager.Instance.GivePlayerAbility(ability.ID, kvp.Key);
                    break;
                }
            }
        }

        UpdateShopAbilities();
    }

    public void EquipSelectedUpgrade(AbilitySlotID slot)
    {
        
    }

    public void UpdateMoney()
    {
        moneyText.text = $"<sprite name=\"computer_chip\">{Money}";
        rerollCostText.text = $"<<sprite name=\"computer_chip\">{currRerollCost}>";
        rerollButton.interactable = Money >= currRerollCost;
    }

    public void UpdateRoundInfo() {
        roundText.text = $"{Game.Instance.CurrentRound}";
        runTimeText.text = $"{Util.GetTimeString(Timer.previousSpeedrunTime)} / {Util.GetTimeString(Timer.previousTargetTime)}";
        prevTargetText.text = $"{Util.GetTimeString(Timer.previousTargetTime)}";
        newTargetText.text = $"{Util.GetTimeString(Timer.targetSpeedrunTime)}";
    }

    // BUTTON REFS

    public void PracticeMode()
    {
        goingToPracticeMode = !goingToPracticeMode;
        shopMap.ToggleSpawnSelectors(goingToPracticeMode);
        selectSpawnText.enabled = goingToPracticeMode;
    }

    public void ReturnToWorld()
    {
        // SceneManager.LoadScene("World");
        Game.Instance.ReturnToPlay(false);
    }

    public void ClickAbilitySlot(ShopAbilitySlot slot)
    {
        if (slot.SlotID == AbilitySlotID.Dash) return;
        if (selectedAbilitySlot == null && AbilityManager.Instance.PlayerAbilities[slot.SlotID] != null)
        {
            // select existing ability
            selectedAbilitySlot = slot;
            shopAbilityDict[slot.SlotID].SetSelected(true);
        }
        else if (selectedAbilitySlot != null)
        {
            // swap with another slot
            if (shopAbilityDict[selectedAbilitySlot.SlotID] != null)
                shopAbilityDict[selectedAbilitySlot.SlotID].SetSelected(false);
            if (slot != selectedAbilitySlot)
            {
                AbilityManager.Instance.SwapAbilities(slot.SlotID, selectedAbilitySlot.SlotID);
                UpdateShopAbilities();
            }
            selectedAbilitySlot = null;
        }
    }
}
