using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : ActivatableEntity
{
    public override bool IsSolid => false;

    SpriteRenderer sr;
    public bool IsCollected = false;
    ChallengeRoom challengeRoomRef;

    [SerializeField] ParticleSystem collectParticles, idleParticles;

    protected override void Awake()
    {
        base.Awake();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public override void OnValidate()
    {
        base.OnValidate();
    }

    public void AttachToRoom(ChallengeRoom room)
    {
        challengeRoomRef = room;
    }


    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);
        if (de is not PlayerMovement) return;
        if (!IsCollected && !AbilityManager.Instance.chronoshift.CanTeleport)
        {
            Collect();
        }
        
    }


    public void Collect(bool noTeleport = false)
    {
        IsCollected = true;
        sr.enabled = false;
        idleParticles.Stop();
        Game.Instance.OnUpdateDataCount?.Invoke();
        collectParticles.Play();

        if (noTeleport) return;
        List<ChronoshiftKeyframe> keyframes = new();
        for (int i = 0; i <= 10; i++)
        {
            Vector3 pos = Vector3.Lerp(Player.Instance.Movement.transform.position, challengeRoomRef.StartPoint.transform.position, i / 10f);
            keyframes.Add(new ChronoshiftKeyframe(pos, Timer.speedrunTime, challengeRoomRef.GetComponent<Room>()));
        }
        AbilityManager.Instance.GetAbility<Chronoshift>().TeleportToPos(keyframes, challengeRoomRef.StartPoint.transform.position);
    }

    [ContextMenu("Collect")]
    public void CollectFromMenu()
    {
        Collect(true);
    }

    public override void OnActivate()
    {
        base.OnActivate();
    }

    public override void ResetEntity()
    {
        sr.enabled = true;
        IsCollected = false;
        idleParticles.Play();
    }
}