/// <summary>
/// SCRIPT_6500.cs
/// Create by fengkk 2016-08-04
/// 技能脚本
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LPC;

/// <summary>
/// 百分比被动属性计算脚本(例如伤害吸血百分比属性记录)
/// </summary>
public class SCRIPT_6500 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);
        // 获取百分比
        int rate = propArgs[1].AsInt;

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, rate));
    }
}

/// <summary>
/// 双百分比被动属性计算脚本
/// </summary>
public class SCRIPT_6501 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);

        // 获取对应百分比
        int rate1 = propArgs[0].AsArray[1].AsInt;
        int rate2 = propArgs[1].AsArray[1].AsInt;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, rate1),
            new LPCArray(propId2, rate2)
        );
    }
}

/// <summary>
/// 被动上触发护盾属性计算脚本(护盾类型、百分比系数、持续回合)
/// </summary>
public class SCRIPT_6502 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);
        // 获取属性参数
        string shieldType = propArgs[0].AsString;

        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SHIELD);

        int rate = propArgs[1].AsInt + skillEffect;
        int round = propArgs[2].AsInt;

        LPCArray propValue = new LPCArray();

        // 加入属性(护盾类型、百分比系数、持续回合)
        propValue.Add(shieldType, rate, round);

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, propValue));
    }
}

/// <summary>
/// 特殊被动上属性计算脚本(属性别名、属性值为array)
/// </summary>
public class SCRIPT_6503 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);

        LPCArray propValue = propArgs[1].AsArray;

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, propValue));
    }
}

/// <summary>
/// (废弃)百分比被动属性计算脚本(例如伤害吸血百分比属性记录,需要记录skill_id到属性里)
/// </summary>
public class SCRIPT_6504 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);
        // 获取百分比
        int rate = propArgs[1].AsInt;

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, new LPCArray(rate, skillId)));
    }
}

/// <summary>
/// 双数组被动属性计算脚本
/// </summary>
public class SCRIPT_6505 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);

        // 获取对应百分比
        LPCArray args1 = propArgs[0].AsArray[1].AsArray;
        LPCArray args2 = propArgs[1].AsArray[1].AsArray;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2)
        );
    }
}

/// <summary>
/// 多被动属性计算脚本（属性值固定值非数组）
/// </summary>
public class SCRIPT_6506 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        LPCArray finalProps = new LPCArray();

        for (int i = 0; i < propArgs.Count; i++)
        {
            LPCArray perProp = new LPCArray();
            perProp.Add(PropMgr.GetPropId(propArgs[i].AsArray[0].AsString));
            perProp.Add(propArgs[i].AsArray[1].AsInt);
            finalProps.Add(perProp);
        }

        // 返回属性列表
        return finalProps;
    }
}

/// <summary>
/// 冲锋专用被动属性计算脚本
/// </summary>
public class SCRIPT_6507 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;

        // 检查是否有遗忘状态，有的话不上该被动
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        // 获取敌方速度最快值
        List<Property> allList;
        if (who.CampId == CampConst.CAMP_TYPE_ATTACK)
            allList = RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_DEFENCE);
        else
            allList = RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_ATTACK);

        int maxAgility = 0;
        for (int i = 0; i < allList.Count; i++)
        {
            // 对象不存在
            if (allList[i] == null)
                continue;

            // 获取敏捷
            int agility = allList[i].QueryOriginalAttrib("agility");

            // 小于等于maxAgility
            if (maxAgility >= agility)
                continue;

            // 记录数值
            maxAgility = agility;
        }

        // 获取技能ID
        LPCArray propArgs = (_param[3] as LPCValue).AsArray;

        // 获取两个属性id
        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, maxAgility * propArgs[0].AsArray[1].AsInt / 1000),
            new LPCArray(propId2, propArgs[1].AsArray[1].AsArray)
        );
    }
}

/// <summary>
/// 刚毅专用被动属性计算脚本
/// </summary>
public class SCRIPT_6508 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);
        int propId3 = PropMgr.GetPropId(propArgs[2].AsArray[0].AsString);

        LPCArray args1 = propArgs[0].AsArray[1].AsArray;
        int args2 = propArgs[1].AsArray[1].AsInt;
        int args3 = propArgs[2].AsArray[1].AsInt;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2),
            new LPCArray(propId3, args3)
        );
    }
}

/// <summary>
/// 愤怒专用被动上属性计算脚本
/// </summary>
public class SCRIPT_6509 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);
        int propId3 = PropMgr.GetPropId(propArgs[2].AsArray[0].AsString);

        int propValue1 = propArgs[0].AsArray[1].AsInt;
        LPCArray propValue2 = propArgs[1].AsArray[1].AsArray;
        string propValue3 = propArgs[2].AsArray[1].AsString;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, propValue1),
            new LPCArray(propId2, propValue2),
            new LPCArray(propId3, propValue3));
    }
}

/// <summary>
/// 被动属性计算脚本,int，array
/// </summary>
public class SCRIPT_6510 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;

        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);

        int args1 = propArgs[0].AsArray[1].AsInt;
        LPCArray args2 = propArgs[1].AsArray[1].AsArray;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2)
        );
    }
}

/// <summary>
/// 黑暗能量被动属性计算脚本,int，array
/// </summary>
public class SCRIPT_6511 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;

        // 获取技能ID
        int skillId = (int)_param[1];
        int curLevel = who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId));

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);

        int args1 = propArgs[0].AsArray[1].AsInt;
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), curLevel, SkillType.SE_RATE);
        LPCArray args2 = new LPCArray(propArgs[1].AsArray[1].AsArray[0].AsInt + skillEffect, propArgs[1].AsArray[1].AsArray[1].AsInt);

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2)
        );
    }
}

/// <summary>
/// 鬼武士专用带概率升级效果的特殊被动上属性计算脚本(属性别名、属性值为array)
/// </summary>
public class SCRIPT_6512 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);

        int upgradeValue = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCArray propValue = new LPCArray(propArgs[1].AsArray[0].AsInt + upgradeValue, propArgs[1].AsArray[1].AsInt, propArgs[1].AsArray[2].AsInt);

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, propValue));
    }
}

/// <summary>
/// 恶魔图腾带概率升级效果的特殊被动上属性计算脚本(属性别名、属性值为array)
/// </summary>
public class SCRIPT_6513 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);

        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);

        LPCArray propValue = new LPCArray(propArgs[1].AsArray[0].AsInt + skillEffect, propArgs[1].AsArray[1].AsInt, propArgs[1].AsArray[2].AsInt);

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, propValue));
    }
}

/// <summary>
/// 属性是mapping的上属性计算脚本
/// </summary>
public class SCRIPT_6514 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, propArgs[1].AsMapping));
    }
}

/// <summary>
/// 水镜带升级效果的特殊上属性计算脚本
/// </summary>
public class SCRIPT_6515 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);

        // 获取对应百分比
        int rate1 = propArgs[0].AsArray[1].AsInt;
        int tarSkillId = propArgs[1].AsArray[1].AsArray[0].AsInt;
        int rate2 = propArgs[1].AsArray[1].AsArray[1].AsInt;

        // 升级效果
        rate2 += rate2 * skillEffect / 1000;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, rate1),
            new LPCArray(propId2, new LPCArray(tarSkillId, rate2))
        );
    }
}

/// <summary>
/// 空间屏障专用被动属性计算脚本(带技能升级效果)
/// </summary>
public class SCRIPT_6516 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);

        // 获取技能升级效果
        int skillEffect1 = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        int skillEffect2 = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SHIELD);

        // 获取对应百分比
        LPCArray args1 = new LPCArray(propArgs[0].AsArray[1].AsArray[0].AsInt + skillEffect1,
                             propArgs[0].AsArray[1].AsArray[1].AsInt,
                             propArgs[0].AsArray[1].AsArray[2].AsInt);

        int shield = propArgs[1].AsArray[1].AsArray[2].AsInt;

        LPCArray args2 = new LPCArray(propArgs[1].AsArray[1].AsArray[0].AsInt,
                             propArgs[1].AsArray[1].AsArray[1].AsInt,
                             shield,
                             propArgs[1].AsArray[1].AsArray[3].AsInt,
                             skillEffect2
                         );

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2)
        );
    }
}

/// <summary>
/// 觉察弱点被动属性计算脚本(例如伤害吸血百分比属性记录)带升级效果
/// </summary>
public class SCRIPT_6517 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);
        // 获取百分比
        int rate = propArgs[1].AsInt;
        int skillUpgrade = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        rate += skillUpgrade;

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, rate));
    }
}

/// <summary>
/// 毒性之躯专用被动属性计算脚本
/// </summary>
public class SCRIPT_6518 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);
        int propId3 = PropMgr.GetPropId(propArgs[2].AsArray[0].AsString);

        int args1 = propArgs[0].AsArray[1].AsInt;
        LPCArray args2 = propArgs[1].AsArray[1].AsArray;
        LPCArray args3 = propArgs[2].AsArray[1].AsArray;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2),
            new LPCArray(propId3, args3)
        );
    }
}

/// <summary>
/// 自我安慰、死者遗愿专用被动属性计算脚本
/// </summary>
public class SCRIPT_6519 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);
        int propId3 = PropMgr.GetPropId(propArgs[2].AsArray[0].AsString);

        LPCArray args1 = propArgs[0].AsArray[1].AsArray;
        int args2 = propArgs[1].AsArray[1].AsInt;
        int args3 = propArgs[2].AsArray[1].AsInt;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2),
            new LPCArray(propId3, args3)
        );
    }
}

/// <summary>
/// 特殊被动上属性计算脚本(属性别名、属性值为array)免疫遗忘版
/// </summary>
public class SCRIPT_6520 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        //Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);

        LPCArray propValue = propArgs[1].AsArray;

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, propValue));
    }
}

/// <summary>
/// 纯粹圣洁专用被动属性计算脚本,int，int, array
/// </summary>
public class SCRIPT_6521 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;

        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);
        int propId3 = PropMgr.GetPropId(propArgs[2].AsArray[0].AsString);

        int args1 = propArgs[0].AsArray[1].AsInt;
        int args2 = propArgs[1].AsArray[1].AsInt;
        LPCArray args3 = propArgs[2].AsArray[1].AsArray;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2),
            new LPCArray(propId3, args3)
        );
    }
}

/// <summary>
/// 通用被动属性计算脚本无视免疫版
/// </summary>
public class SCRIPT_6522 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        //Property who = _param[0] as Property;

        // 获取技能ID
        int skillId = (int)_param[1];

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        LPCArray finalProp = new LPCArray();
        for (int i = 0; i < propArgs.Count; i++)
        {

            LPCArray perProp = new LPCArray();
            perProp.Add(PropMgr.GetPropId(propArgs[i].AsArray[0].AsString));

            if (propArgs[i].AsArray[1].IsArray)
                perProp.Add(propArgs[i].AsArray[1].AsArray);
            if (propArgs[i].AsArray[1].IsInt)
                perProp.Add(propArgs[i].AsArray[1].AsInt);

            finalProp.Add(perProp);
        }

        // 返回属性列表
        return finalProp;
    }
}

/// <summary>
/// 狂怒、不灭怒火专用被动属性计算脚本
/// </summary>
public class SCRIPT_6523 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;
        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId1 = PropMgr.GetPropId(propArgs[0].AsArray[0].AsString);
        int propId2 = PropMgr.GetPropId(propArgs[1].AsArray[0].AsString);
        int propId3 = PropMgr.GetPropId(propArgs[2].AsArray[0].AsString);
        int propId4 = PropMgr.GetPropId(propArgs[3].AsArray[0].AsString);

        int args1 = propArgs[0].AsArray[1].AsInt;
        LPCArray args2 = propArgs[1].AsArray[1].AsArray;
        LPCArray args3 = propArgs[2].AsArray[1].AsArray;
        LPCArray args4 = propArgs[3].AsArray[1].AsArray;

        // 返回属性列表
        return new LPCArray(
            new LPCArray(propId1, args1),
            new LPCArray(propId2, args2),
            new LPCArray(propId3, args3),
            new LPCArray(propId4, args4)
        );
    }
}

/// <summary>
/// 通用被动属性计算脚本
/// </summary>
public class SCRIPT_6524 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;

        // 获取技能ID
        int skillId = (int)_param[1];

        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        LPCArray finalProp = new LPCArray();
        for (int i = 0; i < propArgs.Count; i++)
        {

            LPCArray perProp = new LPCArray();
            perProp.Add(PropMgr.GetPropId(propArgs[i].AsArray[0].AsString));

            if (propArgs[i].AsArray[1].IsArray)
                perProp.Add(propArgs[i].AsArray[1].AsArray);
            if (propArgs[i].AsArray[1].IsString)
                perProp.Add(propArgs[i].AsArray[1].AsString);
            if (propArgs[i].AsArray[1].IsInt)
                perProp.Add(propArgs[i].AsArray[1].AsInt);

            finalProp.Add(perProp);
        }

        // 返回属性列表
        return finalProp;
    }
}

/// <summary>
/// 通用被动属性计算脚本(免疫遗忘版)
/// </summary>
public class SCRIPT_6525 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        //Property who = _param[0] as Property;

        // 获取技能ID
        int skillId = (int)_param[1];

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        LPCArray finalProp = new LPCArray();
        for (int i = 0; i < propArgs.Count; i++)
        {

            LPCArray perProp = new LPCArray();
            perProp.Add(PropMgr.GetPropId(propArgs[i].AsArray[0].AsString));

            if (propArgs[i].AsArray[1].IsArray)
                perProp.Add(propArgs[i].AsArray[1].AsArray);
            if (propArgs[i].AsArray[1].IsString)
                perProp.Add(propArgs[i].AsArray[1].AsString);
            if (propArgs[i].AsArray[1].IsInt)
                perProp.Add(propArgs[i].AsArray[1].AsInt);

            finalProp.Add(perProp);
        }

        // 返回属性列表
        return finalProp;
    }
}

/// <summary>
/// 竞技场时，百分比被动属性计算脚本
/// </summary>
public class SCRIPT_6550 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;

        // 检查是否在竞技场
        int type = InstanceMgr.GetMapTypeByInstanceId(who.Query<string>("instance/id"));
        if (type != MapConst.ARENA_MAP && type != MapConst.ARENA_NPC_MAP && type != MapConst.ARENA_REVENGE_MAP)
            return new LPCArray();

        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);
        // 获取百分比
        int rate = propArgs[1].AsInt;

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, rate));
    }
}

/// <summary>
/// 圣域时，百分比被动属性计算脚本
/// </summary>
public class SCRIPT_6551 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取对象
        Property who = _param[0] as Property;

        // 检查是否在竞技场
        int type = InstanceMgr.GetMapTypeByInstanceId(who.Query<string>("instance/id"));
        if (type != MapConst.DUNGEONS_MAP_2 && type != MapConst.SECRET_DUNGEONS_MAP)
            return new LPCArray();

        // 获取技能ID
        int skillId = (int)_param[1];

        // 检查遗忘状态
        if (who.CheckStatus("D_FORGET") && who.QueryAttrib("forget_immu") == 0)
            return new LPCArray();

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray propArgs = skillInfo.Query<LPCArray>("prop_args");

        int propId = PropMgr.GetPropId(propArgs[0].AsString);
        // 获取百分比
        int rate = propArgs[1].AsInt;

        // 返回属性列表
        return new LPCArray(new LPCArray(propId, rate));
    }
}

/// <summary>
/// 触发属性效果计算脚本,直接返回属性值
/// </summary>
public class SCRIPT_6600 : Script
{
    public override object Call(params object[] _param)
    {
        int prop_para = (_param[1] as LPCValue).AsInt;
        float trigger_fix = (float)_param[4];

        // 效果值类的，触发系数影响的是效果值
        float propValue = prop_para * trigger_fix;
        if (propValue <= 1f)
            propValue = 1f;
        return LPCValue.Create(propValue);
    }
}

/// <summary>
/// 触发属性效果计算脚本,直接返回无类型属性
/// </summary>
public class SCRIPT_6601 : Script
{
    public override object Call(params object[] _param)
    {
        LPCValue propValue = _param[1] as LPCValue;

        return propValue;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据输出伤害，恢复自身生命
/// </summary>
public class SCRIPT_6700 : Script
{
    private static List<string> statusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue; 

        if (trigger.CheckStatus(statusList))
            return false;

        sourceProfile.Append(triggerPara);

        // 取伤害量
        // 注意：extra_attack里的东西不计算吸血，block_value格挡值要算在伤害内
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        if (damageMap == null)
            damageMap = new LPCMapping();

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        if (points.ContainsKey("mp") && points.GetValue<int>("hp") == 0)
            return false;

        int damage = points.GetValue<int>("hp");

        if (damageMap.ContainsKey("origin_atk_rate"))
            damage = damage * damageMap.GetValue<int>("origin_atk_rate") / 1000;

        int hpCure = Mathf.Max(Game.Multiple(damage, propValue.AsInt), 0);

        // 计算额外影响
        string sourcrRid = sourceProfile.GetValue<string>("rid");
        Property sourceOb = Rid.FindObjectByRid(sourcrRid);
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();
        value.Add("hp", hpCure);
        sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));
        // 通知ReceiveCure
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据自身生命百分比回血
/// </summary>
public class SCRIPT_6701 : Script
{
    private static List<string> statusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (trigger.CheckStatus(statusList))
            return false;

        // 取最大生命
        int maxHp = trigger.QueryAttrib("max_hp");

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        int hpCure = Mathf.Max(Game.Multiple(maxHp, propValue.AsInt), 0);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);
        sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
        // 通知ReceiveCure
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用上护盾状态脚本
/// </summary>
public class SCRIPT_6702 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 护盾值计算
        int value = trigger.QueryAttrib(propValue[0].AsString) * propValue[1].AsInt / 1000;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[2].AsInt);
        condition.Add("can_absorb_damage", value);

        // 附加状态
        trigger.ApplyStatus("B_HP_SHIELD", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用上重生状态脚本
/// </summary>
public class SCRIPT_6703 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[2].AsInt);
        condition.Add("cure_rate", propValue[0].AsArray[1].AsInt);
        condition.Add("source_profile", sourceProfile);
        condition.Add("skill_id", propValue[0].AsArray[0].AsInt);

        // 附加状态
        trigger.ApplyStatus("B_REBORN_PASSIVE", condition);

        // 被动技能CD
        CdMgr.SkillCooldown(trigger, propValue[0].AsArray[0].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 陷阱类属性触发作用脚本
/// </summary>
public class SCRIPT_6704 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取陷阱触发实际作用的目标（陷阱攻击的对象）
        Property triggerOb = _param[0] as Property;
        // 获取陷阱携带者
        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        // 获取属性数值，第一个参数是携带者陷阱状态ID，第二个参数是触发者陷阱状态ID
        LPCArray propValue = (_param[3] as LPCValue).AsArray[0].AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取对应的陷阱状态
        int statusId = propValue[0].AsInt;
        List<LPCMapping> allStatus = sourceOb.GetStatusCondition(StatusMgr.GetStatusAlias(statusId));
        // 只可能同时存在一种陷阱
        LPCMapping trapData = allStatus[0]; 
        LPCMapping trapSourceProfile = trapData.GetValue<LPCMapping>("trap_source_profile");
        // 获取陷阱源对象
        Property trapSourceOb = Rid.FindObjectByRid(trapData.GetValue<string>("trap_source_rid"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = triggerOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = triggerOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 1、对攻击者触发伤害
        int atkRate = trapData.GetValue<int>("damage_rate");
        trapSourceProfile.Add("skill_damage_rate", 0);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(trapSourceOb, triggerOb);
        trapSourceProfile.Add("restrain", restrain);

        // 计算伤害类型（反伤）
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK | CombatConst.DAMAGE_TYPE_REFLECT | CombatConst.DAMAGE_TYPE_IGNORE_DEF;

        // 获取caugh_in_trap
        LPCMapping caughInTrap = triggerOb.QueryTemp<LPCMapping>("caugh_in_trap");
        if (caughInTrap == null)
            caughInTrap = LPCMapping.Empty;

        // 获取当前触发次数
        LPCMapping caughMap = caughInTrap.GetValue<LPCMapping>(statusId, LPCMapping.Empty);
        string originalCookie = triggerPara.GetValue<string>("original_cookie");
        int times = caughMap.GetValue<int>(originalCookie);

        // 累计次数
        if (caughMap.ContainsKey(originalCookie))
            caughMap.Add(originalCookie, times + 1);
        else
        {
            // 重置数据
            caughMap = LPCMapping.Empty;
            caughMap.Add(originalCookie, times + 1);
        }

        // 保存数据
        caughInTrap.Add(statusId, caughMap);
        triggerOb.SetTemp("caugh_in_trap", LPCValue.Create(caughInTrap));

        // 计算血量, 策划设置按照50%衰减
        int hp = CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap,
                     improvementMap, trapSourceProfile, damageType) * Game.Pow(5, times) / Game.Pow(10, times);

        // 构建伤害
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", hp);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", triggerOb.GetRid());

        // 提示技能图标
        if (trapSourceProfile.ContainsKey("skill_id"))
            BloodTipMgr.AddSkillTip(sourceOb, trapSourceProfile.GetValue<int>("skill_id"));

        // 一旦触发需要清除陷阱状态
        sourceOb.ClearStatus(statusId);

        // 记录新的rid给最终攻击源信息
        LPCMapping thisSourceProfile = new LPCMapping();
        thisSourceProfile.Append(trapSourceProfile);
        thisSourceProfile.Add("rid", sourceOb.GetRid());
        // 执行受创
        (triggerOb as Char).ReceiveDamage(thisSourceProfile, damageType, damageMap);

        // 如果在回合中
        if (triggerOb.IsRoundIn())
        {
            // 给敌方目标上陷阱状态
            LPCMapping trap = new LPCMapping();
            trap.Add("trap_source_profile", trapSourceProfile);
            trap.Add("debuff_rate", trapData.GetValue<int>("debuff_rate"));
            trap.Add("trap_source_acc", trapSourceOb.QueryAttrib("accuracy_rate"));
            trap.Add("target_resist_rate", triggerOb.QueryAttrib("resist_rate"));
            trap.Add("restrain", restrain);
            trap.Add("trap_effect", trapData.GetValue<string>("trap_effect"));
            trap.Add("trap_effect_round", trapData.GetValue<int>("trap_effect_round"));

            // 记录陷阱信息
            triggerOb.SetTemp("trap", LPCValue.Create(trap));
        }
        else
        {
            // 执行陷阱状态
            // 初次检查成功率
            if (RandomMgr.GetRandom() >= trapData.GetValue<int>("debuff_rate"))
                return false;

            // 计算效果命中
            // 攻击者效果命中、防御者效果抵抗、克制关系
            int rate = CALC_EFFECT_ACCURACY_RATE.Call(trapSourceOb.QueryAttrib("accuracy_rate"),
                           triggerOb.QueryAttrib("resist_rate"),
                           restrain);

            if (RandomMgr.GetRandom() < rate)
            {
                string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
                BloodTipMgr.AddTip(TipsWndType.DamageTip, triggerOb, tips);
                return false;
            }

            LPCMapping condition = new LPCMapping();
            condition.Add("round", trapData.GetValue<int>("trap_effect_round"));
            condition.Add("round_cookie", triggerPara.GetValue<string>("cookie"));
            condition.Add("source_profile", trapSourceProfile);

            // 附加状态
            triggerOb.ApplyStatus(trapData.GetValue<string>("trap_effect"), condition);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创反击
/// </summary>
public class SCRIPT_6705 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        //int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取目标
        if (trigger.CheckStatus("DIED"))
            return true;

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(sourceOb, SkillType.SKILL_TYPE_1));
        counterMap.Add("pick_rid", trigger.GetRid());

        RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
        // 首次反击成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，受创概率追加回合，并给自身上状态
/// </summary>
public class SCRIPT_6706 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        LPCMapping propMap = propValue[0].AsMapping;

        // 检查成功率
        if (RandomMgr.GetRandom() >= propMap.GetValue<int>("rate"))
            return false;

        if (propMap.ContainsKey("status"))
        {
            LPCMapping condition = new LPCMapping();
            condition.Add("round", propMap.GetValue<int>("round"));
            condition.Add("source_profile", sourceProfile);

            trigger.ApplyStatus(propMap.GetValue<string>("status"), condition);
        }

        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创光环来源者进行反击
/// </summary>
public class SCRIPT_6707 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE" };

    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        Property trigger = _param[0] as Property;
        Property propOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;
        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // 反击
        // propValue里记录了所有光环源信息
        for (int i = 0; i < selectPara.Count; i++)
        {
            LPCMapping data = selectPara[i].AsMapping;
            Property sourceOb = Rid.FindObjectByRid(data.GetValue<string>("source_rid"));

            // 角色对象不存在
            if (sourceOb == null || sourceOb.CheckStatus(statusList))
                continue;

            // 反击信息
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(sourceOb, SkillType.SKILL_TYPE_1));
            counterMap.Add("pick_rid", trigger.GetRid());

            // 执行反击回合
            BloodTipMgr.AddSkillTip(sourceOb, data.GetValue<int>("skill_id"));
            RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
        }

        // 首次反击成功记录cookie
        propOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，概率击晕
