using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;
using LPC;

/// <summary>
/// 服务器时间
/// </summary>
public class MsgNotifyServerTime : MsgHandler
{
    public string GetName()
    {
        return "MSG_NOTIFY_SERVER_TIME";
    }

    public void Go(LPCValue para)
    {
    }
}