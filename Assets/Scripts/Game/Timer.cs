using System.ComponentModel;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Static variables for speedrun timing
    public static float targetSpeedrunTime;
    public static float previousSpeedrunTime;
    public static float speedrunTime = 0.0f;
    public static bool timeSpeedrun = false;

    // Pauses timescale if bool passed is true. Sets timescale to 0 so that the game is paused but UI can still be accessed and changed.
    public void pauseTime(bool pause) {
        if (pause) { 
            Time.timeScale = 0.0f;
        }
        else { 
            Time.timeScale = 1.0f;
        }
    }

    void Update() {
        if (timeSpeedrun) { 
            speedrunTime += Time.deltaTime;
        }
    }
}

// Notes:
// Add a maxTimer (default 10 min) and if speedrunTime reaches that, you lose.
// Every run, this maxTimer decreases (based on the user?)