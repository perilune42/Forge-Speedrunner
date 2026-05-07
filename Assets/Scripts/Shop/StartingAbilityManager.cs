using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class StartingAbilityManager : Singleton<StartingAbilityManager>
{

    [Header("Prefabs")]
    [SerializeField] private GameObject upgradePrefab, toolPrefab;
    [SerializeField] private GameObject shopAbilityPrefab;


    [Header("Upgrade Tab Refs")]
    [SerializeField] private Transform abilityLayoutGroup;
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

    private ShopAbilitySlot selectedAbilitySlot;

    

    [Serializable]
    public struct StartingAbilityChoice
    {
        public Ability ability;
        public int level;
    }

    public List<StartingAbilityChoice> availableAbilities;

    public override void Awake()
    {
        base.Awake();
        ResetStartingAbilities();
        SetupAbilityChoices();
        UpdateShopAbilities();
        ClearTooltipInfo();

    }

    void ResetStartingAbilities()
    {
        AbilityManager.startingAbilities = new();
        foreach (var slot in abilitySlots)
        {
            if (slot.SlotID == AbilitySlotID.Dash)
            {
                AbilityManager.startingAbilities[slot.SlotID] = (availableAbilities.Find(a => a.ability is Dash).ability.ID, 0);
            }
            else
            {
                AbilityManager.startingAbilities[slot.SlotID] = (-1, 0);
            }
            shopAbilitySlotDict[slot.SlotID] = slot;
        }
    }

    public void SetupAbilityChoices()
    {
        for (int i = 0; i < upgradeLayoutGroup.childCount; i++)
        {
            Destroy(upgradeLayoutGroup.GetChild(i).gameObject);
        }

        foreach (var abilityChoice in availableAbilities)
        {
            GameObject newUpgrade = Instantiate(upgradePrefab, upgradeLayoutGroup);
            newUpgrade.GetComponent<StartingUpgrade>().Init(abilityChoice.ability, abilityChoice.level , false);
        }
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
            var match = availableAbilities.Where(a => a.ability.ID == AbilityManager.startingAbilities[slot.SlotID].Item1);
            if (match.Count() == 0) continue;
            Ability ability = match.First().ability;

            if (ability != null)
            {
                GameObject shopAbilityObj = Instantiate(shopAbilityPrefab, slot.transform);
                ShopAbility shopAbility = shopAbilityObj.GetComponent<ShopAbility>();
                shopAbility.Init(ability, match.First().level);
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
            tooltipInfoNameText.text = $"{ability.Name} (Lvl. {level + 1})";
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
            txt.text = $"Lvl. {i + 1}: {abilityLevelDesc.Description}";

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

    public void SelectUpgrade(StartingUpgrade upgrade)
    {
        ResetStartingAbilities();
        if (upgrade.Ability is Dash)
        {
            AbilityManager.startingAbilities[AbilitySlotID.Dash] = (upgrade.Ability.ID, upgrade.Ability.CurrentLevel);
        }
        else
        {
            AbilityManager.startingAbilities[AbilitySlotID.Ability1] = (upgrade.Ability.ID, upgrade.Ability.CurrentLevel);
        }
        UpdateShopAbilities();
    }
 
    public void ClickAbilitySlot(ShopAbilitySlot slot)
    {
        if (slot.SlotID == AbilitySlotID.Dash) return;
        if (selectedAbilitySlot == null && AbilityManager.startingAbilities[slot.SlotID].Item1 != -1)
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
                var (slot1, slot2) = (slot.SlotID, selectedAbilitySlot.SlotID);
                (AbilityManager.startingAbilities[slot1], AbilityManager.startingAbilities[slot2]) =
                    (AbilityManager.startingAbilities[slot2], AbilityManager.startingAbilities[slot1]);
                UpdateShopAbilities();
            }
            selectedAbilitySlot = null;
        }
    }
}
