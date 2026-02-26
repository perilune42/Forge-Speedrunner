using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SafeZone : Trigger
{
    private void FixedUpdate()
    {
        if (!playerInside || RoomManager.Instance.TransitionOngoing) return;
        
        if (Player.Instance.Movement.State == BodyState.OnGround)
        {
            var plat = AbilityManager.Instance.GetAbility<Platform>();
            if (plat != null && plat.IsPlayerTouchingPlatform()) 
            {
                return;
            }


            List<Collider2D> res = new();
            Physics2D.OverlapCollider(Player.Instance.Movement.Hurtbox, res);
            if (res.Where((c) => c.GetComponent<Hazard>() != null).Count() > 0)
            {
                return;
            }
            RoomManager.Instance.RespawnPosition = Player.Instance.Movement.transform.position;
        }
        
    }
}