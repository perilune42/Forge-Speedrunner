using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// UI during actual gameplay
public class GameplayUI : Singleton<GameplayUI>
{
    [SerializeField] TMP_Text currTimeText, targetTimeText, speedText;

    [SerializeField] GameObject practiceModeIndicator;

    public GameObject GameEndUI;

    public override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        currTimeText.text = Util.SecondsToTime(Timer.speedrunTime);
        targetTimeText.text = Util.SecondsToTime(Timer.targetSpeedrunTime);
    }

    // Pause and Play methods for Pause Button UI element
    public void Pause()
    {
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
}