/// <summary>
/// CombatReceiveDamage.cs
/// Created by zhaozy 2014-12-2
/// 逻辑战斗受创事件相关处理
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// 逻辑战斗受创事件相关处理
/// </summary>
public static class CombatReceiveDamage
{
    /// <summary>
    /// move_speed速度变化
    /// </summary>
    private static void DoShakeEnd(object param, params object[] paramEx)
    {
    }

    /// <summary>
    /// 受创初始化
    /// </summary>
    public static void InIt()
    {
        EventMgr.UnregisterEvent("CombatReceiveDamage");

        // 注册受创结束事件
        EventMgr.RegisterEvent("CombatReceiveDamage", EventMgrEventType.EVENT_RECEIVE_DAMAGE, ReceiveDamage);

        // 注册受创结束事件
        EventMgr.RegisterEvent("CombatReceiveDamage", EventMgrEventType.EVENT_DAMAGE, DoDamage);
    }

    /// <summary>
    /// 受创回调
    /// </summary>
    public static void DoDamage(int eventId, MixedValue para)
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
        (target as Char).DoReceiveDamage(args);
    }

    /// <summary>
    /// 受创结束事件回调
    /// </summary>
    public static void ReceiveDamage(int eventId, MixedValue para)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 没有受创目标rid
        if (!args.ContainsKey("rid") || !args["rid"].IsString)
            return;

        // 查找对象
        string rid = args["rid"].AsString;
        Property target = Rid.FindObjectByRid(rid);

        // 受创者不存在会在actor对象不存在不处理
        if (target == null || target.Actor == null)
            return;

        // 获取damage_type
        int damageType = args.GetValue<int>("damage_type");
        if (!CHECK_SHOW_DAMAGE_TIPS.Call(target, damageType))
            return;

        // 如果是暴击伤害需要抖动相机
        if ((damageType & CombatConst.DAMAGE_TYPE_DEADLY) == CombatConst.DAMAGE_TYPE_DEADLY)
        {
            // 获取轨迹文件
            TrackInfo mTrack = TrackMgr.GetTrack("HRHshake");

            // 没有配置的轨迹文件
            if (mTrack != null)
                CameraShakeMgr.DoShake(SceneMgr.SceneCamera,
                    mTrack,
                    0.3f,
                    Game.NewCookie("shake"),
                    new CallBack(DoShakeEnd, null));
        }

        // 增加飘血框
        LPCMapping damageMap = args.GetValue<LPCMapping>("damage_map");
        if (damageMap == null)
            return;

        // 获取受创伤害数据
        LPCMapping damagePoints = damageMap.GetValue<LPCMapping>("points");
        if (damagePoints == null)
            return;

        // 显示飘血效果
        BloodTipMgr.AddDamageOrCureTip(target, damageType, damagePoints);
    }
}
