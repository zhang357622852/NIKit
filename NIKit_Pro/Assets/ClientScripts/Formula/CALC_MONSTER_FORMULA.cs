/// <summary>
/// CALC_MONSTER_FORMULA.cs
/// Create by fengkk 2014-11-24
/// 基础公式
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 怪物基本经验
/// </summary>
public class CALC_BE : Formula
{
    public static int Call(int level)
    {
        return (int)(4 + 2 * level);
    }
}

// 对应等级期望玩家防御实伤率(废弃)
public class CALC_STD_USER_DEFENSE_PD : Formula
{
    public static float Call(int level)
    {
        float def_eco = 0.6667f;

        return def_eco;
    }
}

// 对应等级期望玩家抗性实伤率(废弃)
public class CALC_STD_USER_RESISTANCE_PD : Formula
{
    public static float Call(int level)
    {
        float res_eco = 1 - (0.000531f * level - 0.00742f);
        if (res_eco < 0.97f)
            res_eco = 0.97f;
        return res_eco;
    }
}

/// <summary>
/// 怪物攻击初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_ATTACK : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        return Game.Multiple(args.AsInt, CALC_BASIC_ATTRIB.Call(monster, "attack"), 1000);
    }
}

/// <summary>
/// 怪物防御初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_DEFENSE : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        return Game.Multiple(args.AsInt, CALC_BASIC_ATTRIB.Call(monster, "defense"), 1000);
    }
}

/// <summary>
/// 怪物最大生命初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_MAX_HP : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        return Game.Multiple(args.AsInt, CALC_BASIC_ATTRIB.Call(monster, "max_hp"), 1000);

    }
}

/// <summary>
/// 怪物敏捷初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_AGI : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        return Game.Multiple(args.AsInt, CALC_BASIC_ATTRIB.Call(monster, "agility"), 1000);
    }
}

/// <summary>
/// 怪物特殊属性初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_PROPS : Formula
{
    public static LPCArray Call(Property monster, LPCValue args, Property summonerOb)
    {
        return args.AsArray;
    }
}

/// <summary>
/// 怪物速度初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_SPD : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        return Game.Multiple(args.AsInt, CALC_BASIC_ATTRIB.Call(monster, "speed"), 1000) + monster.QueryAttrib("add_speed");
    }
}

/// <summary>
/// 怪物最大能量初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_MAX_MP : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        int maxMp = args.AsInt;

        return maxMp;
    }
}

/// <summary>
/// 怪物奖励属性初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_ATTRIB : Formula
{
    public static LPCValue Call(Property monster, LPCValue args, Property summonerOb)
    {
        return args;
    }
}

/// <summary>
/// 怪物重生次数初始化
/// </summary>
public class CALC_INIT_REBORN_TIMES : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        return args.AsInt;
    }
}

/// <summary>
/// 怪物奖励规则初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_BONUS_RULE : Formula
{
    public static LPCArray Call(Property monster, LPCValue args, Property summonerOb)
    {
        return args.AsArray;
    }
}

/// <summary>
/// 初始化怪物经验奖励系数
/// </summary>
public class CALC_MONSTER_INIT_EXP_CO : Formula
{
    public static float Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 相对经验=怪物难度*2
        float expco = args.AsFloat * 2.0f;
        return expco;
    }
}

/// <summary>
/// 怪物技能初始化（预留）
/// </summary>
public class CALC_MONSTER_INIT_SKILLS : Formula
{
    public static LPCArray Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取实体的rank
        int rank = monster.Query<int>("rank");

        // 获取实体的初始化技能列表
        LPCMapping initSkills = monster.BasicQueryNoDuplicate<LPCMapping>("init_skills");

        // 没有技能列表
        if (initSkills.Count == 0)
            return LPCArray.Empty;

        // 获取该rank阶段技能信息
        LPCArray skills = initSkills.GetValue<LPCArray>(rank);
        if (skills == null)
            return LPCArray.Empty;

        // 返回技能信息
        return skills;
    }
}

