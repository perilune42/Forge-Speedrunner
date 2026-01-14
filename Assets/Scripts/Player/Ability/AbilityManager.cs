using System;
using UnityEngine;

public class AbilityManager : Singleton<AbilityManager>
{
    /// <summary>
    /// Due to a bug with Unity, these aren't actually prefabs for the time being.
    /// The bug pertains to not being able to set InputActionReferences on prefabs.
    /// This bug appeared literally a week ago, and should be fixed in the next week:
    /// https://discussions.unity.com/t/inputactionreference-not-saved-on-prefab/1699980/16
    /// </summary>
    public GameObject[] AbilityPrefabs;
    [SerializeField] private PlayerMovement playerMovement;

    private void Start()
    {
        GivePlayerAbility(AbilityID.Dash);
    }

    
    
    public void GivePlayerAbility(int index)
    {
        Ability ability = Instantiate(AbilityPrefabs[index], playerMovement.transform).GetComponent<Ability>();
        ability.PlayerMovement = playerMovement;
        ability.Initialize();
    }
}

public class AbilityID
{
    public const int Dash = 0;
}