/// <summary>
/// CombatActionEnd.cs
/// Created by zhaozy 2014-12-2
/// 逻辑战斗action结束事件相关处理
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// 逻辑战斗action结束事件相关处理
/// </summary>
public static class CombatActionEnd
{
    /// <summary>
    /// action结束初始化
    /// </summary>
    public static void InIt()
    {
        // 注册SEQUENCE_END事件
        EventMgr.UnregisterEvent("CombatActionEnd");
        EventMgr.RegisterEvent("CombatActionEnd", EventMgrEventType.EVENT_SEQUENCE_END, ActionEnd);
    }

    /// <summary>
    /// action结束事件回调
    /// </summary>
    public static void ActionEnd(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 对象不存在不处理
        string cookie = args.GetValue<string>("cookie");
        if (string.IsNullOrEmpty(cookie))
            return;

        // 行动结束
        RoundCombatMgr.DoActionRoundEnd(cookie, args);
    }
}