/// </summary>
public class SCRIPT_6708 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue; 
        string propName = _param[6] as String;

        // 先检查是否格挡，如果格挡不触发
        if ((triggerPara.GetValue<int>("damage_type") & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 1 根据概率计算是否击晕
        if (RandomMgr.GetRandom() >= propValue.AsInt)
            return false;

        LPCMapping condition = new LPCMapping();
        condition.Add("round", 1);
        if (sourceOb.QueryAttrib("status_no_immu") > 0)
            condition.Add("status_no_immu", sourceOb.QueryAttrib("status_no_immu"));

        trigger.ApplyStatus("D_STUN", condition);

        // 记录原始技能cookie
        trigger.Set("skill_temp_cookie", LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 首次击晕成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，概率追加回合（装备专用）
/// </summary>
public class SCRIPT_6709 : Script
{
    private static List<int> rateList = new List<int>() { 121, 67, 30 };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue; 

        int rate = propValue.AsInt;

        // 概率削减，每次成功后概率都进行一定幅度降低
        int addId = trigger.QueryTemp<int>("add_round_id");
        if (addId > 0)
        {
            if (addId < 4)
                rate = rateList[addId - 1];

            rate = rateList[2];
        }

        // 根据概率计算是否需要追加回合
        if (RandomMgr.GetRandom() >= rate)
        {
            // 失败时清空追加标志
            trigger.SetTemp("add_round_id", LPCValue.Create(0));
            return false;
        }

        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);

        // 成功时记录追加标志
        trigger.SetTemp("add_round_id", LPCValue.Create(trigger.QueryTemp<int>("add_round_id") + 1));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用复活脚本
/// </summary>
public class SCRIPT_6710 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 检查是否在死亡状态
        if (!trigger.CheckStatus("DIED"))
            return false;

        // 检查禁止重生
        if (trigger.CheckStatus(statusList) && trigger.QueryAttrib("no_reborn_immu") == 0)
            return false;

        // 清除死亡状态
        trigger.ClearStatus("DIED");

        // 回血
        int cureRate = propValue[0].AsInt;
        int hpCure = trigger.QueryAttrib("max_hp") * cureRate / 1000;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);
        sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 播放复活光效
        trigger.Actor.DoActionSet("reborn", Game.NewCookie(trigger.GetRid()), LPCMapping.Empty);

        // 提示技能图标
        //BloodTipMgr.AddSkillTip(who, data.GetValue<int>("skill_id"));

        // 清除状态
        trigger.ClearStatus("B_REBORN_STATUS");

        return true;
    }
}

/// <summary>
/// 触发作用复活并追加回合计算脚本
/// </summary>
public class SCRIPT_6711 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        int skillId = propValue[0].AsArray[0].AsInt;

        // 检查是否无法复活
        if (trigger.CheckStatus(statusList) && trigger.QueryAttrib("no_reborn_immu") == 0)
            return false;

        // 检查是否在死亡状态
        if (!trigger.CheckStatus("DIED"))
            return false;

        // 清除死亡状态
        trigger.ClearStatus("DIED");

        // 回血
        int hpCure = trigger.QueryAttrib("max_hp") * propValue[0].AsArray[1].AsInt / 1000;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 检查无法回血状态
        if (!trigger.CheckStatus(banStatusList))
        {
            sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
            (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 播放复活光效
        trigger.Actor.DoActionSet("reborn", Game.NewCookie(trigger.GetRid()), LPCMapping.Empty);

        // 被动技能CD
        CdMgr.SkillCooldown(trigger, skillId);

        // 提示技能图标
        BloodTipMgr.AddSkillTip(trigger, skillId);

        // 获得追加回合
        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);

        return true;
    }
}

/// <summary>
/// 击杀触发技能冷却计算脚本
/// </summary>
public class SCRIPT_6712 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取攻击起源对象
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 检查是否在死亡状态
        if (!trigger.CheckStatus("DIED"))
            return false;

        // 清除技能冷却
        int cd = CdMgr.GetSkillCdRemainRounds(sourceOb, propValue[0].AsArray[0].AsInt);
        CdMgr.DoReduceSkillCd(sourceOb, propValue[0].AsArray[0].AsInt, cd);

        string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
        BloodTipMgr.AddTip(TipsWndType.CureTip, sourceOb, tips);

        // 提示技能图标
        BloodTipMgr.AddSkillTip(sourceOb, propValue[0].AsArray[0].AsInt);

        return true;
    }
}

/// <summary>
/// 击杀触发无法复活计算脚本
/// </summary>
public class SCRIPT_6713 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round_cookie", triggerPara.GetValue<string>("original_cookie"));
        condition.Add("source_profile", sourceProfile);

        trigger.ApplyStatus("D_NO_REBORN", condition);

        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，概率追加回合（催眠大师，邪恶诱惑专用）
/// </summary>
public class SCRIPT_6714 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，概率增加技能冷却时间
/// </summary>
public class SCRIPT_6715 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        // 检查技能源cookie是否相同
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(sourceOb.QueryTemp("trigger_cookie/" + propName).AsString, triggerPara.GetValue<string>("original_cookie")))
                return false;
        }

        // 检查目标是否有免疫状态，如果有则无法起效
        if (trigger.CheckStatus("B_DEBUFF_IMMUNE"))
            return false;

        // 根据概率计算是否需要缩短冷却
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        // 技能冷却操作
        LPCArray skills = trigger.GetAllSkills();
        int cdTag = 0;
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能id
            int skillId = mks.AsArray[0].AsInt;
            // 不受技能cd增加影响的检查
            if (IS_CD_ADD_IMMU_CHECK.Call(skillId))
                continue;
            // 获取可叠加容量
            int canAddTurn = CdMgr.GetSkillCd(trigger, skillId) - CdMgr.GetSkillCdRemainRounds(trigger, skillId);
            // 如果有免疫状态，则不起效
            if (trigger.CheckStatus("B_DEBUFF_IMMUNE"))
                continue;
            CdMgr.AddSkillCd(trigger, skillId, Math.Min(canAddTurn, propValue[0].AsArray[1].AsInt));
            cdTag += 1;
        }

        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_cd_up"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);
        }

        // 提示被动技能
        if (propValue[0].AsArray[2].AsInt > 0)
            BloodTipMgr.AddSkillTip(sourceOb, propValue[0].AsArray[2].AsInt);

        // 记录来源技能的cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，自动消耗能量获取等级加成的生命护盾
/// </summary>
public class SCRIPT_6716 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 检查蓝量
        if (trigger.Query<int>("mp") <= 0)
            return false;

        int absorbDmg = propValue[0].AsArray[2].AsInt * trigger.Query<int>("level");
        absorbDmg += absorbDmg * propValue[0].AsArray[4].AsInt / 1000;

        // 上护盾
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[1].AsInt);
        condition.Add("can_absorb_damage", absorbDmg);

        // 附加状态
        trigger.ApplyStatus("B_HP_SHIELD", condition);

        // 扣除蓝耗
        LPCMapping costMap = new LPCMapping();
        costMap.Add("mp", propValue[0].AsArray[0].AsInt);
        trigger.CostAttrib(costMap);

        // 提示被动技能
        if (propValue[0].AsArray[3].AsInt > 0)
            BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[3].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受控制光环来源者上隐忍状态专用
/// </summary>
public class SCRIPT_6717 : Script
{
    public override object Call(params object[] _param)
    {
        //Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;
        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // 上状态
        // propValue里记录了所有光环源信息
        for (int i = 0; i < selectPara.Count; i++)
        {
            LPCMapping data = selectPara[i].AsMapping;
            LPCArray propValue = data.GetValue<LPCArray>("prop_value");
            Property sourceOb = Rid.FindObjectByRid(propValue[0].AsString);

            if (sourceOb.CheckStatus("DIED"))
                continue;

            // 获取回合
            RoundCombatMgr.DoAdditionalRound(sourceOb, LPCMapping.Empty);
            // 首次反击成功记录技能源cookie
            LPCMapping condition = triggerPara.GetValue<LPCMapping>("condition");
            sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(condition.GetValue<string>("round_cookie")));
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，命中概率增加目标技能的冷却时间X回合
/// </summary>
public class SCRIPT_6718 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };
    private static List<string> statusList = new List<string>(){ "B_DEBUFF_IMMUNE", "B_REBORN", "B_REBORN_PASSIVE", "B_REBORN_APPLY"};

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (trigger.QueryTemp<int>("hp_rate") <= 0)
            return false;

        int sourceSkillId = propValue[0].AsArray[2].AsInt;
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(sourceSkillId), trigger.GetSkillLevel(SkillMgr.GetOriginalSkillId(sourceSkillId)), SkillType.SE_RATE);
        int rate = propValue[0].AsArray[0].AsInt + upRate;

        // 检查目标是否有免疫状态，如果有则无法起效
        if (trigger.CheckStatus(statusList))
            return false;

        if (trigger.Query<int>("is_boss") == 1)
            rate = rate / 2;

        // 检查成功率
        if (RandomMgr.GetRandom() >= rate)
            return false;

        // 获取目标所有具有冷却时间的技能并增加冷却回合
        LPCArray skills = trigger.GetAllSkills();
        int cdTag = 0;
        // 遍历各个技能
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能id
            int skillId = mks.AsArray[0].AsInt;
            // 不受技能cd增加影响的检查
            if (IS_CD_ADD_IMMU_CHECK.Call(skillId))
                continue;
            if (CdMgr.GetSkillCd(trigger, skillId) > 0)
            {
                // 获取可叠加容量
                int canAddTurn = CdMgr.GetSkillCd(trigger, skillId) - CdMgr.GetSkillCdRemainRounds(trigger, skillId);
                // 如果有免疫状态，则不起效
                if (trigger.CheckStatus("B_DEBUFF_IMMUNE"))
                    continue;
                // 叠加不可超过技能本身冷却的上限
                CdMgr.AddSkillCd(trigger, skillId, Math.Min(canAddTurn, propValue[0].AsArray[1].AsInt));
                cdTag += 1;
            }
        }

        if (cdTag > 0)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_cd_up"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，受创百分比清除睡眠状态
/// </summary>
public class SCRIPT_6719 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping triggerPara = _param[4] as LPCMapping;

        // 概率检查没有通过，不能清除睡眠状态
        if (RandomMgr.GetRandom() >= propValue)
            return false;

        List<LPCMapping> statusMap = trigger.GetStatusCondition("D_SLEEP");
        string cookie = triggerPara.GetValue<string>("cookie");
        string original_cookie = triggerPara.GetValue<string>("original_cookie");
        bool isCleared = false;
        List<int> cookieList = null;

        // 清除非本次攻击带来的睡眠状态
        foreach (LPCMapping condition in statusMap)
        {
            // 如果是当前技能带来的睡眠不处理
            if (cookie.Equals(condition.GetValue<string>("round_cookie")) ||
                original_cookie.Equals(condition.GetValue<string>("original_cookie")))
                continue;

            // 清除指定cookie状态
            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { condition.GetValue<int>("cookie") };
            else
                cookieList.Add(condition.GetValue<int>("cookie"));

            // 标识清除状态成功
            isCleared = true;
        }

        // 清除状态
        trigger.ClearStatusByCookie(cookieList, StatusConst.CLEAR_TYPE_BREAK);

        // 返回数据
        return isCleared;
    }
}

/// <summary>
/// 触发属性效果作用脚本，回合开始触发技能
/// </summary>
public class SCRIPT_6720 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };
    //private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        int campId = (trigger.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        if (finalList.Count == 0)
            return false;

        Property finalOb = finalList[RandomMgr.GetRandom(finalList.Count)];

        CombatMgr.DoCastSkill(trigger, finalOb, propValue[0].AsArray[0].AsInt, LPCMapping.Empty);

        // 技能冷却
        int skillId = propValue[0].AsArray[1].AsInt;
        // 技能提示
        BloodTipMgr.AddSkillTip(trigger, skillId);
        if (CdMgr.GetSkillCd(trigger, skillId) > 0)
            CdMgr.SkillCooldown(trigger, skillId);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用，光环来源者进行协同攻击
/// </summary>
public class SCRIPT_6721 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property propOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        // 协同攻击
        // propValue里记录了所有光环源信息
        for (int i = 0; i < propValue.Count; i++)
        {
            if (RandomMgr.GetRandom() >= propValue[i].AsArray[1].AsInt)
                continue;

            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);
            if (sourceOb.CheckStatus("DIED"))
                continue;

            // 协同攻击
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(sourceOb, SkillType.SKILL_TYPE_1));
            counterMap.Add("pick_rid", triggerPara.GetValue<string>("pick_rid"));
            counterMap.Add("joint_rid", trigger.GetRid());

            // 发起协同回合
            RoundCombatMgr.DoJointRound(sourceOb, counterMap);
        }

        // 首次反击成功记录技能源cookie
        propOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，对最低生命的友军单位上护盾
/// </summary>
public class SCRIPT_6722 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取友军单位列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 筛选最低生命的目标
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;
        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED"))
                continue;
            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;
            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }
        Property finalOb = selectOb[RandomMgr.GetRandom(selectOb.Count)];

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue.AsArray[0].AsArray[0].AsInt);

        // 上护盾
        int shield = sourceOb.QueryAttrib("max_hp") * propValue.AsArray[0].AsArray[1].AsInt / 1000;
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue.AsArray[0].AsArray[2].AsInt);
        condition.Add("can_absorb_damage", shield);
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        finalOb.ApplyStatus("B_HP_SHIELD", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创伤害反射
/// </summary>
public class SCRIPT_6723 : Script
{
    private static List<string> statusList = new List<string>(){ "B_EQPT_SHIELD", "B_HP_SHIELD", "B_EN_SHIELD" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取sourceRid
        string sourceRid = sourceProfile.GetValue<string>("rid");

        // 检查伤害来源，如果是自身，则不起效
        if (string.Equals(trigger.GetRid(), sourceRid))
            return false;

        int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping triggerPara = _param[4] as LPCMapping;

        // 获取sourceOb
        Property sourceOb = Rid.FindObjectByRid(sourceRid);

        // 添加来源技能id
        //sourceProfile.Add("skill_id",triggerPara.GetValue<int>("skill_id"));
        sourceProfile.Add("cookie", triggerPara.GetValue<string>("cookie"));
        sourceProfile.Add("original_cookie", triggerPara.GetValue<string>("original_cookie"));

        // 检查伤害免疫，如果免疫则不反
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        if ((damageMap.GetValue<int>("damage_type") & CombatConst.DAMAGE_TYPE_IMMUNITY) == CombatConst.DAMAGE_TYPE_IMMUNITY)
            return false;

        // 检查目标处于护盾状态，如果处于护盾状态，返回原始伤害结构信息
        if (sourceOb.CheckStatus(statusList))
        {
            // 获取伤害结构信息（没经过伤害分摊和护盾削减的伤害）
            if (!triggerPara.ContainsKey("original_damage_map"))
                return false;

            LPCMapping originDamageMap = triggerPara.GetValue<LPCMapping>("original_damage_map");
            LPCMapping points = originDamageMap.GetValue<LPCMapping>("points");
            int damage = points.GetValue<int>("hp");

            // 如果源伤害中就没有伤害，则返回false
            if (damage == 0)
                return false;

            // 包装反射伤害信息
            LPCMapping reDamageMap = new LPCMapping();
            LPCMapping rePoints = new LPCMapping();
            rePoints.Add("hp", damage * propValue / 1000);
            reDamageMap.Add("points", rePoints);

            // 记录克制关系和伤害类型
            reDamageMap.Add("restrain", ElementConst.ELEMENT_NEUTRAL);
            reDamageMap.Add("damage_type", originDamageMap.GetValue<int>("damage_type") | CombatConst.DAMAGE_TYPE_REFLECT);
            reDamageMap.Add("target_rid", trigger.GetRid());

            // 造成伤害
            (trigger as Char).ReceiveDamage(sourceProfile, originDamageMap.GetValue<int>("damage_type"), reDamageMap);

            // 给出反伤标识
            BloodTipMgr.AddTip(
                TipsWndType.DamageTip,
                trigger,
                string.Format("[ff7474]{0}[-]", LocalizationMgr.Get("MonsterTipsWnd_7"))
            );

            // 返回数据
            return true;
        }

        // 最后检查实际伤害结构信息，并返回
        LPCMapping finalPoints = damageMap.GetValue<LPCMapping>("points");
        int finalDamage = finalPoints.GetValue<int>("hp");

        // 如果源伤害中就没有伤害，则返回false
        if (finalDamage == 0)
            return false;

        // 包装反射伤害信息
        LPCMapping finDamageMap = new LPCMapping();
        LPCMapping finalRePoints = new LPCMapping();
        finalRePoints.Add("hp", finalDamage * propValue / 1000);
        finDamageMap.Add("points", finalRePoints);

        // 记录克制关系和伤害类型
        finDamageMap.Add("restrain", ElementConst.ELEMENT_NEUTRAL);
        finDamageMap.Add("damage_type", damageMap.GetValue<int>("damage_type") | CombatConst.DAMAGE_TYPE_REFLECT);
        finDamageMap.Add("target_rid", trigger.GetRid());

        // 通知受创
        (trigger as Char).ReceiveDamage(sourceProfile, damageMap.GetValue<int>("damage_type"), finDamageMap);

        // 给出反伤标识
        BloodTipMgr.AddTip(
            TipsWndType.DamageTip,
            trigger,
            string.Format("[ff7474]{0}[-]", LocalizationMgr.Get("MonsterTipsWnd_7"))
        );

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用对应状态堆叠至固定层数后触发回满能量效果
/// </summary>
public class SCRIPT_6724 : Script
{
    public override object Call(params object[] _param)
    {
        Property targetOb = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (targetOb.CheckStatus("D_NO_MP_CURE") || targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
            return false;

        // 获取能量恢复量
        int mpCure = targetOb.QueryAttrib("max_mp");

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpCure);

        sourceProfile.Add("cookie", Game.NewCookie(targetOb.GetRid()));

        // 执行回能量操作
        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 清除状态
        targetOb.ClearStatus(propValue[0].AsArray[0].AsString);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创触发自身上状态脚本
/// </summary>
public class SCRIPT_6725 : Script
{
    public override object Call(params object[] _param)
    {
        Property triggerOb = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[1].AsInt);
        condition.Add("source_profile", sourceProfile);

        triggerOb.ApplyStatus(propValue[0].AsArray[0].AsString, condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，回合开始清除自身所有控制状态
/// </summary>
public class SCRIPT_6726 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 遍历自身状态列表
        List <LPCMapping> allStatus = trigger.GetAllStatus();
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

        if (ctrlNumList.Count == 0)
            return false;

        // 清除所有控制状态
        trigger.ClearStatus(ctrlNumList, LPCMapping.Empty, StatusConst.CLEAR_TYPE_BREAK);

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发普通攻击
/// </summary>
public class SCRIPT_6727 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (targetOb.CheckStatus("DIED"))
            return false;

        LPCMapping extraPara = new LPCMapping();
        extraPara.Add("skill_id", propValue[0].AsArray[1].AsInt);
        extraPara.Add("pick_rid", targetOb.GetRid());
        // 提示技能
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[2].AsInt);
        // 执行连击
        RoundCombatMgr.DoComboRound(trigger, extraPara);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据敏捷触发一定次数的普通攻击
/// </summary>
public class SCRIPT_6728 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        // 构建参数
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        LPCMapping extraPara = new LPCMapping();
        extraPara.Add("skill_id", propValue[0].AsArray[0].AsInt);
        extraPara.Add("pick_rid", damageMap.GetValue<string>("target_rid"));

        // 执行连击
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[1].AsInt);
        RoundCombatMgr.DoComboRound(trigger, extraPara);
        // 首次成功记录技能源cookie
        trigger.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用攻击目标随机个数队友进行协同攻击
/// </summary>
public class SCRIPT_6729 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 随机挑选N个队友
        List<Property> finalList = new List<Property>();
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId,
            new List<string>(){ "DIED", "D_FREEZE", "D_STUN", "D_SLEEP", "D_CHAOS", "D_PROVOKE", "D_PALSY" });

        // 去除自身收集
        targetList.Remove(trigger);

        // 随机筛选目标
        while (true)
        {
            // 列表为空跳出循环
            if (targetList.Count == 0)
                break;

            // 最终列表大于随机个数跳出循环
            if (finalList.Count >= propValue[0].AsArray[0].AsInt)
                break;

            // 随机筛选目标
            Property ob = targetList[RandomMgr.GetRandom(targetList.Count)];

            // 加入最终目标列表
            finalList.Add(ob);

            // 从目标列表中移除对象
            targetList.Remove(ob);
        }

        for (int i = 0; i < finalList.Count; i++)
        {
            // 协同攻击
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("pick_rid", triggerPara.GetValue<string>("pick_rid"));
            counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(finalList[i], SkillType.SKILL_TYPE_1));
            counterMap.Add("joint_rid", trigger.GetRid());

            // 发起协同回合
            RoundCombatMgr.DoJointRound(finalList[i], counterMap);

            if (finalList[i].CheckStatus("D_NO_MP_CURE") || finalList[i].CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;

            // 回复协同单位能量
            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("mp", propValue[0].AsArray[1].AsInt);
            LPCMapping sourceProfile = finalList[i].GetProfile();
            sourceProfile.Add("cookie", triggerPara.GetValue<string>("cookie"));
            (finalList[i] as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发回能
/// </summary>
public class SCRIPT_6730 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (trigger.CheckStatus("D_NO_MP_CURE") || trigger.CheckStatus("B_CAN_NOT_CHOOSE"))
            return false;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", propValue[0].AsArray[1].AsInt);

        // 执行概率回能操作
        if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
        {
            sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
            (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，回复能量
/// </summary>
public class SCRIPT_6731 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        if (trigger.CheckStatus("D_NO_MP_CURE") || trigger.CheckStatus("B_CAN_NOT_CHOOSE"))
            return false;

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        value.Add("mp", propValue.AsInt);

        sourceProfile.Add("skill_id", 11001);
        sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));

        // 通知ReceiveCure
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据受创伤害量百分比回血
/// </summary>
public class SCRIPT_6732 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (trigger.CheckStatus(banStatusList))
            return false;

        // 获取源伤害量
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("original_damage_map");
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");

        if (!points.ContainsKey("hp"))
            return false;

        int dmgHp = points.GetValue<int>("hp");

        int hpCure = Mathf.Max(propValue.AsInt * dmgHp / 1000, 0);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);
        sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));
        // 通知ReceiveCure
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，对所有友军单位上护盾
/// </summary>
public class SCRIPT_6733 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取友军单位列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue.AsArray[0].AsArray[0].AsInt);

        // 筛选最低生命的目标
        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        // 上护盾
        int shield = trigger.QueryAttrib(propValue.AsArray[0].AsArray[1].AsString) * propValue.AsArray[0].AsArray[2].AsInt / 1000;

        foreach (Property finalOb in finalList)
        {
            LPCMapping condition = new LPCMapping();
            condition.Add("round", propValue.AsArray[0].AsArray[3].AsInt);
            condition.Add("can_absorb_damage", shield);
            condition.Add("source_profile", sourceProfile);
            // 附加状态
            finalOb.ApplyStatus("B_HP_SHIELD", condition);
        }
        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，攻击所有敌军
/// </summary>
public class SCRIPT_6734 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        // 获取敌军单位列表
        int campId = (trigger.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK; 
        List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);
        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        if (trigger.CheckStatus(statusList))
            return false;

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue);
        CombatMgr.DoCastSkill(trigger, targetList[0], propValue, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，对所有友军单位回复能量
/// </summary>
public class SCRIPT_6735 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取友军单位列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue.AsArray[0].AsArray[0].AsInt);

        // 筛选最低生命的目标
        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        // 回能
        int mpCure = propValue.AsArray[0].AsArray[1].AsInt;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpCure);

        foreach (Property finalOb in finalList)
        {
            // 如果有禁止能量恢复状态，则跳过
            if (finalOb.CheckStatus("D_NO_MP_CURE"))
                continue;

            sourceProfile.Add("cookie",Game.NewCookie(finalOb.GetRid()));
            (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，对所有友军单位回复生命
/// </summary>
public class SCRIPT_6736 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取友军单位列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue.AsArray[0].AsArray[0].AsInt);

        // 筛选最低生命的目标
        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        // 回血百分比
        int hpCureRate = propValue.AsArray[0].AsArray[1].AsInt;

        LPCMapping cureMap = new LPCMapping();

        foreach (Property finalOb in finalList)
        {
            // 检查禁止治疗
            if (finalOb.CheckStatus(banStatusList))
                continue;

            int hpCure = hpCureRate * finalOb.QueryAttrib("max_hp") / 1000;
            hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), finalOb.QueryAttrib("reduce_cure"));
            cureMap.Add("hp", hpCure);

            sourceProfile.Add("cookie",Game.NewCookie(finalOb.GetRid()));
            (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 死亡触发技能，作用脚本
/// </summary>
public class SCRIPT_6737 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        CombatMgr.DoCastSkill(trigger, trigger, propValue, LPCMapping.Empty);

        CsvRow skillInfo = SkillMgr.GetSkillInfo(propValue);

        int originSkillId = skillInfo.Query<int>("original_skill_id");

        // 手动CD和扣蓝
        CdMgr.SkillCooldown(trigger, originSkillId);
        LPCMapping costMap = SkillMgr.GetCasTCost(trigger, originSkillId);
        trigger.CostAttrib(costMap);

        // 扣除重生次数
        int rebornTimes = trigger.QueryAttrib("reborn_times") - 1;
        trigger.Set("reborn_times", LPCValue.Create(rebornTimes));
        // 设置已重生次数
        trigger.Set("had_reborn_times", LPCValue.Create(trigger.Query<int>("had_reborn_times") + 1));

        return true;
    }
}

