using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    public enum State {
        Paused,
        Playing
    }
    public State currState;
    public double money;
    public TMP_Text MoneyText;
    public GameObject Upgrade1;
    public GameObject Upgrade2;
    public GameObject Upgrade3;
    public TMP_Text UpgradeText1;
    public TMP_Text UpgradeText2;
    public TMP_Text UpgradeText3;
    void Start()
    {
        MoneyText.text = money + "";
        UpgradeText1.text = "Cost 1";
        UpgradeText2.text = "Cost 2";
        UpgradeText3.text = "Cost 3";
    }
    public Game() {
        currState = State.Paused;
        money = 10; //TODO: Change to 0 when implemented
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
