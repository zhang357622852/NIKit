/// <summary>
/// SCRIPT_8000.cs
/// Create by wangxw 2014-11-27
/// 预留脚本池
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine;

/// <summary>
/// 每状态回合作用回蓝脚本
/// </summary>
public class SCRIPT_8000 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_MP_CURE", "B_CAN_NOT_CHOOSE", };

    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;
        if (who.CheckStatus(banStatusList))
            return false;
        LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", who.QueryAttrib("sts_mp_cure"));
        sourceProfile.Add("cookie", Game.NewCookie(who.GetRid()));
        // 执行回蓝操作
        (who as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

/// <summary>
/// 每状态回合作用回血脚本
/// </summary>
public class SCRIPT_8001 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;

        // 检查禁疗
        if (who.CheckStatus(banStatusList))
            return true;

        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;

        LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

        LPCMapping cureMap = new LPCMapping();
        int hpCure = data.GetValue<int>("hp_cure");
        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, who.QueryAttrib("skill_effect"), who.QueryAttrib("reduce_cure"));
        cureMap.Add("hp", hpCure);
        sourceProfile.Add("cookie",Game.NewCookie(who.GetRid()));
        // 执行回血操作
        (who as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

/// <summary>
/// 每状态回合作用持续伤害脚本(按照最大生命百分比掉血)
/// </summary>
public class SCRIPT_8002 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;

        LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

        // 获得持续伤害量
        int maxHp = who.QueryAttrib("max_hp");
        int statusNum = who.GetStatusCondition("D_INJURY").Count;
        int hpDamage = who.QueryAttrib("sts_injury_max_hp") * maxHp / statusNum / 1000;

        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", hpDamage);
        damageMap.Add("points", points);
        damageMap.Add("damage_type", CombatConst.DAMAGE_TYPE_INJURY);

        // 直接调用接口处理伤害
        LPCMapping args = LPCMapping.Empty;
        args.Add("rid", who.GetRid());
        args.Add("damage_type", CombatConst.DAMAGE_TYPE_INJURY);
        args.Add("source_profile", sourceProfile);
        args.Add("damage_map", damageMap);
        if (sourceProfile.ContainsKey("original_cookie"))
            args.Add("original_cookie", sourceProfile.GetValue<string>("original_cookie"));
        else if (data.ContainsKey("round_cookie"))
            args.Add("original_cookie", data.GetValue<string>("round_cookie"));
        else
            args.Add("original_cookie", Game.NewCookie(who.GetRid()));

        // 直接执行args
        (who as Char).DoReceiveDamage(args);

        return true;
    }
}

/// <summary>
/// 控制状态可否生效的检查脚本
/// </summary>
public class SCRIPT_8003 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;

#if UNITY_EDITOR

        // 如果是处于攻方GM不收状态控制状态
        if (!AuthClientMgr.IsAuthClient &&
            who.CampId == CampConst.CAMP_TYPE_ATTACK &&
            ME.user.QueryTemp<int>("attack_ignore_ctrl") > 0)
            return false;

#endif

        // 转换参数
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_DEBUFF);
        if (amount >= checkArgs[0].AsInt)
            return false;

        // 检查由技能带来的免疫属性
        if (who.QueryAttrib("skl_ctrl_immune") > 0 || who.QueryAttrib("skl_ctrl_immune_sp") > 0 || who.QueryAttrib("tower_immu") > 0 || who.QueryAttrib("secret_immu") > 0)
        {
            BloodTipMgr.AddTip(TipsWndType.BuffTip, who, LocalizationMgr.Get("sp_status_1"));
            return false;
        }

        return true;
    }
}

