using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AbilityManager : Singleton<AbilityManager>
{
    [Header("=== DEBUG OPTIONS ===")]
    // USED FOR SHOP TESTING ONLY, SELF DESTRUCTS ON ABILITY ASSIGNMENT
    [SerializeField] bool shopDebugMode = false;

    // More debug options
    [SerializeField] bool giveAllAbilities = false;
    [SerializeField] bool allAbilitiesAreCharged = false;


    public GameObject AbilityInfoPrefab;
    public GameObject AbilityInfoParent, ChronoshiftInfoParent;
    [SerializeField] private GameObject player;
    public PlayerMovement playerMovement;
    public Dictionary<int, Ability> PlayerAbilities;

    public int ChronoshiftCharges;
    
    public override void Awake()
    {
        base.Awake();
        PlayerAbilities = new();
        ChronoshiftInfoParent = GameObject.FindWithTag("CHRONOSHIFT_INFO_PARENT"); // jank because I can't push direct changes to the World scene
        if (shopDebugMode)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            GivePlayerAbilities();
        }
    }
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(100, 0, 100, 100));
        if (GUILayout.Button("Go to shop"))
        {
            // SceneManager.LoadScene("Shop");
            Game.Instance.GoToShop(true);
        }
        GUILayout.EndArea();
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
            if (presetAbility.StartUnlocked || giveAllAbilities)
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
    }
    
    public void GivePlayerAbility(int index)
    {
        Ability ability = Instantiate(GameRegistry.Instance.Abilities[index], player.transform);
        PlayerAbilities[index] = ability;
        ability.ID = index;
        if (giveAllAbilities)
        {
            ability.CurrentLevel = ability.AllLevels.Length - 1;
        }
        else
        {
            ability.CurrentLevel = GameRegistry.Instance.Abilities[index].CurrentLevel;
        }
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



