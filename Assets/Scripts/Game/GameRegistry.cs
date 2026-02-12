using System;
using Unity.VisualScripting;
using UnityEngine;

// stores constant game resources and definitions to be accessed by other classes in any scene
public class GameRegistry : Singleton<GameRegistry>
{
    public AbilitySceneContainer[] Abilities;

    // NOTE: make sure RoomPrefabs does not include StartRoom!
    public GameObject[] RoomPrefabs;
    public GameObject StartRoom;

    private void OnValidate()
    {
        Awake();
    }


    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public AbilityData[] GetInitialAbilities()
    {
        AbilityData[] arr = new AbilityData[Abilities.Length];
        for (int i = 0; i < Abilities.Length; i++)
        {
            Abilities[i].data.ID = i;
            if (Abilities[i].abilityPrefab.GetComponent<Ability>() is Dash)
                Abilities[i].data.Level = 1;
            else Abilities[i].data.Level = 0;

            arr[i] = Abilities[i].data;
        }
        return arr;
    }
}
