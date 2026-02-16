using UnityEngine;

public enum TimerState
{
    Paused,
    Playing
}

// This Timer class handles all speedrun logic and pausing
public class Timer : Singleton<Timer>
{

    // Public static variables for speedrun timing
    public const float SPEEDRUN_TIME_SHRINKER = 0.95f;
    public const float MAX_TIME = 600f; // Max speedrun time (seconds) in the beggining of a new game
    public static float targetSpeedrunTime = -1f; // Variable to hold max speedrun time
    public static float previousSpeedrunTime;
    public static float previousTargetTime;
    public static float speedrunTime = 0.0f;
    public static bool timeSpeedrun = true;


    // Pauses timescale if bool passed is true. Sets timescale to 0 so that the game is paused but UI can still be accessed and changed.
    public void Pause(bool pause) {
        if (pause) 
        { 
            Time.timeScale = 0.0f;
            currState = TimerState.Paused;
        }
        else 
        { 
            Time.timeScale = 1.0f;
            currState = TimerState.Playing;
        }
        
    }

    void FixedUpdate() {
        if (timeSpeedrun) 
        { 
            speedrunTime += Time.fixedDeltaTime;
        }

        if (speedrunTime >= targetSpeedrunTime) 
        {
            Debug.Log("Time exceeded!");
            // EndGame();
        }
    }

    public override void Awake() {
        base.Awake();
        speedrunTime = 0.0f;
        if (targetSpeedrunTime == -1f)
        {
            targetSpeedrunTime = MAX_TIME;
        }
        else
        {
            // targetSpeedrunTime = previousSpeedrunTime * SPEEDRUN_TIME_SHRINKER;
        }
    }

    public static void RecordTime()
    {
        previousSpeedrunTime = speedrunTime;
        previousTargetTime = targetSpeedrunTime;
        targetSpeedrunTime = previousSpeedrunTime * SPEEDRUN_TIME_SHRINKER;
    }


    public TimerState currState;
    public Timer timer;
    void Start()
    {
        currState = TimerState.Playing;
        timer = gameObject.GetComponent<Timer>();
    }

}

// Notes:
// Every run, this maxTimer decreases (based on the user?) (Right now it is a multipler time previous time)
// Save this data to device?
