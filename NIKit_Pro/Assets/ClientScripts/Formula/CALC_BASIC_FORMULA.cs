/// <summary>
/// CALC_BASIC_FORMULA.cs
/// Create by fengkk 2014-11-24
/// 基础公式
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 单条属性标准战力
/// </summary>
public class CALC_STD_POWER : Formula
{
    public static float Call(int level)
    {
        return 1;
    }
}

/// <summary>
/// 标准转化公式 1，用于将 当前值/标准值 转化为百分比
/// cur_ratio = 当前值/标准值    para = 转化系数
/// 返回值为千分位整数
/// </summary>
public class CALC_STD_CONV : Formula
{
    public static int Call(int cur_ratio, int para)
    {
        if (cur_ratio >= 0)
            return 1000 * cur_ratio / (cur_ratio + para);
        else
            // 用于负防御，其他地方也可使用
            return 1000 * (para - cur_ratio) / para;
    }
}

/// <summary>
/// 计算普通防御的实伤率，返回百分比（为千分位整数）
/// </summary>
public class CALC_NORMAL_DEFENSE_RATE : Formula
{
    public static int Call(int normal_defense)
    {
        int para_calc = 500; // 标准值转化系数
        int std_normal_defense = 666;
        int cur_def = normal_defense * 1000 / std_normal_defense;

        if (cur_def >= 0)
            // 正数的话，1-减伤率
            return 1000 - CALC_STD_CONV.Call(cur_def, para_calc);
        else
            return CALC_STD_CONV.Call(cur_def, para_calc);
    }
}

/// <summary>
/// 标准总金钱公式
/// </summary>
public class CALC_STD_MONEY : Formula
{
    public static int Call(int level)
    {
        // 具体数值通过配置表读取，不在实时计算
        return (int)Math.Ceiling(45 + 3.2f * level + 0.01f * Math.Pow(level, 2) + 0.00003f * Math.Pow(level, 3));
    }
}

/// <summary>
/// 升 N 级标准金钱量
/// </summary>
public class CALC_STD_MONEY_SINGLE_LEVEL : Formula
{
    public static int Call(int level, int step = 1)
    {
        return 0;
    }
}

/// <summary>
/// 1 钻石 = X 标准金钱，当前是60
/// </summary>
public class CALC_STD_MONEY_ONE_DIAMOND : Formula
{
    public static int Call(int level)
    {
        // 1 钻石 = 60 标准金钱
        // 1 钻石 = 3 分钟
        return CALC_STD_MONEY.Call(level) * 60;
    }
}

/// <summary>
// 计算宠物攻击属性初始化
/// </summary>
public class CALC_PET_INIT_ATK : Formula
{
    public static int Call(Property pet, LPCValue args, Property summonerOb)
    {
        // 计算基础白字属性
        int atk = CALC_BASIC_ATTRIB.Call(pet, "attack");

        atk = (int)(args.AsFloat * atk);

        // 返回值
        return atk;
    }
}

/// <summary>
// 计算宠物防御属性初始化
/// </summary>
public class CALC_PET_INIT_DEF : Formula
{
    public static int Call(Property pet, LPCValue args, Property summonerOb)
    {
        // 计算基础白字属性
        int def = CALC_BASIC_ATTRIB.Call(pet, "defense");

        // 加上附加绿字属性
        def = (int)(args.AsFloat * def);

        // 返回值
        return def;
    }
}

/// <summary>
// 计算宠物最大生命属性初始化
/// </summary>
public class CALC_PET_INIT_MAX_HP : Formula
{
    public static int Call(Property pet, LPCValue args, Property summonerOb)
    {
        // 计算基础白字属性
        int max_hp = CALC_BASIC_ATTRIB.Call(pet, "max_hp");

        // 加上附加绿字属性
        max_hp = (int)(args.AsFloat * max_hp);

        // 返回值
        return max_hp;
    }
}

/// <summary>
// 计算宠物最大能量属性初始化
/// </summary>
public class CALC_PET_INIT_MAX_MP : Formula
{
    public static int Call(Property pet, LPCValue args, Property summonerOb)
    {
        // 计算基础白字属性
        int max_mp = CALC_BASIC_ATTRIB.Call(pet, "max_mp");

        // 加上附加绿字属性

        // 返回值
        return max_mp;
    }
}

/// <summary>
// 计算宠物敏捷属性初始化
/// </summary>
public class CALC_PET_INIT_AGI : Formula
{
    public static int Call(Property pet, LPCValue args, Property summonerOb)
    {
        // 计算基础白字属性
        int agi = CALC_BASIC_ATTRIB.Call(pet, "agility");

        agi = (int)(args.AsFloat * agi);

        // 返回值
        return agi;
    }
}

/// <summary>
// 计算宠物攻击速度属性初始化
/// </summary>
public class CALC_PET_INIT_SPD : Formula
{
    public static int Call(Property pet, LPCValue args, Property summonerOb)
    {
        // 计算基础白字属性
        int speed = CALC_BASIC_ATTRIB.Call(pet, "speed");

        // 加上附加绿字属性
        speed = (int)(args.AsFloat * speed);

        // 返回值
        return speed;
    }
}

/// <summary>
// 计算宠物掉落规则初始化
/// </summary>
public class CALC_PET_INIT_BONUS_RULE : Formula
{
    public static LPCArray Call(Property pet, LPCValue args, Property summonerOb)
    {
        // 如果不是副本怪物，而是攻击方宠物
        LPCValue batch = pet.Query<LPCValue>("batch");
        if (batch == null)
            return LPCArray.Empty;

        // 如果是副本怪物（包括竞技场防守宠物）
        return args.AsArray;
    }
}

