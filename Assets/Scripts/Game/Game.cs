using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class Game : Singleton<Game> {
    public int CurrentRound;
    public bool IsPracticeMode = false;

    public float TotalTime = 0f;

    [Header("Progression Modifiers")]
    public float initialGoalTime;
    public float GoalTimeScale;  // next goaltime = prev goal * GTS, round to nearest 5
    public float RewardMultiplier;  // bonus reward = (goal/runtime - 1) * RM
    public float RewardDecay; // bonus reward = (bonus reward) ^ decay
    public float MinReward; // base added to bonus reward
    public float RewardHardcap; // total amount cannot exceed this much
    public float RewardThreshold; // must be under target time by this much to receive reward
    public float RewardMultPerRound;

    [Header("Collectibles")]
    public List<Collectible> Collectibles;
    public float RewardPerExtraData;
    public int DataToWin = 3;


    public MapGenerator Generator;

    [SerializeField] private RoomManager roomManagerRef;
    

    public List<ChronoshiftKeyframe> ChronoshiftKeyframes;
    [SerializeField] private int keyframeInterval;
    private int nextKeyframeTime = 0;
    private Vector3 startPos;
    private float startTime;

    public int BackgroundIndex; // 0 - day, 1 - night, 2 - rain
    public Action OnLoadShop;
    public Action OnUpdateDataCount;

    [Header("Debug Options")]
    public bool OverrideStartingRoom;
    [SerializeField] bool enableRandomMap = true;
    public bool AllRoomsDiscovered = false;
    [SerializeField] bool enableDebugControls = false;

    public Action OnEnterWorld;
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
        List<ChallengeRoom> challengeRooms = new();
        foreach (var room in roomManagerRef.AllRooms)
        {
            ChallengeRoom cRoom = room.GetComponent<ChallengeRoom>();
            if (cRoom != null)
            {
                challengeRooms.Add(cRoom);
                Collectibles.Add(cRoom.Collectible);
            }
        }
        TotalTime = 0f;
    }

    void Start()
    {
        ChronoshiftKeyframes = new();
        nextKeyframeTime = keyframeInterval;
    }

    void FixedUpdate()
    {
        if (Player.Instance.Movement.SpecialState != SpecialState.Chronoshift)
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
    }

    public void EndGame()
    {
        //RoomManager.Instance.gameObject.SetActive(false);
        Player.Instance.gameObject.SetActive(false);
        Timer.Instance.Pause(true);
        GameplayUI.Instance.GameEndUI.SetActive(true);
    }

    public void WinGame()
    {
        //RoomManager.Instance.gameObject.SetActive(false);
        TotalTime += Timer.speedrunTime;
        Player.Instance.gameObject.SetActive(false);
        Timer.Instance.Pause(true);
        GameplayUI.Instance.ShowGameWin();
    }

    public void StartGame()
    {
        //RoomManager.Instance.gameObject.SetActive(true);
        Timer.targetSpeedrunTime = initialGoalTime;
        if (MainMenu.SelectedDifficulty == 2)
        {
            Timer.targetSpeedrunTime -= 150;
        }
        else if (MainMenu.SelectedDifficulty == 0)
        {
            Timer.targetSpeedrunTime += 300;
        }
        CurrentRound = 1;
        ReturnToPlay(false, null, true);
        startPos = Player.Instance.Movement.transform.position;
    }

    public void FinishRound()
    {
        StartCoroutine(FinishRoundCoroutine());
    }

    private IEnumerator FinishRoundCoroutine()
    {
        startTime = Timer.speedrunTime;
        ChronoshiftKeyframes.Add(new ChronoshiftKeyframe(startPos, 0, RoomManager.Instance.StartingRoom));
        AbilityManager.Instance.GetAbility<Chronoshift>().TeleportToPos(ChronoshiftKeyframes, startPos);
        yield return new WaitUntil(() => Player.Instance.Movement.SpecialState != SpecialState.Chronoshift);
        Timer.speedrunTime = startTime;
        TotalTime += Timer.speedrunTime;
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

    public void StartNewRound()
    {
        CurrentRound++;
    }
    public void ReturnToPlay(bool practiceMode, Doorway startDoorway = null, bool gameStart = false)
    {
        ShopManager.Instance.CloseShop();

        if (startDoorway == null)
        {
            RoomManager.Instance.SpawnAtStart(gameStart);
        }
        else
        {
            RoomManager.Instance.SpawnAtDoorway(startDoorway);
        }

        // reset rooms and player
        //RoomManager.Instance.gameObject.SetActive(true);
        RoomManager.Instance.ResetAllEntities();
        Player.Instance.gameObject.SetActive(true);

        // start the count at 0
        Timer.speedrunTime = 0F;
        Timer.Instance.Pause(false);


        // disable ending the game when exceeding time
        Timer.endWhenOutOfTime = !practiceMode;

        AbilityManager.Instance.ResetAbilites();
        AbilityManager.Instance.RechargeAbilities();

        Player.Instance.Movement.OnReset();
        PInput.Instance.OnReset();

        IsPracticeMode = practiceMode;
        GameplayUI.Instance.TogglePracticeMode(practiceMode);


        MusicPlayer.Instance.EnterPlay();
        GameplayUI.Instance.UpdateAbilityInfo();
        if (!practiceMode && !gameStart)
        {
            StartNewRound();
        }
        OnUpdateDataCount?.Invoke();


        OnEnterWorld?.Invoke();

    }



    void Update()
    {
        RoomManager rm = RoomManager.Instance;
        if(Input.GetKeyDown(KeyCode.R))
            rm.ReEnterRoom();
        if(enableDebugControls && Input.GetKeyDown(KeyCode.X))
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
        return Mathf.RoundToInt(Mathf.Min(RewardHardcap, reward)) + GetDataReward();
    }

    public int GetDataReward()
    {
        return Mathf.RoundToInt((GetDataCollected() - GetDataRequired()) * RewardPerExtraData);
    }
    

    public float GetNewGoal()
    {
        float newGoal = Timer.previousTargetTime * GoalTimeScale;
        return Util.RoundToNearest(newGoal, 5);
    }

    public int GetDataCollected()
    {
        return Collectibles.Count(c => c.IsCollected);
    }

    public int GetDataRequired()
    {
        return Mathf.Min(DataToWin, (CurrentRound - 1) / 2);
    }

    public int GetNextDataRequired()
    {
        return Mathf.Min(DataToWin, CurrentRound / 2);
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
        if (Application.isPlaying && GUILayout.Button("Win Current Round"))
        {
            g.FinishRound();
        }
        if (Application.isPlaying && GUILayout.Button("Get All Collectibles"))
        {
            foreach (Collectible data in g.Collectibles)
            {
                data.Collect(true);
            }
        }
        if (Application.isPlaying && GUILayout.Button("Win Game"))
        {
            g.WinGame();
        }
    }
}

#endif
