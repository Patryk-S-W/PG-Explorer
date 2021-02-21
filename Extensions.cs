
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
public static class Extensions
{
    public static int ToInt(this string value, int errVal=0)
    {
        Int32.TryParse(value, out errVal);
        return errVal;
    }
    public static void SetListener(this UnityEvent uEvent, UnityAction call)
    {
        uEvent.RemoveAllListeners();
        uEvent.AddListener(call);
    }
    public static void SetListener<T>(this UnityEvent<T> uEvent, UnityAction<T> call)
    {
        uEvent.RemoveAllListeners();
        uEvent.AddListener(call);
    }
    public static bool HasDuplicates<T>(this List<T> list)
    {
        return list.Count != list.Distinct().Count();
    }
    public static List<U> FindDuplicates<T, U>(this List<T> list, Func<T, U> keySelector)
    {
        return list.GroupBy(keySelector)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key).ToList();
    }
    public static int GetStableHashCode(this string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
                hash = hash * 31 + c;
            return hash;
        }
    }
    public static void ResetMovement(this NavMeshAgent agent)
    {
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
}