/// <summary>
/// 重生状态作用脚本(状态清除时执行复活)
/// </summary>
public class SCRIPT_8004 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;
        int clearType = (int)_param[3];
        LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

        // 如果是排斥清除则不起效
        if (clearType == StatusConst.CLEAR_TYPE_EXCLUSION)
            return false;

        // 检查是否在死亡状态
        if (!who.CheckStatus("DIED"))
            return false;

        // 检查是否有禁止重生效果
        if (who.CheckStatus("D_NO_REBORN") && who.QueryAttrib("no_reborn_immu") == 0)
            return false;

        // 清除死亡状态
        who.ClearStatus("DIED");

        // 回血
        int cureRate = data.GetValue<int>("cure_rate");
        int hpCure = who.QueryAttrib("max_hp") * cureRate / 1000;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);
        sourceProfile.Add("cookie",Game.NewCookie(who.GetRid()));
        (who as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 播放复活光效
        who.Actor.DoActionSet("reborn", Game.NewCookie(who.GetRid()), LPCMapping.Empty);

        // 提示技能图标
        BloodTipMgr.AddSkillTip(who, data.GetValue<int>("skill_id"));

        return true;
    }
}

/// <summary>
/// 死亡状态清除脚本
/// </summary>
public class SCRIPT_8005 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        // 清除状态，显示模型
        who.Actor.SetTweenAlpha(1f);

        // 清除状态成功
        return true;
    }
}

/// <summary>
/// 陷阱状态作用脚本（对触发者，非携带者）
/// </summary>
public class SCRIPT_8006 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;

        // 执行陷阱状态
        // 初次检查成功率
        if (RandomMgr.GetRandom() >= data.GetValue<int>("debuff_rate"))
            return false;

        // 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(data.GetValue<int>("trap_source_acc"),
                       data.GetValue<int>("target_resist_rate"),
                       data.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, who, tips);
            return false;
        }

        LPCMapping condition = new LPCMapping();
        //condition.Add("round", 2);
        condition.Add("round", data.GetValue<int>("trap_effect_round"));
        condition.Add("source_profile", data.GetValue<LPCMapping>("trap_source_profile"));

        // 附加状态
        who.ApplyStatus(data.GetValue<string>("trap_effect"), condition);

        return true;
    }
}

/// <summary>
/// 挑衅apply脚本
/// </summary>
public class SCRIPT_8007 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        LPCMapping improvementMap = who.QueryTemp<LPCMapping>("improvement");
        improvementMap.Add("status_ctrl_id", 1);

        // 获得状态信息
        LPCMapping condition = _param[2] as LPCMapping;

        // 获取source_profile
        LPCMapping sourceProfile = condition.GetValue<LPCMapping>("source_profile");

        // 获取挑衅者信息
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        if (sourceOb == null)
            return false;

        // 获取角色的挑衅信息
        LPCMapping provokeMap = sourceOb.QueryTemp<LPCMapping>("provoke_map");
        if (provokeMap == null)
            provokeMap = LPCMapping.Empty;

        // 追加provokeMap信息
        provokeMap.Add(who.GetRid(), condition.GetValue<int>("cookie"));
        sourceOb.SetTemp("provoke_map", LPCValue.Create(provokeMap));

        // 返回成功
        return true;
    }
}

/// <summary>
/// 挑衅clear脚本
/// </summary>
public class SCRIPT_8008 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        Property who = _param[0] as Property;

        // 获得状态信息
        LPCMapping condition = _param[2] as LPCMapping;

        // 获取source_profile
        LPCMapping sourceProfile = condition.GetValue<LPCMapping>("source_profile");

        // 获取挑衅者信息
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        if (sourceOb == null)
            return false;

        // 获取角色的挑衅信息
        LPCMapping provokeMap = sourceOb.QueryTemp<LPCMapping>("provoke_map");
        if (provokeMap == null)
            provokeMap = LPCMapping.Empty;

        // 追加provokeMap信息
        provokeMap.Remove(who.GetRid());
        sourceOb.SetTemp("provoke_map", LPCValue.Create(provokeMap));

        // 返回成功
        return true;
    }
}

