using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    public enum State {
        Paused,
        Playing
    }
    public State currState;
    public Timer timer;
    void Start()
    {
        currState = State.Playing;
        timer = gameObject.GetComponent<Timer>();
    }



    public void Pause()
    {
        currState = State.Paused;

        // Pause Time using Timer script
        timer.pauseTime(true);
    }
    public void Play() {
        currState = State.Playing;

        // Unpause Time using Timer script
        timer.pauseTime(false);
    }

    public void SwitchStates() {
        if (currState == State.Playing) {
            currState = State.Paused;

            // PauseTime using Timer script
            timer.pauseTime(true);
        } else {
            currState = State.Playing;

            // UnpauseTime using Timer script
            timer.pauseTime(false);
        }
    }
}
