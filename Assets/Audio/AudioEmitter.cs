using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioEmitter : MonoBehaviour
{
    private Dictionary<string, StudioEventEmitter> emitters;

    private void Awake()
    {
        emitters = new Dictionary<string, StudioEventEmitter>();

        foreach (var emitter in GetComponents<StudioEventEmitter>())
        {
            emitters.Add(emitter.EventReference.Path, emitter);
        }
    }

    public void Play(string eventPath)
    {
        if (emitters.TryGetValue(eventPath, out var emitter))
            emitter.Play();
    }

    public void Stop(string eventPath)
    {
        if (emitters.TryGetValue(eventPath, out var emitter))
            emitter.Stop();
    }
}