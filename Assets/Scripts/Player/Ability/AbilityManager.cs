using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AbilityManager : Singleton<AbilityManager>
{
    public GameObject AbilityInfoPrefab;
    public GameObject AbilityInfoParent, ChronoshiftInfoParent;
    [SerializeField] private GameObject player;
    public PlayerMovement playerMovement;
    public Dictionary<int, Ability> PlayerAbilities;

    [HideInInspector] public int ChronoshiftCharges, TotalChronoshiftCharges;
    public Chronoshift chronoshift;
    public override void Awake()
    {
        base.Awake();
        PlayerAbilities = new();
        ChronoshiftInfoParent = GameObject.FindWithTag("CHRONOSHIFT_INFO_PARENT"); // jank because I can't push direct changes to the World scene
        GivePlayerAbilities();

    }

    void Start()
    {
        TotalChronoshiftCharges = 0;
        ChronoshiftCharges = 0;
        GiveChronoshift();
    }

    private void GivePlayerAbilities()
    {
        foreach (Ability ability in PlayerAbilities.Values)
        {
            Destroy(ability.gameObject);
        }
        PlayerAbilities.Clear();
        int count = 0;
        foreach (Ability presetAbility in GameRegistry.Instance.Abilities)
        {
            if (presetAbility.StartUnlocked)
            {
                if (count >= 4 && !(presetAbility is Chronoshift && ChronoshiftCharges > 0))
                {
                    Debug.LogWarning($"Ability {presetAbility.Name} not given");
                }
                else
                {
                    if (presetAbility is Chronoshift) GiveChronoshift();
                    else GivePlayerAbility(presetAbility.ID);
                    if (presetAbility.ID != 0) count++;
                }

            }
        }
    }

    public void GiveChronoshift()
    {
        Ability ability = Instantiate(GameRegistry.Instance.Chronoshift, player.transform);
        ability.ID = -1;
        chronoshift = ability as Chronoshift;
    }
    
    public void GivePlayerAbility(int index)
    {
        Ability ability = Instantiate(GameRegistry.Instance.Abilities[index], player.transform);
        PlayerAbilities[index] = ability;
        ability.ID = index;
        ability.CurrentLevel = GameRegistry.Instance.Abilities[index].CurrentLevel;
        /*
        if (ability.ID != 0 && (allAbilitiesAreCharged || ability.Data.UsesCharges))
        {
            ability.UsesCharges = true;
            ability.MaxCharges = ability.Data.MaxCharges;
            ability.CurCharges = ability.MaxCharges;
        }
        */

    }

    public T GetAbility<T>() where T : Ability
    {
        if (typeof(T) == typeof(Chronoshift)) return chronoshift as T;
        
        foreach (Ability ability in PlayerAbilities.Values)
        {
            if  (ability.GetType() == typeof(T)) return ability as T;
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
        return PlayerAbilities.Values.ToList();
    }

    public void ResetAbilites()
    {
        foreach (var ability in PlayerAbilities.Values)
        {
            ability.OnReset();
        }

    }

    public void RechargeAbilities()
    {
        foreach (Ability ability in PlayerAbilities.Values)
        {
            ability.Recharge();
            if (ability.UsesCharges)
            {
                ability.CurCharges = ability.MaxCharges;
            }
        }
    }
}



