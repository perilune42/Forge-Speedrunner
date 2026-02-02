using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatSource
{
    JumpGravityMult, ClimbGravityMult, GrappleGravityMult
}

[Serializable]
public class Stat
{
    [HideInInspector]
    public float BaseValue;
    public Dictionary<StatSource, float> Multipliers;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
        Multipliers = new Dictionary<StatSource, float>();
    }

    public float Get()
    {
        float totalMult = 1;
        foreach (var kvp in Multipliers)
        {   
            totalMult *= kvp.Value;
        }
        return BaseValue * totalMult;
    }
}