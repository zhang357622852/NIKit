/// <summary>
/// SCRIPT_9100.cs
/// Create by wangxw 2015-01-08
/// 战斗表现脚本：命中处理
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine;

// 通用伤害计算脚本(纯攻击力计算)
public class SCRIPT_9100 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        int targetHp = targetOb.Query<int>("hp");
        sourceProfile.Add("hp_value_compare", targetHp - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(根据攻击力、自身损失生命值来计算伤害)
public class SCRIPT_9101 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("lost_hp_damage_co", skillAttrib[1].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 自身已损失生命值
        int lostHp = sourceProfile.GetValue<int>("max_hp") - sourceOb.Query<int>("hp");
        sourceProfile.Add("lost_hp", lostHp);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_LOST_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 记录攻击对象到临时列表里
        sourceOb.SetTemp("last_hit_rid",LPCValue.Create(targetOb.GetRid()));

        return damageMap;
    }
}

// 扣蓝伤害计算脚本
public class SCRIPT_9102 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;

        // 计算扣蓝伤害
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCMapping mpAtkData = new LPCMapping();
        mpAtkData.Add("mp_dmg_rate", hitScriptArg[0].AsInt + skillEffect);
        mpAtkData.Add("mp_dmg", hitScriptArg[1].AsInt);
        mpAtkData.Add("accuracy_rate", sourceOb.QueryAttrib("accuracy_rate"));
        mpAtkData.Add("resist_rate", targetOb.QueryAttrib("resist_rate"));
        mpAtkData.Add("restrain", restrain);
        int mpDmg = CALC_MP_DAMAGE.Call(mpAtkData, targetOb);
        damageType = mpDmg > 0 ? damageType | CombatConst.DAMAGE_TYPE_MP : damageType;
        mpAtkData.Add("damage_type", damageType);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        // 扣除最大不会超过当前拥有蓝量
        points.Add("mp", Math.Min(mpDmg, targetOb.Query<int>("mp")));
        damageMap.Add("points", points);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用前置信息计算脚本无实际受创（暂时废弃）
public class SCRIPT_9103 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);

        return damageMap;
    }
}

// 后置伤害计算脚本，配合前置信息计算脚本处理实际受创(暂时废弃)
public class SCRIPT_9104 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);

        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetOb.GetRid());

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ridArgs.GetValue<int>("restrain");

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = ridArgs.GetValue<int>("damage_type");

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害)
public class SCRIPT_9105 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt);
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(根据攻击力、概率无视防御)
public class SCRIPT_9106 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 获取技能初始攻击力加成效果
        int atkRate = hitArgs[1].AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算是否无视防御
        int rateUp = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        sourceProfile.Add("rate_ignore_def", hitArgs[0].AsInt + rateUp);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(根据敏捷计算)
public class SCRIPT_9107 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int agiCo = hitScriptArg[0].AsInt;
        // 获取速度的攻击力加成
        int agiCoRate = hitScriptArg[1].AsInt;
        sourceProfile.Add("skill_id", skillId);

        // 计算攻击力加成
        int atkRate = (sourceOb.QueryAttrib("agility") + agiCo) * agiCoRate;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(根据敌方最大生命)
public class SCRIPT_9108 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("target_max_hp_co", skillAttrib[1].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_TARGET_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());
        damageMap.Add("origin_atk_rate", sourceProfile.GetValue<int>("origin_atk_rate"));

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 偷蓝专用计算脚本
public class SCRIPT_9109 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));

        // 计算伤害类型（暴击、格挡、强击）
        //int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算扣蓝伤害
        LPCMapping mpAtkData = new LPCMapping();
        mpAtkData.Add("mp_dmg_rate", hitScriptArg[0].AsInt);
        mpAtkData.Add("mp_dmg", hitScriptArg[1].AsInt);
        mpAtkData.Add("accuracy_rate", sourceOb.QueryAttrib("accuracy_rate"));
        mpAtkData.Add("resist_rate", targetOb.QueryAttrib("resist_rate"));
        mpAtkData.Add("restrain", restrain);
        int mpDmg = CALC_MP_DAMAGE.Call(mpAtkData,targetOb);
        if (mpDmg == 0)
            return LPCMapping.Empty;

        //damageType = mpDmg > 0 ? damageType | CombatConst.DAMAGE_TYPE_MP : damageType;
        mpAtkData.Add("damage_type", CombatConst.DAMAGE_TYPE_MP);
        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("mp", mpDmg);
        damageMap.Add("points", points);
        damageMap.Add("target_rid", targetOb.GetRid());

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, CombatConst.DAMAGE_TYPE_MP, damageMap);

        // 通知自身回蓝
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpDmg);
        (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(根据目标剩余能量计算,剩余能量多少影响是否暴击)
public class SCRIPT_9110 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取敌方剩余能量计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("target_remain_mp_co", skillAttrib[1].AsInt);

        // 根据剩余能量值设置暴击标识
        if (targetOb.Query<int>("mp") > skillAttrib[2].AsInt)
            sourceProfile.Add("extra_crt_id", 1);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 获取目标敌方残留mp
        int targetRemainMp = targetOb.Query<int>("mp");
        sourceProfile.Add("target_remain_mp", targetRemainMp);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_TARGET_REMAIN_MP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算、异常状态暴击)
public class SCRIPT_9111 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 检查目标异常状态，设置暴击标识
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        CsvRow statusInfo;
        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = statusInfo.Query<int>("status_type");
            if (buff_type == hitScriptArg[1].AsInt)
                sourceProfile.Add("extra_crt_id", 1);
            else
                continue;
        }

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御计算)
public class SCRIPT_9112 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御附加真实伤害)
public class SCRIPT_9113 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_DEFNESE_REAL.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(根据攻击力、概率穿刺伤害、无视防御)
public class SCRIPT_9114 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 获取技能初始攻击力加成效果
        int atkRate = hitArgs[1].AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算是否穿刺
        if (RandomMgr.GetRandom() < hitArgs[0].AsInt)
            sourceProfile.Add("is_pierce", 1);

        // 增加无视防御标识
        sourceProfile.Add("rate_ignore_def", 1000);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 打冷却技能专用计算脚本
public class SCRIPT_9115 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "B_REBORN_STATUS", "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY"};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 有重生状态则跳出
        if (targetOb.CheckStatus(statusList))
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 检查目标是否有免疫状态，如果有则无法起效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
            return false;

        // 对BOSS减半
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        int rate = hitArgs[0].AsInt + upRate;
        if (targetOb.Query<int>("is_boss") == 1)
            rate = rate / 2;

        int cdTag = 0;
        if (RandomMgr.GetRandom() < rate)
        {
            LPCArray skills = targetOb.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                // 获取技能id
                int tarSkillId = mks.AsArray[0].AsInt;
                // 不受技能cd增加影响的检查
                if (IS_CD_ADD_IMMU_CHECK.Call(tarSkillId))
                    continue;
                if (CdMgr.GetBaseSkillCd(tarSkillId, 1) > 0)
                {
                    // 获取可叠加容量
                    int canAddTurn = CdMgr.GetSkillCd(targetOb, tarSkillId) - CdMgr.GetSkillCdRemainRounds(targetOb, tarSkillId);
                    // 如果有免疫状态，则不起效
                    if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
                        continue;
                    CdMgr.AddSkillCd(targetOb, tarSkillId, Math.Min(canAddTurn, hitArgs[1].AsInt));
                    cdTag += 1;
                }
            }
        }

        // 技能提示
        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_cd_up"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
        }

        return true;
    }
}

// 特殊伤害计算脚本(根据攻击力、目标技能概率暴击)
public class SCRIPT_9116 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        sourceProfile.Add("sp_crt_rate", hitArgs[0].AsInt);

        // 获取技能初始攻击力加成效果
        int atkRate = hitArgs[1].AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取目标身上的buff/debuff数量
        List<LPCMapping> tarAllStatus = targetOb.GetAllStatus();
        List<LPCMapping> tarBuffStatus = new List<LPCMapping>();
        List<LPCMapping> tarDebuffStatus = new List<LPCMapping>();

        foreach (LPCMapping tarStatusMap in tarAllStatus)
        {
            if (tarStatusMap.GetValue<int>("status_type") == StatusConst.TYPE_BUFF)
                tarBuffStatus.Add(tarStatusMap);
            if (tarStatusMap.GetValue<int>("status_type") == StatusConst.TYPE_DEBUFF)
                tarDebuffStatus.Add(tarStatusMap);
        }

        sourceProfile.Add("tar_debuff_num", tarDebuffStatus.Count);
        sourceProfile.Add("tar_buff_num", tarBuffStatus.Count);

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(根据攻击力、根据目标剩余能量判断是否伤害百分比加成提升)
public class SCRIPT_9117 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 获取技能初始攻击力加成效果
        int atkRate = hitArgs[1].AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        // 计算是否百分比增伤
        if (targetOb.Query<int>("mp") < hitArgs[0].AsInt)
            dmgRate += hitArgs[2].AsInt;

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(攻击力 + 根据敌方持有的持续伤害效果个数增加伤害)
public class SCRIPT_9118 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        // 获取持续伤害状态个数
        int injuryNum = targetOb.GetStatusCondition("D_INJURY").Count;
        sourceProfile.Add("injury_damage_rate", injuryNum * skillAttrib[1].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_TARGET_INJURY_NUM.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(根据攻击力、目标技能概率暴击、概率无视防御)
public class SCRIPT_9119 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        sourceProfile.Add("sp_crt_rate", hitArgs[0].AsInt);

        // 获取技能初始攻击力加成效果
        int atkRate = hitArgs[1].AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算是否无视防御
        sourceProfile.Add("rate_ignore_def", hitArgs[2].AsInt);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 暴击时扣蓝伤害计算脚本(存在单hit依赖情况)
public class SCRIPT_9120 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");
        if (!dependHit.IsInt)
            return false;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damageType = ridArgs.GetValue<int>("damage_type");

        if ((damageType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 检查是否暴击
        if ((damageType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 计算扣蓝伤害
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCMapping mpAtkData = new LPCMapping();
        mpAtkData.Add("mp_dmg_rate", hitScriptArg[0].AsInt + upRate);
        mpAtkData.Add("mp_dmg", hitScriptArg[1].AsInt);
        mpAtkData.Add("accuracy_rate", sourceOb.QueryAttrib("accuracy_rate"));
        mpAtkData.Add("resist_rate", targetOb.QueryAttrib("resist_rate"));
        mpAtkData.Add("restrain", ridArgs.GetValue<int>("restrain"));
        int mpDmg = CALC_MP_DAMAGE.Call(mpAtkData,targetOb);
        damageType = mpDmg > 0 ? damageType | CombatConst.DAMAGE_TYPE_MP : damageType;
        mpAtkData.Add("damage_type", damageType);
        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("mp", mpDmg);
        damageMap.Add("points", points);
        damageMap.Add("target_rid", targetOb.GetRid());
        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+敌人生命值上限来计算伤害，加成伤害不走暴击和强击)
public class SCRIPT_9121 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_TARGET_MAX_HP_NO_DEAD_INFLU.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害+目标控制状态必定暴击)
public class SCRIPT_9122 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 目标控制状态必定暴击
        if (improvementMap.GetValue<int>("status_ctrl_id") == 1)
            damageType = damageType | CombatConst.DAMAGE_TYPE_DEADLY;

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用自爆伤害计算脚本(纯攻击力计算)
public class SCRIPT_9123 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 模型隐藏
        // 角色死亡0.2渐变隐藏模型
        if (sourceOb.Query<LPCValue>("die_action") == null)
        {
            sourceOb.Set("hp", LPCValue.Create(0));
            TRY_DO_DIE.Call(sourceOb);
        }

        return damageMap;
    }
}

// 特殊伤害计算脚本，灾难专用
public class SCRIPT_9124 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // 判断前后排伤害
        if (actionArgs.GetValue<int>("one_raw_id") != 0)
            atkRate = atkRate * 2;
        else
        {
            if (targetOb.FormationRaw == FormationConst.RAW_BACK)
                atkRate = atkRate / 2;
        }

        // 获取负面状态个数
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> debuffList = new List<int>();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != StatusConst.TYPE_DEBUFF)
                continue;

            // 清楚状态
            debuffList.Add(allStatus[i].GetValue<int>("cookie"));
        }
        sourceProfile.Add("debuff_atk_rate", debuffList.Count * 50);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_TARGET_DEBUFF_NUM.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+首领生命值越低，伤害越高)
public class SCRIPT_9125 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        List<Property> selectOb = new List<Property>();

        // 遍历各个角色
        foreach (Property checkOb in targetList)
        {
            // 角色已经死亡
            if (checkOb.CheckStatus("DIED") || checkOb.Query<int>("is_boss") != 1)
                continue;
            selectOb.Add(checkOb);
        }

        int damageUpRate = 0;

        if (selectOb.Count != 0)
            damageUpRate = skillAttrib[1].AsInt * (1000 -
                Game.Divided(selectOb[0].Query<int>("hp") , selectOb[0].QueryAttrib("max_hp")));

        sourceProfile.Add("damage_up_by_hp", damageUpRate);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(根据自身敏捷计算)
public class SCRIPT_9126 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        // 获取敏捷的攻击力加成
        atkRate += sourceOb.QueryAttrib("agility") * hitScriptArg[1].AsInt;
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 魂石结晶专用伤害计算脚本(纯攻击力计算)
public class SCRIPT_9127 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        sourceProfile.Add("damage_up_by_mp", (targetOb.QueryAttrib("max_mp") - targetOb.Query<int>("mp")) * sourceOb.QueryAttrib("dmg_up_low_mp"));

        // 检查减益增幅
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        int debuffNum = 0;
        for (int i = 0; i < allStatus.Count; i++)
        {
            CsvRow info;
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
            if (info.Query<int>("status_type") == StatusConst.TYPE_DEBUFF)
                debuffNum += 1;
        }
        if (debuffNum > 0)
            sourceProfile.Add("damage_up_by_debuff", debuffNum * sourceOb.QueryAttrib("dmg_up_debuff_num"));

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * hitScriptArg[1].AsInt;

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 生命之力的伤害增强效果，次数限制在状态本身限制
        LPCMapping condition = new LPCMapping();
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        if (sourceProfile.ContainsKey("hit_times_dmg_up"))
        {
            List<Property> checkList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

            // 筛选非死亡单位
            foreach (Property checkOb in checkList)
            {
                if (!checkOb.CheckStatus("DIED"))
                    checkOb.ApplyStatus("B_DMG_UP_SOUL", condition);
            }
        }

        return damageMap;
    }
}

// 伤害计算脚本(纯攻击力计算)(敌方能量低于一定值则无视防御)
public class SCRIPT_9128 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 获取目标敌方残留mp
        // 计算是否无视防御
        if (targetOb.Query<int>("mp") < hitScriptArg[1].AsInt)
            sourceProfile.Add("rate_ignore_def", 1000);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯扣血)
public class SCRIPT_9129 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // 计算克制关系
        int restrain = ElementConst.ELEMENT_NEUTRAL;
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK | CombatConst.DAMAGE_TYPE_THORNS;

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", Game.Multiple(targetOb.Query<int>("hp"), atkRate));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 精金魔像能量压制特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害,同时计算目标剩余能量)
public class SCRIPT_9130 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt);

        // 计算目标剩余能量带来的影响
        sourceProfile.Add("damage_up_by_mp", (targetOb.QueryAttrib("max_mp") - targetOb.Query<int>("mp")) * skillAttrib[2].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+目标当前生命来计算伤害)
public class SCRIPT_9131 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;

        // 目标单位生命越低、系数越高，加成越高
        int targetMaxHp = targetOb.QueryAttrib("max_hp");
        if (targetMaxHp == 0)
            atkRate = 0;
        else
        {
            int targetCurHp = targetOb.Query<int>("hp");

            // 这个地方有可能数据溢出
            // 只要目标损失血量大于36000必定溢出（BOSS血量远远大于这个数值）
            // 战斗喵技能效果参数({2300, 400})})
            // skillAttrib数值全部都是千分比
            // atkRate += skillAttrib[1].AsInt * atkRate * (targetMaxHp - targetCurHp) / targetMaxHp / 1000;
            atkRate += Game.Divided(
                Game.Multiple((targetMaxHp - targetCurHp), skillAttrib[1].AsInt * atkRate, 1000000),
                targetMaxHp);
        }

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御计算，带减益增幅)
public class SCRIPT_9132 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // 检查减益增幅
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        int debuffNum = 0;
        for (int i = 0; i < allStatus.Count; i++)
        {
            CsvRow info;
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
            if (info.Query<int>("status_type") == StatusConst.TYPE_DEBUFF)
                debuffNum += 1;
        }
        if (debuffNum > 0)
            sourceProfile.Add("damage_up_by_debuff", hitScriptArg[2].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，无视格挡)
public class SCRIPT_9133 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 增加无视格挡概率
        sourceProfile.Add("rate_ignore_block",hitScriptArg[1].AsInt);

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力加成 + 死亡单位数量额外提高攻击加成)
public class SCRIPT_9134 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;
        int deadUpDmg = hitScriptArg[1].AsInt;
        int maxDeadCo = hitScriptArg[2].AsInt;

        // 搜集场上所有死亡单位
        List<Property> allList = RoundCombatMgr.GetPropertyList();
        List<Property> deathList = new List<Property>();
        foreach (Property deathOb in allList)
        {
            if (deathOb.CheckStatus("DIED"))
                deathList.Add(deathOb);
        }
        // 死亡单位加成
        atkRate = atkRate + atkRate * deadUpDmg * Math.Max(deathList.Count, maxDeadCo) / 1000;

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 爆炸南瓜头专用伤害计算脚本（自爆，攻击力加成 + 自身损失生命的百分比真伤，如果目标死亡则复活百分比血量复活自身）
public class SCRIPT_9135 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("real_damage_lost_hp_co", hitScriptArg[1].AsInt);

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 模型隐藏（自杀）
        CombatActor actor = sourceOb.Actor;
        actor.SetTweenAlpha(0f);

        return damageMap;
    }
}

