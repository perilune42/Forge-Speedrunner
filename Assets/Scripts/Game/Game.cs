using TMPro;
using UnityEngine;

public class Game : Singleton<Game> {
    public enum State {
        Paused,
        Playing
    }
    public State currState;
    public double money;
    public TMP_Text moneyText;
    public GameObject up1;
    public GameObject up2;
    public GameObject up3;
    public double upCost1;
    public double upCost2;
    public double upCost3;
    public bool upBought1;
    public bool upBought2;
    public bool upBought3;
    public TMP_Text upText1;
    public TMP_Text upText2;
    public TMP_Text upText3;
    void Start()
    {
        currState = State.Paused;
        money = 5;
        upCost1 = 1;
        upCost2 = 2;
        upCost3 = 3;
        upBought1 = false;
        upBought2 = false;
        upBought3 = false;
        moneyText.text = money + "";
        upText1.text = "Cost: " + upCost1;
        upText2.text = "Cost: " + upCost2;
        upText3.text = "Cost: " + upCost3;
    }

    private void OnGUI() {
        if (GUILayout.Button("10 more dollar")) {
            money += 10;
            UpdateMoney();
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

    public void UpgradeClicked1() {
        if (upBought1) {
            Debug.Log("Bought Already");
        } else if (money - upCost1 < 0) {
            Debug.Log("You Broke");
        } else {
            money -= upCost1;
            UpdateMoney();
            upText1.text = "Bought";
            upBought1 = true;
        }
    }

    public void UpgradeClicked2() {
        if (upBought2) {
            Debug.Log("Bought Already");
        } else if (money - upCost2 < 0) {
            Debug.Log("You Broke");
        } else {
            money -= upCost2;
            UpdateMoney();
            upText2.text = "Bought";
            upBought2 = true;
        }
    }

    public void UpgradeClicked3() {
        if (upBought3) {
            Debug.Log("Bought Already");
        } else if (money - upCost3 < 0) {
            Debug.Log("You Broke");
        } else {
            money -= upCost3;
            UpdateMoney();
            upText3.text = "Bought";
            upBought3 = true;
        }
    }

    void UpdateMoney() {
        moneyText.text = money + "";
    }
}