/// <summary>
/// 竞技场NPC怪物附加属性初始化
/// </summary>
public class CALC_ARENA_INIT_IMP_PROPS : Formula
{
    public static LPCArray Call(Property monster, LPCValue args, Property summonerOb)
    {
        // assign_props
        LPCArray assign_props = LPCArray.Empty;

        // 满附加等级
        int max_level = args.AsMapping.GetValue<int>("max_level");
        // 单条属性百分比
        int single_imp = args.AsMapping.GetValue<int>("single_imp");
        // 敏捷最大附加
        // 暴击最大附加
        // 命中最大附加
        // 抵抗最大附加

        // 怪物附加属性等级
        int level = monster.GetLevel();
        level = (level > max_level) ? max_level : level;

        // 取怪物属性类别
        int type = monster.Query<int>("type");
        int attrib_value = 0;

        // 初始化攻击百分比附加
        // 攻击型、速度型使魔攻击3份，其他类型攻击1份
        attrib_value = (type == MonsterConst.MONSTER_ATK_TYPE || type == MonsterConst.MONSTER_SPD_TYPE) ? single_imp * 3 : single_imp;
        assign_props.Add(new LPCArray(5, attrib_value * level / max_level));

        // 初始化防御百分比附加
        attrib_value = (type == MonsterConst.MONSTER_DEF_TYPE) ? single_imp * 3 : single_imp;
        assign_props.Add(new LPCArray(6, attrib_value * level / max_level));

        // 初始化生命百分比附加
        attrib_value = (type == MonsterConst.MONSTER_SUP_TYPE || type == MonsterConst.MONSTER_HP_TYPE) ? single_imp * 3 : single_imp * 2;
        assign_props.Add(new LPCArray(7, attrib_value * level / max_level));

        // 初始化敏捷附加
        assign_props.Add(new LPCArray(8, args.AsMapping.GetValue<int>("agi_imp") * level / max_level));
        // 初始化暴击率附加
        assign_props.Add(new LPCArray(12, args.AsMapping.GetValue<int>("crt_imp") * level / max_level));
        // 初始化效果抵抗附加
        assign_props.Add(new LPCArray(11, args.AsMapping.GetValue<int>("res_imp") * level / max_level));
        // 初始化效果命中附加
        assign_props.Add(new LPCArray(10, args.AsMapping.GetValue<int>("acc_imp") * level / max_level));

        // 返回值
        return assign_props;
    }
}

/// <summary>
/// 通天塔怪物通用属性初始化
/// </summary>
public class CALC_TOWER_MONSTER_INIT_ATTRIB : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取难度
        int difficulty = monster.Query<int>("difficulty");
        // 获取层数
        int layer = monster.Query<int>("layer");
        // 获取波次
        //int batch = monster.Query<int>("batch");
        // 获取怪物技能伤害类型
        int skillDmgType = monster.Query<int>("skill_damage_type");
        // 获取属性名
        string attrib = args.AsString;

        // 如果是BOSS
        if (monster.QueryAttrib("is_boss") == 1)
            attrib = string.Format("{0}_{1}", "boss", attrib);

        // 获取标准属性
        int stdAttrib = StdMgr.GetTowerStdAttrib(difficulty, layer, attrib);

        // 获取属性增幅
        LPCMapping para = LPCMapping.Empty;
        para.Add("difficulty", difficulty);
        para.Add("layer", layer);
        int scale = StdMgr.GetStdAttribScale(skillDmgType, MapConst.TOWER_MAP, args.AsString, para);

        // 返回属性数值
        return Game.Multiple(stdAttrib, scale, 1000);
    }
}

/// <summary>
/// 通天塔怪物通用增量属性初始化
/// </summary>
public class CALC_TOWER_MONSTER_INIT_ATTRIB_ADDVALUE : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取难度
        int difficulty = monster.Query<int>("difficulty");
        // 获取层数
        int layer = monster.Query<int>("layer");
        // 获取波次
        //int batch = monster.Query<int>("batch");
        // 获取怪物技能伤害类型
        int skillDmgType = monster.Query<int>("skill_damage_type");
        // 获取属性名
        string attrib = args.AsString;

        // 如果是BOSS
        if (monster.QueryAttrib("is_boss") == 1)
            attrib = string.Format("{0}_{1}", "boss", attrib);

        // 获取标准属性
        int stdAttrib = StdMgr.GetTowerStdAttrib(difficulty, layer, attrib);

        // 获取属性增幅
        LPCMapping para = LPCMapping.Empty;
        para.Add("difficulty", difficulty);
        para.Add("layer", layer);
        int scale = StdMgr.GetStdAttribScale(skillDmgType, MapConst.TOWER_MAP, args.AsString, para);

        // 返回属性数值
        return Game.Multiple(stdAttrib, scale, 1000);
    }
}