/// <summary>
/// 死亡状态作用脚本
/// </summary>
public class SCRIPT_8009 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        // 刷新玩家的光环效果
        HaloMgr.RefreshHaloAffect(who);

        // 重置LockTargetOb,如果当前LockTargetOb是who则取消
        AutoCombatMgr.UnlockCombatTarget(who);

        // 获取角色的挑衅信息
        LPCMapping provokeMap = who.QueryTemp<LPCMapping>("provoke_map");
        if (provokeMap == null)
            return true;

        // 清除数据
        who.DeleteTemp("provoke_map");

        // 清除角色的挑衅目标状态
        List<string> ridList = new List<string>();
        foreach (string rid in provokeMap.Keys)
            ridList.Add(rid);

        // 清除角色的挑衅目标状态
        for (int i = 0; i < ridList.Count; i++)
        {
            // 获取挑衅者信息
            Property sourceOb = Rid.FindObjectByRid(ridList[i]);
            if (sourceOb == null)
                continue;

            // 清除挑衅状态
            sourceOb.ClearStatusByCookie(provokeMap[ridList[i]].AsInt);
        }

        // 清除状态成功
        return true;
    }
}

/// <summary>
/// 检查减益状态数量上限脚本
/// </summary>
public class SCRIPT_8010 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_DEBUFF);
        if (amount >= checkArgs[0].AsInt  || who.QueryAttrib("secret_immu") > 0)
            return false;

        return true;
    }
}

/// <summary>
/// 检查增益状态数量上限脚本
/// </summary>
public class SCRIPT_8011 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_BUFF);
        if (amount >= checkArgs[0].AsInt)
            return false;

        return true;
    }
}

/// <summary>
/// 检查免疫攻击降低效果计算脚本（含减益数量上限检查）
/// </summary>
public class SCRIPT_8012 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_DEBUFF);
        if (amount >= checkArgs[0].AsInt)
            return false;

        // 检查由技能带来的免疫属性
        if (who.QueryAttrib("atk_down_immune") > 0 || who.QueryAttrib("secret_immu") > 0)
        {
            BloodTipMgr.AddTip(TipsWndType.BuffTip, who, LocalizationMgr.Get("sp_status_3"));

            return false;
        }

        return true;
    }
}

/// <summary>
/// 麻痹状态限制行动回合判断脚本
/// </summary>
public class SCRIPT_8013 : Script
{
    public override object Call(params object[] _param)
    {
        // who, type, args
        // who 宠物对象
        // type : 行动回合类型
        // args : status配置表limit_round_args属性

        // 获取对象
        Property who = _param[0] as Property;
        LPCMapping args = (_param[2] as LPCValue).AsMapping;

        // 返回概率true限制、false不限制
        if (RandomMgr.GetRandom() < args.GetValue<int>("rate"))
        {
            BloodTipMgr.AddTip(TipsWndType.DeBuffTip, who, LocalizationMgr.Get("sp_status_4"));
            return true;
        }

        return false;
    }
}

/// <summary>
/// 控制状态可否生效的检查脚本
/// </summary>
public class SCRIPT_8014 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;

#if UNITY_EDITOR

        // 如果是处于攻方GM不收状态控制状态
        if (!AuthClientMgr.IsAuthClient &&
            who.CampId == CampConst.CAMP_TYPE_ATTACK &&
            ME.user.QueryTemp<int>("attack_ignore_ctrl") > 0)
            return false;

#endif

        // 转换参数
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_DEBUFF);
        if (amount >= checkArgs[0].AsInt)
            return false;

        // 检查由技能带来的免疫属性
        if (who.QueryAttrib("skl_ctrl_immune_sp") > 0 || who.QueryAttrib("tower_immu") > 0 || who.QueryAttrib("secret_immu") > 0)
        {
            BloodTipMgr.AddTip(TipsWndType.BuffTip, who, LocalizationMgr.Get("sp_status_1"));
            return false;
        }

        return true;
    }
}

