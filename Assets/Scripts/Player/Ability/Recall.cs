using System;
using System.Collections;
using UnityEngine;

public class Recall : Ability
{
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private int teleportDelay;
    private int curTeleportTime;
    [SerializeField] private float teleportSpeed;
    private GameObject clone;
    private Vector2 storedVelocity;
    private SpecialState storedState;
    private Action stopCloneParticleAction;
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (inputButton.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();
        if (PlayerMovement.SpecialState == SpecialState.Teleport)
        {
            if (clone == null) // failsafe measure in case functions execute in weird order
            {
                PlayerMovement.SpecialState = SpecialState.Normal;
                return;
            }
            PlayerMovement.transform.position = Vector3.MoveTowards(
                PlayerMovement.transform.position,
                clone.transform.position,
                teleportSpeed
            );
            if (Vector3.Distance(PlayerMovement.transform.position, clone.transform.position) < 0.1f)
            {
                PlayerMovement.SpecialState = storedState;
                PlayerMovement.Velocity = storedVelocity;
                stopCloneParticleAction?.Invoke();
                Destroy(clone);
            }
        }
        else if (clone != null)
        {
            curTeleportTime--;
            if (curTeleportTime == 0)
            {
                Teleport();
            }
        }
    }

    public override bool UseAbility()
    {
        if (clone == null)
        {
            curTeleportTime = teleportDelay;
            clone = Instantiate(clonePrefab, transform.position, Quaternion.identity);
            PlayerVFXTrail vfx = clone.GetComponentInChildren<PlayerVFXTrail>();
            stopCloneParticleAction += vfx.PlayParticle(Color.white);
            curCooldown = 10;
            return false;
        }
        else
        {
            Teleport();        
            base.UseAbility();
            return true;
        }
    }

    private void Teleport()
    {
        if (clone == null) return;
        storedVelocity = PlayerMovement.Velocity;   
        if (PlayerMovement.SpecialState == SpecialState.Dash || 
            PlayerMovement.SpecialState == SpecialState.GroundSlam ||
            PlayerMovement.SpecialState == SpecialState.Rocket) storedState = PlayerMovement.SpecialState;
        else storedState = SpecialState.Normal;
        Debug.Log(storedState);
        PlayerMovement.SpecialState = SpecialState.Teleport;
    }

}