// 倍率伤害计算脚本(纯攻击力计算，最终伤害翻倍)
public class SCRIPT_9136 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取目标身上的buff/debuff数量
        List<LPCMapping> tarAllStatus = targetOb.GetAllStatus();
        List<LPCMapping> tarBuffStatus = new List<LPCMapping>();
        List<LPCMapping> tarDebuffStatus = new List<LPCMapping>();

        foreach (LPCMapping tarStatusMap in tarAllStatus)
        {
            if (tarStatusMap.GetValue<int>("status_type") == StatusConst.TYPE_BUFF)
                tarBuffStatus.Add(tarStatusMap);
            if (tarStatusMap.GetValue<int>("status_type") == StatusConst.TYPE_DEBUFF)
                tarDebuffStatus.Add(tarStatusMap);
        }

        sourceProfile.Add("tar_debuff_num", tarDebuffStatus.Count);
        sourceProfile.Add("tar_buff_num", tarBuffStatus.Count);

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        int multiRate = hitScriptArg[1].AsInt + CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK_MULTI.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType, multiRate, hitScriptArg[2].AsInt));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 天星乱舞·暗，专用扣蓝伤害计算脚本
public class SCRIPT_9137 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;

        // 计算扣蓝伤害
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCMapping mpAtkData = new LPCMapping();
        mpAtkData.Add("mp_dmg_rate", hitScriptArg[0].AsInt + skillEffect);
        mpAtkData.Add("mp_dmg", hitScriptArg[1].AsInt);
        mpAtkData.Add("accuracy_rate", sourceOb.QueryAttrib("accuracy_rate"));
        mpAtkData.Add("resist_rate", targetOb.QueryAttrib("resist_rate"));
        mpAtkData.Add("restrain", restrain);
        int mpDmg = CALC_MP_DAMAGE.Call(mpAtkData,targetOb);
        damageType = mpDmg > 0 ? damageType | CombatConst.DAMAGE_TYPE_MP : damageType;
        mpAtkData.Add("damage_type", damageType);
        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("mp", mpDmg);
        damageMap.Add("points", points);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());
        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 进行被动加成
        if (mpDmg > 0)
        {
            LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");
            LPCArray healDmgArgs = sourceImprovement["mp_dmg_cast_heal_real"].AsArray;
            int healPer = healDmgArgs[0].AsArray[0].AsInt;
            // 添加真伤信息
            sourceProfile.Add("attack_damage_co", healDmgArgs[0].AsArray[1].AsInt);
            // 自身回血
            if (!sourceOb.CheckStatus(banStatusList))
            {
                for (int i = 0; i < mpDmg; i++)
                {
                    int maxHp = sourceOb.QueryAttrib("max_hp");
                    // 计算恢复血量
                    int hpCure = Game.Multiple(maxHp, healPer, 1000);
                    // 计算额外影响
                    hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));
                    LPCMapping cureMap = new LPCMapping();
                    cureMap.Add("hp", hpCure);
                    (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
                }
            }
            // 额外真伤
            for (int i = 0; i < mpDmg; i++)
            {
                // 计算伤害结构
                int realDamageType = CombatConst.DAMAGE_TYPE_ATTACK;
                LPCMapping realDamageMap = new LPCMapping();
                LPCMapping realPoints = new LPCMapping();
                realPoints.Add("hp", CALC_ALL_DAMAGE_ATTACK_ATTACK_REAL.Call(0, finalAttribMap, improvementMap, sourceProfile, realDamageType));
                realDamageMap.Add("points", realPoints);
                realDamageMap.Add("restrain", restrain);
                realDamageMap.Add("damage_type", realDamageType);
                realDamageMap.Add("target_rid", targetOb.GetRid());
                // 通知玩家受创
                (targetOb as Char).ReceiveDamage(sourceProfile, damageType, realDamageMap);
            }
        }

        return damageMap;
    }
}

// 清算专用伤害计算脚本(纯攻击力计算 + 减益状态增伤)
public class SCRIPT_9138 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int hpDmg = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        // 减益状态增伤
        List <LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> debuffList = new List<int>();
        foreach (LPCMapping statusData in allStatus)
        {
            CsvRow info;
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));
            // 获取状态的类型
            int buff_type = info.Query<int>("status_type");
            if (buff_type != StatusConst.TYPE_DEBUFF)
                continue;
            debuffList.Add(statusData.GetValue<int>("cookie"));
        }
        hpDmg += Game.Multiple(hpDmg, debuffList.Count * hitScriptArg[1].AsInt, 1000);
        LPCArray hpLowDmgPer = sourceProfile.GetValue<LPCArray>("moyan_hp_low_atk_up");
        int sourceHpPer = 1000 * sourceOb.Query<int>("hp") / sourceOb.QueryAttrib("max_hp");
        int multiDmg = 0;
        if (sourceHpPer > 0)
            multiDmg = Game.Multiple((1000 - sourceHpPer), hpLowDmgPer[0].AsArray[1].AsMapping.GetValue<int>(3), 1000);
        if (sourceHpPer >= 300)
            multiDmg = Game.Multiple((1000 - sourceHpPer), hpLowDmgPer[0].AsArray[1].AsMapping.GetValue<int>(2), 1000);
        if (sourceHpPer >= 500)
            multiDmg = Game.Multiple((1000 - sourceHpPer), hpLowDmgPer[0].AsArray[1].AsMapping.GetValue<int>(1), 1000);
        points.Add("hp", hpDmg + Game.Multiple(hpDmg, multiDmg, 1000));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 魔眼凝视专用伤害计算脚本(纯攻击力计算)
public class SCRIPT_9139 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int hpDmg = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        LPCArray hpLowDmgPer = sourceProfile.GetValue<LPCArray>("moyan_hp_low_atk_up");
        int sourceHpPer = 1000 * sourceOb.Query<int>("hp") / sourceOb.QueryAttrib("max_hp");
        int multiDmg = 0;
        if (sourceHpPer > 0)
            multiDmg = Game.Multiple((1000 - sourceHpPer), hpLowDmgPer[0].AsArray[1].AsMapping.GetValue<int>(3), 1000);
        if (sourceHpPer >= 300)
            multiDmg = Game.Multiple((1000 - sourceHpPer), hpLowDmgPer[0].AsArray[1].AsMapping.GetValue<int>(2), 1000);
        if (sourceHpPer >= 500)
            multiDmg = Game.Multiple((1000 - sourceHpPer), hpLowDmgPer[0].AsArray[1].AsMapping.GetValue<int>(1), 1000);
        points.Add("hp", hpDmg + Game.Multiple(hpDmg, multiDmg, 1000));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 小魔眼专用伤害计算脚本(纯攻击力计算，无视目标一定百分比格挡)
public class SCRIPT_9140 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 增加格挡忽视
        sourceProfile.Add("rate_block_res",hitScriptArg[1].AsInt);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(消耗自身生命)
public class SCRIPT_9141 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        //Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取技能初始攻击力加成效果
        int selfHpCostRate = (_params[6] as LPCValue).AsInt;

        // 消耗自身生命
        int selfHpCost = Game.Multiple(sourceOb.QueryAttrib("max_hp"), selfHpCostRate, 1000);

        if (selfHpCost >= sourceOb.Query<int>("hp"))
        {
            sourceOb.Set("hp", LPCValue.Create(0));
            TRY_DO_DIE.Call(sourceOb);
        }

        sourceOb.Set("hp", LPCValue.Create(Math.Max(sourceOb.Query<int>("hp") - selfHpCost, 0)));

        return LPCMapping.Empty;
    }
}

// 通用伤害计算脚本(纯攻击力计算 + 减益状态增伤)
public class SCRIPT_9142 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int hpDmg = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        // 减益状态增伤
        List <LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> debuffList = new List<int>();
        foreach (LPCMapping statusData in allStatus)
        {
            CsvRow info;
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));
            // 获取状态的类型
            int buff_type = info.Query<int>("status_type");
            if (buff_type != StatusConst.TYPE_DEBUFF)
                continue;
            debuffList.Add(statusData.GetValue<int>("cookie"));
        }
        hpDmg += Game.Multiple(hpDmg, debuffList.Count * hitScriptArg[1].AsInt, 1000);
        points.Add("hp", hpDmg );
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，自身生命越低伤害越高，对有特殊状态的单位伤害提升)
public class SCRIPT_9143 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 添加自身生命越低伤害越高属性，格式：每损失生命百分比，伤害提升百分比
        sourceProfile.Add("low_hp_dmg_up", new LPCArray(new LPCArray(hitScriptArg[1].AsInt, hitScriptArg[2].AsInt)));

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int hpDmg = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        // 对持有特殊状态的单位伤害提升
        if (targetOb.CheckStatus(hitScriptArg[3].AsString))
            hpDmg += Game.Multiple(hpDmg,hitScriptArg[4].AsInt,1000);
        points.Add("hp", hpDmg);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，极冷之壁专用)
public class SCRIPT_9144 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，极冷之壁专用)
public class SCRIPT_9145 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+目标生命值上限来计算伤害，不计算防御等额外加成效果)
public class SCRIPT_9146 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        int atkRate = (_params[6] as LPCValue).AsInt;

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;

        // 计算伤害结构
        int dmg = Game.Multiple(sourceOb.QueryAttrib("max_hp"), atkRate, 1000);
        dmg += dmg * sourceProfile.GetValue<int>("skill_effect") / 1000;

        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", dmg);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御计算，目标防御比自身低时暴击率增加)
public class SCRIPT_9147 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // 判断防御提供的暴击加成
        if (sourceOb.QueryAttrib("defense") > targetOb.QueryAttrib("defense"))
            sourceProfile.Add("more_crt_high_def",hitScriptArg[2].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御计算，目标在嘲讽状态下百分比增伤)
public class SCRIPT_9148 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int damage = CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        // 嘲讽状态下的增伤
        if (targetOb.CheckStatus("D_PROVOKE"))
            damage += Game.Multiple(damage, hitScriptArg[2].AsInt, 1000);
        points.Add("hp", damage);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御计算，目标生命上限越高伤害越大，目标生命上限的参考值不会超过自身防御力的 12 倍，若目标防御高于自身防御力的一半则伤害降低 50%。)
public class SCRIPT_9149 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);
        sourceProfile.Add("tar_max_hp_damage_co", hitScriptArg[2].AsInt);
        sourceProfile.Add("def_limit", hitScriptArg[3].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int damage = CALC_ALL_DAMAGE_ATTACK_DEFNESE_TAR_MAXHP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        if (targetOb.QueryAttrib("defense") > sourceOb.QueryAttrib("defense") / 2)
            damage = damage - Game.Multiple(damage, hitScriptArg[4].AsInt, 1000);
        points.Add("hp", damage);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 陇断连牙专用伤害计算脚本(根据自身敏捷计算，击杀上全体状态)
public class SCRIPT_9150 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        // 获取敏捷的攻击力加成
        atkRate += sourceOb.QueryAttrib("agility") * hitScriptArg[1].AsInt;
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 如果目标死亡
        if (targetOb.CheckStatus("DIED"))
        {
            int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

            // 整理筛选结果，将手选结果放在第一个
            List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            foreach (Property checkOb in finalList)
            {
                // 2 初次检查成功率
                if (RandomMgr.GetRandom() >= (hitScriptArg[2].AsInt))
                    return false;

                // 3 计算效果命中
                // 攻击者效果命中、防御者效果抵抗、克制关系
                // 计算克制关系
                int anoRestrain = ElementMgr.GetMonsterCounter(sourceOb, checkOb);
                int anoRate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"), checkOb.QueryAttrib("resist_rate"), anoRestrain);

                if (RandomMgr.GetRandom() < anoRate)
                {
                    string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
                    BloodTipMgr.AddTip(TipsWndType.DamageTip, checkOb, tips);
                    return false;
                }

                // 上状态
                LPCMapping anoCondition = new LPCMapping();
                anoCondition.Add("round", hitScriptArg[4].AsInt);
                if (actionArgs.ContainsKey("original_cookie"))
                    anoCondition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
                else
                    anoCondition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
                anoCondition.Add("source_profile", sourceProfile);

                // 附加状态
                checkOb.ApplyStatus(hitScriptArg[3].AsString, anoCondition);
            }
        }

        return damageMap;
    }
}

// 通用伤害计算脚本(固定百分比伤害)
public class SCRIPT_9151 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        int atkHpRate = (_params[6] as LPCValue).AsInt;

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", Game.Multiple(targetOb.QueryAttrib("max_hp"), atkHpRate ,1000));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(伤害计算根据攻击力+消耗能量)
public class SCRIPT_9152 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 已消耗能量的加成效果
        LPCMapping costMap = actionArgs.GetValue<LPCMapping>("skill_cost");
        int mpCost = costMap.GetValue<int>("mp");
        if (mpCost <= hitScriptArg[1].AsInt)
            atkRate += (mpCost - 1) * hitScriptArg[2].AsInt;
        else
            atkRate += (mpCost - 1) * hitScriptArg[3].AsInt;

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，无视陷阱伤害类型，对有陷阱的单位必定暴击)
public class SCRIPT_9153 : Script
{
    private static List<string> statusList = new List<string>(){ "B_POISON_TRAP", "B_SLEEP_TRAP", "B_SILENCE_TRAP", "B_FREEZE_TRAP", "B_STUN_TRAP",};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 对陷阱持有单位添加暴击标识
        if (targetOb.CheckStatus(statusList))
            sourceProfile.Add("extra_crt_id", 1);

        // 添加无视陷阱减伤标识
        sourceProfile.Add("ignore_trap_reduce", 1);

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 添加额外伤害类型
        damageType = damageType | hitScriptArg[1].AsInt;

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 梦魇降临专用伤害计算脚本(根据自身敏捷计算，对睡眠单位永远处于优势克制属性)
public class SCRIPT_9154 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        // 获取敏捷的攻击力加成
        atkRate += sourceOb.QueryAttrib("agility") * hitScriptArg[1].AsInt;
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        // 检查是否处于睡眠状态，如果是，则永远优势克制
        if (targetOb.CheckStatus("D_SLEEP"))
            restrain = ElementConst.ELEMENT_ADVANTAGE;
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 检查目标睡眠状态下的概率
        if (targetOb.CheckStatus("D_SLEEP"))
        {
            int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
            // 检查成功率
            if (RandomMgr.GetRandom() < hitScriptArg[2].AsInt + upRate)
            {
                // 加入追加回合、清空CD
                RoundCombatMgr.DoAdditionalRound(sourceOb, LPCMapping.Empty);
                int reduceCd = CdMgr.GetSkillCd(sourceOb, skillId);
                CdMgr.DoReduceSkillCd(sourceOb, skillId, reduceCd);
            }
        }

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，根据已驱散的增益状态数量进行伤害增强)
public class SCRIPT_9155 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetOb.GetRid());

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int dmgValue = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        dmgValue += Game.Multiple(dmgValue, ridArgs.GetValue<LPCArray>("remove_list").Count * hitScriptArg[1].AsInt, 1000);
        points.Add("hp", dmgValue);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 目标生命上限越高伤害越大，加成伤害会走暴击和强击)
public class SCRIPT_9156 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("skill_id", skillId);
        sourceProfile.Add("tar_max_hp_damage_co", hitScriptArg[1].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int damage = CALC_ALL_DAMAGE_ATTACK_TAR_MAXHP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        points.Add("hp", damage);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算、额外概率暴击、目标处于控制状态则永远保持优势属性)
public class SCRIPT_9157 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;
        int atkRate = hitArgs[1].AsInt;

        // 技能自身增加暴击率
        sourceProfile.Add("sp_crt_rate", hitArgs[0].AsInt);

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        // 遍历自身状态列表
        List <LPCMapping> allStatus = targetOb.GetAllStatus();
        LPCArray ctrlList = LPCArray.Empty;
        for (int i = 0; i < allStatus.Count; i++)
        {
            CsvRow statusInfo;
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
            LPCMapping ctrlMap = statusInfo.Query<LPCMapping>("limit_round_args");
            // 如果是控制状态，则清除之
            if (ctrlMap.ContainsKey("ctrl_id"))
                ctrlList.Add(allStatus[i].GetValue<int>("status_id"));
        }
        // 处于控制状态，则永远保持优势属性
        if (ctrlList.Count > 0)
            restrain = ElementConst.ELEMENT_ADVANTAGE;

        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(纯攻击力计算)(敌方处于对应状态则必定暴击)
public class SCRIPT_9158 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;

        // 无视防御的目标状态
        string crtStatus = hitScriptArg[1].AsString;

        // 检查目标是否处于对应状态下，如果是则添加无视防御标识
        if (targetOb.CheckStatus(crtStatus))
            sourceProfile.Add("extra_crt_id", 1);

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算、能量越少攻击力加成越高)
public class SCRIPT_9159 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        int targetRemainMp = targetOb.Query<int>("mp");

        int atkRate = hitScriptArg[0].AsInt + Game.Multiple(hitScriptArg[0].AsInt, hitScriptArg[1].AsInt * (5 - targetRemainMp), 1000);

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算、概率无视防御、目标处于控制状态则永远保持优势属性)
public class SCRIPT_9160 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;
        int atkRate = hitArgs[1].AsInt;

        // 技能自身增加暴击率
        sourceProfile.Add("rate_ignore_def", hitArgs[0].AsInt);

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        // 遍历自身状态列表
        List <LPCMapping> allStatus = targetOb.GetAllStatus();
        LPCArray ctrlList = LPCArray.Empty;
        for (int i = 0; i < allStatus.Count; i++)
        {
            CsvRow statusInfo;
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
            LPCMapping ctrlMap = statusInfo.Query<LPCMapping>("limit_round_args");
            // 如果是控制状态，则清除之
            if (ctrlMap.ContainsKey("ctrl_id"))
                ctrlList.Add(allStatus[i].GetValue<int>("status_id"));
        }
        // 处于控制状态，则永远保持优势属性
        if (ctrlList.Count > 0)
            restrain = ElementConst.ELEMENT_ADVANTAGE;

        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(攻击力按敏捷越高加成越大、目标处于控制状态则永远保持优势属性)
