using System;
using Unity.Collections;
using UnityEngine;

public class Magnet : Entity
{
    public override bool IsSolid => false;

    [SerializeField] private GameObject magnetBeam;
    [SerializeField] private float beamWidth;
    [SerializeField] private float beamRange;
    [SerializeField] private float suckStrength, maxSuckSpeed, minRange, speedDropoff;
    private PlayerMovement playerMovement;
    private Collider2D playerCollider;
    [SerializeField] private int cooldown;
    private BoxCollider2D bigBeamColl;
    private BoxCollider2D coll;
    private int curCooldown;
    [SerializeField] private ParticleSystem particle, particleActive;
    [SerializeField] private float particleSpeed;
    private int suckFrames;
    [SerializeField] private int maxSuckFrames;
    private bool active;
    
    private void Start()
    {
        magnetBeam.transform.localScale = new Vector3(beamRange, beamWidth, 0);
        coll = gameObject.AddComponent<BoxCollider2D>();
        BoxCollider2D beamColl = magnetBeam.GetComponent<BoxCollider2D>();
        coll.size = new Vector2(beamColl.size.x * beamRange, beamColl.size.y * beamWidth);
        coll.offset = new Vector2(beamColl.offset.x * beamRange, beamColl.offset.y * beamWidth);
        bigBeamColl = beamColl;
        bigBeamColl.edgeRadius = 1f;
        Hitbox = coll;

    
        InitializeParticles(particle, particleSpeed);
        InitializeParticles(particleActive, particleSpeed * 2f);
        particle.gameObject.SetActive(true);
        particleActive.gameObject.SetActive(false);
    }


    private void InitializeParticles(ParticleSystem particle, float speed)
    {
        particle.transform.localPosition = new Vector3(coll.size.x, 0, 0);
        var mainModule = particle.main;
        var sr = mainModule.startRotation;
        sr.constant = -transform.eulerAngles.z * Mathf.Deg2Rad;
        mainModule.startRotation = sr;
        
        SetParticleSpeed(particle, speed);

        var ssy = mainModule.startSizeY;
        ssy = coll.size.y * transform.localScale.y * 1.2f;
        mainModule.startSizeY = ssy;
    }

    private void SetParticleSpeed(ParticleSystem particle, float speed)
    {
        var mainModule = particle.main;

        var ss = mainModule.startSpeed;
        ss.constant = speed;
        mainModule.startSpeed = ss;

        var sl = mainModule.startLifetime;
        sl.constant = coll.size.x * transform.localScale.x * 0.95f / mainModule.startSpeed.constant;
        mainModule.startLifetime = sl;
    }

    private void Activate()
    {
        active = true;
        particle.gameObject.SetActive(false);
        particleActive.gameObject.SetActive(true);
        suckFrames = 0;
    }

    private void Deactivate()
    {
        active = false;
        particle.gameObject.SetActive(false);
        particleActive.gameObject.SetActive(false);
        curCooldown = cooldown;            
        playerMovement = null;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerMovement != null && curCooldown <= 0)
        {
            if (!active) Activate();
            
            Suck();
            if (!playerCollider.IsTouching(bigBeamColl))
            {
                if (active) Deactivate();
            }
        }
        else
        {
            curCooldown--;
            if (!particle.gameObject.activeInHierarchy && curCooldown <= 0) 
                particle.gameObject.SetActive(true);
            if (Hitbox.IsTouching(Player.Instance.Movement.SurfaceCollider))
            {
                OnCollide(Player.Instance.Movement, Vector2.zero);
            }
        }
    }

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        if (de is PlayerMovement)
        {
            playerMovement = de as PlayerMovement;
            playerCollider = playerMovement.SurfaceCollider;
        }
    }

    private void Suck()
    {
        Vector2 playerPos = playerMovement.GetCenterPos();
        float angleToMagnet = Mathf.Atan2(transform.position.y - playerPos.y, transform.position.x - playerPos.x);
        Vector2 pullVec = new Vector2(Mathf.Cos(angleToMagnet), Mathf.Sin((angleToMagnet))) * suckStrength;
        Vector2 newVelocity = playerMovement.Velocity + pullVec;
        if (playerMovement.SpecialState != SpecialState.LedgeClimb && playerMovement.SpecialState != SpecialState.WallClimb /*!(newVelocity.magnitude > playerMovement.Velocity.magnitude && newVelocity.magnitude > maxSuckSpeed)*/)
        {
            playerMovement.Velocity = newVelocity;
        }
        suckFrames++;
        if (suckFrames >= maxSuckFrames || Vector2.Distance(playerPos, transform.position) < minRange)
        {
            playerMovement.Velocity *= speedDropoff;
            if (active) Deactivate();
        }
    }
}