/// <summary>
/// 转阶段触发技能，作用脚本
/// </summary>
public class SCRIPT_6738 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        CombatMgr.DoCastSkill(trigger, trigger, propValue, LPCMapping.Empty);

        // 扣除重生次数
        int rebornTimes = trigger.QueryAttrib("reborn_times") - 1;
        trigger.Set("reborn_times", LPCValue.Create(rebornTimes));
        // 设置已重生次数
        trigger.Set("had_reborn_times", LPCValue.Create(trigger.Query<int>("had_reborn_times") + 1));

        // 检查是否需要刷最大生命增加属性
        if (trigger.QueryAttrib("phase_max_hp_up") > 0)
        {
            // 上最大生命增加属性
            trigger.Set("hp_rate_sp", LPCValue.Create(trigger.QueryAttrib("phase_max_hp_up")));
            // 刷新属性
            PropMgr.RefreshAffect(trigger);
        }

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger, propValue);

        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发召唤
/// </summary>
public class SCRIPT_6739 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 判断该召唤怪物是否有召唤最大数量限制
        int maxSummon = trigger.QueryAttrib("max_summon");

        // 获取已经创建的怪物列表,召唤列表格式({ monster1, monster2, monster3 })
        LPCArray summonList = trigger.Query<LPCArray>("summon_list");
        List<int> posList = new List<int>();
        if (summonList != null)
        {
            foreach (LPCValue rid in summonList.Values)
            {
                // 查找角色对象
                Property ob = Rid.FindObjectByRid(rid.AsString);
                if (ob == null)
                    continue;

                // 添加到位置列表中
                posList.Add(ob.FormationPos);
            }
        }

        // 已经达到最大召唤数量限制
        if (maxSummon <= posList.Count)
            return false;

        // 获取召唤怪物id
        int classId = propValue.AsArray[0].AsArray[0].AsInt;

        // 构建召唤参数
        LPCMapping summonInfo = new LPCMapping();
        summonInfo.Add("init_rule", propValue.AsArray[0].AsArray[1].AsString);
        summonInfo.Add("level", trigger.Query<int>("level"));

        // 拼接sceneId
        string sceneId = string.Format("{0}_{1}_{2}", trigger.SceneId, trigger.FormationRaw, trigger.FormationPos);

        // 循环召唤各个位置上的怪物
        for (int i = 0; i < 3; i++)
        {
            // 该位置已经有怪物
            if (posList.IndexOf(i) != -1)
                continue;

            // 召唤怪物SummonEntity(summoner, classId, campId, sceneId, formationId, formationPos, formationRaw, summonInfo)
            CombatSummonMgr.SummonEntity(
                trigger,
                classId,
                trigger.CampId,
                sceneId,
                maxSummon,
                i,
                trigger.FormationRaw,
                summonInfo);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 抉择专用作用脚本，根据自身生命百分比回血，攻击力up
/// </summary>
public class SCRIPT_6740 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("source_profile", sourceProfile);
        // 附加状态
        trigger.ApplyStatus("B_ATK_UP_JZ", condition);

        if (trigger.CheckStatus(banStatusList))
            return false;

        // 取最大生命
        int maxHp = trigger.QueryAttrib("max_hp");

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();
        int hpCure = Mathf.Max(propValue.AsArray[0].AsArray[1].AsInt * maxHp / 1000, 0);
        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);
        sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
        // 通知ReceiveCure
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，必定追加回合
/// </summary>
public class SCRIPT_6741 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);
        // 记录攻击回合信息
        trigger.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创伤害反射(计算按照受创者自身攻击进行百分比加成)
/// </summary>
public class SCRIPT_6742 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 添加攻击源cookie
        sourceProfile.Add("cookie",triggerPara.GetValue<string>("cookie"));
        sourceProfile.Add("original_cookie",triggerPara.GetValue<string>("original_cookie"));

        // 检查伤害来源，如果是自身，则不起效
        if (trigger.GetRid().Equals(sourceProfile.GetValue<string>("rid")))
            return false;

        // 获取伤害结构信息（没经过伤害分摊和护盾削减的伤害）
        if (!triggerPara.ContainsKey("original_damage_map"))
            return false;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("original_damage_map");

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");

        // 如果伤害来源是mp则不起效
        if (points.ContainsKey("mp"))
            return false;

        // 以受创者自身攻击百分比进行计算反伤伤害
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        int sourceAtk = sourceOb.QueryAttrib("attack");

        int reflectDamage = sourceAtk * propValue / 1000;

        // 包装反射伤害信息
        LPCMapping reDamageMap = new LPCMapping();
        LPCMapping rePoints = new LPCMapping();
        rePoints.Add("hp", reflectDamage);
        reDamageMap.Add("points", rePoints);
        // 记录克制关系和伤害类型
        reDamageMap.Add("restrain", ElementConst.ELEMENT_NEUTRAL);
        reDamageMap.Add("damage_type", damageMap.GetValue<int>("damage_type") | CombatConst.DAMAGE_TYPE_REFLECT);
        reDamageMap.Add("target_rid", trigger.GetRid());

        // 造成伤害
        (trigger as Char).ReceiveDamage(sourceProfile, damageMap.GetValue<int>("damage_type"), reDamageMap);

        string tips = string.Format("[ff7474]{0}[-]", LocalizationMgr.Get("MonsterTipsWnd_7"));

        // 给出反伤标识
        BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用攻击目标随机个数队友进行协同攻击,并且回复协同单位1回合技能冷却
