using UnityEngine;

public class Hazard : Entity
{
    public override bool IsSolid => true;
    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        if(de == Player.Instance.Movement)
            RoomManager.Instance.Respawn();
    }
}
