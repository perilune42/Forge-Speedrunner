using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AbilityManager : Singleton<AbilityManager>
{
    // USED FOR SHOP TESTING ONLY, SELF DESTRUCTS ON ABILITY ASSIGNMENT
    [SerializeField] bool shopDebugMode = false;    

    public AbilitySceneContainer[] Abilities;
    public GameObject AbilityInfoPrefab;
    public GameObject AbilityInfoParent;
    [SerializeField] private GameObject player;
    public PlayerMovement playerMovement;
    private List<Ability> playerAbilities;
    
    public override void Awake()
    {
        base.Awake();
        playerAbilities = new();
        if (!AbilitySceneTransfer.Initialized) Init();

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
            SceneManager.LoadScene("Shop");
        }
    }

    /*private void Update()
    {
        Debug.Log(pm == null);
    }*/

    private void Init()
    {
        AbilitySceneTransfer.AbilityDataArray = new AbilityData[Abilities.Length];
        for (int i = 0; i < Abilities.Length; i++)
        {
            Abilities[i].data.ID = i;
            if (Abilities[i].abilityPrefab.GetComponent<Ability>() is Dash) 
                Abilities[i].data.Level = 1;
            else Abilities[i].data.Level = 0;
            AbilitySceneTransfer.AbilityDataArray[i] = Abilities[i].data;
        }
        AbilitySceneTransfer.Initialized = true;
    }

    private void GivePlayerAbilities()
    {
        foreach (Ability ability in playerAbilities)
        {
            Destroy(ability.gameObject);
        }

        foreach (AbilityData abilityData in AbilitySceneTransfer.AbilityDataArray)
        {
            if (abilityData.Level > 0)
            {
                GivePlayerAbility(abilityData.ID);
            }
        }
}
    
    public void GivePlayerAbility(int index)
    {
        Ability ability = Instantiate(Abilities[index].abilityPrefab, player.transform).GetComponent<Ability>();
        playerAbilities.Add(ability);
        ability.Data = AbilitySceneTransfer.AbilityDataArray[index];
        ability.ID = index;
        if (ability.Data.UsesCharges)
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
}

[System.Serializable]
public struct AbilitySceneContainer
{
    public GameObject abilityPrefab;
    public AbilityData data;
}

public enum AbilityType 
{
    Dash,
    GroundSlam,
}