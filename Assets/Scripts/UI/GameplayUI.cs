using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// UI during actual gameplay
public class GameplayUI : Singleton<GameplayUI>
{
    [SerializeField] TMP_Text currTimeText, targetTimeText, speedText, currRoundText;
    [SerializeField] TMP_Text dataText;


    [SerializeField] GameObject practiceModeIndicator;

    [SerializeField] Transform abilityInfoParent, chronoshiftInfoParent;
    [SerializeField] AbilityInfo abilityInfoPrefab;

    public GameObject GameEndUI, GameWinUI;
    [SerializeField] TMP_Text totalTimeText;

    public override void Awake()
    {
        base.Awake();
        Game.Instance.OnUpdateDataCount += UpdateDataCollected;
    }

    private void Update()
    {
        currTimeText.text = Util.GetTimeString(Timer.speedrunTime);
        targetTimeText.text = Util.GetTimeString(Timer.targetSpeedrunTime);
        currRoundText.text = Game.Instance.CurrentRound.ToString();
    }

    public void UpdateAbilityInfo()
    {
        Util.DestoryAllChildren(abilityInfoParent);
        // change to use slot system later
        foreach (Ability ability in AbilityManager.Instance.GetAllAbilities())
        {
            AbilityInfo abilityInfo = Instantiate(abilityInfoPrefab, abilityInfoParent);
            abilityInfo.SetAbility(ability);
        }

        Util.DestoryAllChildren(chronoshiftInfoParent);

        AbilityInfo chronoshiftInfo = Instantiate(abilityInfoPrefab, chronoshiftInfoParent);
        chronoshiftInfo.SetAbility(AbilityManager.Instance.chronoshift);
        chronoshiftInfo.gameObject.SetActive(AbilityManager.Instance.ChronoshiftCharges > 0);
    }

    public void UpdateDataCollected()
    {
        int count = Game.Instance.GetDataCollected();
        int min = Game.Instance.GetDataRequired();
        if (min == 0)
        {
            dataText.text = $"{count}";
        }
        else if (count < min)
        {
            dataText.text = $"<color=red>{count}</color>/{min}";
        }
        else
        {
            dataText.text = $"{count}/{min}";
        }
    }


    // Pause and Play methods for Pause Button UI element
    public void Pause()
    {
        Debug.Log("paused game");
        Timer.Instance.Pause(true);
    }

    public void Play()
    {
        Timer.Instance.Pause(false);
    }

    public void OpenShop()
    {
        Game.Instance.GoToShop(true);
    }

    public void TogglePracticeMode(bool toggle)
    {
        practiceModeIndicator.SetActive(toggle);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetRoom()
    {
        Debug.Log("reset room");
        Play();
        RoomManager.Instance.ReEnterRoom();
    }

    public void ShowGameWin()
    {
        GameWinUI.SetActive(true);
        totalTimeText.text = $"Total Time: {Util.GetTimeString(Game.Instance.TotalTime)}";
    }
}