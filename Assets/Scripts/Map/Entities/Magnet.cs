using System;
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
    private int curCooldown;

    private void Start()
    {
        magnetBeam.transform.localScale = new Vector3(beamRange, beamWidth, 0);
        BoxCollider2D coll = gameObject.AddComponent<BoxCollider2D>();
        BoxCollider2D beamColl = magnetBeam.GetComponent<BoxCollider2D>();
        coll.size = new Vector2(beamColl.size.x * beamRange, beamColl.size.y * beamWidth);
        coll.offset = beamColl.offset;
        bigBeamColl = beamColl;
        bigBeamColl.size = new Vector2(bigBeamColl.size.x + 1f, bigBeamColl.size.y + 1f);
        Hitbox = coll;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerMovement != null && curCooldown <= 0)
        {
            Suck();
            if (!playerCollider.IsTouching(bigBeamColl))
            {
                playerMovement = null;
                curCooldown = 0;
            }
        }
        else
        {
            curCooldown--;
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
        if (true /*!(newVelocity.magnitude > playerMovement.Velocity.magnitude && newVelocity.magnitude > maxSuckSpeed)*/)
        {
            playerMovement.Velocity = newVelocity;
            Debug.Log("sucking");
        }

        if (Vector2.Distance(playerPos, transform.position) < minRange)
        {
            curCooldown = cooldown;
            playerMovement.Velocity *= speedDropoff;
            playerMovement = null;
        }
    }
}
