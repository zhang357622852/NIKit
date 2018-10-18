/// <summary>
/// CrossMap.cs
/// Create by zhaozy 2014-11-18
/// 玩家过图策略
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class CrossMap : Tactics
{
    #region 对外接口

    /// <summary>
    /// 策略执行入口
    /// </summary>
    public override bool Trigger(params object[] _params)
    {
        // 获取玩家对象
        Property entityOb = _params[0] as Property;

        // 没有actor对象不能释放技能
        if (entityOb.Actor == null)
            return false;

        // 构建参数
        LPCMapping para = new LPCMapping();
        para.Add("target_pos", CALC_CROSS_MAP_TATGET_POSITION.Call(entityOb));

        // 取消之前选中状态
        entityOb.Actor.CancelActionSetByName("select");

        // 通知在战斗系统过图
        entityOb.Actor.DoActionSet("crossmap", Game.NewCookie(entityOb.GetRid()), para);

        // 返回触发成功
        return true;
    }

    #endregion
}
