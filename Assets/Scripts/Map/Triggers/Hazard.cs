using UnityEngine;

public class Hazard : Trigger
{
    public override void OnPlayerEnter()
    {
        Debug.Log("dsfds");
        RoomManager.Instance.Respawn();
    }
}