/// <summary>
/// 控制状态作用脚本（上控制状态标识）
/// </summary>
public class SCRIPT_8015 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;
        LPCMapping improvementMap = who.QueryTemp<LPCMapping>("improvement");
        improvementMap.Add("status_ctrl_id", 1);

        return true;
    }
}

/// <summary>
/// 状态apply时执行刷技能属性操作脚本
/// </summary>
public class SCRIPT_8016 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        SkillMgr.RefreshSkillAffect(who);

        return true;
    }
}


/// <summary>
/// 状态刷技能属性操作脚本
/// </summary>
public class SCRIPT_8017 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        SkillMgr.RefreshSkillAffect(who);

        return true;
    }
}

/// <summary>
/// 光环刷新检测脚本（判断光环源是否死亡之类）
/// </summary>
public class SCRIPT_8018 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        // 如果光环源死亡，则不再刷新光环
        if (who.CheckStatus("DIED"))
            return false;

        return true;
    }
}

/// <summary>
/// 替身光环刷新检测脚本（判断光环源是否死亡之类）(废弃)
/// </summary>
public class SCRIPT_8019 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        // 如果光环源死亡，则不再刷新光环
        if (who.CheckStatus("DIED"))
            return false;

        LPCArray propValue = (_param[2] as LPCValue).AsArray;

        // 检查光环源血量，如果低于设计值则无法起效
        int hpRate = Game.Divided(who.Query<int>("hp"), who.QueryAttrib("max_hp"));
        if (hpRate < propValue[0].AsArray[0].AsInt)
            return false;

        return true;
    }
}

/// <summary>
/// 状态刷状态属性操作脚本
/// </summary>
public class SCRIPT_8020 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        PropMgr.RefreshAffect(who, "status");

        return true;
    }
}

/// <summary>
/// 检查持续伤害状态是否免疫以及debuff上限脚本
/// </summary>
public class SCRIPT_8021 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        if (who.QueryAttrib("immu_injury") > 0 || who.QueryAttrib("tower_immu") > 0 || who.QueryAttrib("secret_immu") > 0)
            return false;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_DEBUFF);
        if (amount >= checkArgs[0].AsInt)
            return false;

        return true;
    }
}

/// <summary>
/// 检查免疫防御降低效果计算脚本（含减益数量上限检查）
/// </summary>
public class SCRIPT_8022 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_DEBUFF);
        if (amount >= checkArgs[0].AsInt)
            return false;

        // 检查由技能带来的免疫属性
        if (who.QueryAttrib("def_down_immune") > 0 || who.QueryAttrib("secret_immu") > 0)
        {
            BloodTipMgr.AddTip(TipsWndType.BuffTip, who, LocalizationMgr.Get("sp_status_5"));

            return false;
        }

        return true;
    }
}

/// <summary>
/// 控制状态可否生效的检查脚本(嘲讽)
/// </summary>
public class SCRIPT_8023 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;

        #if UNITY_EDITOR

        // 如果是处于攻方GM不收状态控制状态
        if (!AuthClientMgr.IsAuthClient &&
            who.CampId == CampConst.CAMP_TYPE_ATTACK &&
            ME.user.QueryTemp<int>("attack_ignore_ctrl") > 0)
            return false;

        #endif

        // 转换参数
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        int amount = who.GetStatusAmountByType(StatusConst.TYPE_DEBUFF);
        if (amount >= checkArgs[0].AsInt)
            return false;

        // 检查由技能带来的免疫属性
        if (who.QueryAttrib("skl_ctrl_immune_sp") > 0 || who.QueryAttrib("secret_immu") > 0)
        {
            BloodTipMgr.AddTip(TipsWndType.BuffTip, who, LocalizationMgr.Get("sp_status_1"));
            return false;
        }

        return true;
    }
}

