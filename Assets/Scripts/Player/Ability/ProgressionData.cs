/// <summary>
/// Stores all information stored across runs
/// </summary>
public class ProgressionData : Singleton<ProgressionData>
{
    // no longer a static class, but data persists in static instance
    
    public bool Initialized = false; // should be set to False when player loads into new run

    public override void Awake()
    {
        base.Awake();
        Initialized = true;
    }
}
