using DG.Tweening;
using FMODUnity;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PulseDrive : Ability
{
    [SerializeField] float horzSpeed = 25, vertSpeed = 12;

    [SerializeField] ParticleSystem shockwaveParticles, idleParticles;

    [SerializeField] int pulseDelay = 15;
    [SerializeField] int forceMoveTime = 15;

    int timeToActivation = -1;
    int pulsesRemaining = 0;

    PlayerMovement pm => Player.Instance.Movement;

    public override void Start()
    {
        base.Start();

    }

    public override void OnReset()
    {
        base.OnReset();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (inputButton.HasPressed)
        {
            if (CanUseAbility()) UseAbility();
        }

        if (pulsesRemaining > 0)
        {
            timeToActivation--;
            if (timeToActivation < 0)
            {
                ActivatePulse();
                if (inputButton.IsPressing)
                {
                    pulsesRemaining--;
                    timeToActivation = pulseDelay;
                }
                else pulsesRemaining = 0;
                if (pulsesRemaining == 0)
                {
                    idleParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

       
    }


    public override float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    public override bool UseAbility()
    {
        base.UseAbility();
        idleParticles.Play();
        pm.SpecialState = SpecialState.Normal;
        pulsesRemaining = CurrentLevel >= 2 ? 3 : 1;
        timeToActivation = pulseDelay;
        inputButton.ConsumeBuffer();
        return true;
    }

    public override bool CanUseAbility()
    {
        return base.CanUseAbility() && pm.SpecialState == SpecialState.Normal;
    }

    private void ActivatePulse()
    {
        Vector2 dir = new(-pm.FacingDir.x, 0);

        if (CurrentLevel >= 1 && pm.State == BodyState.OnGround && PInput.Instance.MoveVector.y < 0)
        {
            dir = new(0, -1);
            pm.Velocity = new(pm.Velocity.x, horzSpeed * 1.5f);
        }
        else
        {
            if (Util.SignOr0(dir.x) == Util.SignOr0(pm.Velocity.x))
            {
                pm.Velocity = new(pm.Velocity.x + dir.x * horzSpeed, vertSpeed);
            }
            else
            {
                pm.Velocity = new(dir.x * horzSpeed, vertSpeed);
            }
        }
        PlayParticles(dir);
        pm.ForceMove(dir, forceMoveTime);
        RuntimeManager.PlayOneShot("event:/Parry Sound Launch");

    }

    private void PlayParticles(Vector2 dir)
    {
        if (dir.x != 0) dir = -dir;
        var p = Instantiate(shockwaveParticles, pm.transform);
        p.transform.position = transform.position + (Vector3)(Vector2.up * pm.PlayerHeight * 0.5f);
        p.transform.position += new Vector3(dir.x * pm.PlayerWidth * 0.5f, dir.y * pm.PlayerHeight * 0.5f);

        p.transform.eulerAngles = new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, dir));
        p.Play();
    }
}