public class SCRIPT_9161 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;
        int atkRate = hitArgs[0].AsInt;

        // 敏捷影响因素
        atkRate = hitArgs[0].AsInt + Game.Multiple(hitArgs[0].AsInt, sourceOb.QueryAttrib("agility") * hitArgs[1].AsInt, 1000);

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        // 遍历自身状态列表
        List <LPCMapping> allStatus = targetOb.GetAllStatus();
        LPCArray ctrlList = LPCArray.Empty;
        for (int i = 0; i < allStatus.Count; i++)
        {
            CsvRow statusInfo;
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
            LPCMapping ctrlMap = statusInfo.Query<LPCMapping>("limit_round_args");
            // 如果是控制状态，则清除之
            if (ctrlMap.ContainsKey("ctrl_id"))
                ctrlList.Add(allStatus[i].GetValue<int>("status_id"));
        }
        // 处于控制状态，则永远保持优势属性
        if (ctrlList.Count > 0)
            restrain = ElementConst.ELEMENT_ADVANTAGE;

        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害、消耗能量越多，加成越高)
public class SCRIPT_9162 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;
        int maxHpCo = 0;
        // 已消耗能量的加成效果
        LPCMapping costMap = actionArgs.GetValue<LPCMapping>("skill_cost");
        int mpCost = costMap.GetValue<int>("mp");
        if (mpCost <= hitScriptArg[2].AsInt)
            maxHpCo = hitScriptArg[1].AsInt;
        else
            maxHpCo = hitScriptArg[1].AsInt + (mpCost - 3) * hitScriptArg[3].AsInt;

        sourceProfile.Add("max_hp_damage_co", maxHpCo);
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害、目标生命比率越低加成越高)
public class SCRIPT_9163 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;

        int baseHpDmgCo = skillAttrib[1].AsInt;

        baseHpDmgCo += baseHpDmgCo * (1000 - targetOb.QueryTemp<int>("hp_rate")) * skillAttrib[2].AsInt / 1000000;

        sourceProfile.Add("max_hp_damage_co", baseHpDmgCo);
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害, 概率双倍伤害)
public class SCRIPT_9164 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt);
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int damageValue = CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        // 概率双倍伤害
        if (RandomMgr.GetRandom() < skillAttrib[2].AsInt)
            damageValue += damageValue;
        points.Add("hp", damageValue);
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害、增加暴击概率)
public class SCRIPT_9165 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt);
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算是否无视防御
        int rateUp = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        sourceProfile.Add("sp_crt_rate", skillAttrib[2].AsInt + rateUp);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御计算 + 自身增益状态数量增幅)
public class SCRIPT_9166 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        int dmgValue = CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);

        dmgValue += Game.Multiple(dmgValue, sourceProfile.GetValue<int>("buff_count") * hitScriptArg[2].AsInt, 1000);

        points.Add("hp", dmgValue);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 元素体破壁一击中伤害计算脚本(攻击力 + 根据自身防御计算 + 无视格挡 + 目标防御力低于自身必定暴击)
public class SCRIPT_9167 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 增加无视格挡概率
        sourceProfile.Add("rate_ignore_block",hitScriptArg[2].AsInt);

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        if (targetOb.QueryAttrib("defense") < sourceOb.QueryAttrib("defense"))
            sourceProfile.Add("extra_crt_id", 1);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算 + 上一个hit点造成的扣篮伤害加成)
public class SCRIPT_9168 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        string targetRid = targetOb.Query<string>("rid");
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 获取上一个hit点的扣篮伤害信息
        LPCMapping mpPoints = ridArgs.GetValue<LPCMapping>("points");

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int dmg = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        dmg += dmg * mpPoints.GetValue<int>("mp") * hitScriptArg[1].AsInt / 1000;
        points.Add("hp", dmg);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(根据自身敏捷计算攻击力加成 + 敌人生命值上限伤害加成)
public class SCRIPT_9169 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        // 获取敏捷的攻击力加成
        atkRate += sourceOb.QueryAttrib("agility") * hitScriptArg[1].AsInt;
        sourceProfile.Add("skill_id", skillId);
        sourceProfile.Add("target_max_hp_co", hitScriptArg[2].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_TARGET_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());
        damageMap.Add("origin_atk_rate", sourceProfile.GetValue<int>("origin_atk_rate"));

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(根据攻击力、目标血量、无视防御)
public class SCRIPT_9170 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 获取技能初始攻击力加成效果：（目标血量 - 自身攻击力*5） / 2
        int atkRate = (targetOb.QueryAttrib("max_hp") - sourceOb.QueryAttrib("attack") * hitArgs[0].AsInt) / hitArgs[1].AsInt;
        // 添加最小伤害信息
        sourceProfile.Add("min_dmg", hitArgs[2].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算是否无视防御
        sourceProfile.Add("rate_ignore_def", hitArgs[3].AsInt);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算 + 状态计算)
public class SCRIPT_9171 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray  hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 上状态部分
        // 1 检查是否格挡

        if ((damageType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitScriptArg[1].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitScriptArg[3].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitScriptArg[2].AsString, condition);

        return damageMap;
    }
}

// 竖琴仙子夏之星专用计算脚本
public class SCRIPT_9172 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "B_REBORN_STATUS", "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY"};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 有重生状态则跳出
        if (targetOb.CheckStatus(statusList))
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 对BOSS减半
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        int rate = hitArgs[0].AsInt + upRate;
        if (targetOb.Query<int>("is_boss") == 1)
            rate = rate / 2;

        int cdTag = 0;
        if (RandomMgr.GetRandom() < rate)
        {
            LPCArray skills = targetOb.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                // 获取技能id
                int tarSkillId = mks.AsArray[0].AsInt;
                // 不受技能cd增加影响的检查
                if (IS_CD_ADD_IMMU_CHECK.Call(tarSkillId))
                    continue;
                if (CdMgr.GetBaseSkillCd(tarSkillId, 1) > 0)
                {
                    // 获取可叠加容量
                    int canAddTurn = CdMgr.GetSkillCd(targetOb, tarSkillId) - CdMgr.GetSkillCdRemainRounds(targetOb, tarSkillId);
                    // 如果有免疫状态，则不起效
                    if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
                        continue;
                    CdMgr.AddSkillCd(targetOb, tarSkillId, Math.Min(canAddTurn, hitArgs[1].AsInt));
                    cdTag += 1;
                }
            }
        }

        // 技能提示
        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_cd_up"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
        }

        // 扣篮计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        // 扣除最大不会超过当前拥有蓝量
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;
        damageType = damageType | CombatConst.DAMAGE_TYPE_MP;
        points.Add("mp", Math.Min(hitArgs[2].AsInt, targetOb.Query<int>("mp")));
        damageMap.Add("points", points);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return true;
    }
}

// 清空目标所有蓝量伤害计算脚本
public class SCRIPT_9173 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCValue hitScriptArg = _params[6] as LPCValue;

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;

        // 计算扣蓝伤害
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCMapping mpAtkData = new LPCMapping();
        mpAtkData.Add("mp_dmg_rate", hitScriptArg.AsInt + skillEffect);
        mpAtkData.Add("mp_dmg", targetOb.Query<int>("mp"));
        mpAtkData.Add("accuracy_rate", sourceOb.QueryAttrib("accuracy_rate"));
        mpAtkData.Add("resist_rate", targetOb.QueryAttrib("resist_rate"));
        mpAtkData.Add("restrain", restrain);
        int mpDmg = CALC_MP_DAMAGE.Call(mpAtkData, targetOb);
        damageType = mpDmg > 0 ? damageType | CombatConst.DAMAGE_TYPE_MP : damageType;
        mpAtkData.Add("damage_type", damageType);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        // 扣除最大不会超过当前拥有蓝量
        points.Add("mp", Math.Min(mpDmg, targetOb.Query<int>("mp")));
        damageMap.Add("points", points);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害+敌人生命比率越低伤害越高)
public class SCRIPT_9174 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt * (10000 + (1000 - targetOb.QueryTemp<int>("hp_rate")) * skillAttrib[2].AsInt) / 10000);
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 特殊伤害计算脚本(根据攻击力、吸血、超额变护盾，暂时废弃)
public class SCRIPT_9175 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 获取技能初始攻击力加成效果
        int atkRate = hitArgs[1].AsInt;

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算是否无视防御
        int rateUp = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        sourceProfile.Add("rate_ignore_def", hitArgs[0].AsInt + rateUp);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int dmgValue = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        points.Add("hp", dmgValue);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 吸血
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", dmgValue);

        // 溢出量先转化为护盾再执行回血
        int overCure = dmgValue - sourceOb.QueryAttrib("max_hp");
        if (overCure > 0)
        {
            // 上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", hitArgs[3].AsInt);
            if (actionArgs.ContainsKey("original_cookie"))
                condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            condition.Add("can_absorb_damage", Game.Multiple(overCure, hitArgs[2].AsInt, 1000));
            condition.Add("source_profile", sourceProfile);

            // 附加状态
            sourceOb.ApplyStatus("B_HP_SHIELD", condition);
        }

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(按照敏捷计算，无视格挡)
public class SCRIPT_9176 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        // 获取敏捷的攻击力加成
        atkRate += sourceOb.QueryAttrib("agility") * hitScriptArg[1].AsInt;
        sourceProfile.Add("skill_id", skillId);

        // 增加无视格挡概率
        sourceProfile.Add("rate_ignore_block",hitScriptArg[2].AsInt);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 蝙蝠专用伤害计算脚本(纯攻击力计算 + 每次攻击自身恢复血量)
public class SCRIPT_9177 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        int targetHp = targetOb.Query<int>("hp");
        sourceProfile.Add("hp_value_compare", targetHp - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 计算恢复血量
        int sourceMaxHp = sourceOb.QueryAttrib("max_hp");
        int hpCure =  Game.Multiple(sourceMaxHp, hitScriptArg[1].AsInt, 1000);

        if (hpCure * 10 < Game.Multiple(sourceMaxHp, hitScriptArg[1].AsInt, 100))
            hpCure += 1;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作，特殊回血，不需要检查banStatusList
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return damageMap;
    }
}

// 扣蓝伤害计算脚本，结束时判断目标蓝量触发自身立即获得回合
public class SCRIPT_9178 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;

        // 计算扣蓝伤害
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCMapping mpAtkData = new LPCMapping();
        mpAtkData.Add("mp_dmg_rate", hitScriptArg[0].AsInt + skillEffect);
        mpAtkData.Add("mp_dmg", hitScriptArg[1].AsInt);
        mpAtkData.Add("accuracy_rate", sourceOb.QueryAttrib("accuracy_rate"));
        mpAtkData.Add("resist_rate", targetOb.QueryAttrib("resist_rate"));
        mpAtkData.Add("restrain", restrain);
        int mpDmg = CALC_MP_DAMAGE.Call(mpAtkData, targetOb);
        damageType = mpDmg > 0 ? damageType | CombatConst.DAMAGE_TYPE_MP : damageType;
        mpAtkData.Add("damage_type", damageType);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        // 扣除最大不会超过当前拥有蓝量
        points.Add("mp", Math.Min(mpDmg, targetOb.Query<int>("mp")));
        damageMap.Add("points", points);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 检查目标蓝量立即获得回合
        if (targetOb.Query<int>("mp") < hitScriptArg[2].AsInt)
            RoundCombatMgr.DoAdditionalRound(sourceOb, LPCMapping.Empty);

        return damageMap;
    }
}

// 扣蓝伤害计算脚本，结束时判断目标蓝量触发二段技能
public class SCRIPT_9179 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;

        // 计算扣蓝伤害
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCMapping mpAtkData = new LPCMapping();
        mpAtkData.Add("mp_dmg_rate", hitScriptArg[0].AsInt + skillEffect);
        mpAtkData.Add("mp_dmg", hitScriptArg[1].AsInt);
        mpAtkData.Add("accuracy_rate", sourceOb.QueryAttrib("accuracy_rate"));
        mpAtkData.Add("resist_rate", targetOb.QueryAttrib("resist_rate"));
        mpAtkData.Add("restrain", restrain);
        int mpDmg = CALC_MP_DAMAGE.Call(mpAtkData, targetOb);
        damageType = mpDmg > 0 ? damageType | CombatConst.DAMAGE_TYPE_MP : damageType;
        mpAtkData.Add("damage_type", damageType);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        // 扣除最大不会超过当前拥有蓝量
        points.Add("mp", Math.Min(mpDmg, targetOb.Query<int>("mp")));
        damageMap.Add("points", points);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 检查目标蓝量触发二段技能
        LPCMapping infectArgs = LPCMapping.Empty;
        infectArgs.Add("original_cookie", Game.NewCookie(targetOb.GetRid()));
        if (targetOb.Query<int>("mp") < hitScriptArg[2].AsInt)
            CombatMgr.DoCastSkill(sourceOb, targetOb, hitScriptArg[3].AsInt, infectArgs);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，无视减伤，无视无敌)
public class SCRIPT_9180 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        int targetHp = targetOb.Query<int>("hp");
        sourceProfile.Add("hp_value_compare", targetHp - sourceOb.Query<int>("hp"));

        // 添加无视减伤属性的属性
        sourceProfile.Add("ignore_reduce_dmg", 1000);

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);
        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());
        // 添加无视无敌字段
        damageMap.Add("ignore_imo", 1);

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用伤害计算脚本(纯攻击力计算，无视格挡，对已经处于敏捷降低状态的情况下，再额外附加击晕状态 1 回合)
public class SCRIPT_9181 : Script
{
    private static List<string> statusList = new List<string>(){ "D_AGI_DOWN_1", "D_AGI_DOWN_2", "D_AGI_DOWN_3" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int atkRate = hitScriptArg[0].AsInt;

        // 增加无视格挡概率
        sourceProfile.Add("rate_ignore_block",hitScriptArg[1].AsInt);

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 检查目标身上的状态
        if (targetOb.CheckStatus(statusList))
        {
            // 上击晕状态 1 回合
            LPCMapping condition = new LPCMapping();
            condition.Add("round", 1);
            if (actionArgs.ContainsKey("original_cookie"))
                condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            condition.Add("source_profile", sourceProfile);
            // 如果有无视免疫
            if (sourceOb.QueryAttrib("status_no_immu") > 0)
                condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
            // 附加状态
            targetOb.ApplyStatus("D_STUN", condition);
        }

        return damageMap;
    }
}

// 特殊伤害计算脚本(攻击力+自身生命值上限来计算伤害，暗南瓜专用)
public class SCRIPT_9182 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        //CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能攻击力加成效果、获取损失生命计算伤害的系数
        LPCArray skillAttrib = (_params[6] as LPCValue).AsArray;
        int atkRate = skillAttrib[0].AsInt;
        sourceProfile.Add("max_hp_damage_co", skillAttrib[1].AsInt);
        sourceProfile.Add("hp_value_compare", targetOb.Query<int>("hp") - sourceOb.Query<int>("hp"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int hp = CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        // 检查增伤被动是否起效
        if (sourceOb.QueryAttrib("single_more_dmg") > 0)
        {
            List<Property> numList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
            if (numList.Count == 1)
                hp += hp * sourceOb.QueryAttrib("single_more_mp_cure") / 1000;
        }
        points.Add("hp", hp);
        damageMap.Add("points", points);
        // 记录克制关系
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 伤害计算脚本(攻击力 + 根据自身防御计算，附带暴击率)
public class SCRIPT_9183 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        sourceProfile.Add("sp_crt_rate", hitScriptArg[2].AsInt);

        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 尘取鬼厌恶肮脏伤害计算脚本(攻击力 + 根据自身防御计算，带减益增幅，立即获得回合)
public class SCRIPT_9184 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int atkRate = hitScriptArg[0].AsInt;
        sourceProfile.Add("defense", sourceOb.QueryAttrib("defense"));
        sourceProfile.Add("defense_damage_co", hitScriptArg[1].AsInt);
        sourceProfile.Add("skill_id", skillId);

        // 检查减益增幅
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        int debuffNum = 0;
        for (int i = 0; i < allStatus.Count; i++)
        {
            CsvRow info;
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
            if (info.Query<int>("status_type") == StatusConst.TYPE_DEBUFF)
                debuffNum += 1;
        }
        if (debuffNum > 0)
        {
            // 伤害增强添加
            sourceProfile.Add("damage_up_by_debuff", hitScriptArg[2].AsInt);
            // 增加立即获得回合
            RoundCombatMgr.DoAdditionalRound(sourceOb, LPCMapping.Empty);
        }

        // finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();

        points.Add("hp", CALC_ALL_DAMAGE_ATTACK_DEFNESE.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType));
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 根据自身敏捷进行增幅伤害计算脚本(纯攻击力计算，例如车轮怪高速压制被动的影响)
public class SCRIPT_9185 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        int targetHp = targetOb.Query<int>("hp");
        sourceProfile.Add("hp_value_compare", targetHp - sourceOb.Query<int>("hp"));

        // 获取技能初始攻击力加成效果
        int atkRate = (_params[6] as LPCValue).AsInt;

        // 获取回合增量攻击效果
        atkRate += actionArgs.GetValue<int>("rounds") * sourceOb.QueryAttrib("passive_atk_rate");

        // 目标finalAttrib格式转换
        LPCMapping finalAttribMap = targetOb.GetFinalAttrib();

        // 获取sourceImprovement
        //LPCMapping sourceImprovement = sourceOb.QueryTemp<LPCMapping>("improvement");

        // 目标improvement格式转换
        LPCMapping improvementMap = targetOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        sourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）
        int damageType = CALC_SKILL_DAMAGE_TYPE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        int dmg = CALC_ALL_DAMAGE_ONLY_ATTACK.Call(atkRate, finalAttribMap, improvementMap, sourceProfile, damageType);
        // 计算技能当中的属性带来的增幅
        dmg += dmg * sourceOb.QueryAttrib("agility_to_dmg_up") * sourceOb.QueryAttrib("agility") / 100000;

        points.Add("hp", dmg);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", targetOb.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        // 通知玩家受创
        (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        return damageMap;
    }
}

