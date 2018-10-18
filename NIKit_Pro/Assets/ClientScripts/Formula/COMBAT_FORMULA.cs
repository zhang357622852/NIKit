/// <summary>
/// COMBAT_FORMULA.cs
/// Create by wangxw 2014-12-9
/// 战斗效果相关公式
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// <summary>
/// 获取对象移动速度
/// </summary>
public class CALC_MOVE_SPEED : Formula
{
    public static float Call(Property ob)
    {
        return ob.Query("move_speed").AsFloat;
    }
}

/// <summary>
/// 获取对象的移动速度百分比
/// </summary>
public class CALC_MOVE_SPEED_RATE_PERCENT : Formula
{
    public static float Call(Property ob)
    {
        return 0.001f * ob.QueryAttrib("move_speed_rate");
    }
}

/// <summary>
/// 获取对象攻击速度
/// </summary>
public class CALC_ATTACK_SPEED_RATE_PERCENT : Formula
{
    public static float Call(Property ob)
    {
        return 0.001f * ob.QueryAttrib("attack_speed");
    }
}

/// 计算角色的过图目标位置
/// </summary>
public class CALC_CROSS_MAP_TATGET_POSITION : Formula
{
    public static LPCArray Call(Property ob)
    {
        // 在角色当前的位置基础上前进10个unity单位
        Vector3 pos = ob.GetPosition();
        return new LPCArray(pos.x + 10f, pos.y, pos.z);
    }
}

/// <summary>
/// 是否需要播放受创的Act效果
/// </summary>
public class NEED_DAMAGE_ACT : Formula
{
    private static List<string> statusList = new List<string>(){ "B_EQPT_SHIELD", "B_HP_SHIELD", "B_DMG_IMMUNE", "B_NO_DIE" };

    public static bool Call(Property who, LPCMapping args)
    {
        // 检查是否带盾，如果带盾，不播放受创动作
        if (who.CheckStatus(statusList))
            return false;

        return true;
    }
}

/// <summary>
/// 是否需要播放受创的Act效果
/// </summary>
public class NEED_CURE_ACT : Formula
{
    public static bool Call(Property who, LPCMapping args)
    {
        return true;
    }
}

/// <summary>
/// ACTION HIT结束
/// </summary>
public class ACTION_HIT_END : Formula
{
    public static void Call(int hitIndex, string cookie, LPCMapping actionArgs)
    {
        // 获取hit目标
        LPCMapping hitEntityMap = actionArgs.GetValue<LPCMapping>("hit_entity_map");
        if (hitEntityMap == null || !hitEntityMap.ContainsKey(hitIndex))
            return;

        // 没有收集到任何目标
        LPCArray selectList = hitEntityMap[hitIndex].AsArray;
        if (selectList.Count == 0)
            return;

        // 逐个目标解锁
        for (int i = 0; i < selectList.Count; i++)
        {
            // 查找目标对象
            Property ob = Rid.FindObjectByRid(selectList[i].AsString);

            // 目标对象不存在
            if (ob == null)
                continue;

            // 解锁目标
            ob.UnLock(cookie);
        }
    }
}

/// <summary>
/// 战斗序列结束
/// </summary>
public class ACTION_SEQUENCE_END : Formula
{
    public static void Call(string cookie, LPCMapping actionArgs)
    {
        // 获取hit目标
        LPCMapping hitEntityMap = actionArgs.GetValue<LPCMapping>("hit_entity_map");
        if (hitEntityMap == null || hitEntityMap.Count == 0)
            return;

        // 逐个目标解锁
        foreach (int hitIndex in hitEntityMap.Keys)
        {
            // 没有收集到任何目标
            LPCArray selectList = hitEntityMap[hitIndex].AsArray;
            if (selectList.Count == 0)
                continue;

            // 遍历该selectList
            for (int i = 0; i < selectList.Count; i++)
            {
                // 查找目标对象
                Property ob = Rid.FindObjectByRid(selectList[i].AsString);

                // 目标对象不存在
                if (ob == null)
                    continue;

                // 解锁目标
                ob.UnLock(cookie, true);
            }
        }
    }
}

/// <summary>
/// 判断角色是否需要重生
/// </summary>
public class CHECK_NEED_REBORN : Formula
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY",};

    public static bool Call(Property ob)
    {
        if (ob.CheckStatus(statusList))
            return true;

        return false;
    }
}
