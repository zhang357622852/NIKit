/// <summary>
/// CALC_ATTRIB_FORMULA.cs
/// Create by dingly 2014-12-4
/// 属性计算类公式
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 计算(使魔)附加属性的加值
/// </summary>
public class CALC_PET_IMP_ATTRIB : Formula
{
    public static int Call(Property user, LPCMapping improvement, string attrib)
    {
        switch (attrib)
        {
            case "max_hp":
                // 最大生命加值
                return CALC_PET_IMP_MAX_HP.Call(user, improvement);
            case "max_mp":
                // 最大能量加值
                return CALC_PET_IMP_MAX_MP.Call(user, improvement);
            case "agility":
                // 敏捷加值
                return CALC_PET_IMP_AGI.Call(user, improvement);
            case "speed":
                // 出手速度加值
                return CALC_PET_IMP_SPD.Call(user, improvement);
            case "attack":
                // 攻击力加值
                return CALC_PET_IMP_ATK.Call(user, improvement);
            case "defense":
                // 防御力加值
                return CALC_PET_IMP_DEF.Call(user, improvement);
            case "resist_rate":
                // 效果抵抗加值
                return CALC_PET_IMP_RESIST_RATE.Call(user, improvement);
            case "accuracy_rate":
                // 效果命中加值
                return CALC_PET_IMP_ACCURACY_RATE.Call(user, improvement);
            case "crt_rate":
                // 暴击率加值
                return CALC_PET_IMP_CRITICAL_RATE.Call(user, improvement);
            case "crt_dmg_rate":
                // 暴击伤害加值
                return CALC_PET_IMP_CRT_DMG_RATE.Call(user, improvement);
            case "skill_effect":
                // 技能效果加值
                return CALC_PET_IMP_SKILL_EFFECT.Call(user, improvement);
            default:
                // 不应该执行到这里
                return 0;
        }
    }
}

/// <summary>
/// 计算(使魔)原始属性
/// </summary>
public class CALC_PET_ORIGINAL_ATTRIB : Formula
{
    public static int Call(Property user, LPCMapping improvement, string attrib)
    {
        switch (attrib)
        {
            case "agility":
                // 敏捷
                return CALC_PET_ORIGINAL_AGI.Call(user, improvement);

            case "max_hp":
                // 最大血量
                return CALC_PET_ORIGINAL_MAX_HP.Call(user, improvement);

            default:
                // 不应该执行到这里
                return 0;
        }
    }
}

/// <summary>
/// 计算(怪物)附加属性的加值（废弃）
/// </summary>
public class CALC_MONSTER_IMP_ATTRIB : Formula
{
    public static int Call(Property user, LPCMapping improvement, string attrib)
    {
        switch (attrib)
        {
            case "max_hp":
                // 最大生命加值
                return CALC_MONSTER_IMP_MAX_HP.Call(user, improvement);
            case "attack":
                // 攻击加值
                return CALC_MONSTER_IMP_ATTACK.Call(user, improvement);
            default:
                // 不应该执行到这里
                return 0;
        }
    }
}

/// <summary>
/// 计算(使魔)最大HP加值
/// </summary>
public class CALC_PET_IMP_MAX_HP : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 获取原始属性数值
        int originalAttrib = pet.QueryOriginalAttrib("max_hp");

        // 获取该属性基础数值
        int basicMaxHp = pet.QueryAttrib("max_hp", false);

        // max_hp伤害或者治愈带来的加值
        int additionMaxHp = pet.Query<int>("attrib_addition/max_hp");

        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        // 1. 计算属性加成
        // 2. 计算直接累加的属性（来源:装备，状态，技能等）
        // 3. 计算max_hp伤害或者治愈带来的加值
        return originalAttrib - basicMaxHp + additionMaxHp;
    }
}

/// <summary>
/// 计算(使魔)最大MP加值
/// </summary>
public class CALC_PET_IMP_MAX_MP : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 计算直接累加的属性（来源:装备，状态，技能等）
        return improvement.GetValue<int>("max_mp");
    }
}