/// <summary>
/// 生命初始化（预留）
/// </summary>
public class CALC_INIT_HP : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        int hp = (int)(monster.Query<int>("max_hp") * args.AsInt);

        return hp;
    }
}

/// <summary>
/// 能量初始化（预留）
/// </summary>
public class CALC_INIT_MP : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        return args.AsInt;
    }
}

/// <summary>
/// 进场表现初始化
/// </summary>
public class CALC_INIT_APPEAR_ACTION : Formula
{
    public static string Call(Property monster, LPCValue args, Property summonerOb)
    {
        return args.AsString;
    }
}

/// <summary>
// 计算宠物基础属性
/// </summary>
public class CALC_BASIC_ATTRIB : Formula
{
    public static int Call(Property pet, string attrib)
    {
        switch (attrib)
        {
            case "max_hp":
                // 计算宠物基础最大生命值
                return CALC_PET_INIT_BASE_MAX_HP.Call(pet);
            case "attack":
                // 计算宠物基础攻击
                return CALC_PET_INIT_BASE_ATTACK.Call(pet);
            case "defense":
                // 计算宠物基础防御
                return CALC_PET_INIT_BASE_DEFENSE.Call(pet);
            case "max_mp":
                // 计算宠物基础最大能量
                return CALC_PET_INIT_BASE_MAX_MP.Call(pet);
            case "agility":
                // 计算宠物基础敏捷
                return CALC_PET_INIT_BASE_AGILITY.Call(pet);
            case "speed":
                // 计算宠物基础速度
                return CALC_PET_INIT_BASE_SPEED.Call(pet);
            default:
                return 0;
        }
    }
}

/// <summary>
/// 使魔基础最大生命初始化
/// </summary>
public class CALC_PET_INIT_BASE_MAX_HP : Formula
{
    public static int Call(Property monster)
    {
        /*
        使魔基础生命算法=
            当前星级使魔标准最大生命*(1000+当前使魔生命系数)*50% +
            当前星级使魔标准最大生命*(1000+当前使魔生命系数)*35%/当前星级最大等级*当前使魔等级 +
            已经觉醒？
                增加剩下的 15% 觉醒份额，当前星级使魔标准最大生命*(1000+当前使魔生命系数)*15% +
                觉醒额外赠送属性？ 当前星级使魔标准最大生命*(1000+当前使魔生命系数)×生命觉醒系数
        */

        int result;
        int star = monster.Query<int>("star");

        // 当前使魔的标准最大生命
        int std_max_hp = StdMgr.GetStdAttrib("base_max_hp", star) * (1000 + monster.Query<int>("hp_co")) / 1000;

        result = (std_max_hp * 500 + std_max_hp * 350 * monster.Query<int>("level") / StdMgr.GetStdAttrib("max_level", star)) / 1000;

        // 判断使魔是否觉醒
        if (monster.Query<int>("rank") == 2)
            result += std_max_hp * (150 + monster.Query<int>("hp_awaken")) / 1000;

        return result;
    }
}

/// <summary>
/// 使魔基础攻击初始化
/// </summary>
public class CALC_PET_INIT_BASE_ATTACK : Formula
{
    public static int Call(Property monster)
    {
        int result;
        int star = monster.Query<int>("star");

        // 当前使魔的标准
        int std_attack = StdMgr.GetStdAttrib("base_attack", star) * (1000 + monster.Query<int>("atk_co")) / 1000;

        result = (std_attack * 500 + std_attack * 350 * monster.Query<int>("level") / StdMgr.GetStdAttrib("max_level", star)) / 1000;

        // 判断使魔是否觉醒
        if (monster.Query<int>("rank") == 2)
            result += std_attack * (150 + monster.Query<int>("atk_awaken")) / 1000;

        return result;
    }
}

/// <summary>
/// 使魔基础防御初始化
/// </summary>
public class CALC_PET_INIT_BASE_DEFENSE : Formula
{
    public static int Call(Property monster)
    {
        int result;
        int star = monster.Query<int>("star");

        // 当前使魔的标准
        int std_defense = StdMgr.GetStdAttrib("base_defense", star) * (1000 + monster.Query<int>("def_co")) / 1000;

        result = (std_defense * 500 + std_defense * 350 * monster.Query<int>("level") / StdMgr.GetStdAttrib("max_level", star)) / 1000;

        // 判断使魔是否觉醒
        if (monster.Query<int>("rank") == 2)
            result += std_defense * (150 + monster.Query<int>("def_awaken")) / 1000;

        return result;
    }
}

/// <summary>
/// 使魔基础能量初始化
/// </summary>
public class CALC_PET_INIT_BASE_MAX_MP : Formula
{
    public static int Call(Property monster)
    {
        return 5;
    }
}

/// <summary>
/// 使魔基础敏捷初始化
/// </summary>
public class CALC_PET_INIT_BASE_AGILITY : Formula
{
    public static int Call(Property monster)
    {
        int result;

        result = monster.Query<int>("agility_co") - 1;

        // 判断使魔是否觉醒
        // +1 是因为我们默认使魔觉醒会赠送 1 点敏捷
        if (monster.Query<int>("rank") == 2)
            result += monster.Query<int>("agi_awaken") + 1;

        return result;
    }
}

/// <summary>
/// 使魔基础速度初始化：返回值仅包含觉醒附赠的速度
/// </summary>
public class CALC_PET_INIT_BASE_SPEED : Formula
{
    public static int Call(Property monster)
    {
        int result;

        result = monster.Query<int>("agility");

        // 判断使魔是否觉醒
        if (monster.Query<int>("rank") == 2)
            return result + monster.Query<int>("spd_awaken");

        return result;
    }
}
