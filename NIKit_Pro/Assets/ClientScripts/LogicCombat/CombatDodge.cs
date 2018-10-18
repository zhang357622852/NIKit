/// <summary>
/// CombatDodge.cs
/// Created by zhaozy 2014-12-2
/// 逻辑战斗闪避事件相关处理
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// 逻辑战斗闪避事件相关处理
/// </summary>
public static class CombatDodge
{
    /// <summary>
    /// 闪避初始化
    /// </summary>
    public static void InIt()
    {
        EventMgr.UnregisterEvent("CombatDodge");
        
        // 注册闪避事件
        EventMgr.RegisterEvent("CombatDodge", EventMgrEventType.EVENT_DODGE, Dodge);
    }

    /// <summary>
    /// 闪避事件回调
    /// </summary>
    public static void Dodge(int eventId, MixedValue para)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 没有目标rid
        if (! args.ContainsKey("target_rid") || ! args ["target_rid"].IsString)
            return;

        // 获取target_rid
        string targetRid = args ["target_rid"].AsString;

        // 查找对象
        Property target = Rid.FindObjectByRid(targetRid);

        // 受创者不存在会在actor对象不存在不处理
        if (target == null || target.Actor == null)
            return;

        // 播放闪避效果
        BloodTipMgr.AddDamageOrCureTip(target, CombatConst.DAMAGE_TYPE_DODGE, new LPCMapping());
    }
}