/// <summary>
/// 控制状态回合作用提示脚本
/// </summary>
public class SCRIPT_8024 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        //LPCMapping data = _param[2] as LPCMapping;

        string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_status_ctrl"));
        BloodTipMgr.AddTip(TipsWndType.ActionTip, who, tips);

        return true;
    }
}

/// <summary>
/// 检查伤害增强状态数量上限脚本
/// </summary>
public class SCRIPT_8025 : Script
{
    public override object Call(params object[] _param)
    {
        // who, statusIndex, statusInfo.Query<LPCValue>("check_args")
        // 获取对象
        Property who = _param[0] as Property;
        LPCArray checkArgs = (_param[2] as LPCValue).AsArray;

        // 检查对应类型的状态数量，如果超过 10 个则无法上状态
        List<LPCMapping> statusData = who.GetStatusCondition("B_DMG_UP");
        if (statusData.Count >= checkArgs[0].AsInt)
            return false;

        return true;
    }
}

/// <summary>
/// 狂暴状态作用脚本(状态清除时才执行死亡)
/// </summary>
public class SCRIPT_8026 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        //LPCMapping data = _param[2] as LPCMapping;

        //LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

        // 执行死亡
        who.Set("hp", LPCValue.Create(0));
        TRY_DO_DIE.Call(who);
        who.Actor.ShowHp(false);

        return true;
    }
}

/// <summary>
/// 重生状态作用脚本(状态清除时执行复活，满血满能量)
/// </summary>
public class SCRIPT_8027 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;

        LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

        // 检查是否在死亡状态
        if (!who.CheckStatus("DIED"))
            return false;

        // 检查是否有禁止重生效果
        if (who.CheckStatus("D_NO_REBORN") && who.QueryAttrib("no_reborn_immu") == 0)
            return false;

        // 清除死亡状态
        who.ClearStatus("DIED");

        // 恢复
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", who.QueryAttrib("max_hp"));
        cureMap.Add("mp", 5);
        sourceProfile.Add("cookie",Game.NewCookie(who.GetRid()));
        (who as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 播放复活光效
        who.Actor.DoActionSet("reborn", Game.NewCookie(who.GetRid()), LPCMapping.Empty);

        // 提示技能图标
        BloodTipMgr.AddSkillTip(who, data.GetValue<int>("skill_id"));

        return true;
    }
}

/// <summary>
/// 状态清除时来源指定技能CD清除
/// </summary>
public class SCRIPT_8028 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        //Property who = _param[0] as Property;
        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;

        LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        int skillId = data.GetValue<int>("skill_id");

        int cd = CdMgr.GetSkillCdRemainRounds(sourceOb, skillId);

        CdMgr.DoReduceSkillCd(sourceOb, skillId, cd);

        return true;
    }
}

/// <summary>
/// 凤凰重生状态作用脚本(状态清除时执行复活，回能，上状态，是高级版本的复活)
/// </summary>
public class SCRIPT_8029 : Script
{
    public override object Call(params object[] _param)
    {
        //who, statusId, data
        // 获取对象
        Property who = _param[0] as Property;
        // 获得状态信息
        LPCMapping data = _param[2] as LPCMapping;
        int clearType = (int)_param[3];
        LPCMapping sourceProfile = data.GetValue<LPCMapping>("source_profile");

         // 如果是排斥清除则不起效
        if (clearType == StatusConst.CLEAR_TYPE_EXCLUSION)
            return false;

        // 检查是否在死亡状态
        if (!who.CheckStatus("DIED"))
            return false;

        // 检查是否有禁止重生效果
        if (who.CheckStatus("D_NO_REBORN") && who.QueryAttrib("no_reborn_immu") == 0)
            return false;

        // 清除死亡状态
        who.ClearStatus("DIED");

        // 回血
        int cureRate = data.GetValue<int>("cure_rate");
        int hpCure = who.QueryAttrib("max_hp") * cureRate / 1000;
        int mpCure = data.GetValue<int>("mp_cure");

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);
        cureMap.Add("mp", mpCure);
        sourceProfile.Add("cookie",Game.NewCookie(who.GetRid()));
        (who as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 播放复活光效
        who.Actor.DoActionSet("reborn", Game.NewCookie(who.GetRid()), LPCMapping.Empty);

        // 上状态
        if (!string.Equals(data.GetValue<string>("cast_status"), "no_status"))
        {
            LPCMapping castStatusData = new LPCMapping();
            castStatusData.Add("round", data.GetValue<int>("cast_status_round"));
            castStatusData.Add("source_profile", sourceProfile);

            who.ApplyStatus(data.GetValue<string>("cast_status"), castStatusData);
        }

        // 提示技能图标
        BloodTipMgr.AddSkillTip(who, data.GetValue<int>("skill_id"));

        return true;
    }
}