/// <summary>
/// 通天塔怪物通用属性初始化，属性跟波次相关1,2波通用，第3波不同
/// </summary>
public class CALC_TOWER_MONSTER_INIT_ATTRIB_BATCH : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取难度
        int difficulty = monster.Query<int>("difficulty");
        // 获取层数
        int layer = monster.Query<int>("layer");
        // 获取波次
        int batch = monster.Query<int>("batch");
        // 获取怪物技能伤害类型
        int skillDmgType = monster.Query<int>("skill_damage_type");
        // 获取属性名
        string attrib = args.AsString;

        if (batch == 2)
            attrib = string.Format("{0}_{1}", attrib, batch + 1);

        // 获取标准属性
        int stdAttrib = StdMgr.GetTowerStdAttrib(difficulty, layer, attrib);

        // 获取属性增幅
        LPCMapping para = LPCMapping.Empty;
        para.Add("difficulty", difficulty);
        para.Add("layer", layer);
        int scale = StdMgr.GetStdAttribScale(skillDmgType, MapConst.TOWER_MAP, args.AsString, para);

        // 返回属性数值
        return Game.Multiple(stdAttrib, scale, 1000);
    }
}

/// <summary>
/// 通天塔怪物生命属性初始化，属性跟波次相关
/// </summary>
public class CALC_TOWER_MONSTER_INIT_HP_BATCH : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取难度
        int difficulty = monster.Query<int>("difficulty");
        // 获取层数
        int layer = monster.Query<int>("layer");
        // 获取波次
        int batch = monster.Query<int>("batch");
        // 获取怪物技能伤害类型
        int skillDmgType = monster.Query<int>("skill_damage_type");
        // 获取属性名
        string attrib = string.Format("{0}_{1}", args.AsString, batch);

		// 如果是BOSS
		if (monster.QueryAttrib("is_boss") == 1)
			attrib = string.Format("{0}_{1}", "boss", args.AsString);

        // 获取标准属性
        int stdAttrib = StdMgr.GetTowerStdAttrib(difficulty, layer, attrib);

        // 获取属性增幅
        LPCMapping para = LPCMapping.Empty;
        para.Add("difficulty", difficulty);
        para.Add("layer", layer);
        int scale = StdMgr.GetStdAttribScale(skillDmgType, MapConst.TOWER_MAP, args.AsString, para);

        // 返回属性数值
        return Game.Multiple(stdAttrib, scale, 1000);
    }
}

/// <summary>
/// 试炼塔怪物技能初始化
/// </summary>
public class CALC_TOWER_MONSTER_INIT_SKILLS : Formula
{
    public static LPCArray Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取实体的rank
        int rank = monster.Query<int>("rank");

        // 获取实体的初始化技能列表
        LPCMapping initSkills = monster.BasicQueryNoDuplicate<LPCMapping>("init_skills");

        // 没有技能列表
        if (initSkills.Count == 0)
            return LPCArray.Empty;

        // 获取该rank阶段技能信息
        LPCArray skills = initSkills.GetValue<LPCArray>(rank);
        if (skills == null)
            return LPCArray.Empty;

        LPCArray newSkills = new LPCArray();

        for (int i = 0; i < skills.Count; i++)
        {
            LPCArray maxLvSkill = new LPCArray();
            int skillId = skills[i].AsArray[0].AsInt;
            CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);
            if (skillInfo.Query<int>("skill_family") == SkillType.SKILL_LEADER)
                continue;
            int maxLv = SkillMgr.GetSkillMaxLevel(skillId);
            maxLvSkill.Add(skillId, maxLv);
            newSkills.Add(maxLvSkill);
        }

        LPCArray specSkills = monster.Query<LPCArray>("spec_skills");

        // 检查是否需要增加初始化额外配置的技能
        if (specSkills != null)
            newSkills.Add(specSkills);

        // 返回技能信息
        return newSkills;
    }
}

/// <summary>
/// 隐藏圣域怪物属性初始化
/// </summary>
public class CALC_FRIEND_DUNGEN_MONSTER_INIT : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取复活次数
        int rebornTimes = monster.QueryTemp<int>("spec_reborn_times");

        // 获取怪物技能伤害类型
        int skillDmgType = monster.Query<int>("skill_damage_type");

        // 获取标准属性
        int stdAttrib = StdMgr.GetFriendDungeonStdAttrib(Math.Min(rebornTimes, 19), args.AsString);

        // 获取属性增幅
        int scale = StdMgr.GetStdAttribScale(skillDmgType, MapConst.SECRET_DUNGEONS_MAP, args.AsString, LPCMapping.Empty);

        // 返回属性数值
        return Game.Multiple(stdAttrib, scale, 1000);
    }
}

