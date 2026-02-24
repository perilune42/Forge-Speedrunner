using TMPro;
using UnityEditor;
using UnityEngine;

public class Game : Singleton<Game> {
    public int CurrentRound;
    public bool IsPracticeMode = false;

    [Header("Progression Modifiers")]
    public float initialGoalTime;
    public float GoalTimeScale;  // next goaltime = prev goal * GTS, round to nearest 5
    public float PBTimeScale; // if pb * pbtimescale < next goaltime, use the pb time instead
    public float RewardMultiplier;  // bonus reward = (goal/runtime - 1) * RM
    public float RewardDecay; // bonus reward = (bonus reward) ^ decay
    public float MinReward; // base added to bonus reward
    public float RewardHardcap; // total amount cannot exceed this much
    public float RewardThreshold; // must be under target time by this much to receive reward
    public float RewardMultPerRound;

    public void FinishRound()
    {
        Timer.RecordTime();
        CurrentRound++;
        GoToShop(true);
    }

    public void GoToShop(bool newRound)
    {
        ShopManager.Instance.LoadShop(newRound);
        // deactivate all the stuff in the world
        RoomManager.Instance.gameObject.SetActive(false);
        Player.Instance.gameObject.SetActive(false);

        // stop the count
        Timer.Instance.Pause(true);
    }

    public void ReturnToPlay(bool practiceMode, Doorway startDoorway = null)
    {
        ShopManager.Instance.CloseShop();
        // reset rooms and player
        RoomManager.Instance.gameObject.SetActive(true);
        RoomManager.Instance.ResetAllEntities();
        Player.Instance.gameObject.SetActive(true);

        // start the count at 0
        Timer.speedrunTime = 0F;
        Timer.Instance.Pause(false);



        AbilityManager.Instance.ResetAbilites();
        AbilityManager.Instance.RechargeAbilities();

        Player.Instance.Movement.OnReset();
        PInput.Instance.OnReset();

        IsPracticeMode = practiceMode;
        GameplayUI.Instance.TogglePracticeMode(practiceMode);

        if (startDoorway == null)
        {
            RoomManager.Instance.SpawnAtStart();
        }
        else
        {
            RoomManager.Instance.SpawnAtDoorway(startDoorway);
        }
    }



    void Update()
    {
        RoomManager rm = RoomManager.Instance;
        if(Input.GetKeyDown(KeyCode.R))
            rm.ReEnterRoom();
        else if(Input.GetKeyDown(KeyCode.Z))
            ReturnToPlay(false);
        else if(Input.GetKeyDown(KeyCode.X))
            GoToShop(true);

        if (IsPracticeMode && Input.GetKeyDown(KeyCode.Return))
        {
            GoToShop(false);
        }
    }

    public int GetRunReward()
    {
        float reward = MinReward;
        float factor = Timer.previousTargetTime / Timer.previousSpeedrunTime;
        float bonus = Mathf.Pow((factor - 2 + RewardThreshold) * RewardMultiplier, RewardDecay);
        if (bonus < 0) bonus = 0;
        reward += bonus;
        reward *= Mathf.Pow(RewardMultPerRound, CurrentRound - 1);
        return Mathf.RoundToInt(Mathf.Min(RewardHardcap, reward));
    }

    public void TestGetRunReward()
    {
        float reward = MinReward;
        float factor = Timer.targetSpeedrunTime * RewardThreshold / Timer.speedrunTime;
        float bonus = Mathf.Pow(Mathf.Max(0, (factor - 1)) * RewardMultiplier, RewardDecay);
        Debug.Log(bonus);
        reward += bonus;
        reward *= Mathf.Pow(RewardMultPerRound, CurrentRound);
        Debug.Log(reward);
    }

    public float GetNewGoal()
    {
        // todo: slight adaptive scaling
        float pbTime = Timer.previousSpeedrunTime * PBTimeScale;
        float baseTime = Timer.previousTargetTime * GoalTimeScale;
        return Mathf.Min(baseTime, pbTime);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Game))]
public class Game_Inspector : Editor
{
    override public void OnInspectorGUI()
    {

        Game g = (Game)target;
        DrawDefaultInspector();

        if (Application.isPlaying && GUILayout.Button("Add 30 Seconds"))
        {
            Timer.speedrunTime += 30f;
        }

        if (Application.isPlaying && GUILayout.Button("Sim rewards"))
        {
            Game.Instance.TestGetRunReward();
        }
    }
}

#endif