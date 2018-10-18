/// <summary>
/// CombatDie.cs
/// Created by zhaozy 2014-12-2
/// 逻辑战斗死亡事件相关处理
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// 逻辑战斗死亡事件相关处理
/// </summary>
public static class CombatDie
{
    #region 公共接口

    /// <summary>
    /// CombatDie初始化
    /// </summary>
    public static void InIt()
    {
        EventMgr.UnregisterEvent("CombatDie");

        // 注册死亡事件
        EventMgr.RegisterEvent("CombatDie", EventMgrEventType.EVENT_DIE, WhenDie);

        // 注册死亡事件
        EventMgr.RegisterEvent("CombatDie", EventMgrEventType.EVENT_DIE_ACTION_END, WhenDieActionEnd);
    }

    /// <summary>
    /// Die事件回调
    /// </summary>
    private static void WhenDie(int eventId, MixedValue para)
    {
        // 对象不存在不处理
        Property target = para.GetValue<Property>();
        if (target == null)
            return;

        // 如果是验证客户端不需要掉落
        if (! AuthClientMgr.IsAuthClient)
            BonusMgr.DoCombatBonus(target, new LPCMapping());

        // 删除玩家当前的cur_rounds
        target.DeleteTemp("cur_rounds");
    }

    /// <summary>
    /// Whens the die action end.
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private static void WhenDieActionEnd(int eventId, MixedValue para)
    {
        // 获取参数获取targetOb
        LPCMapping args = para.GetValue<LPCMapping>();
        if (args == null)
            return;

        // 对象不存在不处理
        Property target = Rid.FindObjectByRid(args.GetValue<string>("rid"));
        if (target == null)
            return;

        // 将角色设置会原始位置
        if (target.Actor != null)
            target.Actor.SetPosition(target.MoveBackPos);

        // 抛出死亡结束事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_DIE_END, MixedValue.NewMixedValue<Property>(target), true, true);
    }

    #endregion
}
