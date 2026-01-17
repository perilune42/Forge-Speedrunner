using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    public enum State {
        Paused,
        Playing
    }
    public State currState;
    public double money;
    public Game() {
        currState = State.Paused;
        money = 10; //TODO: Change to 0 when implemented
    }

    void OnGUI() //TODO: Make actual sprites that take away money
    {
        if (GUILayout.Button("Current State: " + currState.ToString()))
        {
            SwitchStates();
        }
        GUILayout.Label("Money: " + money);
        if (GUILayout.Button("-1 Money"))
        {
            if (money - 1 < 0)
            {
                Debug.Log("You are broke.");
            } else {
                money -= 1;
            }
        }

        if (GUILayout.Button("-2 Money")) 
        {
            if (money - 2 < 0)
            {
                Debug.Log("You are broke.");
            } else {
                money -= 2;
            }
        }

        if (GUILayout.Button("-5 Money"))
        {
            if (money - 5 < 0)
            {
                Debug.Log("You are broke.");
            } else {
                money -= 5;
            }
        }
        if (GUILayout.Button("+10 Money"))
        {
            money += 10;
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
