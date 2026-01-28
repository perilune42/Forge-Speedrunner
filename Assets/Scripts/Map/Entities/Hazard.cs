using UnityEngine;

public class Hazard : Entity
{
    public override bool IsSolid => false;
    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        if(de == Player.Instance.Movement)
            RoomManager.Instance.Respawn();
    }
}
