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
    public UnityEngine.UI.Button MainMenuButton, PlayAgainButton;

    public override void Awake()
    {
        base.Awake();
        MainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
        PlayAgainButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("World"); // TODO: change this if we ever stop using world scene
        });
    }

    private void Update()
    {
        currTimeText.text = Util.SecondsToTime(Timer.speedrunTime);
        targetTimeText.text = Util.SecondsToTime(Timer.targetSpeedrunTime);
        speedText.text = Player.Instance.Movement.Velocity.ToString();
    }

    // Pause and Play methods for Pause Button UI element
    public void Pause()
    {
        if (Game.Instance != null)
        {
            Timer.Instance.Pause(true);
        }
    }

    public void Play()
    {
        if (Game.Instance != null)
        {
            Timer.Instance.Pause(false);
        }
    }

    public void OpenShop()
    {
        Game.Instance.GoToShop(true);
    }

    public void TogglePracticeMode(bool toggle)
    {
        practiceModeIndicator.SetActive(toggle);
    }
}