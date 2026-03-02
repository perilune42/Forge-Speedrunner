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
            string path = getPath(emitter.EventReference.Guid);
            emitters.Add(path, emitter);
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

    public static string getPath(FMOD.GUID guid)
    {
        string path = string.Empty;

        //FMOD.Studio.System sys;
        //FMOD.Studio.System.create(out sys);
        //sys.lookupPath(guid, out path);

        RuntimeManager.StudioSystem.lookupPath(guid, out path);

        return path;
    }
}