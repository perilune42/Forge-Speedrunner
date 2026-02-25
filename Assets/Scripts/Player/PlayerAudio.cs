using System;
using UnityEngine;

// used for looped sounds
public class PlayerAudio : MonoBehaviour
{
    AudioEmitter audioEmitter;
    AudioCondition walking, fast, climbing;

    private void Awake()
    {
        PlayerMovement pm = Player.Instance.Movement;
        audioEmitter = GetComponent<AudioEmitter>();
        walking = new(audioEmitter, "event:/Walk Loop",
            () => pm.Velocity.x != 0 && pm.State == BodyState.OnGround);
        fast = new(audioEmitter, "event:/Wind Loop",
            () => pm.IsFast);
        climbing = new(audioEmitter, "event:/Climbing Loop",
            () => pm.SpecialState == SpecialState.WallClimb && pm.Velocity.y != 0);
    }

    private void FixedUpdate()
    {
        walking.Update();
        fast.Update();
        climbing.Update();
    }


}

public class AudioCondition
{
    private readonly AudioEmitter emitter;
    private readonly string eventPath;
    private readonly Func<bool> condition;

    private bool isActive;

    public AudioCondition(AudioEmitter emitter, string eventPath, Func<bool> condition)
    {
        this.emitter = emitter;
        this.eventPath = eventPath;
        this.condition = condition;
    }

    public void Update()
    {
        bool shouldBeActive = condition();

        if (!isActive && shouldBeActive)
        {
            isActive = true;
            emitter.Play(eventPath);
        }
        else if (isActive && !shouldBeActive)
        {
            isActive = false;
            emitter.Stop(eventPath);
        }
    }
}