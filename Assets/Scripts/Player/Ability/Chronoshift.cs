using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Chronoshift : Ability, IStatSource
{
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private int teleportDelay;
    private int curTeleportTime;
    [SerializeField] private float teleportSpeed, teleportAcceleration;
    private float curTeleportSpeed;
    private GameObject clone;
    private Action stopCloneParticleAction;

    [SerializeField] private Volume volume;
    private LiftGammaGain gamma;
    private ColorAdjustments colors;
    [SerializeField] private Vector4 darkVector;
    [SerializeField] private float saturation;
    [SerializeField] private GameObject shatterParticle;

    [SerializeField] private int keyframeInterval;
    private List<ChronoshiftKeyframe> keyframes;
    private ChronoshiftKeyframe curKeyframe;
    private int curKeyframeTime;
    private int keyframeIndex;


    public override void Start()
    {
        base.Start();
        MaxCharges = AbilityManager.Instance.ChronoshiftCharges;
        if (volume.profile.TryGet<LiftGammaGain>(out LiftGammaGain gammaProfile)) gamma = gammaProfile;
        if (volume.profile.TryGet<ColorAdjustments>(out ColorAdjustments colorsProfile)) colors = colorsProfile;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (inputButton.HasPressed && CanUseAbility() && GetCooldown() >= 1f) UseAbility();
        if (PlayerMovement.SpecialState == SpecialState.Chronoshift && !RoomManager.Instance.transitionWaiting)
        {
            if (clone == null) // failsafe measure in case functions execute in weird order
            {
                PlayerMovement.SpecialState = SpecialState.Normal;
                return;
            }

            // move player
            PlayerMovement.transform.position = Vector3.MoveTowards(
                PlayerMovement.transform.position,
                curKeyframe.position,
                curTeleportSpeed); 
            if (Vector3.Distance(PlayerMovement.transform.position, curKeyframe.position) < 0.1f)
            {
                keyframeIndex++;
                if (keyframeIndex < keyframes.Count) 
                {
                    curKeyframe = keyframes[keyframeIndex];
                    if (curKeyframe.room != RoomManager.Instance.activeRoom)
                    {
                        Debug.Log("Should switch rooms");
                        Debug.Log(keyframeIndex);
                        StartCoroutine(RoomManager.Instance.RoomTransition(curKeyframe.room, curKeyframe.position, Vector2.zero, Vector2.zero));
                    }
                    RoomManager.Instance.RespawnPosition = curKeyframe.respawnPosition;
                    Timer.speedrunTime = curKeyframe.time;
                }
            }
            
            
            curTeleportSpeed *= teleportAcceleration;
            if (Vector3.Distance(PlayerMovement.transform.position, clone.transform.position) < 0.1f)
            {
                Instantiate(shatterParticle, clone.transform.position, Quaternion.identity);
                CancelTeleport();
            }
        }
        
        if (PlayerMovement.SpecialState != SpecialState.Chronoshift && clone != null)
        {
            curTeleportTime--;
            if (curTeleportTime == 0)
            {
                Teleport();
            }

            curKeyframeTime--;
            if (curKeyframeTime <= 0)
            {
                ChronoshiftKeyframe kf = new ChronoshiftKeyframe(
                    PlayerMovement.transform.position, 
                    Timer.speedrunTime, 
                    RoomManager.Instance.activeRoom,
                    RoomManager.Instance.RespawnPosition);
                keyframes.Insert(0, kf);
                curKeyframeTime = keyframeInterval;
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
            
                keyframes = new();
                curKeyframeTime = 0;
            
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
        PlayerMovement.GravityMultiplier.Multipliers[this] = 0f;
        PlayerMovement.SpecialState = SpecialState.Chronoshift;
        gamma.gamma.Override(darkVector);
        colors.saturation.Override(saturation);
        stopParticleAction += PlayerVFXTrail.PlayParticle(Color.white);
        PlayerMovement.SurfaceCollider.enabled = false;
        curTeleportSpeed = teleportSpeed;
        PlayerMovement.Velocity = Vector2.zero;

        
        curKeyframe = keyframes[0];
        keyframeIndex = 0;
    }

    private void CancelTeleport()
    {
        stopCloneParticleAction?.Invoke();
        Destroy(clone);
        gamma.gamma.Override(new Vector4(1f, 1f, 1f, 0f));
        colors.saturation.Override(0f);
        PlayerMovement.GravityMultiplier.Multipliers.Remove(this);
        stopParticleAction?.Invoke();
        PlayerMovement.SurfaceCollider.enabled = true;
    }

    public override void OnReset()
    {
        base.OnReset();
    }

    
}

public struct ChronoshiftKeyframe
{
    public Vector3 position;
    public float time;
    public Room room;
    public Vector3 respawnPosition;
    public ChronoshiftKeyframe(Vector3 position, float time, Room room, Vector3 respawnPosition)
    {
        this.position = position;
        this.time = time;
        this.room = room;
        this.respawnPosition = respawnPosition;
    }
}