// 通用生命恢复计算脚本（根据目标生命值的百分比进行恢复）
public class SCRIPT_9200 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int cureHpRate = hitScriptArg[0].AsInt;

        // 获取技能升级效果
        int cureRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        // 获取恢复百分比和最大血量
        // 根据自身、目标选取标识，采用maxHp
        int maxHp = targetOb.QueryAttrib("max_hp");

        if (hitScriptArg[1].AsInt == 1)
            maxHp = sourceOb.QueryAttrib("max_hp");

        int rate = cureHpRate * (1000 + cureRate);

        // 计算恢复血量
        int hpCure = Game.Multiple(maxHp, rate, 1000000);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 通用能量恢复计算脚本（固定恢复值）
public class SCRIPT_9201 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE") || targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        int oriSkillId = SkillMgr.GetOriginalSkillId(skillId);
        int skillLevel = sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId));

        // 检查成功率
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(oriSkillId, skillLevel, SkillType.SE_RATE);

        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt + upRate)
            return false;

        int baseMpCure = hitArgs[1].AsInt;

        // 获取技能升级效果
        int mpCure = CALC_SKILL_UPGRADE_EFFECT.Call(oriSkillId, skillLevel, SkillType.SE_MP_CURE);

        // 参数配置为0时则回复全部能量
        if (baseMpCure == 0)
            mpCure = targetOb.QueryAttrib("max_mp") - targetOb.Query<int>("mp");

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", baseMpCure + mpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 水月之构专用生命恢复计算脚本（过量治疗转化为生命护盾）
public class SCRIPT_9202 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;
        int cureHpRate = hitArgs[0].AsInt;

        // 获取技能升级效果
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        // 计算恢复血量
        int hpCure = sourceOb.Query<int>("level") * cureHpRate;
        hpCure += Game.Multiple(hpCure, skillUpgrade, 1000);
        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        // 获取治疗溢出部分
        int lackHp = targetOb.QueryAttrib("max_hp") - targetOb.Query<int>("hp");
        int overCure = Math.Max((hpCure - lackHp), 0);

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 检查过量治疗，如果溢出转化为护盾
        if (overCure > 0)
        {
            // 上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", hitArgs[1].AsInt);
            if (actionArgs.ContainsKey("original_cookie"))
                condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            condition.Add("can_absorb_damage", overCure);
            condition.Add("source_profile", sourceProfile);

            // 附加状态
            targetOb.ApplyStatus("B_HP_SHIELD", condition);
        }

        return true;
    }
}

// 通用生命恢复计算脚本（根据造成的伤害量的百分比进行恢复）（废弃）
public class SCRIPT_9203 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        LPCMapping ridArgs = new LPCMapping();

        foreach (LPCValue data in indexArgs.Values)
        {
            ridArgs = data.AsMapping;
            break;
        }

        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取上一个hit节点传递下来的伤害信息
        int lastDmg = ridArgs.GetValue<LPCMapping>("points").GetValue<int>("hp");

        // 获取技能升级效果
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        cureHpRate += cureHpRate * skillUpgrade / 1000;

        int hpCure = cureHpRate * lastDmg / 1000;

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 通用复活计算脚本
public class SCRIPT_9204 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int cureRate = hitScriptArg[0].AsInt;

        // 检查是否有无法复活信息
        if (targetOb.CheckStatus(statusList) && targetOb.QueryAttrib("no_reborn_immu") == 0)
            return false;

        // 清除死亡状态
        targetOb.ClearStatus("DIED");

        // 计算重生血量
        int rebornHp = 0;
        if (hitScriptArg[1].AsInt == 1)
        {
            rebornHp = Game.Multiple(sourceOb.QueryAttrib("max_hp"), cureRate);
            rebornHp = Math.Min(rebornHp, targetOb.QueryAttrib("max_hp"));
        }
        else
        {
            rebornHp = Game.Multiple(targetOb.QueryAttrib("max_hp"), cureRate);
        }

        if (rebornHp == 0)
            rebornHp = 1;

        // 设置血量、最大值修正
        targetOb.Set("hp", LPCValue.Create(rebornHp));

        // 播放复活光效
        targetOb.Actor.DoActionSet("reborn", Game.NewCookie(targetOb.GetRid()), LPCMapping.Empty);

        return true;
    }
}

// 通用生命恢复计算脚本（根据自身防御的百分比进行恢复）
public class SCRIPT_9205 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取技能升级效果
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        cureHpRate += cureHpRate * skillUpgrade / 1000;

        int hpCure = cureHpRate * sourceOb.QueryAttrib("defense") / 1000;

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 死亡庇护专用复活/上重生状态脚本
public class SCRIPT_9206 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 检查是否有无法复活信息
        if (targetOb.CheckStatus(statusList) && targetOb.QueryAttrib("no_reborn_immu") == 0)
            return false;

        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        if (targetOb.CheckStatus("DIED"))
        {
            // 清除死亡状态
            targetOb.ClearStatus("DIED");

            targetOb.Set("hp", LPCValue.Create(Math.Min(targetOb.QueryAttrib("max_hp") * hitScriptArg[0].AsInt / 1000, targetOb.QueryAttrib("max_hp"))));

            // 播放复活光效
            targetOb.Actor.DoActionSet("reborn", Game.NewCookie(targetOb.GetRid()), LPCMapping.Empty);
        }
        else
        {
            //上重生状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", hitScriptArg[1].AsInt);
            //condition.Add("skill_id",skillId);

            // 附加状态
            targetOb.ApplyStatus("B_REBORN_STATUS", condition);
        }

        return true;
    }
}

// 前置生命恢复收集信息脚本（百分比平均分配全队存活生命值收集信息）
public class SCRIPT_9207 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        //Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 收集一遍目标
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property liveOb in targetList)
        {
            if (!liveOb.CheckStatus("DIED"))
                finalList.Add(liveOb);
        }
        // 获取剩余生命百分比并平均之
        LPCMapping cureMap = new LPCMapping();
        int sumPercent = 0;

        if (finalList.Count == 0)
            cureMap.Add("average_percent", 0);
        else
        {
            for (int i = 0; i < finalList.Count; i++)
                sumPercent += Game.Divided(finalList[i].Query<int>("hp"), finalList[i].QueryAttrib("max_hp"));

            cureMap.Add("average_percent", sumPercent / finalList.Count);
        }

        return cureMap;
    }
}

// 通用技能CD减少计算脚本
public class SCRIPT_9208 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        CsvRow skillInfo = _params[5] as CsvRow;
        LPCValue hitArgs = _params[6] as LPCValue;

        LPCArray skills = targetOb.GetAllSkills();
        int cdTag = 0;
        foreach (LPCValue mks in skills.Values)
        {
            // 判断技能id是否一致
            int id = mks.AsArray[0].AsInt;
            // 不受技能cd减少影响的检查
            if (IS_CD_REDUCE_IMMU_CHECK.Call(id))
                continue;
            CsvRow tarSkillInfo = SkillMgr.GetSkillInfo(id);

            if (tarSkillInfo.Query<int>("group") == skillInfo.Query<int>("group"))
                continue;

            // 减技能cd
            CdMgr.DoReduceSkillCd(targetOb, id, hitArgs.AsInt);
            cdTag += 1;
        }

        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
            BloodTipMgr.AddTip(TipsWndType.CureTip, targetOb, tips);
        }

        return true;
    }
}

// 通用技能CD全清计算脚本
public class SCRIPT_9209 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        //LPCValue hitArgs = _params[6] as LPCValue;

        LPCArray skills = targetOb.GetAllSkills();
        int cdTag = 0;
        foreach (LPCValue mks in skills.Values)
        {
            // 技能id不一致
            int id = mks.AsArray[0].AsInt;
            // 不受技能cd减少影响的检查
            if (IS_CD_REDUCE_IMMU_CHECK.Call(id))
                continue;
            if (skillId == id)
                continue;

            // 特殊技能不清除
            if (id == 207 || id == 161 || id == 168 || id == 170)
                continue;

            // 判断技能是否在cd中
            if (CdMgr.GetSkillCd(targetOb, id) == 0)
                continue;

            int currentCd = CdMgr.GetSkillCdRemainRounds(targetOb, id);
            CdMgr.DoReduceSkillCd(targetOb, id, currentCd);
            cdTag += 1;
        }

        targetOb.Actor.DoActionSet("cd_down", Game.NewCookie(targetOb.GetRid()), LPCMapping.Empty);

        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
            BloodTipMgr.AddTip(TipsWndType.CureTip, targetOb, tips);
        }

        return true;
    }
}

// 后置生命恢复计算脚本（百分比平均分配全队存活生命值）
public class SCRIPT_9210 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);

        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetOb.GetRid());

        targetOb.Set("hp", LPCValue.Create(Math.Min(targetOb.QueryAttrib("max_hp") * ridArgs.GetValue<int>("average_percent") / 1000, targetOb.QueryAttrib("max_hp"))));

        return true;
    }
}

// 对目标暴击时友方所有单位能量恢复脚本(存在单hit依赖情况)
public class SCRIPT_9211 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        int hitScriptArg = (_params[6] as LPCValue).AsInt;

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE"))
            return false;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        int damgeType = ridArgs.GetValue<int>("damage_type");

        // 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 能量恢复
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", hitScriptArg);
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        foreach (Property cureOb in targetList)
        {
            if (cureOb.CheckStatus("DIED") || targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;
            // 执行回能操作
            (cureOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        return true;
    }
}

// 对目标暴击时自身能量恢复脚本(存在单hit依赖情况)
public class SCRIPT_9212 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        int hitScriptArg = (_params[6] as LPCValue).AsInt;

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE"))
            return false;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        int damgeType = ridArgs.GetValue<int>("damage_type");

        // 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 能量恢复
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", hitScriptArg);

        // 执行回能操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 对目标暴击时自身技能CD冷却减少脚本(存在单hit依赖情况)
public class SCRIPT_9213 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        //LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        int damgeType = ridArgs.GetValue<int>("damage_type");

        // 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 减少冷却
        CdMgr.DoReduceSkillCd(sourceOb, hitScriptArg[0].AsInt, hitScriptArg[1].AsInt);
        string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
        BloodTipMgr.AddTip(TipsWndType.CureTip, sourceOb, tips);

        return true;
    }
}

// 牺牲的生命恢复计算脚本（根据目标生命值的百分比进行恢复，不受禁止治疗影响）
public class SCRIPT_9214 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取技能升级效果
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        cureHpRate += cureHpRate * skillUpgrade / 1000;

        int hpCure = cureHpRate * targetOb.QueryAttrib("max_hp") / 1000;

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        // 自杀
        sourceOb.Set("hp", LPCValue.Create(0));
        TRY_DO_DIE.Call(sourceOb);

        return true;
    }
}

// 通用生命恢复计算脚本（根据己方存活单位数量）
public class SCRIPT_9215 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;

        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property ob in targetList)
        {
            if (!ob.CheckStatus("DIED"))
                finalList.Add(ob);
        }

        cureHpRate = cureHpRate * finalList.Count / targetList.Count;

        // 获取技能升级效果
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        cureHpRate += cureHpRate * skillUpgrade / 1000;

        int hpCure = sourceOb.QueryAttrib("max_hp") * cureHpRate / 1000;

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 计算超重加速技能控制目标出手顺序
public class SCRIPT_9216 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;

        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int curLevel = sourceOb.GetSkillLevel(skillId);
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 判断本回合是否已经出手，如果未出手则提升出手顺序
        if (RoundCombatMgr.CurRoundIsValid(targetOb))
        {
            // 提升目标单位至优先出手
            LPCArray fistHandProp = new LPCArray(208, hitScriptArg[0].AsInt);
            LPCArray propArr = new LPCArray(fistHandProp);
            LPCMapping condition = new LPCMapping();
            condition.Add("round", 1);
            condition.Add("source_rid", sourceOb.GetRid());
            condition.Add("props", propArr);

            targetOb.ApplyStatus("B_FIRST_HAND", condition);

            return true;
        }

        // 已出手情况下，则获取技能回能回冷却总效果
        int mpCure = hitScriptArg[1].AsInt + CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), curLevel, SkillType.SE_MP_CURE);
        int cdCure = hitScriptArg[2].AsInt + CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), curLevel, SkillType.SE_CD);

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpCure);
        // 执行回能操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        LPCArray skills = targetOb.GetAllSkills();
        int cdTag = 0;
        foreach (LPCValue mks in skills.Values)
        {
            // 判断技能id是否一致
            int id = mks.AsArray[0].AsInt;
            // 不受技能cd减少影响的检查
            if (IS_CD_REDUCE_IMMU_CHECK.Call(id))
                continue;
            if (skillId == id)
                continue;

            // 执行回冷却操作
            CdMgr.DoReduceSkillCd(targetOb, id, cdCure);
            cdTag += 1;
        }

        // 冷却减少提示
        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
            BloodTipMgr.AddTip(TipsWndType.CureTip, targetOb, tips);
        }

        return true;
    }
}

// 通用生命恢复计算脚本（根据自身生命值的百分比进行恢复）（废弃）
public class SCRIPT_9217 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取技能升级效果
        int cureRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId),
            sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);
        cureHpRate = cureHpRate * (1000 + cureRate);

        // 计算恢复血量
        int hpCure = Game.Multiple(sourceOb.QueryAttrib("max_hp"), cureHpRate, 1000000);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 通用生命恢复计算脚本（根据自身攻击的百分比进行恢复）
public class SCRIPT_9218 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取技能升级效果
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        cureHpRate += cureHpRate * skillUpgrade / 1000;

        int hpCure = cureHpRate * sourceOb.QueryAttrib("attack") / 1000;

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        // 特殊初期调整+400具体稳chenq
        hpCure += 400;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 圣光之力专用暴击时对生命最低的单位进行生命恢复计算脚本（根据自身防御的百分比进行恢复）
public class SCRIPT_9219 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;

        // 收集友军生命比率最低单位,遍历各个角色
        foreach (Property ob in targetList)
        {
            // 角色无法回血
            if (ob.CheckStatus(banStatusList))
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(ob.Query<int>("hp"), ob.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(ob);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(ob);
        }

        if (selectOb.Count == 0)
            return false;

        Property finalOb = selectOb[RandomMgr.GetRandom(selectOb.Count)];

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 获取技能升级效果
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        cureHpRate += cureHpRate * skillUpgrade / 1000;

        int hpCure = cureHpRate * sourceOb.QueryAttrib("defense") / 1000;

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), finalOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 通用复活计算脚本(无视复活限制)
public class SCRIPT_9220 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE",};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        int hitScriptArg = (_params[6] as LPCValue).AsInt;

        // 检查是否有无法复活信息
        if (targetOb.CheckStatus(statusList))
            return false;

        // 清除死亡状态
        targetOb.ClearStatus("DIED");

        int rebornHp = targetOb.QueryAttrib("max_hp") * hitScriptArg / 1000;

        if (rebornHp == 0)
            rebornHp = 1;

        targetOb.Set("hp",LPCValue.Create( Math.Min(rebornHp, targetOb.QueryAttrib("max_hp"))));

        // 播放复活光效
        targetOb.Actor.DoActionSet("reborn", Game.NewCookie(targetOb.GetRid()), LPCMapping.Empty);

        return true;
    }
}

// 通用生命恢复计算脚本（根据目标生命值的百分比进行恢复）（排除自身治疗）（废弃）
public class SCRIPT_9221 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;
        // 排除自身治疗
        if (targetOb.Equals(sourceOb))
            return false;

        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取技能升级效果
        int cureRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId),
            sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);
        cureHpRate = cureHpRate * (1000 + cureRate);

        // 计算恢复血量
        int hpCure = Game.Multiple(targetOb.QueryAttrib("max_hp"), cureHpRate, 1000000);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 对目标暴击时概率能量恢复脚本(存在单hit依赖情况)
public class SCRIPT_9222 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE"))
            return false;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        int damgeType = ridArgs.GetValue<int>("damage_type");

        // 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitScriptArg[0].AsInt + skillEffect))
            return false;

        // 能量恢复
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", hitScriptArg[1].AsInt);

        // 执行回能操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 周天法专用技能效果计算脚本（复制目标和自身的最高生命、最高能量比例给自身）
