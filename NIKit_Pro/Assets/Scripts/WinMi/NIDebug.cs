using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自定义Debug类
/// </summary>
public static class NIDebug
{
    private static bool isDebugLog = true;

    public static void Log<T>(T message)
    {
        if (isDebugLog)
            Debug.Log(message);
    }

    public static void LogError<T>(T message)
    {
        if (isDebugLog)
            Debug.LogError(message);
    }

    public static void LogWarning<T>(T message)
    {
        if (isDebugLog)
            Debug.LogWarning(message);
    }
}