/// <summary>
/// 隐藏圣域怪物星级初始化
/// </summary>
public class CALC_FRIEND_DUNGEN_MONSTER_INIT_ATTRIB : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 返回属性数值
        return monster.Query<int>(args.AsString);
    }
}

/// <summary>
/// 隐藏圣域怪物技能初始化
/// </summary>
public class CALC_FRIEND_DUNGEN_MONSTER_INIT_SKILL : Formula
{
    public static LPCArray Call(Property monster, LPCValue args, Property summonerOb)
    {
        // 获取实体的rank
        int rank = monster.Query<int>("rank");

        // 获取实体的初始化技能列表
        LPCMapping initSkills = monster.BasicQueryNoDuplicate<LPCMapping>("init_skills");

        // 没有技能列表
        if (initSkills.Count == 0)
            return LPCArray.Empty;

        // 获取该rank阶段技能信息
        LPCArray skills = initSkills.GetValue<LPCArray>(rank);
        if (skills == null)
            return LPCArray.Empty;

        LPCArray newSkills = new LPCArray();

        for (int i = 0; i < skills.Count; i++)
        {
            LPCArray maxLvSkill = new LPCArray();
            int skillId = skills[i].AsArray[0].AsInt;
            int maxLv = SkillMgr.GetSkillMaxLevel(skillId);
            maxLvSkill.Add(skillId, maxLv);
            newSkills.Add(maxLvSkill);
        }

        LPCArray specSkills = monster.Query<LPCArray>("spec_skills");

        // 检查是否需要增加初始化额外配置的技能
        if (specSkills != null)
        {
            for (int i = 0; i < specSkills.Count; i++)
            {
                newSkills.Add(specSkills[i].AsArray);
            }
        }

        // 返回技能信息
        return newSkills;
    }
}

/// <summary>
/// 召唤物属性初始化（直接继承类）
/// </summary>
public class CALC_SUMMON_MONSTER_INIT_ATTRIB : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonOb)
    {
        // 返回属性数值
        return summonOb.Query<int>(args.AsString);
    }
}

/// <summary>
/// 召唤物白字属性初始化（非直接继承类）
/// </summary>
public class CALC_SUMMON_MONSTER_INIT : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonOb)
    {
        // 完整继承属性包括绿字，用QueryAttrib
        int attribValue = summonOb.Query<int>(args.AsString);

        // 获取属性继承百分比
        int scale = monster.Query<int>("summon_rate");

        // 获取技能升级效果
        int upgradeValue = monster.Query<int>("summon_rate_upgrade");

        // 计算继承百分比之后的属性值
        attribValue = Game.Multiple(attribValue, scale, 1000);

        // 计算技能升级带来的影响
        attribValue += Game.Multiple(attribValue, upgradeValue, 1000);

        // 返回属性数值
        return attribValue;
    }
}

/// <summary>
/// 召唤物白字属性加值初始化（非直接继承类）
/// </summary>
public class CALC_SUMMON_MONSTER_INIT_ADD : Formula
{
    public static int Call(Property monster, LPCValue args, Property summonOb)
    {
        // 完整继承属性包括绿字，用QueryAttrib
        int summonObAttrib = summonOb.QueryAttrib(args.AsString);
        int attribValue = summonObAttrib;

        // 获取属性继承百分比
        int scale = monster.Query<int>("summon_rate");

        // 获取技能升级效果
        int upgradeValue = monster.Query<int>("summon_rate_upgrade");

        // 计算继承百分比之后的属性值
        attribValue = Game.Multiple(attribValue, scale, 1000);

        // 计算技能升级带来的影响
        attribValue += Game.Multiple(attribValue, upgradeValue, 1000);

        // 返回属性数值
        return attribValue - summonObAttrib;
    }
}

/// <summary>
/// 召唤物特殊属性初始化（例如触发属性等）
/// </summary>
public class CALC_SUMMON_MONSTER_INIT_ASSIGN_PROP : Formula
{
    private static List<string> propList = new List<string>(){ "attack", "defense","agility", "crt_rate",
                                                               "crt_dmg_rate", "accuracy_rate", "resist_rate",
                                                               "max_hp", "sts_attack_rate", "attack_rate",
                                                               "defense_rate", "hp_rate", "agility_rate",
                                                               "sts_defense_rate", "sts_crt_rate", "sts_agility_rate",};

