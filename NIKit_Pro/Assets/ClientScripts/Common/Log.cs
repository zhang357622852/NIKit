/// <summary>
/// Log类的接口封装
/// </summary>
using System;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LPC;

public class Log : MonoBehaviour
{
    // 日志信息
    private List<string> mUploadLogList = new List<string>();
    private float mLastUpdate = 0f;

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 更新时间还不到
        if (Time.unscaledTime - mLastUpdate < 5f)
            return;

        // 记录上一次更新时间
        mLastUpdate = Time.unscaledTime;

        // 如果玩家没有链接游戏，或者没有log信息需要上传
        if (! Communicate.IsConnectedGS() ||
            mUploadLogList.Count == 0)
            return;

        List<string> tempList = new List<string>(mUploadLogList);
        mUploadLogList.Clear();
        foreach (string l in tempList)
            Operation.CmdErrorLog.Go(l);
    }

    /// <summary>
    /// Handles the log.
    /// </summary>
    /// <param name="logString">Log string.</param>
    /// <param name="stackTrace">Stack trace.</param>
    /// <param name="type">Type.</param>
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 这个地方只是处理Exception和Error
        if (type != LogType.Exception && type != LogType.Error)
            return;

        // 拼接log信息
        logString = logString + "\n" + stackTrace;

        // 如果已经包含了该信息
        if (mUploadLogList.Contains(logString))
            return;

        // 添加消息到更新列表中
        mUploadLogList.Add(logString);
    }
}
