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

    protected override void FixedUpdate()
    {
        if (currCooldown > 0) currCooldown--;
    }

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);
        if (currCooldown > 0) return;
        // bool slammed = false;
        if (de is PlayerMovement pm)
        {
            pm.onGround?.Invoke();
            if (pm.SpecialState == SpecialState.Dash)
            {
                AbilityManager.Instance.GetAbility<Dash>().CancelDash();
            }
            if (pm.SpecialState == SpecialState.GroundSlam)
            {
                // slammed = true;
                // cancel slam
            }
        }
        bool isHorz = bounceDirection == PDir.Left || bounceDirection == PDir.Right;
        if (isHorz)
        {
            de.Velocity.x = -de.Velocity.x + Util.PDir2Vec(bounceDirection).x * bounceSpeed;
        }
        else
        {
            de.Velocity.y = Util.PDir2Vec(bounceDirection).y * bounceSpeed;
            if (bounceDirection == PDir.Up) de.OnAirborne();
        }
        currCooldown = bounceCooldown;

        AudioManager.Instance?.PlaySoundEffect(audioClips[0], transform, 0.5f);
    }
}