/// <summary>
/// 计算(使魔)敏捷加值
/// </summary>
public class CALC_PET_IMP_AGI : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 计算直接累加的属性（来源:装备，状态，技能等）
        int value = improvement.GetValue<int>("agility");
        int basicAgility = pet.QueryAttrib("agility", false);

        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        value += basicAgility * (improvement.GetValue<int>("agility_rate") + improvement.GetValue<int>("leader_agility_rate"))/ 1000;

        // 状态敏捷百分比=(base+imp)*（1+百分比）
        value += (value + basicAgility) * improvement.GetValue<int>("sts_agility_rate") / 1000;

        return value;
    }
}

/// <summary>
/// 计算(使魔)敏捷原始值
/// 只是包含技能，装备，队长光环带来的数值
/// </summary>
public class CALC_PET_ORIGINAL_AGI : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 计算属性（来源:装备，队长光环，技能）
        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        int value = improvement.GetValue<int>("agility") +
            pet.QueryAttrib("agility", false) * (1000 + improvement.GetValue<int>("agility_rate") + improvement.GetValue<int>("leader_agility_rate")) / 1000;

        // 返回数值
        return value;
    }
}

/// <summary>
/// 计算(使魔)max_hp原始值
/// </summary>
public class CALC_PET_ORIGINAL_MAX_HP : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        int basicMaxHp = pet.QueryAttrib("max_hp", false);

        // 计算属性加成rate
        int rate = pet.QueryAttrib("hp_rate_sp") +
            improvement.GetValue<int>("hp_rate") +
            improvement.GetValue<int>("leader_hp_rate");

        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        // 1. 计算属性加成
        // 2. 计算直接累加的属性（来源:装备，状态，技能等）
        return basicMaxHp + improvement.GetValue<int>("max_hp") + Game.Multiple(basicMaxHp, rate);
    }
}

/// <summary>
/// 计算(使魔)出手速度加值
/// </summary>
public class CALC_PET_IMP_SPD : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 计算直接累加的属性（来源:装备，状态，技能等）
        int value = improvement.GetValue<int>("speed") + improvement.GetValue<int>("agility");

        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        // 出手速度的白字=白字敏捷提供的速度+觉醒赠送的速度
        int basic_speed = pet.QueryAttrib("agility", false) + pet.QueryAttrib("speed", false);
        value += basic_speed * improvement.GetValue<int>("speed_rate") / 1000;

        // 状态速度百分比=(base+imp)*（1+百分比）
        value += (value + basic_speed) * improvement.GetValue<int>("sts_speed_rate") / 1000;

        return value;
    }
}

/// <summary>
/// 计算(使魔)攻击力加值
/// </summary>
public class CALC_PET_IMP_ATK : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 计算直接累加的属性（来源:装备，状态，技能等）
        int value = improvement.GetValue<int>("attack");
        int basicAttack = pet.QueryAttrib("attack", false);

        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        value += basicAttack *
        (improvement.GetValue<int>("attack_rate") + improvement.GetValue<int>("leader_attack_rate")) / 1000;

        // 状态攻击力百分比=(base+imp)*（1+百分比）
        value += (value + basicAttack) * improvement.GetValue<int>("sts_attack_rate") / 1000;

        return value;
    }
}

/// <summary>
/// 计算(使魔)防御力加值
/// </summary>
public class CALC_PET_IMP_DEF : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 计算直接累加的属性（来源:装备，状态，技能等）
        int value = improvement.GetValue<int>("defense");
        int basicDefense = pet.QueryAttrib("defense", false);

        // 最后计算按百分比加成的属性（只对白字属性进行百分比加成）
        value += basicDefense *
        (improvement.GetValue<int>("defense_rate") + improvement.GetValue<int>("leader_defense_rate")) / 1000;

        // 状态防御百分比=(base+imp)*（1+百分比）
        value += (value + basicDefense) * improvement.GetValue<int>("sts_defense_rate") / 1000;

        return value;
    }
}

/// <summary>
/// 计算(使魔)效果抵抗加值
/// </summary>
public class CALC_PET_IMP_RESIST_RATE : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        int value = CombatConst.DEFAULT_RESIST_VALUE;

        // 计算直接累加的属性（来源:装备，状态，技能等）
        value += improvement.GetValue<int>("resist_rate") + improvement.GetValue<int>("leader_resist_rate");

        // 计算特殊加值
        value += pet.Query<int>("resist_rate_add");

        return Mathf.Min(value, 1000);
    }
}

