
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public static class ExtensionMethods
{
    public static E GetNextWraparound<E>(this List<E> list, E item)
    {
        return list[(list.IndexOf(item) + 1) % list.Count];
    }

    public static E GetNext<E>(this List<E> list, E item)
    {
        return list[list.IndexOf(item) + 1];
    }

    public static E GetRandom<E>(this List<E> list)
    {
        if (list.Count == 0) return default;
        int randIndex = Random.Range(0, list.Count);
        return list[randIndex];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static List<T> Shuffled<T>(this IList<T> list)
    {
        var newList = new List<T>(list);
        var rng = new System.Random();
        int n = newList.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = newList[k];
            newList[k] = newList[n];
            newList[n] = value;
        }
        return newList;
    }
}