/// <summary>
/// 死亡状态清除后执行的脚本
/// </summary>
public class SCRIPT_8030 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;

        // 刷新角色的光环效果
        HaloMgr.RefreshHaloAffect(who);

        // 清除遗忘状态
        who.ClearStatus("D_FORGET");

        // 清除被替身状态
        who.ClearStatus("B_STANDED");

        // 如果替换主角复活需要清除占位怪物
        foreach (Property ob in who.GetOccupierList())
        {
            // 清除所有状态, 防止角色不能正常死亡
            List<LPCMapping> allStatus = ob.GetAllStatus();
            List<int> cookieList = new List<int>();
            foreach (LPCMapping condition in allStatus)
                cookieList.Add(condition.GetValue<int>("cookie"));

            // 清除全部状态
            ob.ClearStatusByCookie(cookieList);

            // 血量设置为0
            ob.Set("hp", LPCValue.Create(0));

            // 尝试执行死亡操作
            TRY_DO_DIE.Call(ob);
        }

        // 清除状态成功
        return true;
    }
}

/// <summary>
/// 遍历状态免疫时执行的脚本
/// </summary>
public class SCRIPT_8031 : Script
{
    public override object Call(params object[] _param)
    {
        // 状态信息condition,
        LPCMapping condition = _param[0] as LPCMapping;

        int immunityRate = condition.GetValue<int>("status_no_immu");

        if (RandomMgr.GetRandom() >= immunityRate)
            return false;

        // 需要被剔除
        return true;
    }
}

/// <summary>
/// 睡眠状态清除后执行的脚本
/// </summary>
public class SCRIPT_8032 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;
        //int statusId = (int)_param[1];

        // 收集一下敌方目标
        int campId = (who.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

        if (finalList.Count == 0)
            return true;

        // 检查有没有法老王沉睡属性
        LPCMapping improvementMap = who.QueryTemp<LPCMapping>("improvement");
        if (improvementMap.ContainsKey("pharaoh_sleep"))
        {
            LPCArray pharaohAttrib = improvementMap.GetValue<LPCArray>("pharaoh_sleep");
            int clearType = (int)_param[3];

            // 转换数据格式
            LPCArray propValue = pharaohAttrib[0].AsArray;

            // 如果是打断状态，则需要释放技能
            if (clearType == StatusConst.CLEAR_TYPE_BREAK)
            {
                // 释放技能
                LPCMapping condition = _param[2] as LPCMapping;
                LPCMapping args = LPCMapping.Empty;
                args.Add("original_cookie", condition.GetValue<string>("round_cookie"));
                CombatMgr.DoCastSkill(who, finalList[0], propValue[0].AsInt, args);
            }
            else
            {
                // 如果是自然结束，则清除一部分CD
                int pharaohSkill = propValue[1].AsInt;
                int reduceCd = propValue[2].AsInt;
                CdMgr.DoReduceSkillCd(who, pharaohSkill, reduceCd);
            }
        }

        // 清除状态成功
        return true;
    }
}