using System.Security.Cryptography;
using TMPro;
using UnityEngine;

// UI during actual gameplay
public class GameplayUI : MonoBehaviour
{
    [SerializeField] TMP_Text currTimeText, targetTimeText, speedText;

    [SerializeField] GameObject practiceModeIndicator;

    public static GameplayUI Instance;

    private void Awake()
    {
        Instance = this;
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