public class SCRIPT_9223 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        //int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取目标生命和能量比例
        int targetHpPer = 1000 * targetOb.Query<int>("hp") / targetOb.QueryAttrib("max_hp");
        int targetMp = targetOb.Query<int>("mp");
        // 获取自身生命和能量比例
        int selfHpPer = 1000 * sourceOb.Query<int>("hp") / sourceOb.QueryAttrib("max_hp");
        int selfMp = sourceOb.Query<int>("mp");

        // 对比自身和目标最高生命、最高能量比例并复制
        int maxHpPer = Math.Min(Math.Max(targetHpPer,selfHpPer), 1000);
        int maxMp = Math.Max(targetMp, selfMp);

        // 执行复制
        sourceOb.Set("hp", LPCValue.Create(Game.Multiple(sourceOb.QueryAttrib("max_hp"), maxHpPer, 1000)));
        targetOb.Set("hp", LPCValue.Create(Game.Multiple(targetOb.QueryAttrib("max_hp"), maxHpPer, 1000)));
        sourceOb.Set("mp", LPCValue.Create(maxMp));
        targetOb.Set("mp", LPCValue.Create(maxMp));

        // 自身播放光效
        // 播放光效
        string cookie = Game.NewCookie(sourceOb.GetRid());
        //LPCMapping targetSource = targetOb.GetProfile();
        //targetSource.Add("element", targetOb.Query<int>("element"));
        LPCMapping args = new LPCMapping();
        args.Add("actor_name", sourceOb.GetRid());
        args.Add("source_profile", sourceProfile);
        sourceOb.Actor.DoActionSet("magic_copy_target", cookie, args);
        // 构建参数
        LPCMapping roundPara = new LPCMapping();
        roundPara.Add("skill_id", skillId);                    // 释放技能
        roundPara.Add("pick_rid", sourceOb.GetRid());          // 技能拾取目标
        roundPara.Add("cookie", cookie);                       // 技能cookie
        roundPara.Add("rid", sourceOb.GetRid());               // 释放技能角色rid
        roundPara.Append(args);
        RoundCombatMgr.AddRoundAction(cookie, sourceOb, roundPara);

        // 目标播放光效
        //LPCMapping targetSource = targetOb.GetProfile();
        //targetSource.Add("element", targetOb.Query<int>("element"));
        string tarCookie = Game.NewCookie(targetOb.GetRid());
        LPCMapping tarArgs = new LPCMapping();
        tarArgs.Add("actor_name", targetOb.GetRid());
        tarArgs.Add("source_profile", targetOb.GetProfile());
        targetOb.Actor.DoActionSet("magic_copy_target", tarCookie, tarArgs);
        // 构建参数
        LPCMapping tarRoundPara = new LPCMapping();
        tarRoundPara.Add("skill_id", skillId);                    // 释放技能
        tarRoundPara.Add("pick_rid", targetOb.GetRid());          // 技能拾取目标
        tarRoundPara.Add("cookie", tarCookie);                       // 技能cookie
        tarRoundPara.Add("rid", targetOb.GetRid());               // 释放技能角色rid
        tarRoundPara.Append(tarArgs);
        RoundCombatMgr.AddRoundAction(tarCookie, targetOb, tarRoundPara);

        return true;
    }
}

// 通用能量恢复计算脚本（固定恢复值，按权重来）
public class SCRIPT_9224 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE"))
            return false;

        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCMapping hitArgs = (_params[6] as LPCValue).AsMapping;

        // 权重抽取
        List<int> weightList = new List<int>(){ hitArgs.GetValue<int>(1), hitArgs.GetValue<int>(2), hitArgs.GetValue<int>(3) };
        List<int> mpCureList = new List<int>()
            {
                1, 2, 3
            };
        int index = RandomMgr.RandomSelect(weightList);

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpCureList[index]);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 通用生命恢复计算脚本（根据上一个hit点造成的伤害百分比进行恢复，存在hit点依赖）
public class SCRIPT_9225 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        //Property sourceOb = _params[1] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");
        if (!dependHit.IsInt)
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(targetOb.GetRid()));
        foreach (string rid in indexArgs.Keys)
        {
            // 获得对应RID的传递参数信息
            LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(rid);
            LPCMapping cureMap = new LPCMapping();
            LPCMapping points = ridArgs.GetValue<LPCMapping>("points");
            cureMap.Add("hp", points.GetValue<int>("hp"));

            // 执行回血操作
            (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        return true;
    }
}

// 通用生命恢复计算脚本（根据自身生命值的百分比进行恢复）(废弃)
public class SCRIPT_9226 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int cureHpRate = (_params[6] as LPCValue).AsInt;

        // 获取技能升级效果
        int cureRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        // 获取恢复百分比和最大血量
        int maxHp = sourceOb.QueryAttrib("max_hp");
        int rate = cureHpRate * (1000 + cureRate);

        // 计算恢复血量
        int hpCure = Game.Multiple(maxHp, rate, 1000000);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(targetOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}


// 隐藏圣域复活计算脚本
public class SCRIPT_9227 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(targetOb.CampId);
        List<Property> selectOb = new List<Property>();

        // 遍历各个角色
        foreach (Property checkOb in targetList)
        {
            // 角色已经死亡
            if (checkOb.Query<int>("is_main_monster") != 1)
                continue;

            selectOb.Add(checkOb);
        }

        if (selectOb.Count != 0 && selectOb[0].CheckStatus("DIED"))
            return false;

        // 清除死亡状态
        targetOb.ClearStatus("DIED");

        // 记录重生次数
        targetOb.SetTemp("spec_reborn_times",LPCValue.Create(targetOb.QueryTemp<int>("spec_reborn_times") + 1));

        MonsterMgr.InitMonster(targetOb, hitScriptArg[1].AsString);

        // 计算重生血量
        int rebornHp = Game.Multiple(targetOb.QueryAttrib("max_hp"), hitScriptArg[0].AsInt);
        if (rebornHp == 0)
            rebornHp = 1;

        // 设置血量
        targetOb.Set("hp", LPCValue.Create(Math.Min(rebornHp, targetOb.QueryAttrib("max_hp"))));

        // 播放复活光效
        targetOb.Actor.DoActionSet("reborn", Game.NewCookie(targetOb.GetRid()), LPCMapping.Empty);

        return true;
    }
}

// 立即获得回合hit点计算脚本
public class SCRIPT_9228 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        //LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 增加到回合序列当中
        RoundCombatMgr.DoAdditionalRound(targetOb, LPCMapping.Empty);

        // 播放光效
        string cookie = Game.NewCookie(targetOb.GetRid());
        //LPCMapping targetSource = targetOb.GetProfile();
        //targetSource.Add("element", targetOb.Query<int>("element"));
        LPCMapping args = new LPCMapping();
        args.Add("actor_name", targetOb.GetRid());
        args.Add("source_profile", sourceProfile);

        // 播放技能序列
        if (targetOb.Actor.DoActionSet("do_addtion_round", cookie, args))
        {
            // 播放动作
            // 构建参数
            LPCMapping roundPara = new LPCMapping();
            roundPara.Add("skill_id", skillId);                    // 释放技能
            roundPara.Add("pick_rid", targetOb.GetRid());          // 技能拾取目标
            roundPara.Add("cookie", cookie);                       // 技能cookie
            roundPara.Add("rid", targetOb.GetRid());               // 释放技能角色rid
            roundPara.Append(args);
            RoundCombatMgr.AddRoundAction(cookie, targetOb, roundPara);
        }

        return true;
    }
}

// 输血治疗专用，消耗自身当前生命百分比，并治疗目标对应数值生命，并上优先出手状态
public class SCRIPT_9229 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        int hitScriptArg = (_params[6] as LPCValue).AsInt;

        // 消耗自身生命
        int selfHpCost = Game.Multiple(sourceOb.Query<int>("hp"), hitScriptArg, 1000);
        if (selfHpCost == 0)
            return false;
        sourceOb.Set("hp", LPCValue.Create(Math.Max(sourceOb.Query<int>("hp") - selfHpCost, 0)));

        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        // 治疗目标，计算额外影响
        selfHpCost = CALC_EXTRE_CURE.Call(selfHpCost, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", selfHpCost);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 自杀脚本
public class SCRIPT_9230 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        //Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果
        //LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 模型隐藏（自杀）
        CombatActor actor = sourceOb.Actor;
        actor.SetTweenAlpha(0f);
        // 自杀，需要走触发流程
        sourceOb.Set("hp", LPCValue.Create(0));
        TRY_DO_DIE.Call(sourceOb);
        // 自杀，不走触发流程
        //sourceOb.ApplyStatus("DIED", CALC_DIE_CONDITION.Call(sourceOb));

        return true;
    }
}

// 战力转移专用能量恢复计算脚本
public class SCRIPT_9231 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        Property sourceOb = _params[1] as Property;

        int mpCure = sourceOb.Query<int>("mp");

        // 扣除自身蓝
        sourceOb.Set("mp", LPCValue.Create(0));

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE") || targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
            return false;

        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        //LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpCure);

        // 执行回能操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 魔界迷雾专用复活计算脚本
public class SCRIPT_9232 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        int hitScriptArg = (_params[6] as LPCValue).AsInt;

        // 检查是否有无法复活信息
        if (targetOb.CheckStatus(statusList) && targetOb.QueryAttrib("no_reborn_immu") == 0)
            return false;

        List<Property> dieList = new List<Property>();

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(targetOb.CampId);

        // 筛选出死亡对象
        foreach (Property item in allPetList)
        {
            if (item.CheckStatus("DIED"))
                dieList.Add(item);
        }

        if (dieList.Count == 0)
            return false;

        // 清除死亡状态
        targetOb.ClearStatus("DIED");

        // 计算重生血量
        int rebornHp = Game.Multiple(targetOb.QueryAttrib("max_hp"), hitScriptArg);
        if (rebornHp == 0)
            rebornHp = 1;

        // 设置血量
        targetOb.Set("hp", LPCValue.Create(Math.Min(rebornHp, targetOb.QueryAttrib("max_hp"))));

        // 播放复活光效
        targetOb.Actor.DoActionSet("reborn", Game.NewCookie(targetOb.GetRid()), LPCMapping.Empty);

        return true;
    }
}

// 竭力再战复活计算脚本（复活上状态）
public class SCRIPT_9233 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int rebornRate = hitScriptArg[0].AsInt;

        // 检查是否有无法复活信息
        if (targetOb.CheckStatus(statusList) && targetOb.QueryAttrib("no_reborn_immu") == 0)
            return false;

        // 清除死亡状态
        targetOb.ClearStatus("DIED");

        // 计算重生血量
        int rebornHp = Game.Multiple(targetOb.QueryAttrib("max_hp"), rebornRate);
        if (rebornHp == 0)
            rebornHp = 1;

        // 设置血量
        targetOb.Set("hp", LPCValue.Create(Math.Min(rebornHp, targetOb.QueryAttrib("max_hp"))));

        // 播放复活光效
        targetOb.Actor.DoActionSet("reborn", Game.NewCookie(targetOb.GetRid()), LPCMapping.Empty);

        // 上状态
        LPCMapping condition = new LPCMapping();

        if (hitScriptArg[2].AsInt != 0)
            condition.Add("round", hitScriptArg[2].AsInt);

        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        condition.Add("skill_id", skillId);

        // 附加状态
        targetOb.ApplyStatus(hitScriptArg[1].AsString, condition);

        return true;
    }
}

// 特殊能量恢复计算脚本（固定恢复值，恢复满了以后触发立即获得回合）
public class SCRIPT_9234 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE") || targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        int oriSkillId = SkillMgr.GetOriginalSkillId(skillId);
        int skillLevel = sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId));

        // 检查成功率
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(oriSkillId, skillLevel, SkillType.SE_RATE);

        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt + upRate)
            return false;

        int baseMpCure = hitArgs[1].AsInt;

        // 获取技能升级效果
        int mpCure = CALC_SKILL_UPGRADE_EFFECT.Call(oriSkillId, skillLevel, SkillType.SE_MP_CURE);

        // 参数配置为0时则回复全部能量
        if (baseMpCure == 0)
            mpCure = targetOb.QueryAttrib("max_mp") - targetOb.Query<int>("mp");

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", baseMpCure + mpCure);

        // 执行回能操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 如果能量满，则加入立即获得回合序列
        if (targetOb.Query<int>("mp") == targetOb.QueryAttrib("max_mp") && !string.Equals(actionArgs.GetValue<string>("original_cookie"), targetOb.QueryTemp<string>("add_cookie")))
        {
            RoundCombatMgr.DoAdditionalRound(targetOb, LPCMapping.Empty);
            targetOb.SetTemp("add_cookie", LPCValue.Create(actionArgs.GetValue<string>("original_cookie")));
        }

        return true;
    }
}

// 通用生命恢复计算脚本（根据目标已损失生命值的百分比进行恢复）
public class SCRIPT_9235 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int cureHpRate = hitScriptArg[0].AsInt;

        // 获取技能升级效果
        int cureRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        // 获取恢复百分比和最大血量
        // 根据自身、目标选取标识，采用maxHp
        int maxHp = targetOb.QueryAttrib("max_hp") - targetOb.Query<int>("hp");

        if (hitScriptArg[1].AsInt == 1)
            maxHp = sourceOb.QueryAttrib("max_hp") - sourceOb.Query<int>("hp");

        int rate = cureHpRate * (1000 + cureRate);

        // 计算恢复血量
        int hpCure = Game.Multiple(maxHp, rate, 1000000);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 通用生命恢复计算脚本（存在hit依赖情况，根据目标生命值的百分比，根据清除的己方所有人的状态数量进行恢复）
public class SCRIPT_9236 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        // 检查禁止治疗
        if (targetOb.CheckStatus(banStatusList))
            return false;

        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);

        // 根据己方所有人的rid，获取对应的传递信息
        List<Property> sourceCampList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<string> finalList = new List<string>();

        // 筛选非死亡单位
        foreach (Property checkOb in sourceCampList)
        {
            if (!checkOb.CheckStatus("DIED"))
                finalList.Add(checkOb.GetRid());
        }

        // 获取总的清除的状态数量
        int clearNum = 0;
        foreach (string checkRid in finalList)
        {
            LPCMapping clearMap = new LPCMapping();
            clearMap = indexArgs.GetValue<LPCMapping>(checkRid);
            clearNum += clearMap.GetValue<int>("clear_num");
        }

        int cureHpRate = hitScriptArg[0].AsInt;

        // 获取技能升级效果
        int cureRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        // 获取恢复百分比和最大血量
        // 根据自身、目标选取标识，采用maxHp
        int maxHp = targetOb.QueryAttrib("max_hp");

        if (hitScriptArg[1].AsInt == 1)
            maxHp = sourceOb.QueryAttrib("max_hp");

        // 提升的治疗量计算为目标A%生命×（1+全场总净化dbuff数量*B%+技能等级提升治疗量）
        int rate = cureHpRate * (1000 + clearNum * hitScriptArg[2].AsInt + cureRate);

        // 计算恢复血量
        int hpCure = Game.Multiple(maxHp, rate, 1000000);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 通用召唤类计算脚本
public class SCRIPT_9237 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCArray summonArgs = (_params[6] as LPCValue).AsArray;

        LPCArray summonList = MonsterMgr.GetRelatePetList(sourceOb.GetClassID());

        LPCMapping summonInfo = new LPCMapping();
        summonInfo.Add("init_rule", summonArgs[1].AsString);
        // 添加召唤属性百分比参数
        summonInfo.Add("summon_rate", summonArgs[0].AsInt);
        int summonUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SUMMON);
        summonInfo.Add("summon_rate_upgrade", summonUpgrade);
        summonInfo.Add("rank", summonArgs[2].AsInt);

        // 创建召唤实体
        CombatSummonMgr.SummonEntity(sourceOb, summonList[0].AsInt, targetOb.CampId, targetOb.SceneId, targetOb.FormationId,targetOb.FormationPos, targetOb.FormationRaw, summonInfo);

        return true;
    }
}

// 对目标上某种状态成功时自身能量恢复脚本(存在单hit依赖情况)
public class SCRIPT_9238 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        int hitScriptArg = (_params[6] as LPCValue).AsInt;

        // 如果有禁止能量恢复状态，则跳过
        if (targetOb.CheckStatus("D_NO_MP_CURE"))
            return false;

        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        if (ridArgs == null)
            return false;

        int applyTag = ridArgs.GetValue<int>("apply_tag");

        // 检查上一个hit点的状态成功情况
        if (applyTag != 1)
            return false;

        // 能量恢复
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", hitScriptArg);

        // 执行回能操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 竖琴专用技能CD减少计算脚本
public class SCRIPT_9239 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        LPCArray skills = targetOb.GetAllSkills();
        int cdTag = 0;
        foreach (LPCValue mks in skills.Values)
        {
            // 判断技能id是否一致
            int id = mks.AsArray[0].AsInt;
            // 不受技能cd减少影响的检查
            if (IS_CD_REDUCE_IMMU_CHECK.Call(id))
                continue;
            CsvRow tarSkillInfo = SkillMgr.GetSkillInfo(id);

            if (tarSkillInfo.Query<int>("group") == skillInfo.Query<int>("group"))
                continue;

            // 减技能cd
            CdMgr.DoReduceSkillCd(targetOb, id, hitArgs[0].AsInt);
            cdTag += 1;
        }

        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
            BloodTipMgr.AddTip(TipsWndType.CureTip, targetOb, tips);
        }

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", hitArgs[1].AsInt);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return true;
    }
}

