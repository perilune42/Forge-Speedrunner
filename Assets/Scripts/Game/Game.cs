using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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

    public bool OverrideStartingRoom;
    [SerializeField] bool enableRandomMap = true;
    public bool AllRoomsDiscovered = false;
    public MapGen Generator;

    [SerializeField] private RoomManager roomManagerRef;
    

    public List<ChronoshiftKeyframe> ChronoshiftKeyframes;
    [SerializeField] private int keyframeInterval;
    private int nextKeyframeTime = 0;
    private Vector3 startPos;
    private float startTime;

    public int BackgroundIndex; // 0 - day, 1 - night, 2 - rain
    public Action OnLoadShop;

    public override void Awake()
    {
        base.Awake();
        //if (MainMenu.GenerateNewMap)
        if (enableRandomMap)
        {
            OverrideStartingRoom = false;
            var (rooms, passages) = Generator.CreateMap();

            roomManagerRef.AllPassages = passages.ToArray();
            roomManagerRef.FinalizeRooms();

            roomManagerRef.StartingRoom = rooms[0];
            roomManagerRef.StartingSpawn = roomManagerRef.StartingRoom.GetComponent<SpawnRoom>().SpawnPoint;
        }
    }

    void Start()
    {
        StartGame();
        ChronoshiftKeyframes = new();
        nextKeyframeTime = keyframeInterval;
        startPos = Player.Instance.Movement.transform.position;
    }

    void FixedUpdate()
    {
        nextKeyframeTime--;
        if (nextKeyframeTime <= 0)
        {
            ChronoshiftKeyframe kf = new ChronoshiftKeyframe(
                    Player.Instance.Movement.transform.position, 
                    Timer.speedrunTime, 
                    RoomManager.Instance.activeRoom);
                ChronoshiftKeyframes.Insert(0, kf);
                nextKeyframeTime = keyframeInterval;
        }
    }

    public void EndGame()
    {
        //RoomManager.Instance.gameObject.SetActive(false);
        Player.Instance.gameObject.SetActive(false);
        Timer.Instance.Pause(true);
        GameplayUI.Instance.GameEndUI.SetActive(true);
    }

    public void StartGame()
    {
        //RoomManager.Instance.gameObject.SetActive(true);
        Player.Instance.gameObject.SetActive(true);
        Timer.Instance.Pause(false);
        Timer.targetSpeedrunTime = initialGoalTime;
        if (MainMenu.SelectedDifficulty == 2)
        {
            Timer.targetSpeedrunTime -= 300;
        }
        else
        {
            Timer.targetSpeedrunTime += 300;
        }
        CurrentRound = 1;
        MusicPlayer.Instance.EnterPlay();
    }

    public void FinishRound()
    {
        StartCoroutine(FinishRoundCoroutine());
    }

    private IEnumerator FinishRoundCoroutine()
    {
        startTime = Timer.speedrunTime;
        AbilityManager.Instance.GetAbility<Chronoshift>().Teleport(ChronoshiftKeyframes, startPos);
        yield return new WaitUntil(() => Player.Instance.Movement.SpecialState != SpecialState.Chronoshift);
        Timer.speedrunTime = startTime;
        Timer.RecordTime();
        GoToShop(true);
        ChronoshiftKeyframes.Clear();
    }

    public void GoToShop(bool newRound)
    {
        ShopManager.Instance.LoadShop(newRound);
        // deactivate all the stuff in the world
        //RoomManager.Instance.gameObject.SetActive(false);
        Player.Instance.gameObject.SetActive(false);

        // stop the count
        Timer.Instance.Pause(true);
        MusicPlayer.Instance.EnterShop();
        BackgroundIndex = UnityEngine.Random.Range(0, 3);
        OnLoadShop?.Invoke();
    }

    public void ReturnToPlay(bool practiceMode, Doorway startDoorway = null)
    {
        ShopManager.Instance.CloseShop();
        // reset rooms and player
        //RoomManager.Instance.gameObject.SetActive(true);
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

        if (!practiceMode) CurrentRound++;
        MusicPlayer.Instance.EnterPlay();
    }



    void Update()
    {
        RoomManager rm = RoomManager.Instance;
        if(Input.GetKeyDown(KeyCode.R))
            rm.ReEnterRoom();
        else if(Input.GetKeyDown(KeyCode.Z))
            ReturnToPlay(false);
        else if(Input.GetKeyDown(KeyCode.X))
            Game.Instance.FinishRound();

        if (IsPracticeMode && Input.GetKeyDown(KeyCode.Return))
        {
            GoToShop(false);
        }
    }

    public int GetRunReward()
    {
        float reward = MinReward;
        float factor = Timer.previousTargetTime * RewardThreshold / Timer.previousSpeedrunTime;
        float bonus = Mathf.Pow(Mathf.Max(0, factor - 1) * RewardMultiplier, RewardDecay);
        reward += bonus;
        reward *= Mathf.Pow(RewardMultPerRound, CurrentRound - 1);
        return Mathf.RoundToInt(Mathf.Min(RewardHardcap, reward));
    }

    

    public float GetNewGoal()
    {
        float pbTime = Timer.previousSpeedrunTime * PBTimeScale;
        float baseTime = Timer.previousTargetTime * GoalTimeScale;
        float t = Mathf.Clamp01(CurrentRound / 5f); // at 5 rounds or above, completely determined by PB time
        float newGoal = Mathf.Lerp(baseTime, Mathf.Min(baseTime,pbTime), t);
        return Util.RoundToNearest(newGoal, 5);
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