/// <summary>
/// 计算(使魔)效果命中加值
/// </summary>
public class CALC_PET_IMP_ACCURACY_RATE : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        int value = 0;

        // 计算直接累加的属性（来源:装备，状态，技能等）
        value += improvement.GetValue<int>("accuracy_rate") + improvement.GetValue<int>("leader_accuracy_rate");

        // 计算特殊加值
        value += pet.Query<int>("accuracy_rate_add");

        return Mathf.Min(value, 1000);
    }
}

/// <summary>
/// 计算(使魔)暴击率加值
/// </summary>
public class CALC_PET_IMP_CRITICAL_RATE : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        int value = CombatConst.DEFAULT_DEADLY_VALUE;

        // 计算直接累加的属性（来源:装备，状态，技能等）
        value += improvement.GetValue<int>("crt_rate") + improvement.GetValue<int>("leader_crt_rate");

        value += improvement.GetValue<int>("sts_crt_rate");

        // 计算特殊加值
        value += pet.Query<int>("crt_rate_add");

        // 最大100%暴击率
        return Mathf.Min(value, 1000);
    }
}

/// <summary>
/// 计算(使魔)暴击伤害加值
/// </summary>
public class CALC_PET_IMP_CRT_DMG_RATE : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        int value = CombatConst.DEFAULT_DEADLY_DAMAGE;

        // 计算直接累加的属性（来源:装备，状态，技能等）
        value += improvement.GetValue<int>("crt_dmg_rate");

        // 计算特殊加值
        value += pet.Query<int>("crt_dmg_rate_add");

        return value;
    }
}

/// <summary>
/// 计算(使魔)技能效果加值
/// </summary>
public class CALC_PET_IMP_SKILL_EFFECT : Formula
{
    public static int Call(Property pet, LPCMapping improvement)
    {
        // 计算直接累加的属性（来源:装备，状态，技能等）
        int value = improvement.GetValue<int>("skill_effect");

        // 计算敏捷转化的技能效果
        value += CALC_AGILITY_TO_SKILL_EFFECT.Call(pet.QueryAttrib("agility"));

        // 状态效果百分比=value*（1+百分比）
        value += value * improvement.GetValue<int>("sts_skill_effect_rate") / 1000;

        return value;
    }
}

/// <summary>
/// 计算(怪物)最大HP加值
/// </summary>
public class CALC_MONSTER_IMP_MAX_HP : Formula
{
    public static int Call(Property user, LPCMapping improvement)
    {
        // 计算直接累加的属性
        int value = improvement.GetValue<int>("max_hp");

        // 最后计算按百分比加成的属性（需考虑基础属性的影响）
        value += (value + user.Query<int>("max_hp")) *
        improvement.GetValue<int>("rate_max_hp") / 1000;

        return value;
    }
}

/// <summary>
/// 计算(怪物)攻击加值
/// </summary>
public class CALC_MONSTER_IMP_ATTACK : Formula
{
    public static int Call(Property user, LPCMapping improvement)
    {
        // 计算直接累加的属性
        int value = improvement.GetValue<int>("attack");

        // 最后计算按百分比加成的属性（需考虑基础属性的影响）
        value += (value + user.Query<int>("attack")) *
        improvement.GetValue<int>("rate_attack") / 1000;

        return value;
    }
}

/// <summary>
/// 计算战斗攻击速度的影响机制
/// </summary>
public class CALC_BATTLE_SPEED : Formula
{
    private static List<int> chaosSpeedList = new List<int>(){ 2, 2, 4, 6, 8, 10, 12 };

    public static int Call(Property user)
    {
        int speed = user.QueryAttrib("speed");
        int chaosRange = chaosSpeedList[user.Query("star").AsInt] + 1;

        // 计算乱敏
        int chaosSpeed = RandomMgr.GetRandom(chaosRange);
        speed += chaosSpeed;

        return speed;
    }
}


/// <summary>
/// 使魔敏捷属性转化的技能效果作用
/// </summary>
public class CALC_AGILITY_TO_SKILL_EFFECT : Formula
{
    public static int Call(int agility)
    {
        /*if (agility == 90)
            return 0;

        if (agility < 90)
            return 20 * (agility - 90);*/

        return 2 * (agility - 90);
    }
}