// 永生奥秘蝙蝠替身计算脚本
public class SCRIPT_9240 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCArray summonArgs = (_params[6] as LPCValue).AsArray;

        LPCArray summonList = MonsterMgr.GetRelatePetList(sourceOb.GetClassID());

        LPCMapping summonInfo = new LPCMapping();
        summonInfo.Add("init_rule", summonArgs[1].AsString);
        // 添加召唤属性百分比参数
        summonInfo.Add("summon_rate", summonArgs[0].AsInt);
        int summonUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SUMMON);
        summonInfo.Add("summon_rate_upgrade", summonUpgrade);
        summonInfo.Add("rank", sourceOb.Query<int>("rank"));
        summonInfo.Add("is_standin", 1);

        // 创建召唤实体
        Property standOb = CombatSummonMgr.SummonEntity(sourceOb, summonList[0].AsInt, targetOb.CampId, targetOb.SceneId, targetOb.FormationId,targetOb.FormationPos, targetOb.FormationRaw, summonInfo);

        // 初始化技能等级
        LPCArray initSkills = standOb.GetAllSkills();
        LPCArray finalNewSkills = new LPCArray();
        for (int i = 0; i < initSkills.Count; i++)
        {
            LPCArray batSkills = initSkills[i].AsArray;
            int batSkillId = batSkills[0].AsInt;
            int batSkillLv = 0;
            CsvRow batSkillInfo = SkillMgr.GetSkillInfo(batSkillId);

            // 继承普攻技能等级
            if (batSkillInfo.Query<int>("skill_family") == SkillType.SKILL_NORMAL)
                batSkillLv = sourceOb.GetSkillLevel(summonArgs[2].AsInt);
            else
                batSkillLv = batSkills[1].AsInt;

            finalNewSkills.Add(new LPCArray(batSkillId, batSkillLv));
        }

        standOb.Set("skills",LPCValue.Create(finalNewSkills));

        // 给被替身者上被替身状态
        LPCMapping condition = new LPCMapping ();
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));

        condition.Add("source_profile", sourceProfile);
        sourceOb.ApplyStatus("B_STANDED",condition);

        LPCMapping standMap = new LPCMapping();
        standMap.Add("stand_rid", standOb.GetRid());

        return standMap;
    }
}

// 判断是否有控制状态，如果有控制状态则立即获得回合hit点计算脚本
public class SCRIPT_9241 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        //LPCArray hitScriptArg = (_params[6] as LPCValue).AsArray;
        int rate = (_params[6] as LPCValue).AsInt;

        // 检查概率
        if (RandomMgr.GetRandom() >= rate)
            return false;

        // 检查敌人是否存在控制状态
        List <LPCMapping> allStatus = targetOb.GetAllStatus();
        LPCArray ctrlNumList = LPCArray.Empty;
        for (int i = 0; i < allStatus.Count; i++)
        {
            CsvRow statusInfo;
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
            LPCMapping ctrlMap = statusInfo.Query<LPCMapping>("limit_round_args");
            // 如果是控制状态，则清除之
            if (ctrlMap.ContainsKey("ctrl_id"))
                ctrlNumList.Add(allStatus[i].GetValue<int>("status_id"));
        }

        // 如果有控制状态，则增加到回合序列当中
        if (ctrlNumList.Count > 0)
            RoundCombatMgr.DoAdditionalRound(sourceOb, LPCMapping.Empty);

        return true;
    }
}

// 通用上状态脚本百分百成功(无hit依赖情况)
public class SCRIPT_9300 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();

        if (hitArgs[1].AsInt != 0)
            condition.Add("round", hitArgs[1].AsInt);

        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));

        condition.Add("source_profile", sourceProfile);
        condition.Add("skill_id", skillId);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 通用解除状态脚本（解除己方所有减益状态）
public class SCRIPT_9301 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获取状态参数(状态类型)
        LPCValue hitArgs = _params[6] as LPCValue;

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return false;

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> removeList = new List<int>();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != hitArgs.AsInt)
                continue;

            // 清楚状态
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(removeList, StatusConst.CLEAR_TYPE_BREAK);

        LPCMapping clearMap = new LPCMapping();
        clearMap.Add("clear_num", removeList.Count);

        // 如果是减益状态，则播放清除状态光效
        if (hitArgs.AsInt == StatusConst.TYPE_DEBUFF)
        {
            // 添加回合行动列表中
            string cookie = Game.NewCookie(targetOb.GetRid());
            // 播放cure effect
            bool doRet = targetOb.Actor.DoActionSet("CLEAR_DEBUFF", cookie, LPCMapping.Empty);
            // 如果播放成功
            if (doRet)
                RoundCombatMgr.AddRoundAction(cookie, targetOb, LPCMapping.Empty);
        }

        return clearMap;
    }
}

// 通用上生命护盾状态脚本
public class SCRIPT_9302 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_CURSE", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡、诅咒跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 护盾值计算
        int value = 0;

        int influnAttrib = sourceOb.QueryAttrib(hitArgs[0].AsString);

        if (hitArgs[0].AsString.Equals("level"))
        {
            influnAttrib = sourceOb.Query<int>(hitArgs[0].AsString);
            value = influnAttrib * hitArgs[1].AsInt;
        }else
            value = influnAttrib * hitArgs[1].AsInt / 1000;

        // 计算技能升级效果
        value += value * CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SHIELD) / 1000;

        // 计算额外影响
        value = CALC_EXTRE_CURE.Call(value, sourceOb.QueryAttrib("skill_effect") , targetOb.QueryAttrib("reduce_cure"));

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("can_absorb_damage", value);

        // 附加状态
        targetOb.ApplyStatus("B_HP_SHIELD", condition);

        return true;
    }
}

// 通用上生命恢复状态脚本
public class SCRIPT_9303 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_CURSE", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        //int curLevel = sourceOb.GetSkillLevel(skillId);

        // 死亡、诅咒跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 计算升级技能带来的效果
        //int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), curLevel, SkillType.SE_HP_CURE);

        int curHpRate = hitArgs[2].AsInt;

        //curHpRate += curHpRate * skillUpgrade / 1000;

        int value = targetOb.QueryAttrib(hitArgs[1].AsString) * curHpRate / 1000;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[3].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("hp_cure", value);
        condition.Add("source_profile", _params[4] as LPCMapping);
        int healReduceDmg = targetOb.QueryAttrib("heal_reduce_dmg");
        if (healReduceDmg > 0)
            condition.Add("props", new LPCArray(new LPCArray(20, healReduceDmg)));

        // 附加对应生命恢复状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖情况)
public class SCRIPT_9304 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return indexArgs;
    }
}

// 暴击时敌方目标上状态脚本(存在单hit依赖情况)
public class SCRIPT_9305 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 生命链接上状态脚本(无hit依赖、包含状态效果百分比、不计算效果命中)
public class SCRIPT_9306 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 初次检查成功率
        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt)
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[3].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        condition.Add("value_rate", hitArgs[2].AsInt);
        condition.Add("source_rid", sourceOb.Query<string>("rid"));

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return true;
    }
}

// 通用上增益陷阱类状态脚本
public class SCRIPT_9307 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数, 陷阱攻击加成百分比})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        int dmgRate = hitArgs[2].AsInt;

        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        dmgRate += dmgRate * skillUpgrade / 1000;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("trap_source_profile", sourceProfile);
        condition.Add("damage_rate", dmgRate);
        condition.Add("debuff_rate", hitArgs[3].AsInt);
        condition.Add("trap_source_rid", sourceOb.GetRid());
        condition.Add("trap_effect", hitArgs[4].AsString);
        condition.Add("trap_effect_round", hitArgs[5].AsInt);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 通用己方上状态概率成功脚本(不存在单hit依赖情况)
public class SCRIPT_9308 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt)
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();

        if (hitArgs[2].AsInt != 0)
            condition.Add("round", hitArgs[2].AsInt);

        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        LPCArray propValue = sourceOb.QueryTemp<LPCArray>("improvement/skl_katana_prepare_rate");
        condition.Add("props", new LPCArray(new LPCArray(519, propValue[0].AsArray)));
        condition.Add("skill_id", skillId);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return true;
    }
}

// 通用概率上状态脚本(无hit依赖情况，需要计算效果命中)
public class SCRIPT_9309 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        LPCMapping applyMap = new LPCMapping();
        applyMap.Add("apply_tag", 0);
        // 对象为空跳出
        if (targetOb == null)
            return applyMap;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 状态跳出
        if (targetOb.CheckStatus(statusList))
            return applyMap;

        // 播放光效
        string cookie = Game.NewCookie(targetOb.GetRid());
        //LPCMapping targetSource = targetOb.GetProfile();
        //targetSource.Add("element", targetOb.Query<int>("element"));
        LPCMapping args = new LPCMapping();
        args.Add("actor_name", targetOb.GetRid());
        args.Add("source_profile", sourceProfile);

        // 播放技能序列
        if (targetOb.Actor.DoActionSet(hitArgs[3].AsString, cookie, args))
        {
            // 播放动作
            // 构建参数
            LPCMapping roundPara = new LPCMapping();
            roundPara.Add("skill_id", skillId);                    // 释放技能
            roundPara.Add("pick_rid", targetOb.GetRid());          // 技能拾取目标
            roundPara.Add("cookie", cookie);                       // 技能cookie
            roundPara.Add("rid", targetOb.GetRid());               // 释放技能角色rid
            roundPara.Append(args);
            RoundCombatMgr.AddRoundAction(cookie, targetOb, roundPara);
        }

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 2 初次检查成功率
        // 计算升级效果
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return applyMap;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string resTips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, resTips);
            return applyMap;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        int round = hitArgs[2].AsInt;
        if (round > 0)
            condition.Add("round", round);

        // 如果需要根据消耗能量进行计算持续时间
        if (round == -1)
        {
            LPCMapping costMap = actionArgs.GetValue<LPCMapping>("skill_cost");
            condition.Add("round", costMap.GetValue<int>("mp"));
        }

        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_skill_id", skillId);
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        if (targetOb.CheckStatus(hitArgs[1].AsString))
            applyMap.Add("apply_tag", 1);

        return applyMap;
    }
}

// 特殊上护盾脚本，根据属性值以及能量消耗值来计算护盾量
public class SCRIPT_9310 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 状态跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        LPCMapping costMap = actionArgs.GetValue<LPCMapping>("skill_cost");
        int mpCost = costMap.GetValue<int>("mp");
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SHIELD);

        // 护盾值计算( 依据属性值 × 百分比 ×(1 + 当前消耗能量点数 × 对应百分比) )
        int value = sourceOb.QueryAttrib(hitArgs[0].AsString) * hitArgs[1].AsInt / 1000;
        value += value * mpCost * hitArgs[2].AsMapping.GetValue<int>(mpCost) / 1000;
        value += value * skillEffect / 1000;

        // 计算额外影响
        value += value * sourceProfile.GetValue<int>("skill_effect") / 1000;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[3].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("can_absorb_damage", value);

        // 附加状态
        targetOb.ApplyStatus("B_HP_SHIELD", condition);

        return true;
    }
}

// 通用解除状态脚本（解除敌方所有增益状态）
public class SCRIPT_9311 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        // LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数(状态类型)
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        LPCMapping dependMap = new LPCMapping();
        LPCArray removeList = new LPCArray();
        dependMap.Add("remove_list", removeList);

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return dependMap;

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 2 初次检查成功率
        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt)
            return dependMap;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return dependMap;
        }

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != hitArgs[1].AsInt)
                continue;

            // 清楚状态
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(removeList);

        // 如果是减益状态，则播放清除状态光效
        if (hitArgs[1].AsInt == StatusConst.TYPE_DEBUFF)
        {
            // 添加回合行动列表中
            string cookie = Game.NewCookie(targetOb.GetRid());
            // 播放cure effect
            bool doRet = targetOb.Actor.DoActionSet("CLEAR_DEBUFF", cookie, LPCMapping.Empty);
            // 如果播放成功
            if (doRet)
                RoundCombatMgr.AddRoundAction(cookie, targetOb, LPCMapping.Empty);
        }

        // 再次添加清除列表
        dependMap.Add("remove_list", removeList);

        return dependMap;
    }
}

// 通用解除状态脚本（解除敌方随机一个增益状态）
public class SCRIPT_9312 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        // LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数(状态类型)
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return false;

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        if (allStatus.Count == 0)
            return false;

        List<int> removeList = new List<int>();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != hitArgs[1].AsInt)
                continue;

            // 清楚状态
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        if (removeList.Count == 0)
            return false;

        // 随机清除一个状态
        targetOb.ClearStatusByCookie(removeList[RandomMgr.GetRandom(removeList.Count)]);

        return true;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖、无视格挡、计算效果命中)
public class SCRIPT_9313 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 初次检查成功率
        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt)
            return false;

        // 2 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return true;
    }
}

// 特殊上状态脚本(存在单hit依赖情况，根据目标剩余蓝量判断是否需要上状态)
public class SCRIPT_9314 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查目标能量剩余，大于等于目标设定能量必定眩晕（但是继续检查效果命中）
        if (targetOb.Query<int>("mp") < hitArgs[0].AsInt)
            return false;

        // 检查成功率
        if (RandomMgr.GetRandom() >= hitArgs[3].AsInt)
            return false;

        // 2 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 1 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return true;
    }
}

// 霜降专用上状态脚本概率成功(存在单hit依赖情况)
public class SCRIPT_9315 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt)
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 检查是否已有减速状态，如果有清除减速，并上冰冻效果
        if (targetOb.CheckStatus("D_SPD_DOWN"))
        {
            // 清除减速状态
            targetOb.ClearStatus("D_SPD_DOWN");
            // 上冰冻状态
            LPCMapping iceCondition = new LPCMapping();
            iceCondition.Add("round", 1);
            if (actionArgs.ContainsKey("original_cookie"))
                iceCondition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                iceCondition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            iceCondition.Add("source_profile", sourceProfile);
            // 附加状态
            targetOb.ApplyStatus("D_FREEZE", iceCondition);
        }
        else
        {
            // 上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", hitArgs[2].AsInt);
            if (actionArgs.ContainsKey("original_cookie"))
                condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            condition.Add("source_profile", sourceProfile);

            // 附加状态
            targetOb.ApplyStatus(hitArgs[1].AsString, condition);
        }

        return true;
    }
}

// 特殊上状态脚本(存在单hit依赖情况，根据目标剩余蓝量是否大于固定值上状态)
public class SCRIPT_9316 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查目标能量剩余，如果大于设定值，则必定上状态（但是继续检查效果命中）
        if (targetOb.Query<int>("mp") > hitArgs[0].AsInt)
            return false;

        // 2 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 1 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       targetOb.QueryAttrib("resist_rate"),
                       ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return true;
    }
}

// 暴击时自身上状态脚本(存在单hit依赖情况)
public class SCRIPT_9317 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        int damgeType = ridArgs.GetValue<int>("damage_type");

        // 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        sourceOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 暴击时敌方目标上状态脚本(存在单hit依赖情况,检查是否已经存在对应状态效果，如果有，则上状态)
public class SCRIPT_9318 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        string targetRid = targetOb.Query<string>("rid");
        //Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 检查暴击情况
        if ((damgeType & CombatConst.DAMAGE_TYPE_DEADLY) != CombatConst.DAMAGE_TYPE_DEADLY)
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 通用解除状态脚本（必定解除目标所有增益状态）（废弃）
public class SCRIPT_9319 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        // LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return false;

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> removeList = new List<int>();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != StatusConst.TYPE_BUFF)
                continue;

            // 清楚状态
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(removeList);

        return true;
    }
}

// 通用上无回合参数状态（废弃）
public class SCRIPT_9320 : Script
{
    //private static List<string> statusList = new List<string>(){ "DIED", };
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, LPCMapping.Empty);

        return true;
    }
}


// 通用上次数护盾状态脚本
public class SCRIPT_9321 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_CURSE", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡、诅咒跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("can_absorb_times", hitArgs[0].AsInt);

        // 附加状态
        targetOb.ApplyStatus("B_EN_SHIELD", condition);

        return true;
    }
}

// 飞叶草上状态脚本(无hit依赖情况，多次命中上睡眠)
public class SCRIPT_9322 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 获取标识状态
        List<LPCMapping> allSleepSp = targetOb.GetStatusCondition(hitArgs[0].AsString);
        LPCMapping condition = new LPCMapping();
        string originalCookie = actionArgs.GetValue<string>("original_cookie");

        // 遍历全部状态
        foreach (LPCMapping data in allSleepSp)
        {
            // 判断原始original_cookie
            if (! originalCookie.Equals(data.GetValue<string>("original_cookie")))
                continue;

            // 记录数据
            condition = data;
            break;
        }

        // 如果没有该original_cookie带来的状态
        if (condition.Count == 0)
        {
            condition.Add("round", 1);
            if (actionArgs.ContainsKey("original_cookie"))
                condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            condition.Add("original_cookie", actionArgs.GetValue<string>("original_cookie"));
            condition.Add("source_profile", sourceProfile);
            condition.Add("times", 1);

            // 附加状态
            targetOb.ApplyStatus(hitArgs[0].AsString, condition);

            // 返回false
            return false;
        }

        // 设置次数
        int times = condition.GetValue<int>("times") + 1;
        condition.Add("times", times);

        // 不需要附加D_SLEEP
        if (times < 2)
            return false;

        // 上睡眠
        LPCMapping sleepMap = new LPCMapping();
        sleepMap.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            sleepMap.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            sleepMap.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        sleepMap.Add("original_cookie", actionArgs.GetValue<string>("original_cookie"));
        sleepMap.Add("source_profile", sourceProfile);
        targetOb.ApplyStatus("D_SLEEP", sleepMap);

        return true;
    }
}

