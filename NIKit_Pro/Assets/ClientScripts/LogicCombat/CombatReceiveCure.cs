/// <summary>
/// CombatReceiveCure.cs
/// Created by zhaozy 2014-12-2
/// 逻辑战斗受创事件相关处理
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// 逻辑战斗治疗事件相关处理
/// </summary>
public static class CombatReceiveCure
{
    #region 内部接口

    /// <summary>
    /// 治疗事件回调
    /// </summary>
    private static void ReceiveCure(int eventId, MixedValue para)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 没有受创目标rid
        if (!args.ContainsKey("rid") || !args["rid"].IsString)
            return;

        // 获取rid, 查找对象
        string rid = args["rid"].AsString;
        Property target = Rid.FindObjectByRid(rid);

        // 受创者不存在会在actor对象不存在不处理
        if (target == null || target.Actor == null)
            return;

        // 获取治愈类型
        int cureType = args.GetValue<int>("cure_type");
        if (!CHECK_SHOW_CURE_TIPS.Call(target, cureType))
            return;

        LPCMapping cureMap = args["cure_map"].AsMapping;
        if (cureMap == null || (!cureMap.ContainsKey("hp") && !cureMap.ContainsKey("mp")))
        {
            LogMgr.Trace("没有治愈参数，显示文字提示失败");
            return;
        }

        // 添加飘血框
        if ((cureType & CombatConst.CURE_TYPE_ROUND_MAGIC) != CombatConst.CURE_TYPE_ROUND_MAGIC)
            BloodTipMgr.AddDamageOrCureTip(target, CombatConst.CURE_TYPE_MAGIC, cureMap);
    }

    /// <summary>
    /// 受创治疗
    /// </summary>
    public static void DoCure(int eventId, MixedValue para)
    {
        LPCMapping args = para.GetValue<LPCMapping>();

        // 没有受创目标rid
        if (!args.ContainsKey("rid") || !args["rid"].IsString)
            return;

        // 查找对象
        string rid = args["rid"].AsString;
        Property target = Rid.FindObjectByRid(rid);

        // 受创者不存在或者不是Char
        if (target == null || !(target is Char))
            return;

        // 执行真正的受创
        (target as Char).DoReceiveCure(args);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 治疗初始化
    /// </summary>
    public static void InIt()
    {
        EventMgr.UnregisterEvent("ReceiveCure");

        // 注册治愈结束事件
        EventMgr.RegisterEvent("ReceiveCure", EventMgrEventType.EVENT_RECEIVE_CURE, ReceiveCure);

        // 注册受创结束事件
        EventMgr.RegisterEvent("ReceiveCure", EventMgrEventType.EVENT_CURE, DoCure);
    }

    #endregion
}
