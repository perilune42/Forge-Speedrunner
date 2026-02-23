using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Recall : Ability, IStatSource
{
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private int teleportDelay;
    private int curTeleportTime;
    [SerializeField] private float teleportSpeed, teleportAcceleration;
    private float curTeleportSpeed;
    private GameObject clone;
    private Vector2 storedVelocity;
    private Vector2 storedRespawnPos;
    private float storedGravityMult;
    private SpecialState storedState;
    private Action stopCloneParticleAction;
    private Room storedRoom;
    
    [SerializeField] private Volume volume;
    private LiftGammaGain gamma;
    private ColorAdjustments colors;
    [SerializeField] private Vector4 darkVector;
    [SerializeField] private float saturation;
    [SerializeField] private GameObject shatterParticle;
    [SerializeField] private int cooldownDecrease;

    [SerializeField] private float gravityMult, walkSpeedMult;

    public override void Start()
    {
        base.Start();

        if (volume.profile.TryGet<LiftGammaGain>(out LiftGammaGain gammaProfile)) gamma = gammaProfile;
        if (volume.profile.TryGet<ColorAdjustments>(out ColorAdjustments colorsProfile)) colors = colorsProfile;
    }

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
                curTeleportSpeed
            );
            curTeleportSpeed *= teleportAcceleration;

            if (CurrentLevel >= 1)
            {
                foreach (Ability ability in AbilityManager.Instance.PlayerAbilities.Values)
                    ability.ModifyCooldown(-cooldownDecrease);
            }
            if (Vector3.Distance(PlayerMovement.transform.position, clone.transform.position) < 0.1f)
            {
                Instantiate(shatterParticle, clone.transform.position, Quaternion.identity);
                CancelTeleport();
            }
        }
        else if (clone != null)
        {
            curTeleportTime--;
            HandlePostProcessing(1f - (float)curTeleportTime / teleportDelay);
            if (curTeleportTime <= 0)
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
            storedRespawnPos = RoomManager.Instance.RespawnPosition;
            storedRoom = RoomManager.Instance.activeRoom;
            if (CurrentLevel >= 2)
            {
                PlayerMovement.WalkSpeed.Multipliers[this] = walkSpeedMult;
                PlayerMovement.GravityMultiplier.Multipliers[this] = gravityMult;
            }
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
        PlayerMovement.GravityMultiplier.Multipliers[this] = 0f;
        if (PlayerMovement.SpecialState == SpecialState.Dash ||
            PlayerMovement.SpecialState == SpecialState.GroundSlam ||
            PlayerMovement.SpecialState == SpecialState.Rocket) storedState = PlayerMovement.SpecialState;
        else storedState = SpecialState.Normal;
        PlayerMovement.SpecialState = SpecialState.Teleport;
        HandlePostProcessing(1f);
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.white);
        PlayerMovement.SurfaceCollider.enabled = false;
        curTeleportSpeed = teleportSpeed;
        PlayerMovement.Velocity = Vector2.zero;
    }

    private void CancelTeleport()
    {
        PlayerMovement.SpecialState = storedState;
        PlayerMovement.Velocity = storedVelocity;
        RoomManager.Instance.RespawnPosition = storedRespawnPos;
        stopCloneParticleAction?.Invoke();
        Destroy(clone);
        clone = null;
        HandlePostProcessing(0f);
        stopParticleAction?.Invoke();
        PlayerMovement.SurfaceCollider.enabled = true;
        if (CurrentLevel >= 2)
        {
            PlayerMovement.WalkSpeed.Multipliers.Remove(this);
            PlayerMovement.GravityMultiplier.Multipliers.Remove(this);
        }
    }

    private void HandlePostProcessing(float intensity)
    {
        gamma.gamma.Override(new Vector4(darkVector.x, darkVector.y, darkVector.z, darkVector.w * intensity));
        colors.saturation.Override(saturation * intensity);
    }

    public override void OnReset()
    {
        base.OnReset();
        if (clone != null && RoomManager.Instance.activeRoom != storedRoom) CancelTeleport();
    }

}