// 通用解除状态脚本（解除己方随机一个状态）
public class SCRIPT_9323 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        // LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获取状态参数(状态类型)
        int hitArgs = (_params[6] as LPCValue).AsInt;

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return false;

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> removeList = new List<int>();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != hitArgs)
                continue;

            // 清楚状态
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        if (removeList.Count == 0)
            return false;

        // 随机清除一个状态
        targetOb.ClearStatusByCookie(removeList[RandomMgr.GetRandom(removeList.Count)],StatusConst.CLEAR_TYPE_BREAK);

        // 如果是减益状态，则播放清除状态光效
        if (hitArgs == StatusConst.TYPE_DEBUFF)
        {
            // 添加回合行动列表中
            string cookie = Game.NewCookie(targetOb.GetRid());
            // 播放cure effect
            bool doRet = targetOb.Actor.DoActionSet("CLEAR_DEBUFF", cookie, LPCMapping.Empty);
            // 如果播放成功
            if (doRet)
                RoundCombatMgr.AddRoundAction(cookie, targetOb, LPCMapping.Empty);
        }

        return true;
    }
}

// 上特殊陷阱状态脚本（该陷阱的反伤根据自身生命上限越高，伤害越高）
public class SCRIPT_9324 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数, 陷阱攻击加成百分比})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("trap_source_profile", sourceProfile);
        condition.Add("skill_id", skillId);
        condition.Add("damage_rate", hitArgs[2].AsInt);
        condition.Add("max_hp_damage_co", hitArgs[3].AsInt);
        condition.Add("debuff_rate", hitArgs[4].AsInt);
        condition.Add("trap_source_rid", sourceOb.GetRid());
        condition.Add("trap_effect", hitArgs[5].AsString);
        condition.Add("trap_effect_round", hitArgs[6].AsInt);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 2阶段上状态脚本概率成功(存在单hit依赖情况)(防御下降标识)
public class SCRIPT_9325 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;

        // 检查是否2阶段
        if (sourceOb.Query<int>("had_reborn_times") < 1 && sourceOb.QueryAttrib("phase_cast_def_down_id") == 0)
            return false;

        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        // 上技能标识
        BloodTipMgr.AddSkillTip(sourceOb, sourceOb.QueryAttrib("phase_cast_def_down_id"));

        return indexArgs;
    }
}

// 2阶段上状态脚本概率成功(存在单hit依赖情况)(持续伤害标识)
public class SCRIPT_9326 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;

        // 检查是否2阶段
        if (sourceOb.Query<int>("had_reborn_times") < 1 && sourceOb.QueryAttrib("phase_cast_injury_id") == 0)
            return false;

        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        // 上技能标识
        BloodTipMgr.AddSkillTip(sourceOb, sourceOb.QueryAttrib("phase_cast_injury_id"));

        return indexArgs;
    }
}

// 通用偷取状态脚本（解除敌方随机N个增益状态，并偷取对应状态）
public class SCRIPT_9327 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数(状态类型)
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return false;

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 2 初次检查成功率
        if (RandomMgr.GetRandom() >= hitArgs[0].AsInt)
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        if (allStatus.Count == 0)
            return false;

        List<int> removeList = new List<int>();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != hitArgs[1].AsInt)
                continue;

            // 清楚状态
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        if (removeList.Count == 0)
            return false;

        for (int i = 0; i < hitArgs[2].AsInt; i++)
        {
            // 判断列表中是否有可偷取的状态
            if (removeList.Count == 0)
                break;

            // 随机抽取一个状态
            int index = RandomMgr.GetRandom(removeList.Count);
            int removeCookie = removeList[index];

            // 获取对应的状态信息
            LPCMapping stealCondition = LPCValue.Duplicate(targetOb.GetStatusCondition(removeCookie)).AsMapping;
            int statusId = stealCondition.GetValue<int>("status_id");

            // 清除该状态
            targetOb.ClearStatusByCookie(removeCookie);

            // 给自己上状态，加round_cookie保证本回合不生效，这样偷取来的buff可以保留到下回合
            if (actionArgs.ContainsKey("original_cookie"))
                stealCondition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                stealCondition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            sourceOb.ApplyStatus(StatusMgr.GetStatusAlias(statusId), stealCondition);

            // 去掉对应列表中的状态
            removeList.RemoveAt(index);
        }

        return true;
    }
}

// “防御反击”技能上状态脚本(无hit依赖情况)
public class SCRIPT_9328 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 上状态
        // propValue参数：技能ID，触发次数限制
        LPCArray propValue = new LPCArray(hitArgs[1].AsArray[0].AsInt, hitArgs[1].AsArray[2].AsInt);
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[1].AsArray[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("props", new LPCArray(new LPCArray(168,propValue)));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 通用概率改变状态回合脚本(无hit依赖情况，如果是敌方单位需要计算效果命中)
public class SCRIPT_9329 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        //LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 状态跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 播放光效
        string cookie = Game.NewCookie(targetOb.GetRid());
        //LPCMapping targetSource = targetOb.GetProfile();
        //targetSource.Add("element", targetOb.Query<int>("element"));
        LPCMapping args = new LPCMapping();
        args.Add("actor_name", targetOb.GetRid());
        args.Add("source_profile", sourceProfile);
        targetOb.Actor.DoActionSet(hitArgs[3].AsString, cookie, args);
        // 播放动作
        // 构建参数
        LPCMapping roundPara = new LPCMapping();
        roundPara.Add("skill_id", skillId);                    // 释放技能
        roundPara.Add("pick_rid", targetOb.GetRid());          // 技能拾取目标
        roundPara.Add("cookie", cookie);                       // 技能cookie
        roundPara.Add("rid", targetOb.GetRid());               // 释放技能角色rid
        roundPara.Append(args);
        RoundCombatMgr.AddRoundAction(cookie, targetOb, roundPara);

        int sourceCamp = sourceOb.CampId;
        int targetCamp = targetOb.CampId;

        if (sourceCamp != targetCamp)
        {
            // 计算克制关系
            int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
            sourceProfile.Add("restrain", restrain);

            // 2 初次检查成功率
            // 计算升级效果
            int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
            if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
                return false;

            // 3 计算效果命中
            // 攻击者效果命中、防御者效果抵抗、克制关系
            int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                           targetOb.QueryAttrib("resist_rate"),
                           restrain);

            if (RandomMgr.GetRandom() < rate)
            {
                string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
                BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
                return false;
            }
        }

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> changeStatusList = new List<int>();

        // 遍历所有对象身上的状态
        foreach(LPCMapping statusData in allStatus)
        {
            // 获取状态的信息
            CsvRow info = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != hitArgs[1].AsInt)
                continue;

            // 改变状态回合信息
            changeStatusList.Add(statusData.GetValue<int>("cookie"));
        }

        // 增加状态回合数
        targetOb.AddStatusRounds(changeStatusList, hitArgs[2].AsInt);

        return true;
    }
}

// 安魂弥撒专用上状态脚本(无hit依赖情况，状态信息中需要包含skill_id)
public class SCRIPT_9330 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();

        if (hitArgs[1].AsArray[0].AsInt != 0)
            condition.Add("round", hitArgs[1].AsArray[0].AsInt);

        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        condition.Add("skill_id", hitArgs[1].AsArray[1].AsInt);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[0].AsString, condition);

        return true;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖情况，随机状态列表)
public class SCRIPT_9331 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加随机状态
        LPCArray status = new LPCArray ();
        status = hitArgs[1].AsArray;
        targetOb.ApplyStatus(status[RandomMgr.GetRandom(status.Count)].AsString, condition);

        return indexArgs;
    }
}

/// <summary>
/// 通用hit附加状态（废弃）
/// </summary>
public class SCRIPT_9332 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        // 获取状态参数({({状态, 状态持续回合数})})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 附加随机状态
        foreach (LPCValue status in hitArgs.Values)
        {
            // 转换数据格式
            LPCArray data = status.AsArray;

            // 构建参数
            LPCMapping condition = new LPCMapping();
            condition.Add("round", data[1].AsInt);

            // 附加状态
            targetOb.ApplyStatus(data[0].AsString, condition);
        }

        // 返回true
        return true;
    }
}

// 特殊解除状态脚本（解除敌方所有增益状态，根据解除增益状态数量概率消除目标能量）
public class SCRIPT_9333 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        // LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数(状态类型)
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return false;

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        // 2 初次检查成功率
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> removeList = new List<int>();
        CsvRow info;

        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != hitArgs[1].AsInt)
                continue;

            // 清楚状态
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(removeList);

        // 免疫状态下无效
        if (targetOb.CheckStatus("B_DEBUFF_IMMUNE"))
            return true;

        // 获取随机权重
        int randomRate = hitArgs[2].AsInt + skillEffect;

        // 消除能量
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK | CombatConst.DAMAGE_TYPE_MP;

        // 清除状态，概率消除能量
        for (int i = 0; i < removeList.Count; i++)
        {
            // 不满足需求
            if (RandomMgr.GetRandom() >= randomRate)
                continue;

            // 计算伤害结构
            LPCMapping damageMap = new LPCMapping();
            LPCMapping points = new LPCMapping();
            points.Add("mp", hitArgs[3].AsInt);
            damageMap.Add("points", points);
            damageMap.Add("damage_type", damageType);
            damageMap.Add("target_rid", targetOb.GetRid());

            // 通知玩家受创
            (targetOb as Char).ReceiveDamage(sourceProfile, damageType, damageMap);
        }

        // 如果是减益状态，则播放清除状态光效
        if (hitArgs[1].AsInt == StatusConst.TYPE_DEBUFF)
        {
            // 添加回合行动列表中
            string cookie = Game.NewCookie(targetOb.GetRid());

            // 播放cure effect
            bool doRet = targetOb.Actor.DoActionSet("CLEAR_DEBUFF", cookie, LPCMapping.Empty);

            // 如果播放成功
            if (doRet)
                RoundCombatMgr.AddRoundAction(cookie, targetOb, LPCMapping.Empty);
        }

        return true;
    }
}

// 上全体生命链接状态专用
public class SCRIPT_9334 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        LPCArray ridList = new LPCArray();

        // 筛选非死亡单位和目标自身
        foreach (Property checkOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && !Property.Equals(checkOb, targetOb))
                ridList.Add(checkOb.GetRid());
        }

        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        condition.Add("rid_list", ridList);
        condition.Add("value_rate", hitArgs[0].AsInt);
        condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        
        targetOb.ApplyStatus("B_SACRIFICE_ALL", condition);

        return true;
    }
}

// 利欲熏心上状态脚本概率成功(存在单hit依赖情况)
public class SCRIPT_9335 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate && !targetOb.CheckStatus("D_PROVOKE"))
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        int round = hitArgs[2].AsInt;
        if (targetOb.CheckStatus("D_PROVOKE"))
            round = hitArgs[3].AsInt;
        LPCMapping condition = new LPCMapping();
        condition.Add("round", round);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return indexArgs;
    }
}

// 炎龙断斩专用上状态脚本概率成功(存在单hit依赖情况，成功击晕后对敌全体上状态)
public class SCRIPT_9336 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");
        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        // 仍然需要检查状态是否起效，然后开始上第二个全体状态
        if (targetOb.CheckStatus(hitArgs[1].AsString))
        {
            int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

            // 整理筛选结果，将手选结果放在第一个
            List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            foreach (Property checkOb in finalList)
            {
                // 2 初次检查成功率
                if (RandomMgr.GetRandom() >= (hitArgs[3].AsInt))
                    return false;

                // 3 计算效果命中
                // 攻击者效果命中、防御者效果抵抗、克制关系
                int anoRate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                    checkOb.QueryAttrib("resist_rate"),
                    ridArgs.GetValue<int>("restrain"));

                if (RandomMgr.GetRandom() < anoRate)
                {
                    string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
                    BloodTipMgr.AddTip(TipsWndType.DamageTip, checkOb, tips);
                    return false;
                }

                // 上状态
                LPCMapping anoCondition = new LPCMapping();
                anoCondition.Add("round", hitArgs[5].AsInt);
                if (actionArgs.ContainsKey("original_cookie"))
                    anoCondition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
                else
                    anoCondition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
                anoCondition.Add("source_profile", sourceProfile);

                // 附加状态
                checkOb.ApplyStatus(hitArgs[4].AsString, anoCondition);
            }
        }

        return indexArgs;
    }
}

// 催眠凝视上状态脚本概率成功，回能(存在单hit依赖情况)
public class SCRIPT_9337 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        int attribRate = (sourceOb.QueryAttrib("agility") - hitArgs[1].AsInt) * hitArgs[2].AsInt;
        int skillRate = Math.Min(attribRate, hitArgs[3].AsInt);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillRate + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[5].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[4].AsString, condition);

        // 自身回能
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", hitArgs[6].AsInt);

        // 执行回血操作
        if (sourceProfile.GetValue<string>("cookie") == null)
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
        (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        return indexArgs;
    }
}

// 通用解除状态脚本（解除敌方一类目标状态）
public class SCRIPT_9338 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;

        // 对象为空跳出
        if (targetOb == null)
            return false;

        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        // LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数(状态类型)
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus("DIED"))
            return false;

        // 检查是否有目标状态，没有的话直接返回
        LPCArray statusList = hitArgs[1].AsArray;
        LPCArray removeList = new LPCArray();
        for (int i = 0; i < statusList.Count; i++)
        {
            string tarStatus = statusList[i].AsString;
            List<LPCMapping> allStatus = targetOb.GetStatusCondition(tarStatus);
            if (allStatus.Count == 0)
                continue;

            // 添加到清除列表中
            removeList.Add(tarStatus);
        }

        // 没有需要清除的状态
        if (removeList.Count == 0)
            return false;

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 清除目标状态
        targetOb.ClearStatus(removeList, LPCMapping.Empty);

        return true;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖情况，目标增益状态越多，概率越高)
public class SCRIPT_9339 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率（目标增益效果越多，命中率越高）
        int buffNum = targetOb.GetStatusAmountByType(hitArgs[1].AsInt);
        int buffAddRate = buffNum * hitArgs[2].AsInt;
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + buffAddRate + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[4].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[3].AsString, condition);

        return indexArgs;
    }
}

// 通用上状态脚本百分百成功(无hit依赖情况、目标能量为空则必定上某状态)
public class SCRIPT_9340 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 能量高于临界值则不起效
        if (targetOb.Query<int>("mp") >= hitArgs[0].AsInt)
            return false;

        // 上状态
        LPCMapping condition = new LPCMapping();

        if (hitArgs[2].AsInt != 0)
            condition.Add("round", hitArgs[2].AsInt);

        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        condition.Add("skill_id", skillId);

        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return true;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖情况,并且对有减益状态的目标才起效，且目标减益状态越多，概率越高)
public class SCRIPT_9341 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> debuffList = new List<int>();
        // 遍历所有对象身上的状态
        for (int status = 0; status < allStatus.Count; status++)
        {
            CsvRow info;
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[status].GetValue<int>("status_id"));
            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != StatusConst.TYPE_DEBUFF)
                continue;
            // 清除列表
            debuffList.Add(allStatus[status].GetValue<int>("cookie"));
        }

        // 如果没有减益状态则不起效
        if (debuffList.Count == 0)
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int debuffRateUp = debuffList.Count * hitArgs[3].AsInt;
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + debuffRateUp))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return indexArgs;
    }
}

// 通用概率上状态脚本(无hit依赖情况，需要计算效果命中，成功率跟目标身上的减益状态数量相关)
public class SCRIPT_9342 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 状态跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 播放光效
        string cookie = Game.NewCookie(targetOb.GetRid());
        //LPCMapping targetSource = targetOb.GetProfile();
        //targetSource.Add("element", targetOb.Query<int>("element"));
        LPCMapping args = new LPCMapping();
        args.Add("actor_name", targetOb.GetRid());
        args.Add("source_profile", sourceProfile);

        // 播放技能序列
        if (targetOb.Actor.DoActionSet(hitArgs[3].AsString, cookie, args))
        {
            // 播放动作
            // 构建参数
            LPCMapping roundPara = new LPCMapping();
            roundPara.Add("skill_id", skillId);                    // 释放技能
            roundPara.Add("pick_rid", targetOb.GetRid());          // 技能拾取目标
            roundPara.Add("cookie", cookie);                       // 技能cookie
            roundPara.Add("rid", targetOb.GetRid());               // 释放技能角色rid
            roundPara.Append(args);
            RoundCombatMgr.AddRoundAction(cookie, targetOb, roundPara);
        }

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 2 初次检查成功率，跟目标身上的减益状态数量相关
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        List<int> debuffList = new List<int>();
        // 遍历所有对象身上的状态
        for (int status = 0; status < allStatus.Count; status++)
        {
            CsvRow info;
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[status].GetValue<int>("status_id"));
            // 获取状态的类型：debuff / buff
            int buff_type = info.Query<int>("status_type");
            if (buff_type != StatusConst.TYPE_DEBUFF)
                continue;
            // 清除列表
            debuffList.Add(allStatus[status].GetValue<int>("cookie"));
        }
        int debuffRateUp = debuffList.Count * hitArgs[4].AsInt;
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + debuffRateUp))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string resTips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, resTips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        int round = hitArgs[2].AsInt;
        if (round > 0)
            condition.Add("round", round);

        // 如果需要根据消耗能量进行计算持续时间
        if (round == -1)
            condition.Add("round", 5 - sourceOb.Query<int>("mp"));

        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_skill_id", skillId);
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return true;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖情况,带传染)
public class SCRIPT_9343 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获取目标状态是否已经传染过
        List<LPCMapping> infectCheckStatus = targetOb.GetStatusCondition(hitArgs[1].AsString);

        foreach (LPCMapping checkMap in infectCheckStatus)
        {
            // 如果检查到有传染标识，则断开传染链
            if (checkMap.GetValue<int>("infect_id") > 0)
                return false;
        }

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
        {
            // 中断传染链需要清除传染次数累积
            sourceOb.SetTemp("infect_times", LPCValue.Create(0));
            return false;
        }

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            // 中断传染链需要清除传染次数累积
            sourceOb.SetTemp("infect_times", LPCValue.Create(0));
            return false;
        }

        // 上状态
        string cookie = actionArgs.GetValue<string>("cookie");
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 添加传染标识
        condition.Add("infect_id", 1);
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        // 释放传染技能
        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", cookie);
        CombatMgr.DoCastSkill(sourceOb, targetOb, hitArgs[3].AsInt, args);

        sourceOb.SetTemp("infect_times", LPCValue.Create(sourceOb.QueryTemp<int>("infect_times") + 1));

        // 传染次数超过 5 次中断传染链，大于 6 是因为第一次不算
        if (sourceOb.QueryTemp<int>("infect_times") > 6)
            sourceOb.SetTemp("infect_times", LPCValue.Create(0));

        return indexArgs;
    }
}