/// </summary>
public class SCRIPT_6743 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 随机挑选N个队友剔除死亡、眩晕、冻结、麻痹、混乱、挑衅、睡眠
        List<Property> finalList = new List<Property>();
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId,
            new List<string>(){ "DIED", "D_FREEZE", "D_STUN", "D_SLEEP", "D_CHAOS", "D_PROVOKE", "D_PALSY" });

        // 去除自身收集
        targetList.Remove(trigger);

        // 随机筛选目标
        while (true)
        {
            // 列表为空跳出循环
            if (targetList.Count == 0)
                break;

            // 最终列表大于随机个数跳出循环
            if (finalList.Count >= propValue)
                break;

            // 随机筛选目标
            Property ob = targetList[RandomMgr.GetRandom(targetList.Count)];

            // 加入最终目标列表
            finalList.Add(ob);

            // 从目标列表中移除对象
            targetList.Remove(ob);
        }

        for (int i = 0; i < finalList.Count; i++)
        {
            // 协同攻击
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("pick_rid", triggerPara.GetValue<string>("pick_rid"));
            counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(finalList[i], SkillType.SKILL_TYPE_1));
            counterMap.Add("joint_rid", trigger.GetRid());

            // 发起协同回合
            RoundCombatMgr.DoJointRound(finalList[i], counterMap);

            // 回复协同单位 1 回合技能冷却
            LPCArray skills = finalList[i].GetAllSkills();
            int cdTag = 0;
            foreach (LPCValue mks in skills.Values)
            {
                // 获取技能id
                int skillId = mks.AsArray[0].AsInt;
                // 不受技能cd减少影响的检查
                if (IS_CD_REDUCE_IMMU_CHECK.Call(skillId))
                    continue;

                if (CdMgr.GetSkillCd(finalList[i], skillId) > 0)
                {
                    CdMgr.DoReduceSkillCd(finalList[i], skillId, 1);
                    cdTag += 1;
                }
            }

            if (cdTag > 0)
            {
                string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
                BloodTipMgr.AddTip(TipsWndType.CureTip, finalList[i], tips);
            }
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 特殊陷阱属性触发作用脚本（该类陷阱伤害计算根据自身生命上限越高，伤害越高）
/// </summary>
public class SCRIPT_6744 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取陷阱触发实际作用的目标（陷阱攻击的对象）
        Property triggerOb = _param[0] as Property;
        // 获取陷阱携带者
        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        // 获取属性数值，第一个参数是携带者陷阱状态ID，第二个参数是触发者陷阱状态ID
        LPCArray propValue = (_param[3] as LPCValue).AsArray[0].AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取对应的陷阱状态
        int statusId = propValue[0].AsInt;
        List<LPCMapping> allStatus = sourceOb.GetStatusCondition(StatusMgr.GetStatusAlias(statusId));
        // 只可能同时存在一种陷阱
        LPCMapping trapData = allStatus[0]; 
        LPCMapping trapSourceProfile = trapData.GetValue<LPCMapping>("trap_source_profile");
        // 获取陷阱源对象
        Property trapSourceOb = Rid.FindObjectByRid(trapData.GetValue<string>("trap_source_rid"));

        // finalAttrib格式转换
        LPCMapping finalAttribMap = triggerOb.GetFinalAttrib();

        // improvement格式转换
        LPCMapping improvementMap = triggerOb.QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 1、对攻击者触发伤害
        int atkRate = trapData.GetValue<int>("damage_rate");

        // 获取技能升级伤害量提升
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(trapData.GetValue<int>("skill_id")), 
                          trapSourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(trapData.GetValue<int>("skill_id"))), 
                          SkillType.SE_DMG);

        trapSourceProfile.Add("skill_damage_rate", dmgRate);

        // 计算克制关系
        int restrain = ElementMgr.GetMonsterCounter(trapSourceOb, triggerOb);
        trapSourceProfile.Add("restrain", restrain);

        // 计算伤害类型（暴击、格挡、强击）无视防御
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK | CombatConst.DAMAGE_TYPE_REFLECT | CombatConst.DAMAGE_TYPE_IGNORE_DEF;

        // 计算伤害结构
        trapSourceProfile.Add("max_hp_damage_co", trapData.GetValue<int>("max_hp_damage_co"));

        // 获取caugh_in_trap
        LPCMapping caughInTrap = triggerOb.QueryTemp<LPCMapping>("caugh_in_trap");
        if (caughInTrap == null)
            caughInTrap = LPCMapping.Empty;

        // 获取当前触发次数
        LPCMapping caughMap = caughInTrap.GetValue<LPCMapping>(statusId, LPCMapping.Empty);
        string originalCookie = triggerPara.GetValue<string>("original_cookie");
        int times = caughMap.GetValue<int>(originalCookie);

        // 累计次数
        if (caughMap.ContainsKey(originalCookie))
            caughMap.Add(originalCookie, times + 1);
        else
        {
            // 重置数据
            caughMap = LPCMapping.Empty;
            caughMap.Add(originalCookie, times + 1);
        }

        // 保存数据
        caughInTrap.Add(statusId, caughMap);
        triggerOb.SetTemp("caugh_in_trap", LPCValue.Create(caughInTrap));

        // 计算血量, 策划设置按照50%衰减
        int hp = CALC_ALL_DAMAGE_ATTACK_MAX_HP.Call(atkRate, finalAttribMap,
                     improvementMap, trapSourceProfile, damageType) * Game.Pow(5, times) / Game.Pow(10, times);

        // 构建伤害
        LPCMapping damageMap = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", hp);
        damageMap.Add("points", points);
        damageMap.Add("restrain", restrain);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", triggerOb.GetRid());

        // 提示技能图标
        if (trapSourceProfile.ContainsKey("skill_id"))
            BloodTipMgr.AddSkillTip(sourceOb, trapSourceProfile.GetValue<int>("skill_id"));

        // 一旦触发需要清除陷阱状态
        sourceOb.ClearStatus(statusId);

        // 记录新的rid给最终攻击源信息
        LPCMapping thisSourceProfile = new LPCMapping();
        thisSourceProfile.Append(trapSourceProfile);
        thisSourceProfile.Add("rid", sourceOb.GetRid());
        // 执行受创
        (triggerOb as Char).ReceiveDamage(thisSourceProfile, damageType, damageMap);

        // 如果在回合中
        if (triggerOb.IsRoundIn())
        {
            // 给敌方目标上陷阱状态
            LPCMapping trap = new LPCMapping();
            trap.Add("trap_source_profile", trapSourceProfile);
            trap.Add("debuff_rate", trapData.GetValue<int>("debuff_rate"));
            trap.Add("trap_source_acc", trapSourceOb.QueryAttrib("accuracy_rate"));
            trap.Add("target_resist_rate", triggerOb.QueryAttrib("resist_rate"));
            trap.Add("restrain", restrain);
            trap.Add("trap_effect", trapData.GetValue<string>("trap_effect"));
            trap.Add("trap_effect_round", trapData.GetValue<int>("trap_effect_round"));

            // 记录陷阱信息
            triggerOb.SetTemp("trap", LPCValue.Create(trap));
        }
        else
        {
            // 执行陷阱状态
            // 初次检查成功率
            if (RandomMgr.GetRandom() >= trapData.GetValue<int>("debuff_rate"))
                return false;

            // 计算效果命中
            // 攻击者效果命中、防御者效果抵抗、克制关系
            int rate = CALC_EFFECT_ACCURACY_RATE.Call(trapSourceOb.QueryAttrib("accuracy_rate"),
                           triggerOb.QueryAttrib("resist_rate"),
                           restrain);

            if (RandomMgr.GetRandom() < rate)
            {
                string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
                BloodTipMgr.AddTip(TipsWndType.DamageTip, triggerOb, tips);
                return false;
            }

            LPCMapping condition = new LPCMapping();
            condition.Add("round", trapData.GetValue<int>("trap_effect_round"));
            condition.Add("round_cookie", triggerPara.GetValue<string>("cookie"));
            condition.Add("source_profile", trapSourceProfile);

            // 附加状态
            triggerOb.ApplyStatus(trapData.GetValue<string>("trap_effect"), condition);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，暴击触发全队回能
/// </summary>
public class SCRIPT_6745 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        sourceProfile.Add("cookie",triggerPara.GetValue<string>("cookie"));
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", propValue[0].AsArray[1].AsInt);

        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        List<Property> checkList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property checkOb in targetList)
        {
            if (!checkOb.CheckStatus("DIED"))
                checkList.Add(checkOb);
        }

        // 单独成员计算概率
        foreach (Property targetOb in checkList)
        {
            // 如果有无法回能状态需要跳过
            // 如果有禁止能量恢复状态，则跳过
            if (targetOb.CheckStatus("D_NO_MP_CURE") || targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;

            // 概率执行回能操作
            if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
            {
                // 技能提示
                BloodTipMgr.AddSkillTip(targetOb, propValue[0].AsArray[2].AsInt);
                (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
            }
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 死亡触发来源释放技能，作用脚本(例如惩戒光球)
/// </summary>
public class SCRIPT_6746 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取攻击源
        Property trigger = _param[0] as Property;

        if (trigger.CheckStatus("DIED"))
            return false;

        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        Property castOb = Rid.FindObjectByRid(propValue[0].AsArray[0].AsString);

        // 光环源死亡或者来自光环源则不作用
        if (castOb.CheckStatus("DIED") || sourceOb.Equals(castOb))
            return false;

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", propValue[0].AsArray[1].AsInt);
        counterMap.Add("pick_rid", trigger.GetRid());

        RoundCombatMgr.DoCounterRound(castOb, counterMap);

        // 技能提示
        BloodTipMgr.AddSkillTip(castOb, propValue[0].AsArray[1].AsInt);

        return true;
    }
}

/// <summary>
/// 触发作用受创一定次数以后反击
/// </summary>
public class SCRIPT_6747 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        int hitTimes = sourceOb.Query<int>("hit_times") + 1;

        sourceOb.Set("hit_times", LPCValue.Create(hitTimes));

        // 获取目标
        if (trigger.CheckStatus("DIED"))
            return true;

        // 次数触发回能
        if (hitTimes >= propValue[0].AsArray[0].AsInt)
        {
            // 反击信息
            LPCMapping cureMap = new LPCMapping();
            int mpCure = propValue[0].AsArray[1].AsInt;
            if (sourceOb.CheckStatus("D_NO_MP_CURE") || sourceOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                mpCure = 0;
            cureMap.Add("mp", mpCure);

            // 执行回能操作
            sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
            (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
            // 清空受创次数
            sourceOb.Set("hit_times", LPCValue.Create(0));
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，全队回血上状态
/// </summary>
public class SCRIPT_6748 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取友军单位列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[0].AsInt);

        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && !targetOb.CheckStatus(banStatusList))
                finalList.Add(targetOb);
        }

        foreach (Property finalOb in finalList)
        {
            // 回血
            int hpCure = finalOb.QueryAttrib("max_hp") * propValue[0].AsArray[1].AsInt / 1000;
            hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), finalOb.QueryAttrib("reduce_cure"));
            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("hp", hpCure);
            sourceProfile.Add("cookie",Game.NewCookie(finalOb.GetRid()));
            (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

            // 上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", propValue[0].AsArray[3].AsInt);
            condition.Add("source_profile", sourceProfile);
            finalOb.ApplyStatus(propValue[0].AsArray[2].AsString, condition);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据自身防御百分比对生命最低的友军单位进行回血
/// </summary>
public class SCRIPT_6749 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY", "DIED" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;

        // 收集友军生命比率最低单位,遍历各个角色
        foreach (Property ob in targetList)
        {
            // 角色已经死亡
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

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        int hpCure = propValue.AsInt * trigger.QueryAttrib("defense") / 1000;

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), finalOb.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);
        sourceProfile.Add("cookie",Game.NewCookie(finalOb.GetRid()));
        // 通知ReceiveCure
        (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用濒死上状态，光环来源者需要进行cd操作
/// </summary>
public class SCRIPT_6750 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // propValue里记录了所有光环源信息
        for (int i = 0; i < selectPara.Count; i++)
        {
            LPCMapping data = selectPara[i].AsMapping;
            LPCArray propValue = data.GetValue<LPCArray>("prop_value");
            Property sourceOb = Rid.FindObjectByRid(propValue[0].AsString);
            if (sourceOb.CheckStatus("DIED"))
                continue;
            //上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", propValue[2].AsInt);
            trigger.ApplyStatus(propValue[1].AsString, condition);
            // 血量设置为 1 不死
            trigger.Set("hp", LPCValue.Create(1));
            // 光环来源技能进行cd操作
            CdMgr.SkillCooldown(sourceOb, propValue[3].AsInt);
            return true;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用攻击目标随机个数队友进行协同攻击并恢复队友能量和冷却
/// </summary>
public class SCRIPT_6751 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 随机挑选N个队友
        List<Property> finalList = new List<Property>();
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId,
            new List<string>(){ "DIED", "D_FREEZE", "D_STUN", "D_SLEEP", "D_CHAOS", "D_PROVOKE", "D_PALSY" });

        // 去除自身收集
        targetList.Remove(trigger);

        // 随机筛选目标
        while (true)
        {
            // 列表为空跳出循环
            if (targetList.Count == 0)
                break;

            // 最终列表大于随机个数跳出循环
            if (finalList.Count >= propValue[0].AsArray[0].AsInt)
                break;

            // 随机筛选目标
            Property ob = targetList[RandomMgr.GetRandom(targetList.Count)];

            // 加入最终目标列表
            finalList.Add(ob);

            // 从目标列表中移除对象
            targetList.Remove(ob);
        }

        for (int i = 0; i < finalList.Count; i++)
        {
            // 协同攻击
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("pick_rid", triggerPara.GetValue<string>("pick_rid"));
            counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(finalList[i], SkillType.SKILL_TYPE_1));
            counterMap.Add("joint_rid", trigger.GetRid());

            // 发起协同回合
            RoundCombatMgr.DoJointRound(finalList[i], counterMap);

            // 恢复技能冷却
            LPCArray skills = finalList[i].GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                int skillId = mks.AsArray[0].AsInt;
                // 不受技能cd减少影响的检查
                if (IS_CD_REDUCE_IMMU_CHECK.Call(skillId))
                    continue;
                if (CdMgr.GetSkillCd(finalList[i], skillId) == 0)
                    continue;
                CdMgr.DoReduceSkillCd(finalList[i], skillId, propValue[0].AsArray[2].AsInt);
            }

            // 回复协同单位能量
            if (finalList[i].CheckStatus("D_NO_MP_CURE") || finalList[i].CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;

            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("mp", propValue[0].AsArray[1].AsInt);
            LPCMapping sourceProfile = finalList[i].GetProfile();
            sourceProfile.Add("cookie", triggerPara.GetValue<string>("cookie"));
            (finalList[i] as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创指定技能反击
/// </summary>
public class SCRIPT_6752 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取已反击次数
        int counterTimes = sourceOb.QueryTemp<int>("counter_times");

        // 获取目标
        if (trigger.CheckStatus("DIED"))
            return true;

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", propValue[0].AsArray[0].AsInt);
        counterMap.Add("pick_rid", trigger.GetRid());

        RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
        // 首次反击成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));
        // 记录反击次数
        sourceOb.SetTemp("counter_times", LPCValue.Create(counterTimes + 1));

        // 检查防反次数并清除状态,并重置记录次数
        if ((counterTimes + 1) >= propValue[0].AsArray[1].AsInt)
        {
            sourceOb.SetTemp("counter_times", LPCValue.Create(0));
            sourceOb.ClearStatus("B_DEF_COUNTER");
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创光环源上伤害增强状态（带刷属性）
/// </summary>
public class SCRIPT_6753 : Script
{
    public override object Call(params object[] _param)
    {
        //Property triggerOb = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        // 获取光环源对象
        for (int i = 0; i < propValue.Count; i++)
        {
            LPCArray propArg = propValue[i].AsArray;
            Property haloSourceOb = Rid.FindObjectByRid(propArg[0].AsString);
            int addValue = haloSourceOb.QueryTemp<int>("add_dmg_up_value");
            if (addValue >= propArg[4].AsInt)
                continue;
            if (haloSourceOb.CheckStatus("DIED"))
                continue;

            LPCMapping condition = new LPCMapping();
            condition.Add("source_profile", sourceProfile);
            condition.Add("props", new LPCArray(new LPCArray(25, propArg[2].AsInt)));
            haloSourceOb.SetTemp("add_dmg_up_value", LPCValue.Create(addValue + propArg[2].AsInt));
            haloSourceOb.ApplyStatus(propArg[1].AsString, condition);

            BloodTipMgr.AddSkillTip(haloSourceOb, propArg[3].AsInt);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受攻击光环源上伤害增强状态（带刷属性）
/// </summary>
public class SCRIPT_6754 : Script
{
    public override object Call(params object[] _param)
    {
        //Property triggerOb = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        // 获取光环源对象
        for (int i = 0; i < propValue.Count; i++)
        {
            LPCArray propArg = propValue[i].AsArray;
            Property haloSourceOb = Rid.FindObjectByRid(propArg[0].AsString);
            int addValue = haloSourceOb.QueryTemp<int>("add_dmg_up_value");
            if (addValue >= propArg[4].AsInt)
                continue;
            if (haloSourceOb.CheckStatus("DIED"))
                continue;

            LPCMapping condition = new LPCMapping();
            condition.Add("source_profile", sourceProfile);
            condition.Add("props", new LPCArray(new LPCArray(25, propArg[2].AsInt)));
            condition.Add("add_value", propArg[2].AsInt);
            haloSourceOb.SetTemp("add_dmg_up_value", LPCValue.Create(addValue + propArg[2].AsInt));

            haloSourceOb.ApplyStatus(propArg[1].AsString, condition);

            BloodTipMgr.AddSkillTip(haloSourceOb, propArg[3].AsInt);
        }
        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创指定技能反击
/// </summary>
public class SCRIPT_6755 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取已反击次数
        int counterTimes = sourceOb.QueryTemp<int>("counter_times");

        // 获取目标
        if (trigger.CheckStatus("DIED") || sourceOb.CheckStatus("DIED"))
            return false;

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", propValue[0].AsArray[0].AsInt);
        counterMap.Add("pick_rid", trigger.GetRid());

        // 反击
        RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
        //BloodTipMgr.AddSkillTip(sourceOb,SkillMgr.GetOriginalSkillId(propValue[0].AsArray[0].AsInt));

        // 首次反击成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));
        // 记录反击次数
        sourceOb.SetTemp("counter_times", LPCValue.Create(counterTimes + 1));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创概率触发攻击方上状态脚本
/// </summary>
public class SCRIPT_6756 : Script
{
    public override object Call(params object[] _param)
    {
        Property triggerOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        // 检查成功率
        if (RandomMgr.GetRandom() > propValue[0].AsArray[2].AsInt)
            return false;

        // 攻击者效果命中、防御者效果抵抗、克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, triggerOb);
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       triggerOb.QueryAttrib("resist_rate"),
                       restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, triggerOb, tips);
            return false;
        }

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[1].AsInt);
        condition.Add("source_profile", sourceProfile);

        triggerOb.ApplyStatus(propValue[0].AsArray[0].AsString, condition);

        // 记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创一定次数以后反击
/// </summary>
public class SCRIPT_6757 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 检查攻击是否将要造成死亡
        LPCMapping points = triggerPara.GetValue<LPCMapping>("damage_map").GetValue<LPCMapping>("points");
        if (points.GetValue<int>("hp") >= sourceOb.Query<int>("hp"))
            return false;

        int hitTimes = sourceOb.Query<int>("hit_times") + 1;

        sourceOb.Set("hit_times", LPCValue.Create(hitTimes));

        // 获取目标
        if (trigger.CheckStatus("DIED") || sourceOb.CheckStatus("DIED"))
            return true;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 发出仇恨提示
        string tips = string.Format("{0}{1} + 1", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_hatred"));
        BloodTipMgr.AddTip(TipsWndType.DamageTip, sourceOb, tips);

        // 反击
        if (hitTimes >= propValue[0].AsArray[1].AsInt)
        {
            // 反击信息
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("skill_id", propValue[0].AsArray[0].AsInt);
            counterMap.Add("pick_rid", trigger.GetRid());

            RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
            // 清空受创次数
            sourceOb.Set("hit_times", LPCValue.Create(0));
        }

        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创暴击时技能概率二选一反击
/// </summary>
public class SCRIPT_6758 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取目标
        if (trigger.CheckStatus("DIED") || sourceOb.CheckStatus("DIED"))
            return true;

        int counterRate = propValue[0].AsArray[1].AsInt + propValue[0].AsArray[3].AsInt;
        int skillId = propValue[0].AsArray[0].AsInt;

        int oneSkillRate = 1000 * propValue[0].AsArray[3].AsInt / counterRate;
        if (RandomMgr.GetRandom() < oneSkillRate)
            skillId = propValue[0].AsArray[2].AsInt;

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", skillId);
        counterMap.Add("pick_rid", trigger.GetRid());

        RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
        // 首次反击成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 出手触发上白夜状态
/// </summary>
public class SCRIPT_6759 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 检查白夜状态个数,超过设定个数不起效
        List <LPCMapping> status = trigger.GetStatusCondition("B_HIYAKUYA");
        if (status.Count >= propValue[0].AsArray[2].AsInt)
            return false;

        // 执行上白夜状态操作
        LPCMapping condition = new LPCMapping();
        int propValueAtk = propValue[0].AsArray[0].AsInt;
        int propValueDef = propValue[0].AsArray[1].AsInt;
        LPCArray atkProp = new LPCArray(5, propValueAtk);
        LPCArray defProp = new LPCArray(6, propValueDef);
        condition.Add("props", new LPCArray(atkProp, defProp));
        condition.Add("round", 9999);

        trigger.ApplyStatus("B_HIYAKUYA", condition);

        // 被动技能提示
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[3].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，暴击触发回血
/// </summary>
public class SCRIPT_6760 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (trigger.CheckStatus(banStatusList))
            return false;

        LPCMapping cureMap = new LPCMapping();
        int hpCure = trigger.QueryAttrib("max_hp") * propValue[0].AsArray[1].AsInt / 1000;
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));
        cureMap.Add("hp", hpCure);

        // 执行概率回能操作
        if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
        {
            sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
            (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发自身上状态
/// </summary>
public class SCRIPT_6761 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[2].AsInt);
        condition.Add("source_profile", sourceProfile);

        // 执行概率上状态操作
        if (RandomMgr.GetRandom() < propValue[0].AsArray[1].AsInt)
            trigger.ApplyStatus(propValue[0].AsArray[0].AsString, condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 击杀触发概率自身回血（按照击杀目标的最大生命计算恢复量）
/// </summary>
public class SCRIPT_6762 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        if (sourceOb.CheckStatus(banStatusList))
            return false;

        // 按照目标的生命
        int hpCure = (int)(propValue[0].AsArray[1].AsInt * trigger.QueryAttrib("max_hp") / 1000);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), sourceOb.QueryAttrib("reduce_cure"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);

        // 概率对自身执行回血操作
        if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
        {
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
            (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，攻击概率目标上持续伤害状态
/// </summary>
public class SCRIPT_6763 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[1].AsInt);
        condition.Add("source_profile", sourceProfile);
        condition.Add("round_cookie", triggerPara.GetValue<string>("original_cookie"));

        // 检查成功率
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        // 检查格挡
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        if ((damageMap.GetValue<int>("damage_type") & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, trigger);
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       trigger.QueryAttrib("resist_rate"),
                       restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);
            return false;
        }

        trigger.ApplyStatus("D_INJURY", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，受创概率攻击者上持续伤害状态
/// </summary>
public class SCRIPT_6764 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 检查伤害来源，如果是自身，则不起效
        if (Property.Equals(sourceOb, trigger))
            return false;

        // 获取伤害结构信息（没经过伤害分摊和护盾削减的伤害）
        if (!triggerPara.ContainsKey("original_damage_map"))
            return false;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("original_damage_map");

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");

        // 如果伤害来源是mp则不起效
        if (points.ContainsKey("mp"))
            return false;

        // 攻击者上持续伤害状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[1].AsInt);
        condition.Add("source_profile", sourceProfile);
        condition.Add("round_cookie", triggerPara.GetValue<string>("original_cookie"));

        // 检查成功率
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        // 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, trigger);
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       trigger.QueryAttrib("resist_rate"),
                       restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);
            return false;
        }

        trigger.ApplyStatus("D_INJURY", condition);
        // 首次成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用攻击增加目标所有减益状态持续回合
/// </summary>
public class SCRIPT_6765 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取目标
        if (trigger.CheckStatus("DIED"))
            return false;

        // 概率上状态
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[1].AsInt)
            return false;

        // 检查格挡
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        if ((damageMap.GetValue<int>("damage_type") & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, trigger);
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
                       trigger.QueryAttrib("resist_rate"),
                       restrain);

        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);
            return false;
        }

        // 获取目标身上所有减益状态
        List <LPCMapping> allStatus = trigger.GetAllStatus();
        List<int> changeStatusList = new List<int>();

        foreach (LPCMapping statusData in allStatus)
        {
            // 获取状态的信息
            CsvRow info = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));

            // 获取状态的类型
            if (info.Query<int>("status_type") != StatusConst.TYPE_DEBUFF)
                continue;

            // 检查是否本回合提供的状态
            if (! string.Equals(triggerPara.GetValue<string>("cookie"), statusData.GetValue<string>("round_cookie")))
                changeStatusList.Add(statusData.GetValue<int>("cookie"));
        }

        // 增加状态回合数
        trigger.AddStatusRounds(changeStatusList, propValue[0].AsArray[2].AsInt);

        // 给出提示信息
        if (changeStatusList.Count > 0)
            BloodTipMgr.AddSkillTip(sourceOb, propValue[0].AsArray[0].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，暴击触发随机队友减少冷却
/// </summary>
public class SCRIPT_6766 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位、有技能冷却的队友
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
            if (CdMgr.HasSkillICooldown(targetOb))
                finalList.Add(targetOb);
        }

        // 排除自身
        finalList.Remove(trigger);

        if (finalList.Count == 0)
            return false;

        Property finalOb = finalList[RandomMgr.GetRandom(finalList.Count)];

        LPCArray skills = finalOb.GetAllSkills();
        foreach (LPCValue mks in skills.Values)
        {
            // 判断技能id是否在冷却中
            int id = mks.AsArray[0].AsInt;
            // 不受技能cd减少影响的检查
            if (IS_CD_REDUCE_IMMU_CHECK.Call(id))
                continue;
            if (CdMgr.SkillIsCooldown(finalOb, id))
                // 执行回冷却操作
                CdMgr.DoReduceSkillCd(finalOb, id, propValue[0].AsArray[2].AsInt);
        }

        // 冷却减少提示
        string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_cd_reduce"));
        BloodTipMgr.AddTip(TipsWndType.CureTip, finalOb, tips);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据输出伤害，恢复己方全体生命
/// </summary>
public class SCRIPT_6767 : Script
{
    private static List<string> banStatusList = new List<string>(){ "DIED", "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue; 

        // 取伤害量
        // 注意：extra_attack里的东西不计算吸血，block_value格挡值要算在伤害内
        LPCMapping damage_info = triggerPara.GetValue<LPCMapping>("damage_map");
        if (damage_info == null)
            damage_info = new LPCMapping();

        LPCMapping points = damage_info.GetValue<LPCMapping>("points");
        if (points.ContainsKey("mp") && points.GetValue<int>("hp") == 0)
            return false;

        // 增加triggerPara参数
        sourceProfile.Append(triggerPara);

        // 计算基准回血数值
        int damage = points.GetValue<int>("hp");
        int hpCure = Mathf.Max(Game.Multiple(damage, propValue.AsInt), 0);

        // 获取sourceOb
        string sourcrRid = sourceProfile.GetValue<string>("rid");
        Property sourceOb = Rid.FindObjectByRid(sourcrRid);
        int skillEffect = sourceOb.QueryAttrib("skill_effect");

        // 收集目标
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            // 如果有banStatusList中的状态，不处理
            if (targetOb.CheckStatus(banStatusList))
                continue;

            // 计算额外影响
            // 根据伤害量，计算恢复量
            LPCMapping value = new LPCMapping();
            value.Add("hp", CALC_EXTRE_CURE.Call(hpCure, skillEffect, targetOb.QueryAttrib("reduce_cure")));

            sourceProfile.Add("cookie",Game.NewCookie(targetOb.GetRid()));
            (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，对最低生命的N个友军单位清除N个减益状态，并恢复其%生命
/// </summary>
public class SCRIPT_6768 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    /// <summary>
    /// 比较速度
    /// </summary>
    public static int Compara(Property left, Property right)
    {
        int leftHpRate = Game.Divided(left.Query<int>("hp"), left.QueryAttrib("max_hp"));
        int rightHpRate = Game.Divided(right.Query<int>("hp"), right.QueryAttrib("max_hp"));

        if (leftHpRate < rightHpRate)
            return -1;

        if (leftHpRate > rightHpRate)
            return 1;

        return 0;
    }

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        // 整理筛选结果，按照生命最低的顺次排序
        List<Property> finalList = RoundCombatMgr.GetPropertyList(trigger.CampId,
            new List<string>(){ "DIED" });

        // 筛选信息
        finalList.Sort(Compara);

        // 清除减益状态并回血
        int index = Math.Min(finalList.Count, propValue[0].AsArray[1].AsInt);
        for (int i = 0; i < index; i++)
        {
            //筛选减益状态
            List<LPCMapping> allStatus = finalList[i].GetAllStatus();
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
            List<int> clearList = new List<int>();
            // 随机筛选目标
            while (true)
            {
                // 列表为空跳出循环
                if (debuffList.Count == 0)
                    break;

                // 最终列表大于随机个数跳出循环
                if (clearList.Count >= propValue[0].AsArray[2].AsInt)
                    break;

                // 随机筛选目标
                int clearCookie = debuffList[RandomMgr.GetRandom(debuffList.Count)];

                // 加入最终目标列表
                clearList.Add(clearCookie);

                // 从目标列表中移除对象
                debuffList.Remove(clearCookie);
            }

            // 清除状态
            finalList[i].ClearStatusByCookie(clearList, StatusConst.CLEAR_TYPE_BREAK);

            // 回血
            int hpCure = Game.Multiple(
                finalList[i].QueryAttrib("max_hp"),
                propValue[0].AsArray[3].AsInt
            );

            if (finalList[i].CheckStatus(banStatusList))
                continue;

            // 计算额外影响
            hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), finalList[i].QueryAttrib("reduce_cure"));
            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("hp", hpCure);
            // 执行回血操作
            sourceProfile.Add("cookie",Game.NewCookie(finalList[i].GetRid()));
            (finalList[i] as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
            // 提示技能
            BloodTipMgr.AddSkillTip(finalList[i], propValue[0].AsArray[0].AsInt);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发普通攻击(目标随机)
/// </summary>
public class SCRIPT_6769 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        LPCMapping extraPara = new LPCMapping();
        extraPara.Add("skill_id", propValue[0].AsArray[1].AsInt);
        // 随机一个目标释放
        int campId = (trigger.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);
        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }
        if (finalList.Count == 0)
            return false;
        Property finalOb = finalList[RandomMgr.GetRandom(finalList.Count)];
        extraPara.Add("pick_rid", finalOb.GetRid());
        RoundCombatMgr.DoComboRound(trigger, extraPara);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据自身生命百分比回血，耗蓝版本
/// </summary>
public class SCRIPT_6770 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (trigger.CheckStatus(banStatusList))
            return false;

        // 取最大生命
        int maxHp = trigger.QueryAttrib("max_hp");

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        int hpCure = Mathf.Max(propValue[0].AsArray[1].AsInt * maxHp / 1000, 0);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);

        // 通知ReceiveCure
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[0].AsInt);
        sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 扣除蓝量
        LPCMapping costMap = SkillMgr.GetCasTCost(trigger, propValue[0].AsArray[0].AsInt);
        trigger.CostAttrib(costMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 击杀触发随机以百分比生命复活一名死亡队友，如果触发额外增加N回合技能CD
/// </summary>
public class SCRIPT_6771 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _param)
    {
        //Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取攻击起源对象
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 检查目标是否有免疫状态，如果有则无法起效
        if (sourceOb.CheckStatus("B_DEBUFF_IMMUNE"))
            return false;

        // 筛选死亡单位
        List<Property> allList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        List<Property> targetList = new List<Property>();
        foreach (Property targetOb in allList)
        {
            if (targetOb.CheckStatus("DIED") && !targetOb.CheckStatus(statusList))
                targetList.Add(targetOb);
        }
        // 如果没人死亡则直接返回false
        if (targetList.Count == 0)
            return false;

        Property reviveOb = targetList[RandomMgr.GetRandom(targetList.Count)];

        // 清除死亡状态
        if (!reviveOb.CheckStatus("DIED"))
            return false;

        reviveOb.ClearStatus("DIED");

        // 回血
        int cureRate = propValue[0].AsArray[1].AsInt;
        int hpCure = reviveOb.QueryAttrib("max_hp") * cureRate / 1000;
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", hpCure);
        sourceProfile.Add("cookie",Game.NewCookie(reviveOb.GetRid()));
        (reviveOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 播放复活光效
        reviveOb.Actor.DoActionSet("reborn", Game.NewCookie(reviveOb.GetRid()), LPCMapping.Empty);

        int skillId = propValue[0].AsArray[0].AsInt;

        // 获取可叠加容量
        //int canAddTurn = CdMgr.GetSkillCd(sourceOb, skillId) - CdMgr.GetSkillCdRemainRounds(sourceOb, skillId);
        // 如果有免疫状态，则不起效
        if (sourceOb.CheckStatus("B_DEBUFF_IMMUNE"))
            return true;
        // 增加技能CD
        CdMgr.AddSkillCd(sourceOb, skillId, propValue[0].AsArray[2].AsInt);

        // 提示技能图标
        BloodTipMgr.AddSkillTip(reviveOb, propValue[0].AsArray[0].AsInt);

        return true;
    }
}

/// <summary>
/// 触发作用受创时恢复能量
/// </summary>
public class SCRIPT_6772 : Script
{
    public override object Call(params object[] _param)
    {
        //Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", propValue[0].AsArray[1].AsInt);

        // 执行回能操作
        if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
        {
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
            (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据自身生命百分比回血，带被动技能提示
/// </summary>
public class SCRIPT_6773 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (trigger.CheckStatus(banStatusList))
            return false;

        // 取最大生命
        int maxHp = trigger.QueryAttrib("max_hp");

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        int hpCure = Mathf.Max(propValue[0].AsArray[0].AsInt * maxHp / 1000, 0);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);
        // 技能提示
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[1].AsInt);
        // 通知ReceiveCure
        sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，释放技能触发追加回合
/// </summary>
public class SCRIPT_6774 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 技能提示
        //BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[1].AsInt);
        // 通知ReceiveCure
        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 击杀触发回能
/// </summary>
public class SCRIPT_6775 : Script
{
    public override object Call(params object[] _param)
    {
        //Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        if (sourceOb.CheckStatus("D_NO_MP_CURE"))
            return false;

        // 计算回能
        int mpCure = propValue[0].AsArray[1].AsInt;

        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpCure);

        // 概率对自身执行回能操作
        if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
        {
            BloodTipMgr.AddSkillTip(sourceOb, propValue[0].AsArray[2].AsInt);
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
            (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发释放技能
/// </summary>
public class SCRIPT_6776 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 提示被动技能
        BloodTipMgr.AddSkillTip(sourceOb, propValue);

        string roundCookie = sourceOb.QueryTemp<string>("round_cookie");

        if (!string.IsNullOrEmpty(roundCookie))
            sourceOb.SetTemp("trigger_round_cookie", LPCValue.Create(roundCookie));

        CombatMgr.DoCastSkill(sourceOb, trigger, propValue, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 神魔灭杀概率触发回能作用脚本
/// </summary>
public class SCRIPT_6777 : Script
{
    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        //Property trigger = _param[0] as Property;
        //Property propOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;
        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // selectPara里记录了所有光环源信息
        for (int i = 0; i < selectPara.Count; i++)
        {
            LPCMapping data = selectPara[i].AsMapping;
            Property sourceOb = Rid.FindObjectByRid(data.GetValue<string>("source_rid"));
            if (sourceOb.CheckStatus("D_NO_MP_CURE") || sourceOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;
            // 角色对象不存在
            if (sourceOb == null)
                continue;
            // 技能提示
            //BloodTipMgr.AddSkillTip(sourceOb, data.GetValue<int>("skill_id"));
            // 回能
            LPCMapping cureMap = new LPCMapping ();
            cureMap.Add("mp", data.GetValue<int>("cure_mp"));
            LPCMapping sourceProfile = sourceOb.GetProfile();
            sourceProfile.Add("cookie", Game.NewCookie(sourceOb.GetRid()));
            (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 神魔灭杀触发追加回合作用脚本
/// </summary>
public class SCRIPT_6778 : Script
{
    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        //Property trigger = _param[0] as Property;
        Property propOb = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;
        //LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // 提示
        BloodTipMgr.AddSkillTip(propOb,propValue[0].AsArray[1].AsInt);
        // 上神魔灭杀状态，增强伤害
        LPCMapping condition = new LPCMapping ();
        condition.Add("round", 1);
        condition.Add("round_cookie", triggerPara.GetValue<LPCMapping>("source_profile").GetValue<string>("cookie"));
        condition.Add("props", new LPCArray(new LPCArray(25, propValue[0].AsArray[0].AsInt)));
        propOb.ApplyStatus("B_SHENMO_ATK", condition);
        // 追加回合
        RoundCombatMgr.DoAdditionalRound(propOb, LPCMapping.Empty);
        // 返回数据
        return true;
    }
}

/// <summary>
/// 获得回合触发技能释放作用脚本
/// </summary>
public class SCRIPT_6779 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};

    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        //Property trigger = _param[0] as Property;
        Property propOb = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;
        //LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // 技能释放
        for (int i = 0; i < propValue.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);
            if (sourceOb.CheckStatus(statusList))
                continue;
            CombatMgr.DoCastSkill(sourceOb, propOb, propValue[i].AsArray[1].AsInt, LPCMapping.Empty);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创指定技能反击
/// </summary>
public class SCRIPT_6780 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取目标
        if (trigger.CheckStatus("DIED"))
            return true;

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", propValue[0].AsArray[0].AsInt);
        counterMap.Add("pick_rid", trigger.GetRid());

        RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
        // 首次反击成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 获得回合触发能量转生命作用脚本
/// </summary>
public class SCRIPT_6781 : Script
{
    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        //Property trigger = _param[0] as Property;
        Property propOb = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;
        //LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // 消耗所有能量，转换为生命
        int curMp = propOb.Query<int>("mp");
        if (curMp == 0)
            return false;
        int cureHp = Game.Multiple(propOb.QueryAttrib("max_hp"), curMp / propValue[0].AsArray[0].AsInt * propValue[0].AsArray[1].AsInt, 1000);
        // 消耗能量
        LPCMapping data = new LPCMapping ();
        data.Add("mp", curMp);
        propOb.CostAttrib(data);
        // 回血
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("hp", cureHp);
        LPCMapping sourceProfile = propOb.GetProfile();
        sourceProfile.Add("cookie",Game.NewCookie(propOb.GetRid()));
        (propOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 获得回合触发清除状态作用脚本
/// </summary>
public class SCRIPT_6782 : Script
{
    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        //筛选减益状态
        List<LPCMapping> allStatus = propOb.GetAllStatus();
        List<int> debuffList = new List<int>();

        // 遍历所有对象身上的状态
        for (int status = 0; status < allStatus.Count; status++)
        {
            // 获取状态的信息
            CsvRow info = StatusMgr.GetStatusInfo(allStatus[status].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            if (info.Query<int>("status_type") != StatusConst.TYPE_DEBUFF)
                continue;

            // 清除列表
            debuffList.Add(allStatus[status].GetValue<int>("cookie"));
        }

        // 没有debuff需要处理
        if (debuffList.Count == 0)
            return false;

        // 获取属性
        LPCArray props = propValue[0].AsArray;

        // 技能提示
        BloodTipMgr.AddSkillTip(propOb, props[0].AsInt);

        // 获取当前最大血量
        int maxHp = propOb.QueryAttrib("max_hp");

        // 每个debuff需要扣除血量
        int selfHpCost = Game.Multiple(maxHp, props[1].AsInt, 1000);

        // 自身扣血
        if (selfHpCost >= propOb.Query<int>("hp"))
        {
            propOb.Set("hp", LPCValue.Create(0));
            TRY_DO_DIE.Call(propOb);
            return false;
        }

        // 计算修正后的fixMaxHp
        int curHp = propOb.Query<int>("hp");
        int debuffCount = debuffList.Count;
        int costHp = Math.Min(curHp % selfHpCost, debuffCount) + selfHpCost * debuffCount;

        // 设置血量
        propOb.Set("hp", LPCValue.Create(Math.Max(curHp - costHp, 0)));

        // 清除减益状态
        propOb.ClearStatusByCookie(debuffList, StatusConst.CLEAR_TYPE_BREAK);

        // 返回数据
        return true;
    }
}


/// <summary>
/// 触发属性效果作用脚本，攻击概率目标上状态
/// </summary>
public class SCRIPT_6783 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 检查成功率
        int skillId = propValue[0].AsArray[3].AsInt;
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt + upRate)
            return false;

        // 计算效果命中
        // 攻击者效果命中、防御者效果抵抗、克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, trigger);
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(sourceOb.QueryAttrib("accuracy_rate"),
            trigger.QueryAttrib("resist_rate"),
            restrain);

        // 计算效果次数，例如持续伤害这种，可以复数存在的状态
        int statusNum = propValue[0].AsArray[4].AsInt;
        for (int i = 0; i < statusNum; i++)
        {
            if (RandomMgr.GetRandom() < rate)
            {
                string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
                BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);
                return false;
            }

            LPCMapping condition = new LPCMapping();
            condition.Add("round", propValue[0].AsArray[1].AsInt);
            condition.Add("round_cookie", triggerPara.GetValue<string>("cookie"));
            condition.Add("source_profile", sourceProfile);

            trigger.ApplyStatus(propValue[0].AsArray[2].AsString, condition);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发二段并清除冻结状态
/// </summary>
public class SCRIPT_6784 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        int campId = (trigger.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "DIED" });

        LPCArray freezeList = new LPCArray();

        foreach (Property checkOb in finalList)
        {
            if (checkOb.CheckStatus("D_FREEZE"))
                freezeList.Add(checkOb.GetRid());
        }

        // 获取触发属性
        triggerPara.Add("freeze_list", freezeList);

        if (freezeList.Count == 0)
            return false;

        LPCMapping extraPara = LPCMapping.Empty;
        extraPara.Add("freeze_list", freezeList);

        LPCMapping actionPara = new LPCMapping();
        actionPara.Add("skill_id", propValue[0].AsArray[1].AsInt);
        actionPara.Add("pick_rid", finalList[0].GetRid());
        actionPara.Add("extra_para", extraPara);

        // 提示技能
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[2].AsInt);

        // 执行连击
        RoundCombatMgr.DoComboRound(trigger, actionPara);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，全队回能量（废弃）
/// </summary>
public class SCRIPT_6785 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY", "DIED" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取友军单位列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propValue[0].AsArray[0].AsInt);

        List<Property> finalList = new List<Property>();
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus(banStatusList))
                finalList.Add(targetOb);
        }

        foreach (Property finalOb in finalList)
        {
            // 回血
            int mpCure = propValue[0].AsArray[1].AsInt;
            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("mp", mpCure);
            sourceProfile.Add("cookie",Game.NewCookie(finalOb.GetRid()));
            (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 死亡触发来源释放上状态作用脚本(光明黑暗守护者)
/// </summary>
public class SCRIPT_6786 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取攻击源
        Property trigger = _param[0] as Property;

        if (trigger.CheckStatus("DIED"))
            return false;

        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 光环源
        for (int i = 0; i < propValue.Count; i++)
        {
            Property haloOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            // 光环源死亡或者来自光环源则不作用
            if (haloOb.CheckStatus("DIED") || sourceOb.Equals(haloOb))
                continue;

            // 光环源上状态
            LPCArray statusList = propValue[i].AsArray[2].AsArray;

            for (int t = 0; t < statusList.Count; i++)
            {
                LPCMapping condition = new LPCMapping();
                condition.Add("round", statusList[t].AsMapping.GetValue<int>("round"));
                haloOb.ApplyStatus(statusList[t].AsMapping.GetValue<string>("status_id"), condition);
            }

            // 技能提示
            BloodTipMgr.AddSkillTip(haloOb, propValue[i].AsArray[1].AsInt);
        }

        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，治疗己方生命最低单位，根据自身生命百分比回血，带被动技能提示
/// </summary>
public class SCRIPT_6787 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 取最大生命
        int maxHp = trigger.QueryAttrib("max_hp");

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        int hpCure = Mathf.Max(propValue[0].AsArray[1].AsInt * maxHp / 1000, 0);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);

        // 筛选己方生命最低单位
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;

        // 遍历各个角色
        foreach (Property checkOb in targetList)
        {
            // 角色已经死亡
            if (checkOb.CheckStatus("DIED"))
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(checkOb.Query<int>("hp"), checkOb.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(checkOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(checkOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return false;

        Property finalOb = selectOb[RandomMgr.GetRandom(selectOb.Count)];

        if (finalOb.CheckStatus(banStatusList))
            return false;

        // 技能提示
        BloodTipMgr.AddSkillTip(finalOb, propValue[0].AsArray[0].AsInt);
        // 通知ReceiveCure
        sourceProfile.Add("cookie",Game.NewCookie(finalOb.GetRid()));
        (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，必定追加回合，记录战场回合
/// </summary>
public class SCRIPT_6788 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);
        // 记录战场回合
        trigger.SetTemp("trigger_round_cookie", LPCValue.Create(trigger.QueryTemp<string>("round_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发自身释放技能，无任何限制，作用脚本
/// </summary>
public class SCRIPT_6789 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        int skillId = propValue[0].AsArray[0].AsInt;
        BloodTipMgr.AddSkillTip(trigger, skillId);

        CombatMgr.DoCastSkill(trigger, trigger, skillId, LPCMapping.Empty);

        return true;
    }
}

/// <summary>
/// 死亡触发来源释放扣血作用脚本(好友地下城BOSS专用)
/// </summary>
public class SCRIPT_6790 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取攻击源
        //Property trigger = _param[0] as Property;
        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 光环源
        Property haloOb = Rid.FindObjectByRid(propValue[0].AsArray[0].AsString);
        // 光环附带的扣血参数
        LPCArray costValue = propValue[0].AsArray[1].AsArray;

        // 光环源死亡或者来自光环源则不作用
        if (haloOb.CheckStatus("DIED") || sourceOb.Equals(haloOb))
            return false;

        // 检查自身等级是否高于50级，且觉醒
        int selfHpCost = 0;
        if (sourceOb.Query<int>("level") > costValue[0].AsInt && sourceOb.Query<int>("rank") > costValue[1].AsInt)
        {
            // 光环源扣血
            selfHpCost = Game.Multiple(haloOb.QueryAttrib("max_hp"), costValue[3].AsInt, 1000);
            if (selfHpCost >= haloOb.Query<int>("hp"))
            {
                haloOb.Set("hp", LPCValue.Create(0));
                TRY_DO_DIE.Call(haloOb);
                return false;
            }

            haloOb.Set("hp", LPCValue.Create(Math.Max(haloOb.Query<int>("hp") - selfHpCost, 0)));
            return true;
        }

        // 光环源扣血
        selfHpCost = Game.Multiple(haloOb.QueryAttrib("max_hp"), costValue[2].AsInt, 1000);
        if (selfHpCost >= haloOb.Query<int>("hp"))
        {
            haloOb.Set("hp", LPCValue.Create(0));
            TRY_DO_DIE.Call(haloOb);
            return false;
        }

        haloOb.Set("hp",LPCValue.Create(Math.Max(haloOb.Query<int>("hp") - selfHpCost, 0)));

        return true;
    }
}

/// <summary>
/// 获得回合时控制状态下，则触发技能释放并清除控制状态作用脚本
/// </summary>
public class SCRIPT_6791 : Script
{
    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        Property propOb = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;
        //LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // 判断能量是否足够
        LPCMapping mpCostMap = SkillMgr.GetCasTCost(propOb, propValue);
        if (propOb.Query<int>("mp") < mpCostMap.GetValue<int>("mp"))
            return false;

        // 检查控制状态并解除之
        List<LPCMapping> allStatus = propOb.GetAllStatus();
        List<int> removeList = new List<int>();
        CsvRow info;
        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取控制状态标识
            LPCMapping limitData = info.Query<LPCMapping>("limit_round_args");
            // 添加到清除列表
            if (limitData.ContainsKey("ctrl_id"))
                removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        if (removeList.Count == 0)
            return false;

        // 清除控制状态
        propOb.ClearStatusByCookie(removeList, StatusConst.CLEAR_TYPE_BREAK);

        // 技能释放
        CombatMgr.DoCastSkill(propOb, propOb, propValue, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据所受伤害比全队回血，奉献定义专用
/// </summary>
public class SCRIPT_6792 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取伤害量
        LPCMapping points = triggerPara.GetValue<LPCMapping>("damage_map").GetValue<LPCMapping>("points");
        int dmgedValue = points.GetValue<int>("hp");
        if (dmgedValue == 0)
            return false;

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        int hpCure = Mathf.Max(Game.Multiple(dmgedValue, propValue[0].AsArray[1].AsInt), 0);

        // 计算升级效果
        int skillId = propValue[0].AsArray[0].AsInt;
        int cureRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), trigger.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_HP_CURE);
        hpCure += Game.Multiple(hpCure, cureRate, 1000);
        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        //sourceProfile.Add("skill_id", 11000);
        sourceProfile.Add("cookie",Game.NewCookie(trigger.GetRid()));

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger,skillId);

        // 全体通知ReceiveCure
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);
        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (targetOb.CheckStatus(banStatusList))
                continue;
            (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 黑血吊针专用触发作用濒死上状态
/// </summary>
public class SCRIPT_6793 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        int skillId = propValue[0].AsArray[0].AsInt;

        // 上状态 血量设置为 1 不死
        int round = propValue[0].AsArray[2].AsInt;
        trigger.Set("hp", LPCValue.Create(1));
        LPCMapping condition = new LPCMapping();
        condition.Add("round", round);

        trigger.ApplyStatus(propValue[0].AsArray[1].AsString, condition);

        // 上伤害无效状态
        LPCMapping immuCondition = new LPCMapping();
        immuCondition.Add("skill_id", skillId);
        immuCondition.Add("round", round);
        trigger.ApplyStatus("B_DMG_IMMUNE_SP", immuCondition);

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger,skillId);

        // 被动技能CD
        CdMgr.SkillCooldown(trigger, skillId);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，命中触发溅射，火焰溅射专用
/// </summary>
public class SCRIPT_6794 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        LPCMapping points = triggerPara.GetValue<LPCMapping>("damage_map").GetValue<LPCMapping>("points");
        if (!points.ContainsKey("hp"))
            return false;

        // 收集溅射目标
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (Property.Equals(targetOb, trigger) || targetOb.CheckStatus("DIED"))
                continue;

            finalList.Add(targetOb);
        }

        // 技能提示
        int skillId = propValue[0].AsArray[0].AsInt;

        BloodTipMgr.AddSkillTip(sourceOb, skillId);

        // 计算溅射伤害
        int dmgedValue = points.GetValue<int>("hp");
        int baseSplash = Game.Multiple(dmgedValue, propValue[0].AsArray[1].AsInt, 1000);
        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);
        baseSplash += Game.Multiple(baseSplash, dmgRate, 1000);
        for (int i = 0; i < finalList.Count; i++)
        {
            int restrain = ElementMgr.GetMonsterCounter(sourceOb, finalList[i]);
            int damageType = CombatConst.DAMAGE_TYPE_ATTACK;
            LPCMapping damageMap = new LPCMapping();
            LPCMapping extraPoints = new LPCMapping();
            extraPoints.Add("hp", baseSplash);
            damageMap.Add("points", extraPoints);
            damageMap.Add("restrain", restrain);
            damageMap.Add("damage_type", damageType);
            damageMap.Add("target_rid", finalList[i].GetRid());

            LPCMapping sourceProfile = new LPCMapping ();
            sourceProfile = sourceOb.GetProfile();
            sourceProfile.Add("damage_map", damageMap);
            sourceProfile.Add("skill_id", skillId);
            sourceProfile.Add("cookie", triggerPara.GetValue<string>("cookie"));
            sourceProfile.Add("original_cookie", triggerPara.GetValue<string>("original_cookie"));
            sourceProfile.Add("type", triggerPara.GetValue<int>("type"));
            // 通知玩家受创
            (finalList[i] as Char).ReceiveDamage(sourceProfile, damageType, damageMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 血凝吊针专用触发上护盾回能作用脚本
/// </summary>
public class SCRIPT_6795 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        int skillId = propValue[0].AsArray[0].AsInt;

        LPCMapping points = triggerPara.GetValue<LPCMapping>("damage_map").GetValue<LPCMapping>("points");
        if (!points.ContainsKey("hp"))
            return false;

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger,skillId);

        // 护盾值计算
        int value = Game.Multiple(points.GetValue<int>("hp"), propValue[0].AsArray[1].AsInt, 1000);
        // 计算升级效果
        int shieldRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), trigger.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SHIELD);
        value += Game.Multiple(value, shieldRate, 1000);
        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[2].AsInt);
        condition.Add("can_absorb_damage", value);

        // 附加状态
        trigger.ApplyStatus("B_HP_SHIELD", condition);

        // 回能
        if (!trigger.CheckStatus("D_NO_MP_CURE") && !trigger.CheckStatus("B_CAN_NOT_CHOOSE"))
        {
            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("mp", propValue[0].AsArray[3].AsInt);

            // 执行回血操作
            sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));
            (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 死亡触发来源释放上状态作用脚本(死者遗愿专用)
/// </summary>
public class SCRIPT_6796 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取攻击源
        //Property trigger = _param[0] as Property;

        //Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        for (int i = 0; i < propValue.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);
            if (sourceOb.CheckStatus("DIED"))
                continue;

            // 上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("props", new LPCArray(new LPCArray(331, propValue[i].AsArray[1].AsInt)));

            // 检查状态数量
            List<LPCMapping> allStatus = sourceOb.GetStatusCondition("B_BASE_ATK_UP");

            if (allStatus.Count >= propValue[i].AsArray[2].AsInt)
                continue;
            // 技能提示
            BloodTipMgr.AddSkillTip(sourceOb,propValue[i].AsArray[3].AsInt);

            sourceOb.ApplyStatus("B_BASE_ATK_UP", condition);
        }

        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，击晕
/// </summary>
public class SCRIPT_6797 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue; 
        string propName = _param[6] as String;

        // 技能提示
        BloodTipMgr.AddSkillTip(sourceOb,propValue[0].AsArray[0].AsInt);

        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[2].AsInt);

        trigger.ApplyStatus("D_STUN", condition);

        // 记录原始技能cookie
        trigger.Set("skill_temp_cookie", LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 首次击晕成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用攻击目标随机个数队友进行协同攻击
/// </summary>
public class SCRIPT_6798 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        // 随机挑选N个队友剔除死亡、眩晕、冻结、麻痹、混乱、挑衅、睡眠
        List<Property> finalList = new List<Property>();
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId,
            new List<string>(){ "DIED", "D_FREEZE", "D_STUN", "D_SLEEP", "D_CHAOS", "D_PROVOKE", "D_PALSY" });

        // 去除自身收集
        targetList.Remove(trigger);

        // 随机筛选目标
        while (true)
        {
            // 列表为空跳出循环
            if (targetList.Count == 0)
                break;

            // 最终列表大于随机个数跳出循环
            if (finalList.Count >= propValue[0].AsArray[2].AsInt)
                break;

            // 随机筛选目标
            Property ob = targetList[RandomMgr.GetRandom(targetList.Count)];

            // 加入最终目标列表
            finalList.Add(ob);

            // 从目标列表中移除对象
            targetList.Remove(ob);
        }

        for (int i = 0; i < finalList.Count; i++)
        {
            // 协同攻击
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("pick_rid", targetOb.GetRid());
            counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(finalList[i], SkillType.SKILL_TYPE_1));
            counterMap.Add("joint_rid", trigger.GetRid());

            // 发起协同回合
            RoundCombatMgr.DoJointRound(finalList[i], counterMap);
        }

        // 首次成功记录技能源cookie
        trigger.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger,propValue[0].AsArray[0].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用上状态
/// </summary>
public class SCRIPT_6799 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[1].AsInt);

        trigger.ApplyStatus(propValue[0].AsArray[0].AsString, condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性检查技能CD脚本
/// </summary>
public class SCRIPT_6800 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property trigger = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        //LPCArray triggerPara = _param[4] as LPCArray;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (CdMgr.SkillIsCooldown(trigger, propValue[0].AsArray[0].AsInt))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性检查技能CD脚本，检查是否可复活
/// </summary>
public class SCRIPT_6801 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property trigger = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 检查是否被不能复活限制(要害打击)
        if (triggerPara.ContainsKey("skill_id"))
        {
            if (triggerPara.GetValue<int>("skill_id") == 150)
                return false;
        }

        if (CdMgr.SkillIsCooldown(trigger, propValue[0].AsArray[0].AsInt))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 上状态触发节点检查脚本
/// </summary>
public class SCRIPT_6802 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property trigger = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        // 检查该触发状态是否为控制状态
        CsvRow statusInfo;

        statusInfo = StatusMgr.GetStatusInfo(triggerPara.GetValue<int>("status"));
        if (statusInfo.Query<int>("affect") != StatusConst.STOP_ACTION)
            return false;

        LPCMapping condition = triggerPara.GetValue<LPCMapping>("condition");
        string cookie = condition.GetValue<string>("round_cookie");
        string propName = _param[5] as String;
        LPCArray effectObList = new LPCArray();

        // propValue里记录了所有光环源信息
        for (int i = 0; i < propValue.Count; i++)
        {
            Property ob = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            // 如果光环源死亡跳过
            if (ob.CheckStatus("DIED"))
                continue;

            // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
            if (ob.QueryTemp("trigger_cookie/" + propName) != null)
            {
                if (string.Equals(cookie, ob.QueryTemp("trigger_cookie/" + propName).AsString))
                    continue;
            }

            LPCMapping data = new LPCMapping();
            data.Add("prop_value", propValue[i].AsArray);
            effectObList.Add(data);
        }

        // 返回数据
        return LPCValue.Create(effectObList);
    }
}

