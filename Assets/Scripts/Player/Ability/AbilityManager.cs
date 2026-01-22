using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : Singleton<AbilityManager>
{
   
    public GameObject[] AbilityPrefabs;
    public GameObject AbilityInfoPrefab;
    public GameObject AbilityInfoParent;
    [SerializeField] private GameObject player;
    public PlayerMovement playerMovement;
    private List<Ability> playerAbilities;
    private void Start()
    {
        playerAbilities = new();
        
        GivePlayerAbility(AbilityID.Dash);
        
        // only for testing the ground slam
        GivePlayerAbility(AbilityID.GroundSlam);
    }

    /*private void Update()
    {
        Debug.Log(pm == null);
    }*/

    public void GivePlayerAbility(int index)
    {
        Ability ability = Instantiate(AbilityPrefabs[index], player.transform).GetComponent<Ability>();
        playerAbilities.Add(ability);
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

public class AbilityID
{
    public const int Dash = 0;
    public const int GroundSlam = 1;
}