// 通用上状态脚本概率成功(无hit依赖情况,，带传染)
public class SCRIPT_9344 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 状态跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 播放光效
        string cookie = Game.NewCookie(targetOb.GetRid());
        //LPCMapping targetSource = targetOb.GetProfile();
        //targetSource.Add("element", targetOb.Query<int>("element"));
        LPCMapping args = new LPCMapping();
        args.Add("actor_name", targetOb.GetRid());
        args.Add("source_profile", sourceProfile);

        // 播放技能序列
        if (targetOb.Actor.DoActionSet(hitArgs[3].AsString, cookie, args))
        {
            // 播放动作
            // 构建参数
            LPCMapping roundPara = new LPCMapping();
            roundPara.Add("skill_id", skillId);                    // 释放技能
            roundPara.Add("pick_rid", targetOb.GetRid());          // 技能拾取目标
            roundPara.Add("cookie", cookie);                       // 技能cookie
            roundPara.Add("rid", targetOb.GetRid());               // 释放技能角色rid
            roundPara.Append(args);
            RoundCombatMgr.AddRoundAction(cookie, targetOb, roundPara);
        }

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        sourceProfile.Add("restrain", restrain);

        // 2 初次检查成功率
        // 计算升级效果
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
        {
            // 中断传染链需要清除传染次数累积
            sourceOb.SetTemp("infect_times", LPCValue.Create(0));
            return false;
        }

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string resTips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, resTips);
            // 中断传染链需要清除传染次数累积
            sourceOb.SetTemp("infect_times", LPCValue.Create(0));
            return false;
        }

        // 获取目标状态是否已经传染过
        List<LPCMapping> infectCheckStatus = targetOb.GetStatusCondition(hitArgs[1].AsString);
        int infectId = 0;
        foreach (LPCMapping checkMap in infectCheckStatus)
        {
            // 如果检查到有传染标识，则断开传染链
            if (checkMap.GetValue<int>("infect_id") > 0)
                infectId = 1;
        }

        // 上状态，需要检查是不是已经被传染过了
        if (infectId == 0)
        {
            LPCMapping condition = new LPCMapping();
            int round = hitArgs[2].AsInt;
            if (round > 0)
                condition.Add("round", round);

            // 如果需要根据消耗能量进行计算持续时间
            if (round == -1)
                condition.Add("round", 5 - sourceOb.Query<int>("mp"));

            if (actionArgs.ContainsKey("original_cookie"))
                condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            condition.Add("source_skill_id", skillId);
            condition.Add("source_profile", sourceProfile);
            // 如果有无视免疫
            if (sourceOb.QueryAttrib("status_no_immu") > 0)
                condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
            // 添加传染标识
            condition.Add("infect_id", 1);
            // 感染提示
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_infect"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            // 附加状态
            targetOb.ApplyStatus(hitArgs[1].AsString, condition);
        }

        // 释放传染技能前需要检查传染次数，传染次数超过设定值则中断传染链
        if (sourceOb.QueryTemp<int>("infect_times") > hitArgs[4].AsInt)
        {
            // 重置传染次数
            sourceOb.SetTemp("infect_times", LPCValue.Create(0));
            // 中断传染链
            return false;
        }

        // 释放传染技能
        LPCMapping infectArgs = LPCMapping.Empty;
        infectArgs.Add("original_cookie", cookie);
        CombatMgr.DoCastSkill(sourceOb, targetOb, hitArgs[3].AsInt, infectArgs);

        sourceOb.SetTemp("infect_times", LPCValue.Create(sourceOb.QueryTemp<int>("infect_times") + 1));

        return true;
    }
}

// 教廷护佑专用生命护盾状态脚本
public class SCRIPT_9345 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_CURSE", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        //LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡、诅咒跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 护盾值计算
        int value = 0;
        // 获取敏捷和生命上限
        int agility = sourceOb.QueryAttrib("agility");
        int maxHp = sourceOb.QueryAttrib("max_hp");

        value = maxHp * (hitArgs[0].AsInt + (agility - hitArgs[1].AsInt) * hitArgs[2].AsInt) / 10000;

        // 计算技能升级效果
        value += value * CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SHIELD) / 1000;

        // 计算额外影响
        value = CALC_EXTRE_CURE.Call(value, sourceOb.QueryAttrib("skill_effect") , targetOb.QueryAttrib("reduce_cure"));

        // 检查是否已有生命护盾
        int round = hitArgs[3].AsInt;
        if (targetOb.CheckStatus("B_HP_SHIELD"))
        {
            List<LPCMapping> shieldData = targetOb.GetStatusCondition("B_HP_SHIELD");
            foreach (LPCMapping data in shieldData)
            {
                int fixedRound = data.GetValue<int>("round") < round ? round : data.GetValue<int>("round");
                LPCMapping newCondition = new LPCMapping();
                newCondition.Add("round", fixedRound);
                if (actionArgs.ContainsKey("original_cookie"))
                    newCondition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
                else
                    newCondition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
                // 叠加护盾量
                newCondition.Add("can_absorb_damage", data.GetValue<int>("can_absorb_damage") + value);
                // 重新上护盾状态
                targetOb.ApplyStatus("B_HP_SHIELD", newCondition);
            }
            return true;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", round);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("can_absorb_damage", value);

        // 附加状态
        targetOb.ApplyStatus("B_HP_SHIELD", condition);

        return true;
    }
}

// 特殊生命恢复状态脚本，如果已有生命恢复状态则直接吞噬并恢复对应回合的生命
public class SCRIPT_9346 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        //int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        //int curLevel = sourceOb.GetSkillLevel(skillId);

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 计算升级技能带来的效果
        //int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), curLevel, SkillType.SE_HP_CURE);

        if (!targetOb.CheckStatus("B_HP_RECOVER"))
        {
            int curHpRate = hitArgs[2].AsInt;

            //curHpRate += curHpRate * skillUpgrade / 1000;

            int value = targetOb.QueryAttrib(hitArgs[1].AsString) * curHpRate / 1000;

            // 上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", hitArgs[3].AsInt);
            if (actionArgs.ContainsKey("original_cookie"))
                condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
            condition.Add("hp_cure", value);
            condition.Add("source_profile", _params[4] as LPCMapping);
            int healReduceDmg = targetOb.QueryAttrib("heal_reduce_dmg");
            if (healReduceDmg > 0)
                condition.Add("props", new LPCArray(new LPCArray(20, healReduceDmg)));

            // 附加对应生命恢复状态
            targetOb.ApplyStatus(hitArgs[0].AsString, condition);
        }
        else
        {

            // 检查禁止治疗
            if (targetOb.CheckStatus(banStatusList))
                return false;

            // 获取当前所有生命恢复状态的剩余回合，和单回合恢复血量
            List<LPCMapping> allHpCureStatus = targetOb.GetStatusCondition("B_HP_RECOVER");
            int allstatusCure = 0;
            foreach (LPCMapping statusData in allHpCureStatus)
            {
                allstatusCure += statusData.GetValue<int>("round") * statusData.GetValue<int>("hp_cure");
            }
            // 清除生命恢复状态，并执行回血操作
            targetOb.ClearStatus("B_HP_RECOVER");

            // 计算额外影响
            allstatusCure = CALC_EXTRE_CURE.Call(allstatusCure, sourceOb.QueryAttrib("skill_effect"), targetOb.QueryAttrib("reduce_cure"));

            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("hp", allstatusCure);

            // 执行回血操作
            if (sourceProfile.GetValue<string>("cookie") == null)
                sourceProfile.Add("cookie", Game.NewCookie(sourceOb.GetRid()));
            (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        return true;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖情况，能量消耗越多几率越高)
public class SCRIPT_9347 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCMapping costMap = actionArgs.GetValue<LPCMapping>("skill_cost");
        int mpCost = costMap.GetValue<int>("mp");
        int mpRate = mpCost * hitArgs[0].AsInt;
        if (RandomMgr.GetRandom() >= (mpRate + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return indexArgs;
    }
}

// 根据自身暴击率情况来上状态脚本概率成功(存在单hit依赖情况)
public class SCRIPT_9348 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int fixRate = hitArgs[0].AsInt * sourceOb.QueryAttrib("crt_rate") / hitArgs[3].AsInt;
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (fixRate + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        return indexArgs;
    }
}

// 永生奥秘专用给替身上状态(存在单hit依赖情况)
public class SCRIPT_9349 : Script
{
    //private static List<string> statusList = new List<string>(){ "DIED", };
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        string targetRid = targetOb.Query<string>("rid");
        //Property sourceOb = _params[1] as Property;
        //int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 获取替身对象
        string standRid = ridArgs.GetValue<string>("stand_rid");

        Property standOb = Rid.FindObjectByRid(standRid);

        // 上状态
        LPCMapping condition = new LPCMapping();

        if (hitArgs[1].AsInt != 0)
            condition.Add("round", hitArgs[1].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        standOb.ApplyStatus(hitArgs[0].AsString, condition);

        return indexArgs;
    }
}

// 通用上状态脚本概率成功(存在单hit依赖情况、暗影枷锁专用)
public class SCRIPT_9350 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        string targetRid = targetOb.Query<string>("rid");
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // 获得技能依赖关系
        LPCValue dependHit = actionArgs.GetValue<LPCValue>("depend_hit");

        if (!dependHit.IsInt)
            return false;

        LPCMapping sourceProfile = _params[4] as LPCMapping;
        // 获取状态参数({成功率, 状态别名, 状态持续回合数})
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;

        // 死亡跳出
        if (targetOb.CheckStatus(statusList))
            return false;

        // 获得所有节点的传递参数信息
        LPCMapping hitExtraArgs = actionArgs.GetValue<LPCMapping>("hit_extra_args");

        // 获得对应节点的传递参数信息
        LPCMapping indexArgs = hitExtraArgs.GetValue<LPCMapping>(dependHit.AsInt);
        // 获得对应RID的传递参数信息
        LPCMapping ridArgs = indexArgs.GetValue<LPCMapping>(targetRid);

        // 1 检查是否格挡
        int damgeType = ridArgs.GetValue<int>("damage_type");

        if ((damgeType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 2 初次检查成功率
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= (hitArgs[0].AsInt + skillEffect))
            return false;

        // 3 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            targetOb.QueryAttrib("resist_rate"),
            ridArgs.GetValue<int>("restrain"));

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return false;
        }

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", hitArgs[2].AsInt);
        if (actionArgs.ContainsKey("original_cookie"))
            condition.Add("round_cookie", actionArgs.GetValue<string>("original_cookie"));
        else
            condition.Add("round_cookie", actionArgs.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        // 如果有无视免疫
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));
        // 附加状态
        targetOb.ApplyStatus(hitArgs[1].AsString, condition);

        if (targetOb.CheckStatus(hitArgs[1].AsString))
        {
            // 立即获得回合
            RoundCombatMgr.DoAdditionalRound(sourceOb, LPCMapping.Empty);
        }

        return indexArgs;
    }
}

// 释放二段技能
public class SCRIPT_9400 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果 
        int nextSkillId = (_params[6] as LPCValue).AsInt;

        // 构建参数，添加原始机能cookie参数
        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", actionArgs.GetValue<string>("cookie"));

        // 释放技能
        CombatMgr.DoCastSkill(sourceOb, targetOb, nextSkillId, args);

        return true;
    }
}

// 释放二段技能(两个二段概率选择其一)
public class SCRIPT_9401 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        int skillId = (int)_params[2];
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        LPCArray hitArgs = (_params[6] as LPCValue).AsArray;
        // 获取技能初始攻击力加成效果
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        int allRate = hitArgs[0].AsInt + upRate;
        int nextSkillIdSingle = hitArgs[1].AsInt;
        int nextSkillIdAll = hitArgs[2].AsInt;
        int nSkillId = nextSkillIdSingle;
        if (RandomMgr.GetRandom() < allRate)
            nSkillId = nextSkillIdAll;

        // 构建参数，添加原始机能cookie参数
        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", actionArgs.GetValue<string>("cookie"));

        // 释放技能
        CombatMgr.DoCastSkill(sourceOb, targetOb, nSkillId, args);

        return true;
    }
}

// 光棍拳法专用释放二段技能
public class SCRIPT_9402 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;
        List<LPCMapping> allStatus = sourceOb.GetAllStatus();
        foreach(LPCMapping statusData in allStatus)
        {
            CsvRow info;
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));

            // 获取状态的类型
            int buff_type = info.Query<int>("status_type");
            // 如果有减益状态直接不起效
            if (buff_type == StatusConst.TYPE_DEBUFF)
                return false;
        }

        // 获取技能初始攻击力加成效果
        int nextSkillId = (_params[6] as LPCValue).AsInt;

        // 构建参数，添加原始机能cookie参数
        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", actionArgs.GetValue<string>("cookie"));

        // 释放技能
        CombatMgr.DoCastSkill(sourceOb, targetOb, nextSkillId, args);

        return true;
    }
}

// 根据目标生命和自身对比，释放不同的二段技能
public class SCRIPT_9403 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果 
        LPCArray skillArgs = (_params[6] as LPCValue).AsArray;
        int nextSkillId = skillArgs[0].AsInt;
        // 构建参数，添加原始机能cookie参数
        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", actionArgs.GetValue<string>("cookie"));

        // 与目标生命进行对比确定二段技能ID
        int sourceHpRate = sourceOb.QueryTemp<int>("hp_rate");
        int targetHpRate = targetOb.QueryTemp<int>("hp_rate");

        // 如果目标生命比率低于自身则用第二个参数的技能ID
        if (sourceHpRate > targetHpRate)
            nextSkillId = skillArgs[1].AsInt;

        CombatMgr.DoCastSkill(sourceOb, targetOb, nextSkillId, args);

        return true;
    }
}

// 释放二段技能（直接在当前hit里随机目标）
public class SCRIPT_9404 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        //Property targetOb = _params[0] as Property;
        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;
        // CsvRow skillInfo = _params[5] as CsvRow;

        // 获取技能初始攻击力加成效果 
        int nextSkillId = (_params[6] as LPCValue).AsInt;

        // 构建参数，添加原始机能cookie参数
        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", actionArgs.GetValue<string>("cookie"));

        // 随机目标
        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property checkOb in targetList)
        {
            if (!checkOb.CheckStatus("DIED") && checkOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                finalList.Add(checkOb);
        }

        if (finalList.Count == 0)
            return false;

        // 释放技能
        CombatMgr.DoCastSkill(sourceOb, finalList[RandomMgr.GetRandom(finalList.Count)], nextSkillId, args);

        return true;
    }
}

// 棱镜射线专用二段释放脚本
public class SCRIPT_9405 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        int comboTimes = actionArgs.GetValue<int>("combo_times");

        // 获取技能初始攻击力加成效果 
        LPCArray nextSkillIdList = (_params[6] as LPCValue).AsArray;

        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", actionArgs.GetValue<string>("cookie"));

        switch(comboTimes)
        {
            case 0:

                // 释放技能
                CombatMgr.DoCastSkill(sourceOb, targetOb, nextSkillIdList[0].AsInt, args);
                break;

            default:

                // 收集目标
                List<Property> targetList = RoundCombatMgr.GetPropertyList(targetOb.CampId, new List<string>(){ "DIED" , "B_CAN_NOT_CHOOSE", });
                List<Property> finalList = new List<Property>();

                // 筛选非死亡单位
                foreach (Property checkOb in targetList)
                {
                    if (checkOb.Query<int>("hp") > 0 && checkOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                        finalList.Add(checkOb);
                }

                // 没有目标
                if (finalList.Count == 0)
                    break;

                // 释放技能
                CombatMgr.DoCastSkill(sourceOb, finalList[RandomMgr.GetRandom(finalList.Count)], nextSkillIdList[1].AsInt, args);
                break;
        }

        return true;
    }
}

// 深渊追击专用二段释放脚本
public class SCRIPT_9406 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // targetOb sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property targetOb = _params[0] as Property;
        // 对象为空跳出
        if (targetOb == null)
            return false;

        Property sourceOb = _params[1] as Property;
        LPCMapping actionArgs = _params[3] as LPCMapping;

        // CsvRow skillInfo = _params[5] as CsvRow;
        int comboTimes = actionArgs.GetValue<int>("combo_times");

        // 获取技能初始攻击力加成效果 
        LPCArray nextSkillIdList = (_params[6] as LPCValue).AsArray;

        LPCMapping args = LPCMapping.Empty;
        args.Add("original_cookie", actionArgs.GetValue<string>("cookie"));

        switch(comboTimes)
        {
            case 0:
                // 释放技能
                CombatMgr.DoCastSkill(sourceOb, targetOb, nextSkillIdList[0].AsInt, args);
                break;

            default:
                if (targetOb.Query<int>("hp") <= 0 || targetOb.QueryTemp<int>("halo/halo_can_not_choose") != 0)
                    return false;
                // 释放技能
                CombatMgr.DoCastSkill(sourceOb, targetOb, nextSkillIdList[1].AsInt, args);
                break;
        }

        return true;
    }
}