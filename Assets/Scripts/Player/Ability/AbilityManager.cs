using System;
using UnityEngine;

public class AbilityManager : Singleton<AbilityManager>
{
   
    public GameObject[] AbilityPrefabs;
    public GameObject AbilityInfoPrefab;
    public GameObject AbilityInfoParent;
    [SerializeField] private GameObject player;
    public PlayerMovement playerMovement;
    private void Start()
    {
        GivePlayerAbility(AbilityID.Dash);
        GivePlayerAbility(AbilityID.GroundSlam);
    }

    /*private void Update()
    {
        Debug.Log(pm == null);
    }*/

    public void GivePlayerAbility(int index)
    {
        Ability ability = Instantiate(AbilityPrefabs[index], player.transform).GetComponent<Ability>();
    }
}

public class AbilityID
{
    public const int Dash = 0;
    public const int GroundSlam = 1;
}