using UnityEngine;
using TMPro;
using System;

public class SpeedRunTimerUI : MonoBehaviour
{
    private TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        
    }


    void Update()
    {
        text.text = secondsToTime(Timer.speedrunTime); 
    }

    private String secondsToTime(float time)
    {
        String toReturn = "";
        int seconds = (int)(time % 60);
        int minutes = (int)(time/60);

        if (minutes < 10)
        {
            toReturn += "0";
            toReturn += minutes;
        }
        else
        {
            toReturn += minutes;
        }

        toReturn += ":";

        if (seconds < 10)
        {
            toReturn += "0";
            toReturn += seconds;
        }
        else
        {
            toReturn += seconds;
        }

        return toReturn;
    }
}