/// <summary>
/// 回合开始触发节点检查脚本（只在正常回合起效）
/// </summary>
public class SCRIPT_6803 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property trigger = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        if (triggerPara.ContainsKey("provoke_id"))
        {
            if (triggerPara.GetValue<int>("provoke_id") == 1)
                return false;
        }

        // 检查技能CD
        if (CdMgr.SkillIsCooldown(trigger, propValue[0].AsArray[1].AsInt))
            return false;

        if (triggerPara.GetValue<int>("type") != RoundCombatConst.ROUND_TYPE_NORMAL && 
            triggerPara.GetValue<int>("type") != RoundCombatConst.ROUND_TYPE_ADDITIONAL)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 攻击带死亡标记的单位触发光环源协同攻击的检查脚本
/// </summary>
public class SCRIPT_6804 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property trigger = _param[0] as Property;
        //LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        Property pickOb = Rid.FindObjectByRid(triggerPara.GetValue<string>("pick_rid"));
        if (pickOb.CheckStatus("D_MARK") || pickOb.CheckStatus("D_MARK_SP"))
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 触发反击检查脚本
/// </summary>
public class SCRIPT_6805 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((damageType & selectArgs.AsInt) == 0)
            return false;

        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        string propName = _param[5] as String;

        // 如果来源是伤害反射，则不处理
        // 如果来源是持续伤害，则不处理
        int originalDamageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((originalDamageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT ||
            (originalDamageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 检查攻击是否将要造成死亡
        LPCMapping points = triggerPara.GetValue<LPCMapping>("damage_map").GetValue<LPCMapping>("points");
        if (points.GetValue<int>("hp") >= sourceOb.Query<int>("hp"))
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        int counterRate = propValue[0].AsArray[1].AsInt + propValue[0].AsArray[3].AsInt;

        // 概率反击
        if (RandomMgr.GetRandom() >= counterRate)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 上状态触发节点检查脚本(愤怒状态堆叠层数达标时触发)
/// </summary>
public class SCRIPT_6806 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property source = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string statusString = propValue[0].AsArray[0].AsString;

        // 检查是否为愤怒状态
        CsvRow statusInfo;
        statusInfo = StatusMgr.GetStatusInfo(triggerPara.GetValue<int>("status"));
        if (statusInfo.Query<string>("alias") != statusString)
            return false;

        // 检查对应状态是否满足设定数量
        if (source.GetStatusCondition(statusString).Count < propValue[0].AsArray[1].AsInt)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 概率触发节点检查脚本
/// </summary>
public class SCRIPT_6807 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property source = _param[0] as Property;

        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((damageType & selectArgs.AsInt) == 0)
            return false;

        Property targetOb = _param[4] as Property;
        if (targetOb.Query<int>("hp") <= 0)
            return false;

        LPCArray propValue = (_param[1] as LPCValue).AsArray;

        // 检查只有正常、追加、反击回合起效
        int roundType = triggerPara.GetValue<int>("type");
        if (roundType == RoundCombatConst.ROUND_TYPE_NORMAL ||
            roundType == RoundCombatConst.ROUND_TYPE_ADDITIONAL ||
            roundType == RoundCombatConst.ROUND_TYPE_GASSER ||
            roundType == RoundCombatConst.ROUND_TYPE_COUNTER)
        {
            if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
                return true;
        }

        // 返回数据
        return false;
    }
}

/// <summary>
/// 死亡触发技能释放检查脚本
/// </summary>
public class SCRIPT_6808 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        int propValue = (_param[1] as LPCValue).AsInt;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        CsvRow skillInfo = SkillMgr.GetSkillInfo(propValue);

        int originSkillId = skillInfo.Query<int>("original_skill_id");

        // 检查是否在CD中
        if (CdMgr.SkillIsCooldown(sourceOb, originSkillId))
            return false;

        // 检查蓝是否足够
        if (sourceOb.Query<int>("mp") < skillInfo.Query<LPCMapping>("cost_arg").GetValue<int>("mp"))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 转阶段触发技能检查脚本
/// </summary>
public class SCRIPT_6809 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;

        // 检查重生次数
        if (sourceOb.QueryAttrib("reborn_times") <= 0)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 连续触发技能检查脚本
/// </summary>
public class SCRIPT_6810 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        int roundype = triggerPara.GetValue<int>("type");
        string propName = _param[5] as String;

        // 检查回合类型
        if (roundype == RoundCombatConst.ROUND_TYPE_COMBO ||
            roundype == RoundCombatConst.ROUND_TYPE_COUNTER ||
            roundype == RoundCombatConst.ROUND_TYPE_JOINT ||
            roundype == RoundCombatConst.ROUND_TYPE_RAMPAGE)
            return false;

        // 获取角色对象
        // 如果角色对象不存在，或者角色已经死亡不处理
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        Property targetOb = Rid.FindObjectByRid(damageMap.GetValue<string>("target_rid"));
        if (targetOb == null ||
            targetOb.CheckStatus("DIED"))
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 自身生命低于百分比触发
/// </summary>
public class SCRIPT_6811 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCValue propValue = _param[1] as LPCValue;

        int rate = Game.Divided(sourceOb.Query<int>("hp") ,sourceOb.QueryAttrib("max_hp"));
        if (rate >= propValue.AsArray[0].AsArray[0].AsInt)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 装备追加回合效果检查脚本(只能在正常回合下进行触发，即限制了战场回合触发次数)
/// </summary>
public class SCRIPT_6812 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property trigger = _param[0] as Property;
        //LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        if (triggerPara.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_NORMAL)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 触发检查脚本，检查回合类型
/// </summary>
public class SCRIPT_6813 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        int roundType = triggerPara.GetValue<int>("type");

        // 检查是不是普通回合和追加回合
        if (roundType != RoundCombatConst.ROUND_TYPE_NORMAL &&
            roundType != RoundCombatConst.ROUND_TYPE_ADDITIONAL &&
            roundType != RoundCombatConst.ROUND_TYPE_GASSER)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 检查重生次数触发检查脚本
/// </summary>
public class SCRIPT_6814 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        //LPCValue propValue = _param[1] as LPCValue;

        // 检查重生次数
        if (sourceOb.Query<int>("had_reborn_times") > 0)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 概率触发节点检查脚本(需要检查是否为普通攻击导致)
/// </summary>
public class SCRIPT_6815 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property source = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        int skillId = triggerPara.GetValue<int>("skill_id");

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        if (skillInfo.Query<int>("attrib_type") == SkillType.SKILL_TYPE_NORMAL)
            return false;

        // 检查是不是连击回合
        if (triggerPara.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_COMBO)
            return false;

        if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 检查濒死光环源是否在技能CD中的触发检查脚本
/// </summary>
public class SCRIPT_6816 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;

        LPCArray effectObList = new LPCArray();

        // propValue里记录了所有光环源信息
        for (int i = 0; i < propValue.Count; i++)
        {
            Property ob = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            // 如果光环源死亡跳过
            if (ob.CheckStatus("DIED"))
                continue;

            int skillId = propValue[i].AsArray[3].AsInt;

            // 如果有诅咒状态需要特殊处理光环源CD
            if (sourceOb.CheckStatus("D_CURSE"))
            {
                CdMgr.SkillCooldown(ob, skillId);
                continue;
            }

            if (CdMgr.SkillIsCooldown(ob, skillId))
                continue;

            LPCMapping data = new LPCMapping();
            data.Add("prop_value", propValue[i].AsArray);
            effectObList.Add(data);
        }

        if (effectObList.Count == 0)
            return false;

        // 返回数据
        return LPCValue.Create(effectObList);
    }
}

/// <summary>
/// 装备触发击晕检查脚本
/// </summary>
public class SCRIPT_6817 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        Property sourceOb = _param[0] as Property;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        Property targetOb = _param[4] as Property;
        string propName = _param[5] as String;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");

        if (points.ContainsKey("mp") && !points.ContainsKey("hp"))
            return false;

        // 攻击源的cookie相同则不起效，防止同技能的多HIT多击晕
        string cookie = triggerPara.GetValue<string>("original_cookie");
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 判断技能原始cookie是否一致，不一致则生效，一致则不生效
        if (cookie.Equals(targetOb.Query<string>("skill_temp_cookie")))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 刚毅被动触发回血检查脚本
/// </summary>
public class SCRIPT_6818 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        int roundType = triggerPara.GetValue<int>("type");

        Property sourceOb = _param[0] as Property;

        // 死亡检查
        if (sourceOb.CheckStatus("DIED"))
            return false;

        // 检查回合类型，只有正常回合与追加回合起效
        if (roundType == RoundCombatConst.ROUND_TYPE_NORMAL ||
            roundType == RoundCombatConst.ROUND_TYPE_ADDITIONAL ||
            roundType == RoundCombatConst.ROUND_TYPE_GASSER)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 受创回血触发检查脚本
/// </summary>
public class SCRIPT_6819 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property source = _param[0] as Property;

        // 如果血量为0则不起效
        if (source.Query<int>("hp") <= 0)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创回血触发检查脚本耗蓝版本
/// </summary>
public class SCRIPT_6820 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property source = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;

        // 蓝量检查
        int mpCost = SkillMgr.GetCasTCost(source, propValue[0].AsArray[0].AsInt).GetValue<int>("mp");
        if (mpCost > source.Query<int>("mp"))
            return false;

        // 如果血量为0则不起效
        if (source.Query<int>("hp") <= 0)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发反击概率检查脚本
/// </summary>
public class SCRIPT_6821 : Script
{
    public override object Call(params object[] _param)
    {
        // sourceOb, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        int propValue = (_param[1] as LPCValue).AsInt;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string propName = _param[5] as String;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 如果来源是伤害反射，则不处理
        int damageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;

        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        //概率反击
        if (RandomMgr.GetRandom() >= propValue)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受创自身反击光环的触发反击概率检查脚本
/// </summary>
public class SCRIPT_6822 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string propName = _param[5] as String;

        // 如果来源是伤害反射，则不处理
        int damageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;

        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (propOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, propOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 反击概率检查
        // propValue里记录了所有光环源信息
        LPCArray counterObList = new LPCArray();
        for (int i = 0; i < propValue.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            // 角色对象不存在
            if (sourceOb == null)
                continue;

            // 概率限制
            int randomRate = RandomMgr.GetRandom();
            if (randomRate >= propValue[i].AsArray[1].AsInt)
                continue;
            LPCMapping data = new LPCMapping();
            data.Add("source_rid", propValue[i].AsArray[0].AsString);
            data.Add("skill_id", propValue[i].AsArray[2].AsInt);
            counterObList.Add(data);
        }

        if (counterObList.Count == 0)
            return false;

        // 返回数据
        return LPCValue.Create(counterObList);
    }
}

/// <summary>
/// 催眠大师追加回合检查脚本
/// </summary>
public class SCRIPT_6823 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        int propValue = (_param[1] as LPCValue).AsInt;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        // 检查场上被睡眠状态的敌方单位数
        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        List<Property> allList = RoundCombatMgr.GetPropertyList(campId);
        int sleepNum = 0;
        for (int i = 0; i < allList.Count; i++)
        {
            if (allList[i].CheckStatus("D_SLEEP"))
                sleepNum += 1;
        }

