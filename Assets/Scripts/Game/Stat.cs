using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStatSource
{
}

[Serializable]
public class Stat
{
    [HideInInspector]
    public float BaseValue;
    public Dictionary<IStatSource, float> Multipliers;
    public Dictionary<IStatSource, float> Offsets;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
        Multipliers = new Dictionary<IStatSource, float>();
        Offsets = new Dictionary<IStatSource, float>();
    }

    public float Get()
    {
        float totalOffset = 0f;
        float totalMult = 1;
        foreach (var kvp in Multipliers)
        {   
            totalMult *= kvp.Value;
        }
        foreach (var kvp in Offsets)
        {
            totalOffset += kvp.Value;
        }
        return BaseValue * totalMult + totalOffset;
    }

    public void Reset()
    {
        Multipliers.Clear();
        Offsets.Clear();
    }
}

[Serializable]
public class VecStat
{
    [HideInInspector]
    public Vector2 BaseValue;
    public Dictionary<IStatSource, float> Multipliers;
    public Dictionary<IStatSource, Vector2> Offsets;

    public VecStat(Vector2 baseValue)
    {
        BaseValue = baseValue;
        Multipliers = new Dictionary<IStatSource, float>();
        Offsets = new Dictionary<IStatSource, Vector2>();
    }

    public Vector2 Get()
    {
        Vector2 totalOffset = Vector2.zero;
        float totalMult = 1;
        foreach (var kvp in Multipliers)
        {
            totalMult *= kvp.Value;
        }
        foreach (var kvp in Offsets)
        {
            totalOffset += kvp.Value;
        }
        return BaseValue * totalMult + totalOffset;
    }
    public void Reset()
    {
        Multipliers.Clear();
        Offsets.Clear();
    }
}