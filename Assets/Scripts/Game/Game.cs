using TMPro;
using UnityEditor;
using UnityEngine;

public class Game : Singleton<Game> {
    public int CurrentRound = 1;
    public bool IsPracticeMode = false;

    [Header("Progression Modifiers")]
    public float initialGoalTime = 300f;
    public float GoalTimeScale = 0.8f;  // next goaltime = prev goal * GTS, round to nearest 5
    public float RewardMultiplier = 50f;  // reward = (goal/runtime - 1) * RM
    public float RewardFalloffPoint = 1.5f; // if goal/runtime > RFP, apply RFM to remaining time
    public float RewardHardcap = 2.5f;
    public float RewardFalloffMultiplier = 10f;
    public float MinReward = 10f;
    public float RewardMultPerRound = 1.15f;

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
        if (factor <= RewardFalloffPoint)
        {
            reward += (factor - 1) * RewardMultiplier;
        }
        else if (factor <= RewardHardcap)
        {
            Debug.Log($"Reward: {(RewardFalloffPoint - 1) * RewardMultiplier} | {(factor - RewardFalloffPoint) * RewardFalloffMultiplier}");
            reward += (RewardFalloffPoint - 1) * RewardMultiplier + (factor - RewardFalloffPoint) * RewardFalloffMultiplier;
        }
        else
        {
            reward += (RewardFalloffPoint - 1) * RewardMultiplier + (RewardHardcap - RewardFalloffPoint) * RewardFalloffMultiplier;
        }
        reward *= Mathf.Pow(RewardMultPerRound, CurrentRound - 1);
        return Mathf.RoundToInt(reward);
    }

    public float GetNewGoal()
    {
        // todo: slight adaptive scaling
        return Timer.previousTargetTime * GoalTimeScale;
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
    }
}

#endif