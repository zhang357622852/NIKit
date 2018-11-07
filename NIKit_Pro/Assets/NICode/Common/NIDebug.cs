/// <summary>
/// NIDebug.cs
/// Created by WinMi 2018/10/23
/// 调试打印类
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class NIDebug
{
    /// <summary>
    /// Debug 开关
    /// </summary>
    private static bool isDebug = true;

    public static bool IsDebug
    {
        get
        {
            return IsDebug;
        }

        set
        {
            IsDebug = value;
        }
    }

    /// <summary>
    /// Logs message to the Unity Console.
    /// </summary>
    public static void Log<T>(T message)
    {
        if (isDebug)
            Debug.Log(message);
    }

    /// <summary>
    /// Logs message to the Unity Console.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="paras"></param>
    public static void Log(string message, params object[] paras)
    {
        string content = message;

        if (paras != null)
            content = string.Format(message, paras);

        Debug.Log(content);
    }

    /// <summary>
    /// A variant of Debug.Log that logs an error message to the console.
    /// </summary>
    public static void LogError<T>(T message)
    {
        if (isDebug)
            Debug.LogError(message);
    }

    /// <summary>
    /// Error the specified message and paras.
    /// </summary>
    public static void LogError(string message, params object[] paras)
    {
        string content = message;

        if (paras != null)
            content = string.Format(message, paras);

        Debug.LogError(content);
    }

    /// <summary>
    /// A variant of Debug.Log that logs a warning message to the console.
    /// </summary>
    public static void LogWarning<T>(T message)
    {
        if (isDebug)
            Debug.LogWarning(message);
    }

    /// <summary>
    /// A variant of Debug.Log that logs a warning message to the console.
    /// </summary>
    public static void LogWarning(string message, params object[] paras)
    {
        string content = message;

        if (paras != null)
            content = string.Format(message, paras);

        Debug.LogWarning(content);
    }

    /// <summary>
    /// A variant of Debug.Log that logs an error message to the console.
    /// </summary>
    public static void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }
}
