using FMODUnity;

public class MusicPlayer : Singleton<MusicPlayer>
{
    StudioEventEmitter emitter;

    public override void Awake()
    {
        emitter = GetComponent<StudioEventEmitter>();
        if (Instance == null) emitter.Play();
        base.Awake();
        DontDestroyOnLoad(gameObject);
        
        RuntimeManager.GetBus("bus:/Music").setVolume(0.15f);
        
    }

    public void EnterShop()
    {
        emitter.SetParameter("Shop", 1);
    }

    public void EnterPlay()
    {
        emitter.SetParameter("Shop", 0);
    }

}