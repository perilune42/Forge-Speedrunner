using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    public enum State {
        Paused,
        Playing
    }
    public TMP_Text stateText;
    public State currState;
    public Game() {
        currState = State.Paused;
    }
    //Remove when timer is implemented
    private void Update() {
        if (currState == State.Playing) {
            stateText.text = "Playing";
        } else if (currState == State.Paused) {
            stateText.text = "Paused";
        }
    }
    public void Pause()
    {
        currState = State.Paused;
    }
    public void Play() {
        currState = State.Playing;
    }

    public void SwitchStates() {
        if (currState == State.Playing) {
            currState = State.Paused;
        } else {
            currState = State.Playing;
        }
    }


}