    private static List<string> triggerPropList = new List<string>(){ "damaged_counter_atk_eqpt", "stun_when_attack", "addition_round_eqpt", "vamp_hp_when_dmg_eqpt",};

    public static LPCArray Call(Property monster, LPCValue args, Property summonOb)
    {
        LPCArray newFinalProps = new LPCArray();

        LPCArray propTrigger = new LPCArray();
        foreach (string propName in triggerPropList)
        {
            LPCArray propArgs = new LPCArray();

            string triggerPropPath = "trigger/" + propName;

            int triggerValue = summonOb.QueryTemp<int>(triggerPropPath);

            propArgs.Add(PropMgr.GetPropId(propName), triggerValue);

            propTrigger.Add(propArgs);
        }
        newFinalProps.Append(propTrigger);

        // 获取属性继承百分比
        int scale = monster.Query<int>("summon_rate");

        // 获取技能升级效果
        int upgradeValue = monster.Query<int>("summon_rate_upgrade");

        // 获取装备属性
        LPCArray eqptProps = summonOb.QueryTemp<LPCArray>("equip_props");
        LPCArray newEqptProps = new LPCArray();
        if (eqptProps != null)
        {
            foreach (LPCValue checkArgs in eqptProps.Values)
            {
                CsvRow propInfo = PropMgr.GetPropInfo(checkArgs.AsArray[0].AsInt);
                foreach (string propName in propList)
                {
                    if (propInfo.Query<string>("prop_name").Equals(propName))
                    {
                        LPCArray newProps = new LPCArray();
                        int newValue = checkArgs.AsArray[1].AsInt;
                        // 计算继承百分比之后的属性值
                        newValue = Game.Multiple(newValue, scale, 1000);
                        // 计算技能升级带来的影响
                        newValue += Game.Multiple(newValue, upgradeValue, 1000);
                        newProps.Add(checkArgs.AsArray[0].AsInt, newValue);
                        newEqptProps.Add(newProps);
                    }
                }
            }
            newFinalProps.Append(newEqptProps);
        }

        // 获取技能属性
        LPCArray skillProps = summonOb.QueryTemp<LPCArray>("skill_props"); 
        LPCArray newSkillProps = new LPCArray();
        if (skillProps != null)
        {
            foreach (LPCValue checkArgs in skillProps.Values)
            {
                CsvRow propInfo = PropMgr.GetPropInfo(checkArgs.AsArray[0].AsInt);
                foreach (string propName in propList)
                {
                    if (propInfo.Query<string>("prop_name").Equals(propName))
                    {
                        LPCArray newProps = new LPCArray();
                        int newValue = checkArgs.AsArray[1].AsInt;
                        // 计算继承百分比之后的属性值
                        newValue = Game.Multiple(newValue, scale, 1000);
                        // 计算技能升级带来的影响
                        newValue += Game.Multiple(newValue, upgradeValue, 1000);
                        newProps.Add(checkArgs.AsArray[0].AsInt, newValue);
                        newSkillProps.Add(newProps);
                    }
                }
            }
            newFinalProps.Append(newSkillProps);
        }

        // 获取状态属性
        LPCArray statusProps = summonOb.QueryTemp<LPCArray>("status_props"); 
        LPCArray newStatusProps = new LPCArray();
        if (statusProps != null)
        {
            foreach (LPCValue checkArgs in statusProps.Values)
            {
                CsvRow propInfo = PropMgr.GetPropInfo(checkArgs.AsArray[0].AsInt);
                foreach (string propName in propList)
                {
                    if (propInfo.Query<string>("prop_name").Equals(propName))
                    {
                        LPCArray newProps = new LPCArray();
                        int newValue = checkArgs.AsArray[1].AsInt;
                        // 计算继承百分比之后的属性值
                        newValue = Game.Multiple(newValue, scale, 1000);
                        // 计算技能升级带来的影响
                        newValue += Game.Multiple(newValue, upgradeValue, 1000);
                        newProps.Add(checkArgs.AsArray[0].AsInt, newValue);
                        newStatusProps.Add(newProps);
                    }
                }
            }
            newFinalProps.Append(newStatusProps);
        }

        return newFinalProps;
    }
}