        // 检查成功率
        if (RandomMgr.GetRandom() >= propValue * sleepNum)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创指定技能反击检查脚本
/// </summary>
public class SCRIPT_6824 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};

    public override object Call(params object[] _param)
    {
        //source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string propName = _param[5] as String;

        // 如果有控制列状态在身上，则不触发防御反击
        if (sourceOb.CheckStatus(statusList))
            return false;

        // 如果来源是伤害反射，则不处理
        int damageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;

        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 获取已反击次数
        int counterTimes = sourceOb.QueryTemp<int>("counter_times");

        // 检查已反击次数，如果传入参数限制是-1则不需要进行次数判断，单仍需记录
        if (propValue[0].AsArray[1].AsInt > 0 && counterTimes >= propValue[0].AsArray[1].AsInt)
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创反击检查脚本，成功率与自身生命相关
/// </summary>
public class SCRIPT_6825 : Script
{
    public override object Call(params object[] _param)
    {
        //source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string propName = _param[5] as String;

        // 如果来源是伤害反射，则不处理
        int damageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;

        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 检查攻击是否将要造成死亡
        LPCMapping points = triggerPara.GetValue<LPCMapping>("damage_map").GetValue<LPCMapping>("points");
        if (points.GetValue<int>("hp") >= sourceOb.Query<int>("hp"))
            return false;

        // 检查成功率
        int maxHp = sourceOb.QueryAttrib("max_hp");
        int lostHpRate = Game.Divided((maxHp - sourceOb.Query<int>("hp")), maxHp);
        int rate = propValue[0].AsArray[1].AsInt + propValue[0].AsArray[3].AsInt * lostHpRate / propValue[0].AsArray[2].AsInt;
        if (RandomMgr.GetRandom() >= rate)
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 释放技能概率触发检查脚本
/// </summary>
public class SCRIPT_6826 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        int skillId = triggerPara.GetValue<int>("skill_id");

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        if (skillInfo.Query<int>("attrib_type") == SkillType.SKILL_TYPE_NORMAL)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 概率触发
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 神魔灭杀光环的触发概率检查脚本
/// </summary>
public class SCRIPT_6827 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        // 如果不是正常回合以及追加回合则不起效
        int type = triggerPara.GetValue<int>("type");
        if (type != RoundCombatConst.ROUND_TYPE_NORMAL &&
            type != RoundCombatConst.ROUND_TYPE_ADDITIONAL)
            return false;

        // 概率检查
        // propValue里记录了所有光环源信息
        LPCArray cureList = new LPCArray();
        for (int i = 0; i < propValue.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            // 角色对象不存在
            if (sourceOb == null)
                continue;

            // 概率限制
            int randomRate = RandomMgr.GetRandom();
            if (randomRate >= propValue[i].AsArray[1].AsInt)
                continue;
            LPCMapping data = new LPCMapping();
            data.Add("source_rid", propValue[i].AsArray[0].AsString);
            data.Add("cure_mp", propValue[i].AsArray[2].AsInt);
            data.Add("skill_id", propValue[i].AsArray[3].AsInt);
            cureList.Add(data);
        }

        if (cureList.Count == 0)
            return false;

        // 返回数据
        return LPCValue.Create(cureList);
    }
}

/// <summary>
/// 满能量触发检查脚本
/// </summary>
public class SCRIPT_6828 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;

        // 获取治愈类型
        int cureType = triggerPara.GetValue<int>("cure_type");

        // 如果不是需要触发的治愈类型，不能触发
        if ((cureType & selectArgs.AsInt) == 0)
            return false;

        Property propOb = _param[0] as Property;
        if (propOb.Query<int>("mp") < propOb.QueryAttrib("max_mp"))
            return false;

        // 判断trigger_round_cookie是否一致
        if (string.Equals(propOb.QueryTemp<string>("trigger_round_cookie"), propOb.QueryTemp<string>("round_cookie")))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 能量受创指定技能反击检查脚本
/// </summary>
public class SCRIPT_6829 : Script
{
    public override object Call(params object[] _param)
    {
        //source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        Property targetOb = _param[4] as Property;
        string propName = _param[5] as String;
        LPCMapping oriDmgMap = triggerPara.GetValue<LPCMapping>("original_damage_map");
        LPCMapping points = oriDmgMap.GetValue<LPCMapping>("points");

        // 没有能量受创不处理
        if (!points.ContainsKey("mp"))
            return false;
        // 如果来源是伤害反射，则不处理
        int damageType = oriDmgMap.GetValue<int>("damage_type");
        if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;
        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;
        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 记录能量受创点数
        targetOb.SetTemp("mp_dmg",LPCValue.Create(targetOb.QueryTemp<int>("mp_dmg") + points.GetValue<int>("mp")));

        // 检查能量受创点数
        if (targetOb.QueryTemp<int>("mp_dmg") < propValue[0].AsArray[1].AsInt)
            return false;

        // 清空受创记录
        targetOb.SetTemp("mp_dmg",LPCValue.Create(0));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 技能CD状态检查的触发检查脚本
/// </summary>
public class SCRIPT_6830 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        int skillId = propValue[0].AsArray[0].AsInt;

        // 技能CD检查，CD时生效，不在CD时不起效
        if (!CdMgr.SkillIsCooldown(sourceOb, skillId))
            return false;

        int roundType = triggerPara.GetValue<int>("type");

        // 检查回合类型，只有正常回合与追加回合起效
        if (roundType == RoundCombatConst.ROUND_TYPE_NORMAL ||
            roundType == RoundCombatConst.ROUND_TYPE_ADDITIONAL ||
            roundType == RoundCombatConst.ROUND_TYPE_GASSER)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 触发检查脚本，每个战场回合仅限 1 次检查
/// </summary>
public class SCRIPT_6831 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property propOb = _param[0] as Property;
        //LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        string triggerRoundCookie = propOb.QueryTemp<string>("trigger_round_cookie");

        if (string.Equals(triggerRoundCookie,propOb.QueryTemp<string>("round_cookie")))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 复活次数检查触发脚本
/// </summary>
public class SCRIPT_6832 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        // 检查复活次数是否超限制
        if (sourceOb.QueryTemp<int>("spec_reborn_times") >= propValue[0].AsArray[1].AsInt && propValue[0].AsArray[1].AsInt != -1)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 暴击受创触发脚本检查，如果伤害为 0 则不触发
/// </summary>
public class SCRIPT_6833 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((damageType & selectArgs.AsInt) == 0)
            return false;

        // 如果血量为0则不起效
        Property source = _param[0] as Property;
        if (source.Query<int>("hp") <= 0)
            return false;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        if (points.GetValue<int>("hp") == 0 || !points.ContainsKey("hp"))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 出其不意反击检查脚本
/// </summary>
public class SCRIPT_6834 : Script
{
    public override object Call(params object[] _param)
    {
        // sourceOb, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        //int propValue = (_param[1] as LPCValue).AsInt;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;
        string propName = _param[5] as String;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 如果来源是伤害反射，则不处理
        int damageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        //if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
        //    return false;

        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        // 检查能量反击
        int conDamageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((conDamageType & selectArgs.AsInt) == 0)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 释放技能概率触发检查脚本（敏捷触发限制专用）
/// </summary>
public class SCRIPT_6835 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        //int skillId = triggerPara.GetValue<int>("skill_id");

        //CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 概率触发
        int rate = (sourceOb.QueryAttrib("agility") - 100) * 125 / 100;
        if (RandomMgr.GetRandom() >= Math.Min(rate, propValue[0].AsArray[0].AsInt))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发击晕检查脚本，扰乱缠绕专用
/// </summary>
public class SCRIPT_6836 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        // 获取触发来源的信息
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        Property targetOb = _param[4] as Property;
        string propName = _param[5] as String;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");

        if (points.ContainsKey("mp") && !points.ContainsKey("hp"))
            return false;

        // 攻击源的cookie相同则不起效，防止同技能的多HIT多击晕
        string cookie = triggerPara.GetValue<string>("original_cookie");
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 判断技能原始cookie是否一致，不一致则生效，一致则不生效
        if (cookie.Equals(targetOb.Query<string>("skill_temp_cookie")))
            return false;

        // 先检查是否格挡，如果格挡不触发
        if ((triggerPara.GetValue<int>("damage_type") & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return false;

        // 获取初始概率
        int rate = propValue[0].AsArray[1].AsInt;
        // 获取目标状态信息
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        CsvRow statusInfo;
        LPCArray debuffList = new LPCArray();
        // 遍历所有对象身上的状态
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = statusInfo.Query<int>("status_type");
            if (buff_type == StatusConst.TYPE_DEBUFF)
                debuffList.Add(allStatus[i].GetValue<int>("status_id"));
            else
                continue;
        }
        if (debuffList.Count > 0)
            rate = propValue[0].AsArray[3].AsInt;
        // 检查概率
        if (RandomMgr.GetRandom() >= rate)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发协同检查脚本
/// </summary>
public class SCRIPT_6837 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        // 获取触发来源的信息
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //Property targetOb = _param[4] as Property;
        string propName = _param[5] as String;

        // 概率检查
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[1].AsInt)
            return false;

        // 反击协同回合不起效
        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 攻击源的cookie相同则不起效，防止同技能的多HIT多击晕
        string cookie = triggerPara.GetValue<string>("original_cookie");
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创指定技能反击（通用）
/// </summary>
public class SCRIPT_6838 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取目标
        if (trigger.CheckStatus("DIED"))
            return false;

        // 技能提示
        BloodTipMgr.AddSkillTip(sourceOb, propValue[0].AsArray[2].AsInt);

        // 清除架势状态
        sourceOb.ClearStatus("B_PREPARE");

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", propValue[0].AsArray[1].AsInt);
        counterMap.Add("pick_rid", trigger.GetRid());

        RoundCombatMgr.DoCounterRound(sourceOb, counterMap);

        // 首次反击成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创指定技能反击检查脚本（通用）
/// </summary>
public class SCRIPT_6839 : Script
{
    public override object Call(params object[] _param)
    {
        //source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string propName = _param[5] as String;

        // 如果来源是伤害反射，则不处理
        int damageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;

        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 检查成功率
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 概率触发节点检查脚本
/// </summary>
public class SCRIPT_6840 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;

        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        if (points.ContainsKey("mp"))
            return false;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((damageType & selectArgs.AsInt) == 0)
            return false;

        Property targetOb = _param[4] as Property;
        string propName = _param[5] as String;
        if (targetOb.Query<int>("hp") <= 0)
            return false;

        // 回合类型判断
        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        string cookie = triggerPara.GetValue<string>("original_cookie");
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用，光环来源者进行协同攻击
/// </summary>
public class SCRIPT_6841 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        string targetRid = damageMap.GetValue<string>("target_rid");
        Property propOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        // 协同攻击
        // propValue里记录了所有光环源信息
        int jointNum = 0;
        for (int i = 0; i < propValue.Count; i++)
        {
            if (RandomMgr.GetRandom() >= propValue[i].AsArray[1].AsInt)
                continue;

            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            // 协同攻击
            LPCMapping counterMap = new LPCMapping();
            counterMap.Add("skill_id", SkillMgr.GetSkillByPosType(sourceOb, SkillType.SKILL_TYPE_1));
            counterMap.Add("pick_rid", targetRid);
            counterMap.Add("joint_rid", trigger.GetRid());
            counterMap.Add("invalid_assailant", 1);       // 添加无效凶手标识，不夺取击杀归属

            jointNum += 1;
            BloodTipMgr.AddSkillTip(sourceOb, propValue[i].AsArray[2].AsInt);
            // 发起协同回合
            RoundCombatMgr.DoJointRound(sourceOb, counterMap);
        }

        // 首次反击成功记录技能源cookie
        if (jointNum > 0)
            propOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用上凤凰重生状态脚本
/// </summary>
public class SCRIPT_6842 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[3].AsInt);
        condition.Add("cure_rate", propValue[0].AsArray[1].AsInt);
        condition.Add("mp_cure", propValue[0].AsArray[2].AsInt);
        condition.Add("source_profile", sourceProfile);
        condition.Add("skill_id", propValue[0].AsArray[0].AsInt);
        condition.Add("cast_status", propValue[0].AsArray[4].AsString);
        condition.Add("cast_status_round", propValue[0].AsArray[5].AsInt);

        // 附加状态
        trigger.ApplyStatus("B_REBORN_PASSIVE_APPLY", condition);

        // 被动技能CD
        CdMgr.SkillCooldown(trigger, propValue[0].AsArray[0].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 死亡触发技能（对伤害来源目标进行选择），作用脚本
/// </summary>
public class SCRIPT_6843 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        CombatMgr.DoCastSkill(trigger, targetOb, propValue, LPCMapping.Empty);

        CsvRow skillInfo = SkillMgr.GetSkillInfo(propValue);

        int originSkillId = skillInfo.Query<int>("original_skill_id");

        // 手动CD和扣蓝
        CdMgr.SkillCooldown(trigger, originSkillId);
        LPCMapping costMap = SkillMgr.GetCasTCost(trigger, originSkillId);
        trigger.CostAttrib(costMap);

        // 扣除重生次数
        int rebornTimes = trigger.QueryAttrib("reborn_times") - 1;
        trigger.Set("reborn_times", LPCValue.Create(rebornTimes));
        // 设置已重生次数
        trigger.Set("had_reborn_times", LPCValue.Create(trigger.Query<int>("had_reborn_times") + 1));

        return true;
    }
}

/// <summary>
/// 触发作用受创伤害反射(计算按照受创者自身攻击进行百分比加成，并且反伤给敌方全体)
/// </summary>
public class SCRIPT_6844 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        sourceProfile.Add("cookie",triggerPara.GetValue<string>("cookie"));
        sourceProfile.Add("original_cookie",triggerPara.GetValue<string>("original_cookie"));

        // 检查伤害来源，如果是自身，则不起效
        if (trigger.GetRid().Equals(sourceProfile.GetValue<string>("rid")))
            return false;

        // 获取伤害结构信息（没经过伤害分摊和护盾削减的伤害）
        if (!triggerPara.ContainsKey("original_damage_map"))
            return false;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("original_damage_map");

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");

        // 如果伤害来源是mp则不起效
        if (points.ContainsKey("mp"))
            return false;

        // 以受创者自身攻击百分比进行计算反伤伤害
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        int sourceAtk = sourceOb.QueryAttrib("attack");

        int reflectDamage = sourceAtk * propValue / 1000;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(trigger.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

        foreach (Property checkOb in finalList)
        {
            if (checkOb.QueryTemp<int>("halo/halo_can_not_choose") > 0)
                continue;

            // 包装反射伤害信息
            LPCMapping reDamageMap = new LPCMapping();
            LPCMapping rePoints = new LPCMapping();
            rePoints.Add("hp", reflectDamage);
            reDamageMap.Add("points", rePoints);
            // 记录克制关系和伤害类型
            reDamageMap.Add("restrain", ElementConst.ELEMENT_NEUTRAL);
            reDamageMap.Add("damage_type", damageMap.GetValue<int>("damage_type") | CombatConst.DAMAGE_TYPE_REFLECT);
            reDamageMap.Add("target_rid", checkOb.GetRid());

            // 造成伤害
            (checkOb as Char).ReceiveDamage(sourceProfile, damageMap.GetValue<int>("damage_type"), reDamageMap);

            string tips = string.Format("[ff7474]{0}[-]", LocalizationMgr.Get("MonsterTipsWnd_7"));

            // 给出反伤标识
            BloodTipMgr.AddTip(TipsWndType.DamageTip, checkOb, tips);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 陷阱触发检查脚本
/// </summary>
public class SCRIPT_6845 : Script
{
    public override object Call(params object[] _param)
    {
        // sourceOb, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        //int propValue = (_param[1] as LPCValue).AsInt;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果没有目标伤害类型，则触发，反之不触发
        if ((damageType & selectArgs.AsInt) == 0)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 触发属性效果作用脚本，必定追加回合，且清除自身技能CD
/// </summary>
public class SCRIPT_6846 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        RoundCombatMgr.DoAdditionalRound(trigger, LPCMapping.Empty);

        int reduceCd = CdMgr.GetSkillCd(trigger, propValue);

        CdMgr.DoReduceSkillCd(trigger, propValue, reduceCd);

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger,propValue);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发追加回合检查脚本（多次触发追加回合判断，只取一次）
/// </summary>
public class SCRIPT_6847 : Script
{
    public override object Call(params object[] _param)
    {
        // sourceOb, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        //int propValue = (_param[1] as LPCValue).AsInt;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string propName = _param[5] as String;

        string cookie = triggerPara.GetValue<string>("original_cookie");

        // 攻击源的cookie相同则不起效，防止同技能的多段多目标触发
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 战场回合触发检查脚本
/// </summary>
public class SCRIPT_6848 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue selectArgs = _param[3] as LPCValue;

        Property propOb = _param[0] as Property;

        // 判断trigger_round_cookie是否一致
        if (string.Equals(propOb.QueryTemp<string>("trigger_round_cookie"), propOb.QueryTemp<string>("round_cookie")))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发上护盾作用脚本
/// </summary>
public class SCRIPT_6849 : Script
{
    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        //Property trigger = _param[0] as Property;
        Property propOb = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;
        //LPCArray selectPara = (_param[7] as LPCValue).AsArray;
        string cookie = triggerPara.GetValue<LPCMapping>("source_profile").GetValue<string>("cookie");

        // 播放技能光效
        LPCMapping args = new LPCMapping();
        args.Add("actor_name", propOb.GetRid());
        args.Add("source_profile", sourceProfile);
        propOb.Actor.DoActionSet("B_EQPT_SHIELD", cookie, args);

        // 技能提示
        int skillId = propValue[0].AsArray[3].AsInt;
        BloodTipMgr.AddSkillTip(propOb,skillId);
        // 上护盾状态
        int value = 0;
        string attribName = propValue[0].AsArray[0].AsString;
        int valueRate = propValue[0].AsArray[1].AsInt;
        int influnAttrib = propOb.QueryAttrib(attribName);

        if (attribName.Equals("level"))
        {
            influnAttrib = propOb.Query<int>(attribName);
            value = influnAttrib * valueRate;
        }else
            value = influnAttrib * valueRate / 1000;

        // 计算技能升级效果
        value += value * CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), propOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_SHIELD) / 1000;

        LPCMapping condition = new LPCMapping ();
        condition.Add("round", propValue[0].AsArray[2].AsInt);
        condition.Add("round_cookie", cookie);
        condition.Add("can_absorb_damage", value);
        propOb.ApplyStatus("B_HP_SHIELD", condition);

        // 记录战场回合
        propOb.SetTemp("trigger_round_cookie", LPCValue.Create(propOb.QueryTemp<string>("round_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 能量消耗、反伤触发检查脚本
/// </summary>
public class SCRIPT_6850 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue selectArgs = _param[3] as LPCValue;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        Property propOb = _param[0] as Property;

        // 获取伤害结构信息（没经过伤害分摊和护盾削减的伤害）
        if (!triggerPara.ContainsKey("original_damage_map"))
            return false;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("original_damage_map");

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");

        // 如果伤害来源是mp则不起效
        if (points.ContainsKey("mp"))
            return false;

        // 检查能量是否足够
        LPCMapping costData = SkillMgr.GetCasTCost(propOb,propValue[0].AsArray[2].AsInt);

        if (costData.GetValue<int>("mp") > propOb.Query<int>("mp"))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用受创伤害反射(计算按照受创者自身属性进行百分比加成，并且反伤给敌方全体)
/// </summary>
public class SCRIPT_6851 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 添加攻击源cookie
        sourceProfile.Add("cookie", triggerPara.GetValue<string>("cookie"));
        sourceProfile.Add("original_cookie",triggerPara.GetValue<string>("original_cookie"));

        // 检查伤害来源，如果是自身，则不起效
        if (trigger.GetRid().Equals(sourceProfile.GetValue<string>("rid")))
            return false;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("original_damage_map");

        // 以受创者自身攻击百分比进行计算反伤伤害
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 技能提示
        int skillId = propValue[0].AsArray[2].AsInt;
        BloodTipMgr.AddSkillTip(sourceOb, skillId);

        int sourceAttrib = sourceOb.QueryAttrib(propValue[0].AsArray[0].AsString);

        int reflectDamage = sourceAttrib * propValue[0].AsArray[1].AsInt / 1000;

        int dmgRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_DMG);

        reflectDamage += Game.Multiple(reflectDamage, dmgRate, 1000);

        // 包装反射伤害信息
        LPCMapping reDamageMap = new LPCMapping();
        LPCMapping rePoints = new LPCMapping();
        rePoints.Add("hp", reflectDamage);
        reDamageMap.Add("points", rePoints);
        // 记录克制关系和伤害类型
        reDamageMap.Add("restrain", ElementConst.ELEMENT_NEUTRAL);
        reDamageMap.Add("damage_type", damageMap.GetValue<int>("damage_type") | CombatConst.DAMAGE_TYPE_REFLECT);
        reDamageMap.Add("target_rid", trigger.GetRid());

        // 造成伤害
        (trigger as Char).ReceiveDamage(sourceProfile, damageMap.GetValue<int>("damage_type"), reDamageMap);

        string tips = string.Format("[ff7474]{0}[-]", LocalizationMgr.Get("MonsterTipsWnd_7"));

        // 给出反伤标识
        BloodTipMgr.AddTip(TipsWndType.DamageTip, trigger, tips);

        // 消耗能量
        LPCMapping costData = SkillMgr.GetCasTCost(sourceOb,skillId);
        sourceOb.CostAttrib(costData);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 技能触发技能检查脚本
/// </summary>
public class SCRIPT_6852 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue selectArgs = _param[3] as LPCValue;

        //Property propOb = _param[0] as Property;

        int triggerSkillId = propValue[0].AsArray[1].AsInt;

        // 自身不能触发自身
        if (triggerPara.GetValue<int>("skill_id") == triggerSkillId)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 释放技能概率触发检查脚本，概率根据技能升级效果变化
/// </summary>
public class SCRIPT_6853 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        int skillId = triggerPara.GetValue<int>("skill_id");

        int propSkill = propValue[0].AsArray[1].AsInt;

        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        if (skillInfo.Query<int>("attrib_type") == SkillType.SKILL_TYPE_NORMAL)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 概率触发
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(propSkill), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(propSkill)), SkillType.SE_RATE);

        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt + upRate)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发自身释放技能，无任何限制，作用脚本
/// </summary>
public class SCRIPT_6854 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 检查成功率
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        int skillId = propValue[0].AsArray[1].AsInt;
        int oriSkillId = propValue[0].AsArray[2].AsInt;
        BloodTipMgr.AddSkillTip(trigger, oriSkillId);

        CombatMgr.DoCastSkill(trigger, trigger, skillId, LPCMapping.Empty);

        return true;
    }
}

/// <summary>
/// 触发检查脚本，检查回合类型，不检查技能类型
/// </summary>
public class SCRIPT_6855 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        int roundType = triggerPara.GetValue<int>("type");

        // 检查是不是普通回合和追加回合
        if (roundType != RoundCombatConst.ROUND_TYPE_NORMAL &&
            roundType != RoundCombatConst.ROUND_TYPE_ADDITIONAL &&
            roundType != RoundCombatConst.ROUND_TYPE_GASSER)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 命中单位未死亡触发检查脚本(未死亡触发XXX之类的技能)
/// </summary>
public class SCRIPT_6856 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property source = _param[0] as Property;
        //LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        Property target = _param[4] as Property;

        // 获取目标生命来判断，不用死亡状态来判断，是有原因的。。。
        int targetHp = target.Query<int>("hp");

        if (targetHp <= 0)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发上状态
/// </summary>
public class SCRIPT_6857 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        sourceProfile.Add("cookie",triggerPara.GetValue<string>("cookie"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        List<Property> checkList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property checkOb in targetList)
        {
            if (!checkOb.CheckStatus("DIED"))
                checkList.Add(checkOb);
        }

        // 获取伤害量
        LPCMapping damage_info = triggerPara.GetValue<LPCMapping>("damage_map");
        if (damage_info == null)
            damage_info = new LPCMapping();

        LPCMapping points = damage_info.GetValue<LPCMapping>("points");
        int damageValue = points.GetValue<int>("hp");

        // 构造护盾信息
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue[0].AsArray[1].AsInt);
        condition.Add("round_cookie", triggerPara.GetValue<string>("cookie"));
        condition.Add("source_profile", sourceProfile);
        condition.Add("can_absorb_damage", Game.Multiple(damageValue, propValue[0].AsArray[0].AsInt, 1000));

        // 单独成员计算概率
        foreach (Property targetOb in checkList)
            targetOb.ApplyStatus("B_HP_SHIELD", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发二段（受创触发）
/// </summary>
public class SCRIPT_6858 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        if (sourceOb.CheckStatus(statusList))
            return false;

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger,propValue[0].AsArray[0].AsInt);
        // 执行触发技能
        CombatMgr.DoCastSkill(sourceOb, trigger, propValue[0].AsArray[1].AsInt, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 概率触发检查脚本
/// </summary>
public class SCRIPT_6859 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        int skillId = propValue[0].AsArray[3].AsInt;

        // 概率检查
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[1].AsInt)
            return false;

        int roundType = triggerPara.GetValue<int>("type");

        // 检查回合类型，只有正常回合与追加回合起效
        if (roundType == RoundCombatConst.ROUND_TYPE_NORMAL ||
            roundType == RoundCombatConst.ROUND_TYPE_ADDITIONAL ||
            roundType == RoundCombatConst.ROUND_TYPE_GASSER)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 触发属性效果作用脚本，治疗自身和光环源，并追加光环源普攻
/// </summary>
public class SCRIPT_6860 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取光环源
        for (int i = 0; i < propValue.Count; i++)
        {
            Property haleSourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);
            if (haleSourceOb.CheckStatus(statusList))
                continue;

            // 取最大生命
            int selfMaxHp = trigger.QueryAttrib("max_hp");
            int haloSourceMaxHp = haleSourceOb.QueryAttrib("max_hp");

            // 根据伤害量，计算恢复量
            LPCMapping selfCureValue = new LPCMapping();
            LPCMapping haloSourceCureValue = new LPCMapping();

            int selfHpCure = Mathf.Max(propValue[i].AsArray[2].AsInt * selfMaxHp / 1000, 0);
            int haloSourceHpCure = Mathf.Max(propValue[i].AsArray[2].AsInt * haloSourceMaxHp / 1000, 0);

            // 计算额外影响
            selfHpCure = CALC_EXTRE_CURE.Call(selfHpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));
            haloSourceHpCure = CALC_EXTRE_CURE.Call(haloSourceHpCure, haleSourceOb.QueryAttrib("skill_effect"), haleSourceOb.QueryAttrib("reduce_cure"));

            selfCureValue.Add("hp", selfHpCure);
            haloSourceCureValue.Add("hp", haloSourceHpCure);

            // 技能提示
            BloodTipMgr.AddSkillTip(trigger, propValue[i].AsArray[3].AsInt);
            BloodTipMgr.AddSkillTip(haleSourceOb, propValue[i].AsArray[3].AsInt);
            // 通知ReceiveCure
            sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));
            if (!trigger.CheckStatus(banStatusList))
                (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, selfCureValue);
            if (!haleSourceOb.CheckStatus(banStatusList))
                (haleSourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, haloSourceCureValue);

            // 主角追加普攻
            int campId = (haleSourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
            List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);
            List<Property> finalList = new List<Property>();
            // 筛选非死亡单位，目标随机
            foreach (Property targetOb in targetList)
            {
                if (!targetOb.CheckStatus("DIED") && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                    finalList.Add(targetOb);
            }

            if (finalList.Count == 0)
                continue;

            Property pickOb = finalList[RandomMgr.GetRandom(finalList.Count)];
            CombatMgr.DoCastSkill(haleSourceOb, pickOb, propValue[i].AsArray[4].AsInt, LPCMapping.Empty);
        }
        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创反击检查脚本，需要检查指定技能是否在CD状态，在CD状态才有概率进行指定技能反击
/// </summary>
public class SCRIPT_6861 : Script
{
    public override object Call(params object[] _param)
    {
        //source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string propName = _param[5] as String;

        // 如果来源是伤害反射，则不处理
        int damageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((damageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;

        // 如果来源是持续伤害，则不处理
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 检查攻击是否将要造成死亡
        LPCMapping points = triggerPara.GetValue<LPCMapping>("damage_map").GetValue<LPCMapping>("points");
        if (points.GetValue<int>("hp") >= sourceOb.Query<int>("hp"))
            return false;

        // 检查自身指定技能是否在CD状态
        int cdCheckSkill = propValue[0].AsArray[2].AsInt;
        if (!CdMgr.SkillIsCooldown(sourceOb, cdCheckSkill))
            return false;

        // 检查成功率
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[1].AsInt)
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用清除自身所有减益状态
/// </summary>
public class SCRIPT_6862 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        List<LPCMapping> allStatus = trigger.GetAllStatus();
        List<int> removeList = new List<int>();
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
            removeList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        if (removeList.Count == 0)
            return true;

        // 清楚状态
        trigger.ClearStatusByCookie(removeList,StatusConst.CLEAR_TYPE_BREAK);

        // 添加回合行动列表中
        string cookie = Game.NewCookie(trigger.GetRid());
        // 播放cure effect
        bool doRet = trigger.Actor.DoActionSet("CLEAR_DEBUFF", cookie, LPCMapping.Empty);
        // 如果播放成功
        if (doRet)
            RoundCombatMgr.AddRoundAction(cookie, trigger, LPCMapping.Empty);

        // 技能提示
        BloodTipMgr.AddSkillTip(trigger,propValue[0].AsArray[1].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本百分比恢复自身生命
/// </summary>
public class SCRIPT_6863 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;
        sourceProfile.Append(triggerPara);
        sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));

        // 收集本阵营生命最低对象
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;
        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED"))
                continue;

            // 如果是自己跳出
            if (string.Equals(targetOb.GetRid(), trigger.GetRid()))
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }

        // 自身生命小于 50% 时，回自己，小于则回生命比例最低的队友
        int cureRate = propValue[0].AsArray[1].AsInt;
        if (trigger.QueryTemp<int>("hp_rate") < propValue[0].AsArray[0].AsInt)
        {
            if (trigger.CheckStatus(banStatusList))
                return false;
            int hpCure = Game.Multiple(trigger.QueryAttrib("max_hp"), cureRate);
            // 计算额外影响
            hpCure = CALC_EXTRE_CURE.Call(hpCure, trigger.QueryAttrib("skill_effect"), trigger.QueryAttrib("reduce_cure"));
            // 根据伤害量，计算恢复量
            LPCMapping value = new LPCMapping();
            value.Add("hp", hpCure);
            // 通知ReceiveCure
            (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);
        }
        else
        {
            // 没有选择到目标
            if (selectOb.Count == 0)
                return false;

            Property mateOb = selectOb[RandomMgr.GetRandom(selectOb.Count)];

            if (mateOb.CheckStatus(banStatusList))
                return false;
            int hpCure = Game.Multiple(mateOb.QueryAttrib("max_hp"), cureRate);
            // 计算额外影响
            hpCure = CALC_EXTRE_CURE.Call(hpCure, mateOb.QueryAttrib("skill_effect"), mateOb.QueryAttrib("reduce_cure"));
            // 根据伤害量，计算恢复量
            LPCMapping value = new LPCMapping();
            value.Add("hp", hpCure);
            // 通知ReceiveCure
            (mateOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);
        }

        // 首次反击成功记录技能源cookie
        trigger.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 概率触发节点检查脚本（反击和协同回合不起效，不管什么伤害类型包括扣篮）
/// </summary>
public class SCRIPT_6865 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 概率检查
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;
        LPCMapping triggerPara = _param[2] as LPCMapping;
//        LPCValue selectArgs = _param[3] as LPCValue;
//
//        // 获取伤害类型
//        int damageType = triggerPara.GetValue<int>("damage_type");
//
//        // 如果不是需要触发的伤害类型，不能触发
//        if ((damageType & selectArgs.AsInt) == 0)
//            return false;

        // 回合类型判断
        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创概率触发检查脚本，纯概率检查，属性为数组，第一个元素是概率
/// </summary>
public class SCRIPT_6866 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //int skillId = propValue[0].AsArray[3].AsInt;

        // 概率检查
        if (RandomMgr.GetRandom() >= propValue[0].AsArray[0].AsInt)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发回能
/// </summary>
public class SCRIPT_6867 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        //Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        if (trigger.CheckStatus("D_NO_MP_CURE") || trigger.CheckStatus("B_CAN_NOT_CHOOSE"))
            return false;
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", propValue[0].AsArray[1].AsInt);

        // 执行回血操作
        sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 能量达到一定值触发检查脚本
/// </summary>
public class SCRIPT_6868 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue selectArgs = _param[3] as LPCValue;

//        // 获取治愈类型
//        int cureType = triggerPara.GetValue<int>("cure_type");
//
//        // 如果不是需要触发的治愈类型，不能触发
//        if ((cureType & selectArgs.AsInt) == 0)
//            return false;

        if (propOb.Query<int>("mp") < propValue[0].AsArray[2].AsInt)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 死亡触发来源释放攻击技能作用脚本(灵魂收割专用)
/// </summary>
public class SCRIPT_6869 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};

    public override object Call(params object[] _param)
    {
        // 获取攻击源
        Property trigger = _param[0] as Property;

        //Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        for (int i = 0; i < propValue.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            if (sourceOb.CheckStatus(statusList))
                continue;

            int campId = trigger.CampId;

            // 检查阵营归属
            if (trigger.CampId == sourceOb.CampId)
                campId = (trigger.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

            List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);

            // 技能提示
            BloodTipMgr.AddSkillTip(sourceOb, propValue[i].AsArray[1].AsInt);

            CombatMgr.DoCastSkill(sourceOb, targetList[0], propValue[i].AsArray[2].AsInt, LPCMapping.Empty);
        }

        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，治疗己方生命最低单位，根据目标生命百分比回血，带被动技能提示
/// </summary>
public class SCRIPT_6870 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        //sourceProfile.Add("skill_id", 11000);

        // 筛选己方生命最低单位
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;

        // 遍历各个角色
        foreach (Property checkOb in targetList)
        {
            // 角色已经死亡
            if (checkOb.CheckStatus("DIED"))
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(checkOb.Query<int>("hp"), checkOb.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(checkOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(checkOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return false;

        Property finalOb = selectOb[RandomMgr.GetRandom(selectOb.Count)];

        if (finalOb.CheckStatus(banStatusList))
            return false;

        // 取最大生命
        int maxHp = finalOb.QueryAttrib("max_hp");

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();

        int hpCure = Mathf.Max(propValue[0].AsArray[1].AsInt * maxHp / 1000, 0);

        // 计算额外影响
        hpCure = CALC_EXTRE_CURE.Call(hpCure, finalOb.QueryAttrib("skill_effect"), finalOb.QueryAttrib("reduce_cure"));

        value.Add("hp", hpCure);

        // 技能提示
        BloodTipMgr.AddSkillTip(finalOb, propValue[0].AsArray[0].AsInt);
        // 通知ReceiveCure
        sourceProfile.Add("cookie",Game.NewCookie(finalOb.GetRid()));
        (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据输出伤害，恢复生命比率最低友军单位生命
/// </summary>
public class SCRIPT_6871 : Script
{
    private static List<string> banStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCValue propValue = _param[3] as LPCValue;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue; 

        sourceProfile.Append(triggerPara);

        // 筛选己方生命最低单位
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;

        // 遍历各个角色
        foreach (Property checkOb in targetList)
        {
            // 角色已经死亡
            if (checkOb.CheckStatus("DIED"))
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(checkOb.Query<int>("hp"), checkOb.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(checkOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(checkOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return false;

        Property finalOb = selectOb[RandomMgr.GetRandom(selectOb.Count)];

        if (finalOb.CheckStatus(banStatusList))
            return false;

        // 取伤害量
        // 注意：extra_attack里的东西不计算吸血，block_value格挡值要算在伤害内
        LPCMapping damage_info = triggerPara.GetValue<LPCMapping>("damage_map");
        if (damage_info == null)
            damage_info = new LPCMapping();

        LPCMapping points = damage_info.GetValue<LPCMapping>("points");
        if (points.ContainsKey("mp") && points.GetValue<int>("hp") == 0)
            return false;

        int damage = points.GetValue<int>("hp");

        int hpCure = Mathf.Max(Game.Multiple(damage, propValue.AsInt), 0);

        // 计算额外影响
        string sourcrRid = sourceProfile.GetValue<string>("rid");
        Property sourceOb = Rid.FindObjectByRid(sourcrRid);
        hpCure = CALC_EXTRE_CURE.Call(hpCure, sourceOb.QueryAttrib("skill_effect"), finalOb.QueryAttrib("reduce_cure"));

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();
        value.Add("hp", hpCure);
        sourceProfile.Add("cookie", Game.NewCookie(finalOb.GetRid()));
        // 通知ReceiveCure
        (finalOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 肾上腺素光环的触发概率检查脚本
/// </summary>
public class SCRIPT_6872 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE" };

    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        if (propOb.CheckStatus("DIED"))
            return false;

        // 如果不是正常回合以及追加回合则不起效
        int type = triggerPara.GetValue<int>("type");
        if (type != RoundCombatConst.ROUND_TYPE_NORMAL &&
            type != RoundCombatConst.ROUND_TYPE_ADDITIONAL)
            return false;

        // 概率检查
        // propValue里记录了所有光环源信息
        LPCArray castList = new LPCArray();
        for (int i = 0; i < propValue.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);
            int extraSkillRate = sourceOb.QueryTemp<int>("extra_skill_rate");
            // 角色对象不存在
            if (sourceOb == null || sourceOb.CheckStatus(statusList))
                continue;

            // 概率限制
            int randomRate = RandomMgr.GetRandom();
            if (randomRate >= propValue[i].AsArray[1].AsInt + extraSkillRate)
            {
                // 如果没成功需要添加额外概率到光环源身上
                sourceOb.SetTemp("extra_skill_rate", LPCValue.Create(extraSkillRate + propValue[i].AsArray[3].AsInt));
                continue;
            }
            LPCMapping data = new LPCMapping();
            data.Add("source_rid", propValue[i].AsArray[0].AsString);
            data.Add("skill_id", propValue[i].AsArray[2].AsInt);
            data.Add("main_skill_id", propValue[i].AsArray[4].AsInt);
            castList.Add(data);
        }

        if (castList.Count == 0)
            return false;

        // 返回数据
        return LPCValue.Create(castList);
    }
}

/// <summary>
/// 肾上腺素概率触发技能作用脚本
/// </summary>
public class SCRIPT_6873 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE" };

    public override object Call(params object[] _param)
    {
        //参数：target, source, sourceProfile, propValue, triggerPara, applyArgs, propName, selectPara
        Property trigger = _param[0] as Property;
        //Property propOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;
        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        // 如果上一次攻击的对象已经死亡或者为空，则随机抽取单位
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                finalList.Add(targetOb);
        }

        if (finalList.Count == 0)
            return false;

        // selectPara里记录了所有光环源信息
        for (int i = 0; i < selectPara.Count; i++)
        {
            LPCMapping data = selectPara[i].AsMapping;
            Property sourceOb = Rid.FindObjectByRid(data.GetValue<string>("source_rid"));

            // 角色对象不存在
            if (sourceOb == null || sourceOb.CheckStatus(statusList))
                continue;
            // 技能提示
            BloodTipMgr.AddSkillTip(sourceOb, data.GetValue<int>("main_skill_id"));

            Property pickOb = trigger;
            // 来源释放技能对上次攻击过得目标
            string lastHitRid = sourceOb.QueryTemp<string>("last_hit_rid");
            if (!string.IsNullOrEmpty(lastHitRid))
            {
                pickOb = Rid.FindObjectByRid(lastHitRid);

                if (pickOb != null && !pickOb.CheckStatus("DIED"))
                {
                    CombatMgr.DoCastSkill(sourceOb, pickOb, data.GetValue<int>("skill_id"), LPCMapping.Empty);
                    // 清空额外概率
                    sourceOb.SetTemp("extra_skill_rate",LPCValue.Create(0));
                    continue;
                }
            }

            pickOb = finalList[RandomMgr.GetRandom(finalList.Count)];

            CombatMgr.DoCastSkill(sourceOb, pickOb, data.GetValue<int>("skill_id"), LPCMapping.Empty);
            // 清空额外概率
            sourceOb.SetTemp("extra_skill_rate",LPCValue.Create(0));
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 纯正血统触发检查脚本（检查光环源是否死亡）
/// </summary>
public class SCRIPT_6874 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        LPCArray dieSourceList = new LPCArray();

        // 获取死亡的光环源
        for (int i = 0; i < propValue.Count; i++)
        {
            LPCArray propArg = propValue[i].AsArray;
            string sourceRid = propArg[0].AsString;
            Property sourceOb = Rid.FindObjectByRid(sourceRid);
            // 检查原始技能CD
            if (CdMgr.SkillIsCooldown(sourceOb, propArg[1].AsInt))
                continue;
            // 检查是否死亡，光环源没死，不添加后续列表
            if (sourceOb.CheckStatus("DIED"))
                dieSourceList.Add(propArg);
        }

        if (dieSourceList.Count == 0)
            return false;

        // 返回数据
        return LPCValue.Create(dieSourceList);
    }
}

/// <summary>
/// 死亡触发来源释放自身技能作用脚本
/// </summary>
public class SCRIPT_6875 : Script
{
    // 因为效果为获得回合，所以不用在状态筛选列表中加控制状态的判断。
    private static List<string> statusList = new List<string>(){ "DIED", };

    public override object Call(params object[] _param)
    {
        // 获取攻击源
        //Property trigger = _param[0] as Property;

        //Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;
        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        for (int i = 0; i < selectPara.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(selectPara[i].AsArray[0].AsString);

            // 检查起效cookie是否为同一回合，如果是跳过
            if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
            {
                if (string.Equals(sourceOb.QueryTemp("trigger_cookie/" + propName).AsString, triggerPara.GetValue<string>("original_cookie")))
                    continue;
            }

            // 如果是光环源的回合则跳过
            if (sourceOb.Equals(RoundCombatMgr.GetCurRoundOb()) || sourceOb.CheckStatus(statusList))
                continue;

            // 技能提示
            BloodTipMgr.AddSkillTip(sourceOb, selectPara[i].AsArray[1].AsInt);

            CombatMgr.DoCastSkill(sourceOb, sourceOb, selectPara[i].AsArray[2].AsInt, LPCMapping.Empty);

            // 记录回合起效cookie
            sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));
        }

        return true;
    }
}

/// <summary>
/// 死亡触发攻击技能检查脚本
/// </summary>
public class SCRIPT_6876 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        int propValue = (_param[1] as LPCValue).AsInt;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        // 检查目标方是否还有活人，如果没有则不再继续触发
        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);
        if (targetList.Count == 0)
            return false;

        CsvRow skillInfo = SkillMgr.GetSkillInfo(propValue);

        int originSkillId = skillInfo.Query<int>("original_skill_id");

        // 检查是否在CD中
        if (CdMgr.SkillIsCooldown(sourceOb, originSkillId))
            return false;

        // 检查蓝是否足够
        if (sourceOb.Query<int>("mp") < skillInfo.Query<LPCMapping>("cost_arg").GetValue<int>("mp"))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 光环源回能，防多段触发，节点检查脚本
/// </summary>
public class SCRIPT_6877 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue selectArgs = _param[3] as LPCValue;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        if (points.ContainsKey("mp") && !points.ContainsKey("hp"))
            return false;

        string propName = _param[5] as String;
        LPCArray effectList = new LPCArray();
        for (int i = 0; i < propValue.Count; i++)
        {
            Property haloSourceOb = Rid.FindObjectByRid(propValue[i].AsArray[0].AsString);

            if (haloSourceOb.CheckStatus("D_NO_MP_CURE") || haloSourceOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;

            // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
            string cookie = triggerPara.GetValue<string>("original_cookie");
            if (haloSourceOb.QueryTemp("trigger_cookie/" + propName) != null)
            {
                if (string.Equals(cookie, haloSourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                    continue;
            }

            // 添加有效信息
            LPCMapping data = new LPCMapping ();
            data.Add("source_rid", propValue[i].AsArray[0].AsString);
            data.Add("source_skill_id", propValue[i].AsArray[1].AsInt);
            data.Add("source_cure_mp", propValue[i].AsArray[2].AsInt);
            effectList.Add(data);
        }

        if (effectList.Count == 0)
            return false;

        // 返回数据
        return LPCValue.Create(effectList);
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发光环源回能
/// </summary>
public class SCRIPT_6878 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        //Property trigger = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        //Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;
        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        for (int i = 0; i < selectPara.Count; i++)
        {
            Property haloSourceOb = Rid.FindObjectByRid(selectPara[i].AsMapping.GetValue<string>("source_rid"));
            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("mp", selectPara[i].AsMapping.GetValue<int>("source_cure_mp"));
            // 技能提示
            BloodTipMgr.AddSkillTip(haloSourceOb, selectPara[i].AsMapping.GetValue<int>("source_skill_id"));
            // 执行回能操作
            sourceProfile.Add("cookie", Game.NewCookie(selectPara[i].AsMapping.GetValue<string>("source_rid")));
            (haloSourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
            // 记录回合起效cookie
            haloSourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果能量满触发技能释放（废弃）
/// </summary>
public class SCRIPT_6879 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_NO_MP_CURE", "B_CAN_NOT_CHOOSE" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        int propValue = (_param[3] as LPCValue).AsInt;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);
        LPCArray effectList = new LPCArray();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            // 状态检查
            if (targetOb.CheckStatus(statusList))
                continue;
            LPCMapping cureMap = new LPCMapping();
            cureMap.Add("mp", propValue);
            // 执行回能操作
            sourceProfile.Add("cookie", Game.NewCookie(targetOb.GetRid()));
            (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
            effectList.Add(targetOb.GetRid());
        }

        // 记录战场回合
        if (effectList.Count != 0)
            trigger.SetTemp("trigger_round_cookie", LPCValue.Create(trigger.QueryTemp<string>("round_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 检查濒死触发技能CD中的触发检查脚本
/// </summary>
public class SCRIPT_6880 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;

        int skillId = propValue[0].AsArray[0].AsInt;

        // 检查CD
        if (CdMgr.SkillIsCooldown(sourceOb, skillId))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用濒死上状态通用常用状态
/// </summary>
public class SCRIPT_6881 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        //LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        if (trigger.CheckStatus("DIED"))
            return false;

        // 血量设置为 1 不死
        trigger.Set("hp", LPCValue.Create(1));

        //上状态
        LPCArray statusDataList = propValue[0].AsArray[1].AsArray;
        foreach(LPCValue statusData in statusDataList.Values)
        {
            LPCMapping condition = new LPCMapping();
            condition.Add("round", statusData.AsMapping.GetValue<int>("round"));
            condition.Add("round_cookie", Game.NewCookie(trigger.GetRid()));
            condition.Add("source_profile", sourceProfile);
            trigger.ApplyStatus(statusData.AsMapping.GetValue<string>("status"), condition);
        }

        // 技能进行cd操作
        CdMgr.SkillCooldown(trigger, propValue[0].AsArray[0].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 格挡指定技能反击检查脚本
/// </summary>
public class SCRIPT_6882 : Script
{
    public override object Call(params object[] _param)
    {
        //source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;
        string propName = _param[5] as String;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((damageType & selectArgs.AsInt) == 0)
            return false;

        // 如果来源是伤害反射，则不处理
        int oriDamageType = triggerPara.GetValue<LPCMapping>("original_damage_map").GetValue<int>("damage_type");
        if ((oriDamageType & CombatConst.DAMAGE_TYPE_REFLECT) == CombatConst.DAMAGE_TYPE_REFLECT)
            return false;

        // 如果来源是持续伤害，则不处理
        if ((oriDamageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
            return false;

        int type = triggerPara.GetValue<int>("type");
        if (type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_COUNTER)
            return false;

        // 概率检查
        if (RandomMgr.GetRandom() >= (propValue[0].AsArray[1].AsInt))
            return false;

        string cookie = triggerPara.GetValue<string>("original_cookie");
        // 攻击源的cookie相同则不起效，防止同技能的多HIT多反击
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用格挡指定技能反击
/// </summary>
public class SCRIPT_6883 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 获取目标
        if (trigger.CheckStatus("DIED"))
            return true;

        // 反击信息
        LPCMapping counterMap = new LPCMapping();
        counterMap.Add("skill_id", propValue[0].AsArray[0].AsInt);
        counterMap.Add("pick_rid", trigger.GetRid());

        RoundCombatMgr.DoCounterRound(sourceOb, counterMap);
        // 首次反击成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 神魔灭杀专用满能量触发检查脚本
/// </summary>
public class SCRIPT_6884 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;
        LPCMapping realCureMap = triggerPara.GetValue<LPCMapping>("real_cure_map");

        // 真实治愈量如果是 0 则不继续触发
        if (realCureMap.GetValue<int>("mp") == 0)
            return false;

        // 获取治愈类型
        int cureType = triggerPara.GetValue<int>("cure_type");

        // 如果不是需要触发的治愈类型，不能触发
        if ((cureType & selectArgs.AsInt) == 0)
            return false;

        Property propOb = _param[0] as Property;
        if (propOb.Query<int>("mp") < propOb.QueryAttrib("max_mp"))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 猩红仪式触发作用濒死触发对凶手释放技能
/// </summary>
public class SCRIPT_6885 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        //LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        if (trigger.CheckStatus("DIED"))
            return false;

        // 获取凶手信息
        LPCMapping assInfo = trigger.QueryTemp<LPCMapping>("assailant_info");
        Property assOb = Rid.FindObjectByRid(assInfo.GetValue<string>("rid"));

        // 血量设置为 1 不死
        trigger.Set("hp", LPCValue.Create(1));

        // 对凶手释放技能
        CombatMgr.DoCastSkill(trigger, assOb, propValue[0].AsArray[1].AsInt, LPCMapping.Empty);

        // 技能进行cd操作
        CdMgr.SkillCooldown(trigger, propValue[0].AsArray[0].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发根据自身生命百分比实施状态操作(高于生命比例清除，低于生命比例上状态)
/// </summary>
public class SCRIPT_6886 : Script
{
    public override object Call(params object[] _param)
    {
        Property triggerOb = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        LPCArray propArg = propValue[0].AsArray;
        string statusName = propArg[1].AsString;
        // 生命低于参数比例时，上状态，高于参数比例时，清除状态
        if (triggerOb.QueryTemp<int>("hp_rate") <= propArg[0].AsInt)
        {
            LPCMapping condition = new LPCMapping();
            condition.Add("source_profile", sourceProfile);

            triggerOb.ApplyStatus(statusName, condition);
        }
        else
        {
            // 如果有则清除，没有则不管
            if (triggerOb.CheckStatus(statusName))
                triggerOb.ClearStatus(statusName);
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，攻击概率造成目标生命上限减少
/// </summary>
public class SCRIPT_6887 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        LPCArray propArg = propValue[0].AsArray;
        //Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 检查成功率
        if (RandomMgr.GetRandom() >= propArg[0].AsInt)
            return false;

        // 获取最大生命上限伤害
        int originMaxHp = trigger.QueryOriginalAttrib("max_hp");
        // 单次最高降低量
        int singleMaxValue = Game.Multiple(originMaxHp, propArg[2].AsInt, 1000);
        // 百分比护盾计算前的伤害量
        LPCMapping beforeDmg = triggerPara.GetValue<LPCMapping>("shield_absorb_before_damage");
        int maxHpDmg = Math.Min(Game.Multiple(beforeDmg.GetValue<int>("hp") ,propArg[1].AsInt ,1000), singleMaxValue);

        // 最高生命上限伤害限制
        int finalMaxValue = CombatConst.DEFAULT_MAX_HP_DMG;

        if (maxHpDmg - trigger.Query<int>("attrib_addition/max_hp") - Game.Multiple(originMaxHp, finalMaxValue, 1000) >= 0)
            return false;

        // 计算最大伤害类型
        int damageType = CombatConst.DAMAGE_TYPE_MAX_ATTRIB;

        // 计算伤害结构
        LPCMapping damageMap = new LPCMapping();
        LPCMapping valuePoints = new LPCMapping();

        valuePoints.Add("max_hp", maxHpDmg);
        damageMap.Add("points", valuePoints);
        damageMap.Add("damage_type", damageType);
        damageMap.Add("target_rid", trigger.GetRid());

        sourceProfile.Add("damage_map", damageMap);

        sourceProfile.Add("original_cookie", triggerPara.GetValue<string>("original_cookie"));

        // 通知玩家受创
        (trigger as Char).ReceiveDamage(sourceProfile, damageType, damageMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 概召唤物自身是否满血触发检查脚本
/// </summary>
public class SCRIPT_6888 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        //LPCArray propValue = (_param[1] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        // int skillId = propValue[0].AsArray[3].AsInt;

        if (sourceOb.QueryTemp<int>("hp_rate") >= 1000)
            return true;

        // 返回数据
        return false;
    }
}

/// <summary>
/// 触发属性效果作用脚本，复活召唤物宿主
/// </summary>
public class SCRIPT_6889 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        LPCArray propArg = propValue[0].AsArray;
        // 获取宿主对象
        string summonerRid = trigger.Query<string>("summoner_rid");
        Property summonerOb = Rid.FindObjectByRid(summonerRid);

        // 清除死亡状态
        summonerOb.ClearStatus("DIED");

        // 计算重生血量
        int cureRate = propArg[0].AsInt;
        int rebornMp = propArg[1].AsInt;
        int rebornHp = 0;

        rebornHp = Game.Multiple(summonerOb.QueryAttrib("max_hp"), cureRate, 1000);

        if (rebornHp == 0)
            rebornHp = 1;

        // 设置血量、最大值修正
        summonerOb.Set("hp", LPCValue.Create(rebornHp));
        summonerOb.Set("mp", LPCValue.Create(rebornMp));

        // 播放复活光效
        summonerOb.Actor.DoActionSet("reborn", Game.NewCookie(summonerRid), LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 检查目标是不是BOSS，如果是BOSS，则不起效
/// </summary>
public class SCRIPT_6890 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property sourceOb = _param[0] as Property;
        //LPCArray propValue = (_param[1] as LPCValue).AsArray;
        Property target = _param[4] as Property;

        //int skillId = propValue[0].AsArray[0].AsInt;

        // 检查是不是BOSS，如果是BOSS则不起效
        if (target.QueryAttrib("is_boss") == 1)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，根据输出伤害，恢复自身生命,溢出部分转化为护盾
/// </summary>
public class SCRIPT_6891 : Script
{
    private static List<string> statusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue; 
        LPCArray propArg = propValue[0].AsArray;
        if (trigger.CheckStatus(statusList))
            return false;

        sourceProfile.Append(triggerPara);

        // 取伤害量
        // 注意：extra_attack里的东西不计算吸血，block_value格挡值要算在伤害内
        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        if (damageMap == null)
            damageMap = new LPCMapping();

        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        if (points.ContainsKey("mp") && points.GetValue<int>("hp") == 0)
            return false;

        int damage = points.GetValue<int>("hp");

        if (damageMap.ContainsKey("origin_atk_rate"))
            damage = damage * damageMap.GetValue<int>("origin_atk_rate") / 1000;

        int hpCure = Mathf.Max(Game.Multiple(damage, propArg[0].AsInt), 0);

        // 溢出量先转化为护盾
        int overCure = hpCure + trigger.Query<int>("hp") - trigger.QueryAttrib("max_hp");
        if (overCure > 0)
        {
            // 上状态
            LPCMapping condition = new LPCMapping();
            condition.Add("round", propArg[1].AsInt);
            if (triggerPara.ContainsKey("original_cookie"))
                condition.Add("round_cookie", triggerPara.GetValue<string>("original_cookie"));
            else
                condition.Add("round_cookie", triggerPara.GetValue<string>("cookie"));
            condition.Add("can_absorb_damage", Game.Multiple(overCure, propArg[2].AsInt, 1000));
            condition.Add("source_profile", sourceProfile);

            // 附加状态
            trigger.ApplyStatus("B_HP_SHIELD", condition);
        }

        // 根据伤害量，计算恢复量
        LPCMapping value = new LPCMapping();
        value.Add("hp", hpCure);
        sourceProfile.Add("cookie", Game.NewCookie(trigger.GetRid()));
        // 通知ReceiveCure
        (trigger as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, value);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 亡魂嗅觉触发检查脚本
/// </summary>
public class SCRIPT_6892 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        //Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        // 返回数据
        return LPCValue.Create(propValue);
    }
}

/// <summary>
/// 死亡触发来源（不管来源是否死亡）释放自身技能作用脚本
/// </summary>
public class SCRIPT_6893 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取攻击源
        //Property trigger = _param[0] as Property;

        //Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;
        LPCArray selectPara = (_param[7] as LPCValue).AsArray;

        for (int i = 0; i < selectPara.Count; i++)
        {
            Property sourceOb = Rid.FindObjectByRid(selectPara[i].AsArray[0].AsString);

            // 检查起效cookie是否为同一回合，如果是跳过
            if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
            {
                if (string.Equals(sourceOb.QueryTemp("trigger_cookie/" + propName).AsString, triggerPara.GetValue<string>("original_cookie")))
                    continue;
            }

            // 技能提示
            int originSkill = selectPara[i].AsArray[1].AsInt;
            BloodTipMgr.AddSkillTip(sourceOb, originSkill);

            CombatMgr.DoCastSkill(sourceOb, sourceOb, selectPara[i].AsArray[2].AsInt, LPCMapping.Empty);

            // 原始技能进行CD
            CdMgr.SkillCooldown(sourceOb, originSkill);

            // 记录回合起效cookie
            sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));
        }

        return true;
    }
}

/// <summary>
/// 回合开始触发节点检查脚本（只在正常回合起效、且受控制状态限制）
/// </summary>
public class SCRIPT_6894 : Script
{
    private static List<string> statusList = new List<string>(){ "DIED", "D_SLEEP", "D_STUN", "D_FREEZE"};

    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property trigger = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;

        if (trigger.CheckStatus(statusList))
            return false;

        if (triggerPara.ContainsKey("provoke_id"))
        {
            if (triggerPara.GetValue<int>("provoke_id") == 1)
                return false;
        }

        // 检查技能CD
        if (CdMgr.SkillIsCooldown(trigger, propValue[0].AsArray[1].AsInt))
            return false;

        if (triggerPara.GetValue<int>("type") != RoundCombatConst.ROUND_TYPE_NORMAL && 
            triggerPara.GetValue<int>("type") != RoundCombatConst.ROUND_TYPE_ADDITIONAL)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发检查脚本，检查回合类型，不检查技能类型，同时检查多hit
/// </summary>
public class SCRIPT_6895 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property sourceOb = _param[0] as Property;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        int roundType = triggerPara.GetValue<int>("type");
        string propName = _param[5] as String;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        // 分摊伤害不起效
        if (damageMap.ContainsKey("link_id"))
            return false;

        // 攻击源的cookie相同则不起效，防止同技能的多HIT多击晕
        string cookie = triggerPara.GetValue<string>("original_cookie");
        if (sourceOb.QueryTemp("trigger_cookie/" + propName) != null)
        {
            if (string.Equals(cookie, sourceOb.QueryTemp("trigger_cookie/" + propName).AsString))
                return false;
        }

        // 检查是不是普通回合和追加回合
        if (roundType != RoundCombatConst.ROUND_TYPE_NORMAL &&
            roundType != RoundCombatConst.ROUND_TYPE_ADDITIONAL &&
            roundType != RoundCombatConst.ROUND_TYPE_GASSER)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 契约草人触发检查脚本
/// </summary>
public class SCRIPT_6896 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_FORGET", };
    private static List<string> sourceStatusList = new List<string>(){ "D_FORGET", };

    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;

        // 获取触发来源的信息
        //LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        //string propName = _param[5] as String;

        LPCArray effectSourceList = new LPCArray();

        for (int i = 0; i < propValue.Count; i++)
        {
            LPCArray propArgs = propValue[i].AsArray;
            Property sourceOb = Rid.FindObjectByRid(propArgs[0].AsString);
            if (propOb.CheckStatus(statusList))
                continue;
            if (sourceOb.CheckStatus(sourceStatusList))
                continue;
            // 检查技能CD
            if (CdMgr.SkillIsCooldown(sourceOb, propArgs[1].AsInt))
                continue;
            effectSourceList.Add(propArgs);
        }

        // 返回数据
        return LPCValue.Create(effectSourceList);
    }
}

/// <summary>
/// 死亡触发来源释放技能，作用脚本(契约草人)
/// </summary>
public class SCRIPT_6897 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取攻击源
        //Property trigger = _param[0] as Property;

        //if (trigger.CheckStatus("DIED"))
        //    return false;

        Property sourceOb = _param[1] as Property;
        //LPCMapping sourceProfile = _param[2] as LPCMapping;
        //LPCArray propValue = (_param[3] as LPCValue).AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        LPCArray selectPara = (_param[7] as LPCValue).AsArray;
        if (selectPara.Count == 0)
            return false;

        for (int i = 0; i < selectPara.Count; i++)
        {
            LPCArray selectArgs = selectPara[i].AsArray;
            Property castOb = Rid.FindObjectByRid(selectArgs[0].AsString);

            // 光环源死亡或者来自光环源则不作用
            if (castOb.CheckStatus("DIED") || sourceOb.Equals(castOb))
                return false;

            // 释放技能
            CombatMgr.DoCastSkill(castOb, sourceOb, selectArgs[2].AsInt, LPCMapping.Empty);

            // 技能CD
            int haloSkill = selectArgs[1].AsInt;
            CdMgr.SkillCooldown(castOb, haloSkill);

            // 技能提示
            BloodTipMgr.AddSkillTip(castOb, haloSkill);
        }
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，对最低能量的友军单位释放技能
/// </summary>
public class SCRIPT_6898 : Script
{
    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        LPCValue propValue = _param[3] as LPCValue;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;

        // 获取友军单位列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(trigger.CampId);

        // 筛选最低能量的目标
        List<Property> selectOb = new List<Property>();
        int maxMp = 5;
        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED"))
                continue;
            // 如果mp较大不处理
            int targetMp = targetOb.Query<int>("mp");
            if (targetMp > maxMp)
                continue;
            // 如果mp相同
            if (targetMp == maxMp)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // targetMp较小重新记录数据
            maxMp = targetMp;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }
        Property finalOb = selectOb[RandomMgr.GetRandom(selectOb.Count)];
        LPCArray propArgs = propValue.AsArray[0].AsArray;
        // 提示被动技能
        BloodTipMgr.AddSkillTip(trigger, propArgs[0].AsInt);

        // 释放技能
        CombatMgr.DoCastSkill(sourceOb, finalOb, propArgs[1].AsInt, LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 技能可否选择检查脚本
/// </summary>
public class SCRIPT_6900 : Script
{
    public override object Call(params object[] _param)
    {
        // who, skillId, checkArgs(配置表中的参数), target(释放目标),
        Property sourceOb = _param[0] as Property;
        int skillId = (int)_param[1];

        // 调用公式检查
        bool canChoose = CHEK_SKILL_CAN_CAST.Call(sourceOb, skillId);

        // 返回数据
        return canChoose;
    }
}

/// <summary>
/// 技能可否选择检查脚本(主动复活技能可否选择用)
/// </summary>
public class SCRIPT_6901 : Script
{
    public override object Call(params object[] _param)
    {
        // who, skillId, checkArgs(配置表中的参数), target(释放目标),
        Property sourceOb = _param[0] as Property;
        int skillId = (int)_param[1];

        // 检查是否有死亡目标，筛选出死亡单位
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].CheckStatus("DIED"))
                finalList.Add(targetList[i]);
        }

        if (finalList.Count == 0)
            return false;

        // 返回数据
        bool canChoose = CHEK_SKILL_CAN_CAST.Call(sourceOb, skillId);

        return canChoose;
    }
}

/// <summary>
/// 伤害类型触发属性判断检查
/// </summary>
public class SCRIPT_6902 : Script
{
    private static List<string> statusList = new List<string>(){ "B_CAN_NOT_CHOOSE", };

    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property source = _param[0] as Property;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;
        Property target = _param[4] as Property;
        if (target.CheckStatus(statusList))
            return false;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((damageType & selectArgs.AsInt) == 0)
            return false;

        // 如果血量为0则不起效
        if (source.Query<int>("hp") <= 0)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 伤害类数值触发的判断检查
/// </summary>
public class SCRIPT_6903 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property propOb = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue selectArgs = _param[3] as LPCValue;

        // 起效阈值
        int lostHpRate = propValue[0].AsArray[2].AsInt;

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        int hpDmg = points.GetValue<int>("hp");
        int dmgRate = 1000 * hpDmg / propOb.QueryAttrib("max_hp");
        int loseInterval = propOb.QueryTemp<int>("lost_hp_rate") + dmgRate;
        // 损失值未达标
        if (loseInterval < lostHpRate)
        {
            propOb.SetTemp("lost_hp_rate", LPCValue.Create(loseInterval));
            return false;
        }

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发作用损失生命多少决定能量恢复
/// </summary>
public class SCRIPT_6904 : Script
{
    public override object Call(params object[] _param)
    {
        //Property trigger = _param[0] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        LPCMapping damageMap = triggerPara.GetValue<LPCMapping>("damage_map");
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        int hpDmg = points.GetValue<int>("hp");
        int dmgRate = 1000 * hpDmg / sourceOb.QueryAttrib("max_hp");
        // 最大值修正
        int loseInterval = Math.Min(sourceOb.QueryTemp<int>("lost_hp_rate") + dmgRate, 1000);

        // 起效阈值
        int lostHpRate = propValue[0].AsArray[2].AsInt;

        int mpCure = Math.Min(loseInterval / lostHpRate * propValue[0].AsArray[1].AsInt, sourceOb.QueryAttrib("max_mp")-sourceOb.Query<int>("mp"));
        if (sourceOb.CheckStatus("D_NO_MP_CURE") || sourceOb.CheckStatus("B_CAN_NOT_CHOOSE"))
            mpCure = 0;
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mpCure);

        // 执行回能操作
        if (RandomMgr.GetRandom() < propValue[0].AsArray[0].AsInt && mpCure != 0)
        {
            sourceProfile.Add("cookie",Game.NewCookie(sourceOb.GetRid()));
            (sourceOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
        }

        // 损失值达标，则需要减去起效阈值相同的值，再存入临时数据中
        sourceOb.SetTemp("lost_hp_rate", LPCValue.Create(loseInterval - lostHpRate));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发二段（暴击概率触发，不带技能提示）
/// </summary>
public class SCRIPT_6905 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        Property trigger = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        string propName = _param[6] as String;

        // 执行触发技能
        CombatMgr.DoCastSkill(sourceOb, trigger, propValue[0].AsArray[1].AsInt, LPCMapping.Empty);

        // 首次成功记录技能源cookie
        sourceOb.SetTemp("trigger_cookie/" + propName, LPCValue.Create(triggerPara.GetValue<string>("original_cookie")));

        // 返回数据
        return true;
    }
}

/// <summary>
/// 鬼武士大招可否选择检查脚本
/// </summary>
public class SCRIPT_6906 : Script
{
    public override object Call(params object[] _param)
    {
        // who, skillId, checkArgs(配置表中的参数), target(释放目标),
        Property sourceOb = _param[0] as Property;
        int skillId = (int)_param[1];

        if (!sourceOb.CheckStatus("B_PREPARE"))
            return false;

        bool canChoose = CHEK_SKILL_CAN_CAST.Call(sourceOb, skillId);

        // 返回数据
        return canChoose;
    }
}

/// <summary>
/// 生命链接类技能可否选择检查脚本
/// </summary>
public class SCRIPT_6907 : Script
{
    public override object Call(params object[] _param)
    {
        // who, skillId, checkArgs(配置表中的参数), target(释放目标),
        Property sourceOb = _param[0] as Property;
        int skillId = (int)_param[1];

        // 检查当己方阵营只剩下自己，且无法选中自己时的技能，如：生命链接
        /// 目标筛选接口：阵营ID、包含对象RID列表、排斥状态、包含状态
        /// 当前范例为己方阵营、包含自己、排除死亡的所有对象列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId,
            new List<string>(){ "DIED" },
            new List<string>(){ sourceOb.GetRid() });

        if (targetList.Count == 1 && targetList[0].GetRid() == sourceOb.GetRid())
            return false;

        bool canChoose = CHEK_SKILL_CAN_CAST.Call(sourceOb, skillId);

        // 返回数据
        return canChoose;
    }
}

/// <summary>
/// 检查当前自身阵营是否有已行动的目标，如果没有已行动的目标，则某些技能不能释放
/// </summary>
public class SCRIPT_6908 : Script
{
    public override object Call(params object[] _param)
    {
        // who, skillId, checkArgs(配置表中的参数), target(释放目标),
        Property sourceOb = _param[0] as Property;
        int skillId = (int)_param[1];

        /// 目标筛选接口：阵营ID、包含对象RID列表、排斥状态、包含状态
        /// 当前范例为己方阵营、包含自己、排除死亡的所有对象列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId,
            new List<string>(){ "DIED" },
            new List<string>(){ sourceOb.GetRid() });

        LPCArray roundDownList = new LPCArray ();
        foreach (Property checkOb in targetList)
        {
            if (RoundCombatMgr.IsRoundDone(checkOb) && !string.Equals(checkOb.GetRid(),sourceOb.GetRid()))
                roundDownList.Add(checkOb.GetRid());
        }

        if (roundDownList.Count == 0)
            return false;

        bool canChoose = CHEK_SKILL_CAN_CAST.Call(sourceOb, skillId);

        // 返回数据
        return canChoose;
    }
}

/// <summary>
/// 死亡伪装、冥神降临技能可否释放检查脚本
/// </summary>
public class SCRIPT_6909 : Script
{
    public override object Call(params object[] _param)
    {
        // who, skillId, checkArgs(配置表中的参数), target(释放目标),
        Property sourceOb = _param[0] as Property;
        int skillId = (int)_param[1];

        // 死亡伪装、冥神降临检查
        if (sourceOb.QueryTemp<int>("hp_rate") >= 500)
            return false;

        bool canChoose = CHEK_SKILL_CAN_CAST.Call(sourceOb, skillId);

        // 返回数据
        return canChoose;
    }
}

/// <summary>
/// 永生奥秘触发技能可否选择检查脚本
/// </summary>
public class SCRIPT_6910 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_FORGET", };

    public override object Call(params object[] _param)
    {
        // who, skillId, checkArgs(配置表中的参数), target(释放目标),
        Property sourceOb = _param[0] as Property;
        //int skillId = (int)_param[1];

        if (sourceOb.CheckStatus(statusList))
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 伤害类型触发属性判断检查，附带概率检查
/// </summary>
public class SCRIPT_6911 : Script
{
    private static List<string> statusList = new List<string>(){ "B_CAN_NOT_CHOOSE", };

    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property source = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCMapping triggerPara = _param[2] as LPCMapping;
        LPCValue selectArgs = _param[3] as LPCValue;
        Property target = _param[4] as Property;
        if (target.CheckStatus(statusList))
            return false;

        // 获取伤害类型
        int damageType = triggerPara.GetValue<int>("damage_type");

        // 如果不是需要触发的伤害类型，不能触发
        if ((damageType & selectArgs.AsInt) == 0)
            return false;

        // 如果血量为0则不起效
        if (source.Query<int>("hp") <= 0)
            return false;

        //概率检查
        LPCArray propArg = propValue[0].AsArray;
        if (RandomMgr.GetRandom() >= propArg[0].AsInt)
            return false;

        // 返回数据
        return true;
    }
}

/// <summary>
/// 触发属性效果作用脚本，触发自身上状态（暴击概率触发，不带技能提示）
/// </summary>
public class SCRIPT_6912 : Script
{
    //private static List<string> elementList = new List<string>{ "normal_attack", "fire_attack", "cold_attack", "lightning_attack", "poison_attack" };

    public override object Call(params object[] _param)
    {
        //Property trigger = _param[0] as Property;
        //Property sourceOb = _param[1] as Property;
        LPCMapping sourceProfile = _param[2] as LPCMapping;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCArray propArg = propValue[0].AsArray;
        //LPCMapping triggerPara = _param[4] as LPCMapping;
        //LPCValue applyArg = _param[5] as LPCValue;
        //string propName = _param[6] as String;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("round", propArg[2].AsInt);
        condition.Add("source_profile", sourceProfile);

        // 附加状态
        sourceOb.ApplyStatus(propArg[1].AsString, condition);

        // 被动技能CD
        CdMgr.SkillCooldown(sourceOb, propArg[3].AsInt);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 邪恶诱惑追加回合效果检查脚本(只能在正常回合下进行触发，且需要检查目标状态是否对应)
/// </summary>
public class SCRIPT_6913 : Script
{
    public override object Call(params object[] _param)
    {
        // source, propValue, triggerPara, select_args, target, propName
        Property trigger = _param[0] as Property;
        LPCArray propValue = (_param[1] as LPCValue).AsArray;
        LPCArray propArg = propValue[0].AsArray;
        // 获取触发来源的信息
        LPCMapping triggerPara = _param[2] as LPCMapping;
        //LPCValue select_args = _param[3] as LPCValue;
        string triggerCookie = triggerPara.GetValue<string>("round_cookie");
        if (triggerPara.GetValue<int>("type") != RoundCombatConst.ROUND_TYPE_NORMAL)
            return false;

        //概率检查
        if (RandomMgr.GetRandom() >= propArg[0].AsInt)
            return false;

        // 检查目标状态
        int campId = (trigger.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId, new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

        // 比对状态来源和状态回合cookie
        LPCArray checkList = new LPCArray ();
        foreach (Property checkOb in finalList)
        {
            if (checkOb.CheckStatus(propArg[1].AsString))
            {
                List<LPCMapping> allStatus = checkOb.GetStatusCondition(propArg[1].AsString);
                LPCMapping condition = allStatus[0];

                // 检查来源
                if (!condition.ContainsKey("source_profile"))
                    continue;
                // 非自身来源跳过
                string sourceRid = condition.GetValue<LPCMapping>("source_profile").GetValue<string>("rid");
                if (!string.Equals(sourceRid, trigger.GetRid()))
                    continue;

                // 检查回合cookie
                if (!condition.ContainsKey("round_cookie"))
                    continue;

                if (!string.Equals(condition.GetValue<string>("round_cookie"), triggerCookie))
                    continue;

                checkList.Add(checkOb.GetRid());
            }
        }

        if (checkList.Count == 0)
            return false;

        // 返回数据
        return true;
    }
}
