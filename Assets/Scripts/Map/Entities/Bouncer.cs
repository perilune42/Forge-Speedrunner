using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : Entity
{
    [SerializeField] private PDir bounceDirection;
    [SerializeField] private float bounceSpeed;
    public override bool IsSolid => false;
    public override bool StrictCollisions => true;

    private const int bounceCooldown = 6;
    private int currCooldown = 0;

    [SerializeField] private List<AudioClip> audioClips;
    [SerializeField] Animator animator;

    const float verticalBoost = 5f;

    protected override void FixedUpdate()
    {
        if (currCooldown > 0) currCooldown--;
    }

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);

        if (de is not PlayerMovement) return;
        PlayerMovement pm = de as PlayerMovement;
        if (currCooldown > 0) return;

        Vector2 bounceVec = Util.PDir2Vec(bounceDirection);
        

        

        if (AbilityManager.Instance.TryGetAbility<Parry>(out Parry parry))
        {
            if (parry.ParryPrimed)
            {
                pm.PreCollisionVelocity = -GetBounceVelcoity(bounceVec, pm);
                parry.CollideWithWall(this, -bounceVec);
                parry.SpecialEntity = this;
                return;
            }
        }
        pm.Velocity = GetBounceVelcoity(bounceVec, pm);
        SpecialState prevState = pm.SpecialState;
        pm.onGround?.Invoke();
        if (prevState == SpecialState.Dash)
        {
            AbilityManager.Instance.GetAbility<Dash>().CancelDash();
        }
        else if (prevState == SpecialState.GroundSlam)
        {
            // less vertical boost when slammed
            de.Velocity.y = bounceVec.y * bounceSpeed * 0.33f;
        }
        
        AbilityManager.Instance.GetAbility<Dash>().Recharge();

        PlayBouncerEffects();
    }

    public void PlayBouncerEffects()
    {
        currCooldown = bounceCooldown;
        animator.Play("BouncerActive");


        RuntimeManager.PlayOneShotAttached("event:/Bouncepad", gameObject);
    }

    private Vector2 GetBounceVelcoity(Vector2 bounceVec, DynamicEntity de)
    {
        Vector2 velocity;
        // bool slammed = false;
        bool isHorz = bounceDirection == PDir.Left || bounceDirection == PDir.Right;
        if (isHorz)
        {
            velocity.x = -de.Velocity.x + bounceVec.x * bounceSpeed;
            velocity.y = verticalBoost;
        }
        else
        {
            velocity.y = bounceVec.y * bounceSpeed;
            if (bounceDirection == PDir.Up) de.OnAirborne();
            velocity.x = 0;
        }
        return velocity;
    }
}