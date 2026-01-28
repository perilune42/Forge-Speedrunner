using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    // TODO - Modify volume via UI sliders later
    [SerializeField] [Range(-80.0f, 20.0f)] private float masterVolume = 0.0f;
    [SerializeField] [Range(-80.0f, 20.0f)] private float musicVolume = 0.0f;
    [SerializeField] [Range(-80.0f, 20.0f)] private float soundEffectVolume = 0.0f;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private GameObject soundEffectPrefab;

    private void Start()
    {
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSoundEffectsVolume(soundEffectVolume);
    }

    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("Master", volume);
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("Music", volume);
    }

    public void SetSoundEffectsVolume(float volume)
    {
        mixer.SetFloat("SoundEffects", volume);
    }

    public void PlaySoundEffect(AudioClip audioClip, Transform transform, float volume = 1.0f)
    {
        GameObject audioSourceObject = Instantiate(soundEffectPrefab, transform);
        AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(audioSourceObject, audioSource.clip.length);
    }
}
