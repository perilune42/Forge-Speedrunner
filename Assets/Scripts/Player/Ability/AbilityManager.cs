using System;
using System.Collections.Generic;
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


    // migrated to GameRegistry, as they are constant definitions
    // public AbilitySceneContainer[] Abilities;
    public GameObject AbilityInfoPrefab;
    public GameObject AbilityInfoParent;
    [SerializeField] private GameObject player;
    public PlayerMovement playerMovement;
    private List<Ability> playerAbilities;
    
    public override void Awake()
    {
        base.Awake();
        playerAbilities = new();

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
        if (GUILayout.Button("Go to shop"))
        {
            // SceneManager.LoadScene("Shop");
            Game.Instance.GoToShop();
        }
    }


    private void GivePlayerAbilities()
    {
        foreach (Ability ability in playerAbilities)
        {
            Destroy(ability.gameObject);
        }
        int count = 0;
        foreach (AbilityData abilityData in ProgressionData.Instance.AbilityDataArray)
        {

            if (abilityData.Level > 0 || giveAllAbilities)
            {
                if (count >= 4)
                {
                    Debug.LogWarning($"Ability {abilityData.name} not given");
                }
                else
                {
                    GivePlayerAbility(abilityData.ID);
                    if (abilityData.ID != 0) count++;
                }

            }
        }
}
    
    public void GivePlayerAbility(int index)
    {
        Ability ability = Instantiate(GameRegistry.Instance.Abilities[index].abilityPrefab, player.transform).GetComponent<Ability>();
        playerAbilities.Add(ability);
        ability.Data = ProgressionData.Instance.AbilityDataArray[index];
        ability.ID = index;
        if (giveAllAbilities)
        {
            ability.Level = 2;
        }
        else
        {
            ability.Level = ProgressionData.Instance.AbilityDataArray[index].Level;
        }
        if (ability.ID != 0 && (allAbilitiesAreCharged || ability.Data.UsesCharges))
        {
            ability.UsesCharges = true;
            ability.MaxCharges = ability.Data.MaxCharges;
            ability.CurCharges = ability.MaxCharges;
        }

    }

    public T GetAbility<T>() where T : Ability
    {
        foreach (Ability ability in playerAbilities)
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
        return playerAbilities;
    }

    /// <summary>
    /// Use during room transitions to reset ability states
    /// Currently only Grapple uses this functionality
    /// </summary>
    public void ResetAbilites()
    {
        if (TryGetAbility<Grapple>(out Grapple grapple))
        {
            grapple.Reset();
        }
    }
}

[System.Serializable]
public struct AbilitySceneContainer
{
    public GameObject abilityPrefab;
    public AbilityData data;
}


