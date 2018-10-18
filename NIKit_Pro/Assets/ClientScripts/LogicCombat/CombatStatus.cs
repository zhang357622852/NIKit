/// <summary>
/// CombatStatus.cs
/// Copy from zhangyg 2014-10-22
/// 逻辑战斗状态事件管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// 逻辑战斗status事件
/// </summary>
public static class CombatStatus
{
    #region 内部接口

    /// <summary>
    /// Combat出发status节点事件回调
    /// </summary>
    private static void DoCombatStatus(int eventId, MixedValue para)
    {
        LPCMapping args = para.GetValue<LPCMapping>();

        // 事件参数必须包含
        // 1. rid : 目标rid
        if (!args.ContainsKey("rid"))
            return;

        // 获取信息
        string rid = args.GetValue<string>("rid");
        Property sourceOb = Rid.FindObjectByRid(rid);
        if (sourceOb == null)
            return;

        // 附加状态
        string status = args.GetValue<string>("status");
        StatusMgr.ApplyStatus(sourceOb,
            status,
            args.GetValue<LPCMapping>("condition"));
    }

    /// <summary>
    /// Combat出发status节点事件回调
    /// </summary>
    private static void DoApplyStatus(int eventId, MixedValue para)
    {
        LPCMapping args = para.GetValue<LPCMapping>();

        // 事件参数必须包含
        // 1. rid : 目标rid
        if (!args.ContainsKey("rid"))
            return;

        // 没有状态id不处理
        if (!args.ContainsKey("status") || !args["status"].IsInt)
            return;

        // 获取状态id
        int status = args["status"].AsInt;

        // 取得状态的别名
        string alias = StatusMgr.GetStatusAlias(status);
        if (string.IsNullOrEmpty(alias))
            return;

        // 获取信息
        string rid = args.GetValue<string>("rid");
        Property sourceOb = Rid.FindObjectByRid(rid);

        // sourceOb对象不存在不处理
        if (sourceOb == null || sourceOb.Actor == null)
            return;

        // 加提示信息
        BloodTipMgr.AddStatusTip(sourceOb, alias);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 逻辑战斗状态事件管理初始化
    /// </summary>
    public static void InIt()
    {
        // 注册战斗系统抛出状态事件
        EventMgr.UnregisterEvent("CombatStatus");
        EventMgr.RegisterEvent("CombatStatus", EventMgrEventType.EVENT_STATUS, DoCombatStatus);

        // 注册附加状态
        EventMgr.UnregisterEvent("ApplyStatus");
        EventMgr.RegisterEvent("ApplyStatus", EventMgrEventType.EVENT_APPLY_STATUS, DoApplyStatus);
    }

    #endregion
}
