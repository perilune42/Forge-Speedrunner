using System.Collections.Generic;
using UnityEngine;

public class Hazard : Entity
{
    public override bool IsSolid => false;
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerInside && !RoomManager.Instance.TransitionOngoing)
        {
            var dist = Hitbox.Distance(Player.Instance.Movement.Hurtbox);
            // stupid hack to bypass contact offset making hurtbox bigger
            if (dist.distance <= -Physics2D.defaultContactOffset)
            {
                RoomManager.Instance.Respawn();
            }
        }
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        DynamicEntity de = collision.GetComponent<DynamicEntity>();
        if (de == null) return;
        if (de.GetComponent<GrappleHand>() != null)
        {
            de.GetComponent<GrappleHand>().Grapple.Abort();
        }
    }
} 

