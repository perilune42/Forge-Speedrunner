using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AbilitySlotID
{
    Dash, Ability1, Ability2, Ability3, Ability4
}

public class AbilityManager : Singleton<AbilityManager>
{
    public static Dictionary<AbilitySlotID, (int, int)> startingAbilities = null;       // slot: [ability id, level]


    public GameObject AbilityInfoPrefab;
    public GameObject AbilityInfoParent, ChronoshiftInfoParent;
    [SerializeField] private GameObject player;
    public PlayerMovement playerMovement;
    public Dictionary<AbilitySlotID, Ability> PlayerAbilities;

    [HideInInspector] public int ChronoshiftCharges, TotalChronoshiftCharges;
    public Chronoshift chronoshift;
    public override void Awake()
    {
        base.Awake();
        PlayerAbilities = new();
        ChronoshiftInfoParent = GameObject.FindWithTag("CHRONOSHIFT_INFO_PARENT"); // jank because I can't push direct changes to the World scene

    }

    void Start()
    {
        TotalChronoshiftCharges = 0;
        ChronoshiftCharges = 0;
        GivePlayerAbilities();
        GiveChronoshift();
    }

    // does NOT restore dash by default
    private void ClearAbilities()
    {
        foreach (Ability ability in PlayerAbilities.Values)
        {
            if (ability != null)
                Destroy(ability.gameObject);
        }
        PlayerAbilities = new()
        {
            [AbilitySlotID.Dash] = null,
            [AbilitySlotID.Ability1] = null,
            [AbilitySlotID.Ability2] = null,
            [AbilitySlotID.Ability3] = null,
            [AbilitySlotID.Ability4] = null
        };
    }

    private void GivePlayerAbilities()
    {
        ClearAbilities();
        int count = 0;

        if (startingAbilities != null)
        {
            foreach (var kvp in startingAbilities)
            {
                var slot = kvp.Key;
                var (abilityID, level) = kvp.Value;
                if (abilityID != -1 && PlayerAbilities[slot] == null)
                {
                    GivePlayerAbility(abilityID, slot, level);
                    break;
                }
            }
        }
        else
        {
            foreach (Ability presetAbility in GameRegistry.Instance.Abilities)
            {
                bool giveAbility = presetAbility.StartUnlocked;
                if (giveAbility)
                {
                    if (count >= 4 && !(presetAbility is Chronoshift && ChronoshiftCharges > 0))
                    {
                        Debug.LogWarning($"Ability {presetAbility.Name} not given");
                    }
                    else
                    {
                        if (presetAbility is Chronoshift) GiveChronoshift();
                        else
                        {
                            foreach (AbilitySlotID slot in PlayerAbilities.Keys)
                            {
                                if (PlayerAbilities[slot] == null)
                                {
                                    GivePlayerAbility(presetAbility.ID, slot);
                                    break;
                                }
                            }
                        }
                        if (presetAbility.ID != 0) count++;
                    }

                }
            }
        }
            
    }

    public void GiveChronoshift()
    {
        Ability ability = Instantiate(GameRegistry.Instance.Chronoshift, player.transform);
        ability.ID = -1;
        chronoshift = ability as Chronoshift;
        chronoshift.SetInputButton(PInput.Instance.Chronoshift);
    }
    
    public void GivePlayerAbility(int index, AbilitySlotID slot, int level = -1)
    {
        Ability ability = Instantiate(GameRegistry.Instance.Abilities[index], player.transform);
        PlayerAbilities[slot] = ability;
        ability.ID = index;
        ability.CurrentLevel = level != -1 ? level : GameRegistry.Instance.Abilities[index].CurrentLevel;
        /*
        if (ability.ID != 0 && (allAbilitiesAreCharged || ability.Data.UsesCharges))
        {
            ability.UsesCharges = true;
            ability.MaxCharges = ability.Data.MaxCharges;
            ability.CurCharges = ability.MaxCharges;
        }
        */
        if (ability is Chronoshift)
        {
            ability.SetInputButton(PInput.Instance.Chronoshift);
        }
        else
        {
            ability.SetInputButton(PInput.Instance.AbilityButtonDict[slot]);
        }

    }

    public T GetAbility<T>() where T : Ability
    {
        if (typeof(T) == typeof(Chronoshift)) return chronoshift as T;
        
        foreach (Ability ability in PlayerAbilities.Values)
        {
            if  (ability != null && ability.GetType() == typeof(T)) return ability as T;
        }

        return null;
    }

    public Ability GetAbilityByID(int id)
    {
        foreach (Ability ability in PlayerAbilities.Values)
        {
            if (ability != null && ability.ID == id) return ability;
        }
        return null;
    }

    public bool TryGetAbility<T>(out T ability) where T : Ability
    {
        ability = GetAbility<T>();
        return ability != null;
    }

    public List<Ability> GetAllAbilities()
    {
        return PlayerAbilities.Values.Where(a => a != null).ToList();
    }

    public void ResetAbilites()
    {
        foreach (var ability in GetAllAbilities())
        {
            ability.OnReset();
        }

    }

    public void RechargeAbilities()
    {
        foreach (Ability ability in GetAllAbilities())
        {
            ability.Recharge();
            if (ability.UsesCharges)
            {
                ability.CurCharges = ability.MaxCharges;
            }
        }
    }

    public void SwapAbilities(AbilitySlotID slot1, AbilitySlotID slot2)
    {
        // C# my beloved
        (PlayerAbilities[slot1], PlayerAbilities[slot2]) = (PlayerAbilities[slot2], PlayerAbilities[slot1]);
        if (PlayerAbilities[slot1] != null) 
            PlayerAbilities[slot1].SetInputButton(PInput.Instance.AbilityButtonDict[slot1]);
        if (PlayerAbilities[slot2] != null)
            PlayerAbilities[slot2].SetInputButton(PInput.Instance.AbilityButtonDict[slot2]);
    }
}



