using System;
using Unity.VisualScripting;
using UnityEngine;

// stores constant game resources and definitions to be accessed by other classes in any scene
public class GameRegistry : Singleton<GameRegistry>
{
    public Ability[] Abilities;


    public override void Awake()
    {
        base.Awake();
        for (int i = 0; i < Abilities.Length; i++)
        {
            Ability ability = Abilities[i];
            ability.ID = i;
        }
        DontDestroyOnLoad(gameObject);
    }

}