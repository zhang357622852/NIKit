/// <summary>
/// CALC_SKILL_FORMULA.cs
/// Create by fengkk 2014-11-24
/// 技能相关的计算公式
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 计算技能效果修正（直接修正）
/// </summary>
public class CALC_SKILL_EFFECT : Formula
{
    public static int Call(Property who, int skill_id)
    {
        LPCValue improvement = who.QueryTemp("improvement");

        // 非玩家、非宠物，简化计算
        if (!(who is User) && (who.Query<int>("pet_type") == 0))
        {
            // 怪物，只支持 skill_name_effect_
            return improvement.AsMapping.GetValue<int>("skill_name_effect_" + skill_id);
        }

        int all_skill_total, skill_style_fix, attrib_type_fix, skill_name_fix, skill_type_fix,
        attrib_type, skill_type, style_type;

        // 获取技能配置信息
        CsvRow skillRow = SkillMgr.GetSkillInfo(skill_id);
        // 没有配置的技能不处理
        if (skillRow == null)
            return 0;

        // 获取技能职业
        style_type = skillRow.Query<int>("style_type");
        // 获取技能系别 物理、法术
        attrib_type = skillRow.Query<int>("attrib_type");
        // 获取技能类别，主要、次要、战术、被动
        skill_type = skillRow.Query<int>("skill_type");

        // 所有技能效果
        all_skill_total = improvement.AsMapping.GetValue<int>("all_skill_effect");
        // 某职业技能效果
        skill_style_fix = improvement.AsMapping.GetValue<int>("skill_style_type_effect_" + style_type);
        // 某系技能效果：物理、法术
        attrib_type_fix = improvement.AsMapping.GetValue<int>("skill_attrib_type_effect_" + attrib_type);
        // 具体技能效果
        skill_name_fix = improvement.AsMapping.GetValue<int>("skill_name_effect_" + skill_id);
        // 技能类别效果
        skill_type_fix = improvement.AsMapping.GetValue<int>("skill_skill_type_effect_" + skill_type);

        return all_skill_total + skill_style_fix + attrib_type_fix + skill_name_fix + skill_type_fix;
    }
}

/// <summary>
/// 计算忽视类属性的效果
/// </summary>
public class CALC_IGNORE_ATTRIB_FIX : Formula
{
    public static int Call(int base_value, int ignore_value, int ignore_rate = 0, int below_zero = 0)
    {
        // 不允许低于0时，原本就小于0的不变化
        if (below_zero == 0 && base_value < 0)
            return base_value;

        int remain_value;

        if (ignore_rate > 0)
            // ignore_rate 千分位
            remain_value = base_value + ignore_value + Game.Multiple(ignore_rate, base_value);
        else
            remain_value = base_value + ignore_value;

        if (below_zero == 0)
        // 最多忽视到0
            return Math.Max(0, remain_value);
        else
            return remain_value;
    }
}

/// <summary>
/// 计算格挡与否
/// </summary>
public class CALC_BLOCK_RATE : Formula
{
    public static bool Call(LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int restrain)
    {
        // 检查是否有免疫格挡标识
        if (sourceProfile.ContainsKey("rate_ignore_block"))
        {
            if (RandomMgr.GetRandom() < sourceProfile.GetValue<int>("rate_ignore_block"))
                return false;
        }
        int blockRes = 0;
        if (sourceProfile.ContainsKey("rate_block_res"))
            blockRes = sourceProfile.GetValue<int>("rate_block_res");

        // 如果被克制，则攻击时被格挡几率提高50%
        if (restrain == ElementConst.ELEMENT_DISADVANTAGE)
            restrain = CombatConst.RESTRAIN_BLOCK_VALUE;
        else
            restrain = 0;

        // 获取非叠加类格挡属性
        LPCArray blockSpArg = improvementMap.GetValue<LPCArray>("block_rate_sp");
        int blockRateSp = 0;
        if (blockSpArg != null)
            blockRateSp = blockSpArg.MaxInt();

        int blockRate = blockRateSp + improvementMap.GetValue<int>("block_rate") + sourceProfile.GetValue<int>("blind_rate") + restrain - blockRes;

        if (RandomMgr.GetRandom() < blockRate)
            return true;

        return false;
    }
}

/// <summary>
/// 计算是否无视防御
/// </summary>
public class CALC_IS_IGNORE_DEF : Formula
{
    public static bool Call(LPCMapping sourceProfile)
    {
        // 计算是否无视防御
        if (RandomMgr.GetRandom() < sourceProfile.GetValue<int>("rate_ignore_def"))
            return true;

        // 计算生命百分比无视防御的属性效果
        if (sourceProfile.GetValue<int>("hp_rate") < sourceProfile.GetValue<int>("ignore_def_by_hp"))
            return true;

        return false;
    }
}

/// <summary>
/// 计算是否穿刺
/// </summary>
public class CALC_IS_PIERCE : Formula
{
    public static bool Call(LPCMapping sourceProfile)
    {
        if (sourceProfile.GetValue<int>("is_pierce") == 1)
            return true;

        return false;
    }
}

/// <summary>
/// 计算暴击与否
/// </summary>
public class CALC_DEADLY_ATTACK_RATE : Formula
{
    public static bool Call(LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int restrain)
    {
        // 检查目标身上的受创概率无法被暴击属性
        if (improvementMap.ContainsKey("dmg_no_crt"))
        {
            if (RandomMgr.GetRandom() < improvementMap.GetValue<int>("dmg_no_crt"))
                return false;
        }

        // 如果有特殊暴击标识，则必定暴击（例如剩余能量大于多少必定暴击之类）
        if (sourceProfile.GetValue<int>("extra_crt_id") > 0)
            return true;

        // 检查生命低于百分比造成固定暴击属性
        int tarMaxHp = finalAttribMap.GetValue<int>("max_hp");
        if (tarMaxHp < sourceProfile.GetValue<int>("low_hp_crt"))
            return true;

        // 如果克制，暴击几率提升15%
        // 如果被克制，则暴击几率降低15%
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            restrain = CombatConst.RESTRAIN_DEADLY_VALUE;
        else if (restrain == ElementConst.ELEMENT_DISADVANTAGE)
            restrain = -CombatConst.RESTRAIN_DEADLY_VALUE;

        // 特殊暴击率提升（例如生命越低，暴击越高）
        int spCrtRate = 0;
        if (sourceProfile.ContainsKey("more_crt_low_hp"))
        {
            LPCArray propValue = sourceProfile.GetValue<LPCArray>("more_crt_low_hp");
            spCrtRate = propValue[0].AsArray[1].AsInt * (1000 - sourceProfile.GetValue<int>("hp_rate")) / propValue[0].AsArray[0].AsInt;
        }

        if (sourceProfile.ContainsKey("more_crt_high_def"))
            spCrtRate += sourceProfile.GetValue<int>("more_crt_high_def");

        if (sourceProfile.GetValue<int>("hp_value_compare") > 0 && sourceProfile.ContainsKey("tarhp_high_crt_up"))
            spCrtRate += sourceProfile.GetValue<int>("tarhp_high_crt_up");

        int baseRate = sourceProfile.GetValue<int>("crt_rate") +
                       sourceProfile.GetValue<int>("sp_crt_rate") -
                       improvementMap.GetValue<int>("de_crt_rate") +
                       restrain + spCrtRate;

        if (RandomMgr.GetRandom() < baseRate)
            return true;

        return false;
    }
}

/// <summary>
/// 计算伤害类型
/// </summary>
public class CALC_SKILL_DAMAGE_TYPE : Formula
{
    public static int Call(LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int restrain)
    {
        // 计算伤害类型（暴击，触发等等等等）
        int damageType = CombatConst.DAMAGE_TYPE_ATTACK;

        // 计算是否格挡
        bool is_block = CALC_BLOCK_RATE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算是否暴击（格挡不会触发暴击）
        bool is_deadly_attack = false;
        if (!is_block)
            is_deadly_attack = CALC_DEADLY_ATTACK_RATE.Call(finalAttribMap, improvementMap, sourceProfile, restrain);

        // 计算是否强击（克制时，且攻击没有被格挡、没有暴击时，必定强击）
        bool is_strike = false;
        if (restrain == ElementConst.ELEMENT_ADVANTAGE && !is_block && !is_deadly_attack)
            is_strike = true;

        // 计算是否有减伤
        //bool is_reduce_dmg = CALC_IS_REDUCE_DAMAGE.Call(improvementMap, sourceProfile);

        // 计算无视防御
        bool is_ignore_def = CALC_IS_IGNORE_DEF.Call(sourceProfile);

        // 计算穿刺攻击
        bool is_pierce = CALC_IS_PIERCE.Call(sourceProfile);

        // 伤害类型
        damageType = is_block ? damageType | CombatConst.DAMAGE_TYPE_BLOCK : damageType;             // 格挡
        damageType = is_deadly_attack ? damageType | CombatConst.DAMAGE_TYPE_DEADLY : damageType;    // 暴击
        damageType = is_strike ? damageType | CombatConst.DAMAGE_TYPE_STRIKE : damageType;           // 强击
        //damageType = is_reduce_dmg ? damageType | CombatConst.DAMAGE_TYPE_REDUCE : damageType;     // 减伤
        damageType = is_ignore_def ? damageType | CombatConst.DAMAGE_TYPE_IGNORE_DEF : damageType;   // 无视防御
        damageType = is_pierce ? damageType | CombatConst.DAMAGE_TYPE_PIERCE : damageType;           // 穿刺攻击

        return damageType;
    }
}

/// <summary>
/// 计算玩家自身攻击力+技能放大系数
/// </summary>
public class CALC_SKILL_PLAYER_NORMAL_ATTACK : Formula
{
    public static int Call(LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageRate)
    {
        // 获取基础攻击力
        int baseAtk = sourceProfile.GetValue<int>("base_atk");

        // 计算特殊基础放大系数
        int baseUpRate = 0;
        if (sourceProfile.ContainsKey("more_atk_low_hp"))
        {
            LPCArray propValue = sourceProfile.GetValue<LPCArray>("more_atk_low_hp");
            baseUpRate += propValue[0].AsArray[1].AsInt * (1000 - sourceProfile.GetValue<int>("hp_rate")) / propValue[0].AsArray[0].AsInt;
        }

        // 额外攻击力加成
        int extraAtk = Game.Multiple(sourceProfile.GetValue<int>("max_hp"), sourceProfile.GetValue<int>("hp_into_attack"), 1000);
        // 按照当前生命百分比增加白字攻击力加成
        extraAtk += Game.Multiple(sourceProfile.GetValue<int>("hp") ,sourceProfile.GetValue<int>("atk_up_by_hp"), 1000);
        // 状态影响单独处理，和绿字处理不一样，破攻，加攻
        extraAtk += Game.Multiple(extraAtk ,sourceProfile.GetValue<int>("sts_attack_rate"), 1000);

        return (baseAtk * (1000 + baseUpRate) / 1000 + sourceProfile.GetValue<int>("imp_atk") + extraAtk) * damageRate / 1000;
    }
}

/// <summary>
/// 计算技能伤害中-攻方的-百分比修正量
/// </summary>
public class CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX : Formula
{
    public static int Call(LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        int final = 0;

        // 被克制时，伤害降低10%
        int restrain = sourceProfile.GetValue<int>("restrain");
        if (restrain == ElementConst.ELEMENT_DISADVANTAGE)
            final += -CombatConst.RESTRAIN_DAMAGE_VALUE;

        // 1、先检查检查格挡情况，触发格挡伤害降低30%
        if ((damageType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            final += -300;

        // 2、检查暴击情况
        if ((damageType & CombatConst.DAMAGE_TYPE_DEADLY) == CombatConst.DAMAGE_TYPE_DEADLY)
        {
            // 检查暴击加深情况
            if (sourceProfile.ContainsKey("crt_dmg_co_by_hp"))
                final += sourceProfile.GetValue<int>("crt_dmg_rate") +
                    (1000 - sourceProfile.GetValue<int>("hp_rate")) * sourceProfile.GetValue<int>("crt_dmg_co_by_hp") / 1000;
            else
                final += sourceProfile.GetValue<int>("crt_dmg_rate");
        }

        // 3、检查强击情况，强击伤害提升30%(暴击和强击不会同时出现，所以此处不用判断)
        if ((damageType & CombatConst.DAMAGE_TYPE_STRIKE) == CombatConst.DAMAGE_TYPE_STRIKE)
            final += CombatConst.RESTRAIN_STRIKE_VALUE;

        return final;
    }
}

/// <summary>
/// 计算技能伤害中-攻方的-百分比修正量(不计算暴击和强击的情况)
/// </summary>
public class CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX_SP : Formula
{
    public static int Call(LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        int final = 0;

        // 被克制时，伤害降低10%
        int restrain = sourceProfile.GetValue<int>("restrain");
        if (restrain == ElementConst.ELEMENT_DISADVANTAGE)
            final += -CombatConst.RESTRAIN_DAMAGE_VALUE;

        // 1、先检查检查格挡情况，触发格挡伤害降低30%
        if ((damageType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            final += -300;

        return final;
    }
}

/// <summary>
/// 计算额外伤害（防御之前）
/// </summary>
public class CALC_EXTRA_DAMAGE : Formula
{
    public static int Call(int attack, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 有憎恨与嫉妒时的增伤检查，按照自身生命上限追加伤害
        if (sourceProfile.ContainsKey("tarhp_low_add_dmg") && sourceProfile.GetValue<int>("hp_value_compare") <= 0)
            attack += sourceProfile.GetValue<int>("max_hp") * sourceProfile.GetValue<int>("tarhp_low_add_dmg") / 1000;

        return attack;
    }
}

/// <summary>
/// 根据防御计算伤害
/// </summary>
public class CALC_SKILL_DAMAGE_BY_DEFENSE : Formula
{
    public static int Call(int attack, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 计算额外伤害（增减）
        attack = CALC_EXTRA_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 检查无视防御情况
        if ((damageType & CombatConst.DAMAGE_TYPE_IGNORE_DEF) == CombatConst.DAMAGE_TYPE_IGNORE_DEF)
            return attack;
        else
        {
            int fixedDef = finalAttribMap.GetValue<int>("defense") * (1000 - sourceProfile.GetValue<int>("ignore_def")) / 1000;
            return CALC_NORMAL_DEFENSE_RATE.Call(fixedDef) * attack / 1000;
        }
    }
}

/// <summary>
/// 计算真实伤害
/// </summary>
public class CALC_REAL_DAMAGE : Formula
{
    public static int Call(int attack, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 检查是否有暴猿属性加成(暴击附加一定敌方当前生命比例的真实伤害)
        if (sourceProfile.ContainsKey("barasaru") && ((damageType & CombatConst.DAMAGE_TYPE_DEADLY) == CombatConst.DAMAGE_TYPE_DEADLY))
        {
            attack += sourceProfile.GetValue<int>("barasaru") * finalAttribMap.GetValue<int>("cur_hp") / 1000;
            // 提示技能图标
            Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
            BloodTipMgr.AddSkillTip(sourceOb, sourceProfile.GetValue<int>("skill_id"));
        }

        // 按照自身损失血量百分比的真伤
        if (sourceProfile.GetValue<int>("real_damage_lost_hp_co") > 0)
            attack += sourceProfile.GetValue<int>("lost_hp") * sourceProfile.GetValue<int>("real_damage_lost_hp_co") / 1000;

        return attack;
    }
}

/// <summary>
/// 计算技能伤害中-守方的-百分比修正量
/// </summary>
public class CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX : Formula
{
    // 收集improvement中的相关属性
    private static List<string> elementReduceDmg = new List<string>()
    {
        /*
            //怪物元素类型
            public const int ELEMENT_NONE = 0;
            public const int ELEMENT_FIRE = 1;
            public const int ELEMENT_STORM = 2;
            public const int ELEMENT_WATER = 3;
            public const int ELEMENT_LIGHT = 4;
            public const int ELEMENT_DARK = 5;
            */
        "reduce_none", "reduce_fire_damage_rate", "reduce_storm_damage_rate", "reduce_water_damage_rate", "reduce_light_damage_rate", "reduce_dark_damage_rate"
    };

    public static int Call(int attack, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 如果攻方有无视减伤属性，则直接返回伤害
        if (sourceProfile.ContainsKey("ignore_reduce_dmg"))
            return attack;

        // 计算常规的百分比变化
        LPCArray reduceDmg = improvementMap.GetValue<LPCArray>("reduce_damage_rate");
        if (reduceDmg == null)
            reduceDmg = new LPCArray();

        // 先加入元素减伤到列表中
        reduceDmg.Add(improvementMap.GetValue<int>(elementReduceDmg[sourceProfile.GetValue<int>("element")]));

        // 1.增益带来减伤
        if (improvementMap.ContainsKey("dmg_reuce_by_source_buffs"))
            reduceDmg.Add(sourceProfile.GetValue<int>("buff_count") * improvementMap.GetValue<int>("dmg_reuce_by_source_buffs"));
        //2.能量带来减伤
        if (improvementMap.ContainsKey("mp_reduce_dmg"))
        {
            int reduceMpCo = improvementMap.GetValue<int>("mp_reduce_dmg");
            int reducePer = (finalAttribMap.GetValue<int>("max_mp") - finalAttribMap.GetValue<int>("cur_mp")) * reduceMpCo;
            reduceDmg.Add(reducePer);
        }
        // 3.计算特殊减伤(攻方敏捷低于守方)
        if (improvementMap.ContainsKey("high_agi_reduce_dmg"))
        {
            if (finalAttribMap.GetValue<int>("agility") > sourceProfile.GetValue<int>("agility"))
                reduceDmg.Add(improvementMap.GetValue<int>("high_agi_reduce_dmg"));
        }
        // 4.如果有特殊减伤，例如幻影之壁的被动暗影迷雾，被没有增益状态目标攻击减少90%
        if (improvementMap.ContainsKey("no_buff_reduce_dmg") && sourceProfile.GetValue<int>("buff_count") == 0)
            reduceDmg.Add(improvementMap.GetValue<int>("no_buff_reduce_dmg"));
        // 5.特殊技能效果加成，自身生命越低所受伤害越低
        if (improvementMap.ContainsKey("low_hp_dmged_down"))
        {
            LPCArray reduceArgs = improvementMap.GetValue<LPCArray>("low_hp_dmged_down");
            int lostHpCo = reduceArgs[0].AsArray[0].AsInt;
            int reduceCo = reduceArgs[0].AsArray[1].AsInt;
            int lost = 1000 - 1000 * finalAttribMap.GetValue<int>("cur_hp") / finalAttribMap.GetValue<int>("max_hp");
            reduceDmg.Add(lost / lostHpCo * reduceCo);
        }
        // 6.计算特殊减伤(根据场上存活敌人数量计算)
        if (improvementMap.ContainsKey("reduce_dmg_by_live_enemy"))
        {
            int liveCount = sourceProfile.GetValue<int>("our_live_count");
            int dieCount = sourceProfile.GetValue<int>("our_die_count");
            if (liveCount + dieCount != 0)
                reduceDmg.Add(improvementMap.GetValue<int>("reduce_dmg_by_live_enemy") * dieCount / (liveCount + dieCount));
        }

        // 取同时起效里效果最高的减伤
        int reduceRate = 0;
        if (reduceDmg.Count != 0)
            reduceRate = reduceDmg.MaxInt();

        // 如果有特殊受伤不会降低标识
        if (improvementMap.ContainsKey("dmg_up_by_status"))
        {
            // 判断是否有对应状态
            string targetStatus = improvementMap.GetValue<LPCArray>("dmg_up_by_status")[0].AsArray[0].AsString;
            Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
            if (sourceOb.CheckStatus(targetStatus))
                reduceRate = 0;
        }

        // 如果有陷阱状态的减伤，且没有忽视陷阱减伤属性，则需要额外添加陷阱减伤，只计算最高的陷阱减伤
        int extraReduce = 0;
        if (improvementMap.ContainsKey("trap_reduce_dmg") && !sourceProfile.ContainsKey("ignore_trap_reduce"))
        {
            LPCArray trapReduceDmg = improvementMap.GetValue<LPCArray>("trap_reduce_dmg");
            extraReduce += trapReduceDmg.MaxInt();
        }

        // 种族元素减伤需要叠加
        attack = attack * Math.Max((1000 - reduceRate - extraReduce), 0) / 1000;

        return attack;
    }
}

/// <summary>
/// 对最终伤害进行修正
/// </summary>
public class CALC_FINAL_DAMAGE_FIX : Formula
{
    public static int Call(int attack, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 做上下10%的扰动
        // 伤害浮动调试需要判断下
        int percent = 0;

        Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
        if (improvementMap.ContainsKey("low_agi_dmg_immune"))
        {
            if (finalAttribMap.GetValue<int>("agility") > sourceOb.QueryAttrib("agility"))
                return 0;
        }

#if UNITY_EDITOR
        if (sourceProfile.GetValue<int>("ignore_damage_floating_exchange") != 1 &&
            improvementMap.GetValue<int>("ignore_damage_floating_exchange") != 1)
            percent = RandomMgr.GetRandom(); // 0-999
        else
            percent = 0;
#else
        // 伤害浮动
        percent = RandomMgr.GetRandom(); // 1-1000
#endif
        // 技能升级效果
        attack += attack * sourceProfile.GetValue<int>("skill_damage_rate") / 1000;

        // 受创者死亡标记额外受到伤害增强
        attack = attack * (1000 - improvementMap.GetValue<int>("de_damage_rate")) / 1000;

        // 伤害浮动
        attack = attack * (10000 - percent) / 10000;

        // 计算最终伤害概率翻倍
        if (sourceProfile.ContainsKey("rate_times_dmg"))
        {
            LPCArray propValue = sourceProfile.GetValue<LPCArray>("rate_times_dmg");

            if (RandomMgr.GetRandom() <= propValue[0].AsArray[0].AsInt)
                attack = attack * propValue[0].AsArray[1].AsInt;
        }

        // 伤害增强
        if (sourceProfile.ContainsKey("damage_up_rate"))
            attack += attack * sourceProfile.GetValue<int>("damage_up_rate") / 1000;

        // 目标剩余能量起效的伤害增强
        if (sourceProfile.ContainsKey("damage_up_by_mp"))
            attack += attack * sourceProfile.GetValue<int>("damage_up_by_mp") / 1000;

        // 目标剩余生命起效的伤害增强
        if (sourceProfile.ContainsKey("damage_up_by_hp"))
            attack += attack * sourceProfile.GetValue<int>("damage_up_by_hp") / 1000;

        // 目标减益数量起效的伤害增强
        if (sourceProfile.ContainsKey("damage_up_by_debuff"))
            attack += attack * sourceProfile.GetValue<int>("damage_up_by_debuff") / 1000;

        // 根据敌方死亡目标数量技能伤害+%
        if (sourceProfile.ContainsKey("die_skl_damage_up"))
        {
            LPCArray propValue = sourceProfile.GetValue<LPCArray>("die_skl_damage_up");
            attack += attack * propValue[0].AsArray[1].AsInt * sourceProfile.GetValue<int>("target_die_count") / 1000;
        }

        // 根据己方死亡目标数量技能伤害+%
        if (sourceProfile.ContainsKey("dead_more_atk_dmg"))
            attack += attack * sourceProfile.GetValue<int>("dead_more_atk_dmg") * sourceProfile.GetValue<int>("our_die_count") / 1000;

        // 如果目标有特殊所受伤害增加属性
        if (improvementMap.ContainsKey("damaged_up_by_mate"))
        {
            attack += attack * sourceProfile.GetValue<int>("target_live_count") * improvementMap.GetValue<int>("damaged_up_by_mate") / 1000;
        }

        // 如果目标有特殊所受伤害增加属性、场上目标方死亡成员越多，目标受伤越多
        if (improvementMap.ContainsKey("dead_more_dmged"))
        {
            attack += attack * sourceProfile.GetValue<int>("target_die_count") * improvementMap.GetValue<int>("dead_more_dmged") / 1000;
        }

        // 如果目标有特殊所受伤害增加属性
        if (improvementMap.ContainsKey("dmg_up_by_status"))
        {
            LPCArray statusArgs = improvementMap.GetValue<LPCArray>("dmg_up_by_status");
            if (sourceOb.CheckStatus(statusArgs[0].AsString))
                attack += Game.Multiple(attack, statusArgs[1].AsInt , 1000);
        }

        // 如果有根据场上敌人存活数量计算的增伤
        if (sourceProfile.ContainsKey("dmged_up_by_live_emeny"))
        {
            int liveCount = sourceProfile.GetValue<int>("target_live_count");
            int DieCount = sourceProfile.GetValue<int>("target_die_count");
            if (liveCount + DieCount != 0)
            {
                int liveRate = sourceProfile.GetValue<int>("dmged_up_by_live_emeny") * liveCount / (liveCount + DieCount);
                attack += attack * liveRate / 1000;
            }
        }

        // 特殊技能效果加成，自身减益效果增加伤害
        if (sourceProfile.ContainsKey("debuff_atk_up"))
            attack += attack * sourceProfile.GetValue<int>("debuff_num") * sourceProfile.GetValue<int>("debuff_atk_up") / 1000;

        // 特殊技能效果加成，目标减益效果增加伤害
        if (sourceProfile.ContainsKey("tar_debuff_dmg_up"))
            attack += attack * sourceProfile.GetValue<int>("tar_debuff_num") * sourceProfile.GetValue<int>("tar_debuff_dmg_up") / 1000;

        // 特殊技能效果加成，自身生命越低伤害越高
        if (sourceProfile.ContainsKey("low_hp_dmg_up"))
        {
            LPCArray dmgArgs = sourceProfile.GetValue<LPCArray>("low_hp_dmg_up");
            int lostPerHp = dmgArgs[0].AsArray[0].AsInt;
            int dmgUp = dmgArgs[0].AsArray[1].AsInt;
            int lostHp = 1000 - sourceProfile.GetValue<int>("hp_rate");
            attack += Game.Multiple(attack, lostHp / lostPerHp * dmgUp, 1000);
        }

        // 特殊技能效果加成，自身生命越低伤害越高（通天塔版）
        if (sourceProfile.ContainsKey("low_hp_dmg_up_tower"))
        {
            LPCArray TdmgArgs = sourceProfile.GetValue<LPCArray>("low_hp_dmg_up_tower");
            int lostCo = TdmgArgs[0].AsArray[0].AsInt;
            int lostHpRate = 1000 - sourceProfile.GetValue<int>("hp_rate");
            attack += Game.Multiple(attack, lostHpRate / lostCo, 1000);
        }

        // 计算效果增幅
        attack += attack * sourceProfile.GetValue<int>("skill_effect") / 1000;

        // 如果目标有固定伤害百分比属性，则最多只按固定生命百分比进行伤害
        if (improvementMap.GetValue<int>("fixed_damage_rate") > 0)
        {
            int maxDmg = Game.Multiple(finalAttribMap.GetValue<int>("max_hp"), improvementMap.GetValue<int>("fixed_damage_rate"), 1000);
            attack = Math.Min(maxDmg, attack);
        }

        // 如果有反击减伤属性，需要最后做减伤处理
        if (sourceProfile.ContainsKey("counter_reduce_dmg"))
            attack -= Game.Multiple(attack, sourceProfile.GetValue<int>("counter_reduce_dmg"), 1000);

        // 如果攻击方有最小伤害信息，则需要判断最小伤害值
        if (sourceProfile.ContainsKey("min_dmg"))
            attack = Math.Max(sourceProfile.GetValue<int>("min_dmg"), attack);

        // 通天塔怪物伤害的最大值修正
        if (sourceProfile.ContainsKey("tower_dmg_limit"))
            attack = sourceProfile.GetValue<int>("tower_dmg_limit");

        return attack;
    }
}

// 计算“元素吸收”的治愈量
public class CALC_CURE_VALUE_BY_ELEMENTS : Formula
{
    private static List<KeyValuePair<string, string>> elementList = new List<KeyValuePair<string, string>>()
    {
        new KeyValuePair<string, string>("cure_by_fire_damage", "fire_attack"),
        new KeyValuePair<string, string>("cure_by_cold_damage", "cold_attack"),
        new KeyValuePair<string, string>("cure_by_lightning_damage", "lightning_attack"),
        new KeyValuePair<string, string>("cure_by_poison_damage", "poison_attack")
    };

    public static int Call(Property target, LPCMapping source_info, LPCMapping extraPara, LPCMapping damage_info)
    {
        // 火焰吸收,如5%,就是将5%的火焰伤害转为你的生命植,相当与10%的火焰抵抗
        int cure_value = 0;

        // 没有伤害数据
        if (damage_info.Count == 0)
            return cure_value;

        // 获取本次伤害
        int damage_hp = damage_info["points"].AsMapping.GetValue<int>("hp");

        // 遍历数据
        for (int i = 0; i < elementList.Count; i++)
        {
            int cure_rate = extraPara.GetValue<int>(elementList[i].Key) + extraPara.GetValue<int>("cure_by_elements_damage");

            // 吸收有最大值
            // cure_rate = Math.Min(cure_rate, CombatConst.DEFAULT_MAX_ELEMENT_DAMAGE_RATE);

            int damage = damage_info.GetValue<int>(elementList[i].Value);
            if (damage == 0)
                continue;

            // 计算吸收量
            int temp_cure = (int)((float)damage * cure_rate / 1000f);

            // 先扣除转移的伤害量
            damage_hp -= temp_cure;

            // 再加上恢复量
            cure_value += temp_cure;
        }

        // 修改最终伤害值
        damage_info["points"].AsMapping["hp"] = LPCValue.Create(damage_hp);

        return cure_value;
    }
}

/// <summary>
/// 计算技能伤害(纯攻击力)
/// </summary>
public class CALC_ALL_DAMAGE_ONLY_ATTACK : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(纯攻击力，最终伤害倍率化，倍击专用)
/// </summary>
public class CALC_ALL_DAMAGE_ONLY_ATTACK_MULTI : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType, int multiRate, int maxTimes)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 伤害翻倍
        int times = 1;
        while (true)
        {
            if (times >= maxTimes)
                break;
            if (RandomMgr.GetRandom() >= multiRate)
                break;
            times += 1;
        }

        attack = attack * times;

        if (times > 1)
        {
            Property sourceOb = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));
            string tips = string.Format("{0}{1}{2}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], times, LocalizationMgr.Get("tip_multi_dmg"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, sourceOb, tips);
        }

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 速度加成)(废弃)
/// </summary>
public class CALC_ALL_DAMAGE_RATE_ATTACK_SPEED : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 计算加成
        damageRate += sourceProfile.GetValue<int>("speed") * sourceProfile.GetValue<int>("speed_co") / 1000;

        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 目标持续伤害效果个数加成)
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_TARGET_INJURY_NUM : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 计算加成
        damageRate += sourceProfile.GetValue<int>("injury_damage_rate");

        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 目标减益效果个数加成)
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_TARGET_DEBUFF_NUM : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 计算加成
        damageRate += sourceProfile.GetValue<int>("debuff_atk_rate");

        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 防御加成)
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_DEFNESE : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算防御加成
        attack += sourceProfile.GetValue<int>("defense") * sourceProfile.GetValue<int>("defense_damage_co") / 1000;

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 防御加成 + 目标生命上限加成)
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_DEFNESE_TAR_MAXHP : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算防御加成
        attack += sourceProfile.GetValue<int>("defense") * sourceProfile.GetValue<int>("defense_damage_co") / 1000;

        // 计算目标生命上限加成
        int hpAddAtk = Math.Min(sourceProfile.GetValue<int>("tar_max_hp_damage_co") * finalAttribMap.GetValue<int>("max_hp") / 1000,
            sourceProfile.GetValue<int>("def_limit") * sourceProfile.GetValue<int>("defense"));
        attack += hpAddAtk;

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 目标生命上限加成)
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_TAR_MAXHP : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算目标生命上限加成
        attack += Game.Multiple(sourceProfile.GetValue<int>("tar_max_hp_damage_co"), finalAttribMap.GetValue<int>("max_hp"), 1000);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 防御百分比加成真实伤害)
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_DEFNESE_REAL : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 以防御的百分比附加真实伤害
        attack += sourceProfile.GetValue<int>("defense") * sourceProfile.GetValue<int>("defense_damage_co") / 1000;

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害(攻击力 + 攻击力百分比加成真实伤害)
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_ATTACK_REAL : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 以攻击的百分比附加真实伤害
        attack += sourceProfile.GetValue<int>("attack") * sourceProfile.GetValue<int>("attack_damage_co") / 1000;

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害（攻击力+损失生命值）
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_LOST_HP : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 根据损失生命值计算伤害增加
        attack += sourceProfile.GetValue<int>("lost_hp") * sourceProfile.GetValue<int>("lost_hp_damage_co") / 1000;

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害（攻击力+敌方剩余能量）
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_TARGET_REMAIN_MP : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 计算剩余能量加成
        damageRate += damageRate * sourceProfile.GetValue<int>("target_remain_mp") * sourceProfile.GetValue<int>("target_remain_mp_co") / 1000;

        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 根据敌方剩余能量计算伤害增加
        attack += sourceProfile.GetValue<int>("target_remain_mp") * sourceProfile.GetValue<int>("target_remain_mp_co") / 1000;

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害（攻击力+敌方最大生命值加成）
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_TARGET_MAX_HP : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 根据敌方最大生命值计算伤害增加
        int hpAtk = finalAttribMap.GetValue<int>("max_hp") * sourceProfile.GetValue<int>("target_max_hp_co") / 1000;
        // 计算原始攻击加成的比例
        int originAtkRate = 1000 * attack / (attack + hpAtk);
        sourceProfile.Add("origin_atk_rate", originAtkRate);
        attack += hpAtk;

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害（攻击力+自身生命值上限加成）
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_MAX_HP : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 根据自身生命值上限百分比计算伤害增加
        attack += sourceProfile.GetValue<int>("max_hp") * sourceProfile.GetValue<int>("max_hp_damage_co") / 1000;

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算技能伤害（攻击力 + 无暴击影响的敌人生命值上限加成）
/// </summary>
public class CALC_ALL_DAMAGE_ATTACK_TARGET_MAX_HP_NO_DEAD_INFLU : Formula
{
    public static int Call(int damageRate, LPCMapping finalAttribMap, LPCMapping improvementMap, LPCMapping sourceProfile, int damageType)
    {
        // 取攻击力和攻击加成
        int attack = CALC_SKILL_PLAYER_NORMAL_ATTACK.Call(finalAttribMap, improvementMap, sourceProfile, damageRate);

        // 计算攻击方放大系数
        attack = attack * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 根据敌人生命值上限百分比计算伤害增加
        int noInfluDmg = finalAttribMap.GetValue<int>("max_hp") * sourceProfile.GetValue<int>("max_hp_damage_co") / 1000;
        attack += noInfluDmg * (1000 + CALC_SKILL_DAMAGE_ATTACKER_PERCENT_FIX_SP.Call(finalAttribMap, improvementMap, sourceProfile, damageType)) / 1000;

        // 计算防御
        attack = CALC_SKILL_DAMAGE_BY_DEFENSE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 计算 守方 的各类减伤属性
        attack = CALC_SKILL_DAMAGE_DEFENSER_PERCENT_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 真实伤害类计算
        attack = CALC_REAL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        // 最终伤害修正（增减、伤害扰动等）
        attack = CALC_FINAL_DAMAGE_FIX.Call(attack, finalAttribMap, improvementMap, sourceProfile, damageType);

        return attack;
    }
}

/// <summary>
/// 计算治疗额外影响
/// </summary>
public class CALC_EXTRE_CURE : Formula
{
    public static int Call(int hpCure, int skillEffect, int reduceCure)
    {
        // 计算效果增幅
        hpCure += hpCure * skillEffect / 1000;

        hpCure = hpCure - hpCure * reduceCure / 1000;

        return hpCure;
    }
}

/// <summary>
/// 计算技能扣蓝伤害
/// </summary>
public class CALC_MP_DAMAGE : Formula
{
    public static int Call(LPCMapping data, Property targetOb)
    {
        // 1 初次检查成功率
        if (RandomMgr.GetRandom() >= data.GetValue<int>("mp_dmg_rate"))
            return 0;

        // 2 检查效果命中
        int rate = CALC_EFFECT_ACCURACY_RATE.Call(data.GetValue<int>("accuracy_rate"),
                       data.GetValue<int>("resist_rate"),
                       data.GetValue<int>("restrain"));
        if (RandomMgr.GetRandom() < rate)
        {
            string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_resist"));
            BloodTipMgr.AddTip(TipsWndType.DamageTip, targetOb, tips);
            return 0;
        }

        return data.GetValue<int>("mp_dmg");
    }
}

/// <summary>
/// 无敌计算
/// </summary>
public class INVINCIBLE_DAMAGE : Formula
{
    public static LPCMapping Call(Property who, int damageType, LPCMapping damageMap, LPCMapping sourceProfile)
    {
        // 没有任何伤害数值
        if (damageMap.Count == 0)
            return damageMap;

        // 先把damage_type扔进damagemap里
        damageMap.Add("damage_type", damageType);

        // 1.检查目标是处于伤害免疫状态、且伤害信息中没有无视无敌，且伤害类型不为穿刺
        if (who.CheckStatus("B_DMG_IMMUNE") && 
            !damageMap.ContainsKey("ignore_imo") && 
            (damageType & CombatConst.DAMAGE_TYPE_PIERCE) != CombatConst.DAMAGE_TYPE_PIERCE)
        {
            LPCMapping points = new LPCMapping();
            points.Add("hp", 0);
            damageMap.Add("points", points);
            damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_IMMUNITY);
            return damageMap;
        }

        // 1.检查目标是处于特殊伤害免疫状态，如镜花水月
        if (who.CheckStatus("B_MIRAGE_SHIELD"))
        {
            LPCMapping sourceImprovement = who.QueryTemp<LPCMapping>("improvement");
            LPCArray shieldArgs = sourceImprovement["cost_mp_dmg_immune"].AsArray;
            LPCMapping oriPoints = damageMap.GetValue<LPCMapping>("points");
            int oriDamage = oriPoints.GetValue<int>("hp");

            // 遍历属性内的所有光环源
            for (int i = 0; i < shieldArgs.Count; i++)
            {
                Property haloSource = Rid.FindObjectByRid(shieldArgs[i].AsArray[0].AsString);

                // 检查伤害量是否超过限制
                if (oriDamage <= Game.Multiple(haloSource.QueryAttrib("max_hp"), shieldArgs[i].AsArray[2].AsInt, 1000))
                    continue;

                // 检查成功率
                if (RandomMgr.GetRandom() >= shieldArgs[i].AsArray[1].AsInt)
                    continue;

                // 检查技能CD，暂时屏蔽看效果
                //if (CdMgr.SkillIsCooldown(haloSource, shieldArgs[i].AsArray[3].AsInt))
                //    continue;

                // 检查剩余能量
                if (SkillMgr.GetCasTCost(haloSource, shieldArgs[i].AsArray[3].AsInt).GetValue<int>("mp") > haloSource.Query<int>("mp"))
                    continue;

                LPCMapping points = new LPCMapping();
                points.Add("hp", 0);
                damageMap.Add("points", points);
                damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_IMMUNITY);

                // 暂时取消次数限制看效果
                //haloSource.SetTemp("mirage_times", LPCValue.Create(haloSource.QueryTemp<int>("mirage_times") + 1));
                // 对光环源技能进行CD和能量消耗
                // 扣能量
                LPCMapping haloCostMap = new LPCMapping();
                haloCostMap.Add("mp", 1);
                haloSource.CostAttrib(haloCostMap);

                // 技能CD，暂时屏蔽看效果
                //CdMgr.SkillCooldown(haloSource, shieldArgs[i].AsArray[3].AsInt);
                //haloSource.SetTemp("mirage_times", LPCValue.Create(0));
                // 技能提示
                BloodTipMgr.AddSkillTip(who,shieldArgs[i].AsArray[3].AsInt);
                return damageMap;
            }
        }

        return damageMap;
    }
}

/// <summary>
/// 护盾吸收伤害
/// </summary>
public class SHIELD_ABSORB_DAMAGE : Formula
{
    private static List<string> statusList = new List<string>(){ "B_EQPT_SHIELD", "B_HP_SHIELD", };

    public static LPCMapping Call(Property who, int damageType, LPCMapping damageMap, LPCMapping sourceProfile)
    {
        // 没有任何伤害数值
        if (damageMap.Count == 0)
            return damageMap;

        // 伤害为0直接返回
        LPCMapping oriPoints = damageMap.GetValue<LPCMapping>("points");
        if (oriPoints.GetValue<int>("hp") == 0)
            return damageMap;

        // 检查目标是否有单体攻击免疫属性
        if (sourceProfile.GetValue<int>("skill_range_type") == SkillType.RANGE_TYPE_SINGLE &&
            who.QueryAttrib("single_atk_immune") != 0)
        {
            LPCMapping points = new LPCMapping();
            points.Add("hp", 0);
            damageMap.Add("points", points);
            damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_IMMUNITY);
            return damageMap;
        }

        // 如果是消耗能量无敌
        if (who.CheckStatus("B_EN_COST_SHIELD"))
        {
            // 获取状态信息
            LPCMapping data = who.GetStatusCondition("B_EN_COST_SHIELD")[0];

            // 检查能量消耗是否足够，不足则直接返回伤害
            int mpCostValue = data.GetValue<int>("mp_cost");
            if (who.Query<int>("mp") < mpCostValue)
                return damageMap;

            // 消耗掉能量使护盾生效
            LPCMapping mpCost = new LPCMapping ();
            mpCost.Add("mp", mpCostValue);
            who.CostAttrib(mpCost);
            LPCMapping points = new LPCMapping();
            points.Add("hp", 0);
            damageMap.Add("points", points);
            damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_IMMUNITY | CombatConst.DAMAGE_TYPE_CHANGE);

            BloodTipMgr.AddSkillTip(who, data.GetValue<int>("skill_id"));
            return damageMap;
        }

        // 如果是特殊无敌需要检查伤害类型
        if (who.CheckStatus("B_DMG_IMMUNE_TYPE"))
        {
            // 检查伤害类型
            LPCMapping data = who.GetStatusCondition("B_DMG_IMMUNE_TYPE")[0];

            // 如果不是目标伤害类型，则直接返回伤害，如果是则无敌
            int tDamageType = data.GetValue<int>("damage_type");
            if ((damageType & tDamageType) == tDamageType)
                return damageMap;

            LPCMapping points = new LPCMapping();
            points.Add("hp", 0);
            damageMap.Add("points", points);
            damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_IMMUNITY);
            BloodTipMgr.AddSkillTip(who, data.GetValue<int>("skill_id"));
            return damageMap;
        }

        // 检查目标是处于伤害免疫状态
        if (who.CheckStatus("B_DMG_IMMUNE_SP"))
        {
            LPCMapping points = new LPCMapping();
            points.Add("hp", 0);
            damageMap.Add("points", points);
            damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_IMMUNITY);

            // 如果是特殊无敌需要提示
            BloodTipMgr.AddSkillTip(who, who.GetStatusCondition("B_DMG_IMMUNE_SP")[0].GetValue<int>("skill_id"));

            return damageMap;
        }

        // 2.检查目标处于次数护盾状态
        if (who.CheckStatus("B_EN_SHIELD"))
        {
            List<LPCMapping> shieldStatus = who.GetStatusCondition("B_EN_SHIELD");
            LPCMapping statusMap = shieldStatus[0];
            LPCMapping timesPoints = new LPCMapping();
            timesPoints.Add("hp", 0);
            damageMap.Add("points", timesPoints);
            damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_ABSORB);

            // 扣除状态可以吸收次数
            if (statusMap.GetValue<int>("can_absorb_times") == 1)
                who.ClearStatusByCookie(statusMap.GetValue<int>("cookie"));
            else
                statusMap.Add("can_absorb_times", statusMap.GetValue<int>("can_absorb_times") - 1);

            return damageMap;
        }

        // 2.检查目标处于护盾状态
        if (!who.CheckStatus(statusList))
            return damageMap;

        // 获取状态
        List<int> removeList = new List<int>();
        List <LPCMapping> allStatus = who.GetAllStatus();
        for (int i = 0; i < allStatus.Count; i++)
        {
            // 不是护盾状态
            if (! allStatus[i].ContainsKey("can_absorb_damage"))
                continue;

            // 承受伤害
            LPCMapping points = damageMap.GetValue<LPCMapping>("points");
            int receiveDmg = points.GetValue<int>("hp");

            // 可吸收伤害量，从临时血池里取出来
            int canAbsorbDmg = allStatus[i].GetValue<int>("can_absorb_damage");

            // 实际承受伤害
            int realDamage = Mathf.Max(receiveDmg - canAbsorbDmg, 0);

            // 剩余没了
            if (canAbsorbDmg <= receiveDmg)
                removeList.Add(allStatus[i].GetValue<int>("cookie"));
            else
                // 重置临时血池中的承伤量
                allStatus[i].Add("can_absorb_damage", Mathf.Max(canAbsorbDmg - receiveDmg, 0));

            // 重置伤害数值
            points.Add("hp", realDamage);
            damageMap.Add("points", points);
            if (realDamage != 0)
                damageMap.Add("damage_type", damageType);
            else
                damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_ABSORB);
        }

        // 清除状态
        who.ClearStatusByCookie(removeList);

        return damageMap;
    }
}

/// <summary>
/// 伤害分摊
/// </summary>
public class LINK_DAMAGE : Formula
{
    private static LPCMapping B_SACRIFICE_SP(Property who, int damgeType, LPCMapping damageMap, LPCMapping sourceProfile, LPCMapping oriPoints)
    {
        List<LPCMapping> allStatus = who.GetStatusCondition("B_SACRIFICE_SP");
        if (allStatus.Count == 0)
            return null;

        // 确保只有一个生命链接状态
        LPCMapping statusData = allStatus[0];

        // 获取分摊者rid
        string linkRid = statusData.GetValue<string>("source_rid");

        // 通知分摊者受创
        Property linkOb = Rid.FindObjectByRid(linkRid);

        // 如果分摊源死亡，直接返回伤害
        if (linkOb.CheckStatus("DIED"))
            return null;

        // 判断光环源能量是否足够，不够直接返回伤害
        int sourceMpRemain = linkOb.Query<int>("mp");
        int linkSkillId = statusData.GetValue<int>("skill_id");
        LPCMapping costData = SkillMgr.GetCasTCost(linkOb, linkSkillId);
        int mpCost = costData.GetValue<int>("mp");
        if (sourceMpRemain < mpCost)
            return null;

        // 判断是否到达目标血量，只有在目标血量以下才能起效，否则直接返回伤害
        int workHpRate = statusData.GetValue<int>("work_hp_rate");
        if (who.QueryTemp<int>("hp_rate") > workHpRate)
            return null;

        // 获取源伤害
        int sourceDmg = damageMap.GetValue<LPCMapping>("points").GetValue<int>("hp");

        // 获取分摊百分比计算实际伤害
        int fixDmg = (1000 - statusData.GetValue<int>("value_rate")) * sourceDmg / 1000;

        // 重新包装伤害结构
        LPCMapping fixPoints = new LPCMapping();
        fixPoints.Add("hp", fixDmg);
        damageMap.Add("points", fixPoints);

        // 构建分摊者受创信息
        LPCMapping linkDmgMap = new LPCMapping();
        linkDmgMap.Append(damageMap);
        LPCMapping linkPoints = new LPCMapping();
        linkPoints.Add("hp", sourceDmg - fixDmg);
        linkDmgMap.Add("points", linkPoints);
        linkDmgMap.Add("damage_type", damgeType);
        linkDmgMap.Add("link_id", 1);
        linkDmgMap.Add("target_rid", linkOb.GetRid());

        (linkOb as Char).ReceiveDamage(sourceProfile, damgeType, linkDmgMap);

        // 技能提示，并执行消耗光环源的能量
        BloodTipMgr.AddSkillTip(linkOb, linkSkillId);
        linkOb.CostAttrib(costData);

        // 返回伤害数据
        return damageMap;
    }

    private static LPCMapping B_SACRIFICE(Property who, int damgeType, LPCMapping damageMap, LPCMapping sourceProfile, LPCMapping oriPoints)
    {
        List<LPCMapping> allStatus = who.GetStatusCondition("B_SACRIFICE");
        if (allStatus.Count == 0)
            return null;

        // 确保只有一个生命链接状态
        LPCMapping statusData = allStatus[0];

        // 获取分摊者rid
        string linkRid = statusData.GetValue<string>("source_rid");

        // 通知分摊者受创
        Property linkOb = Rid.FindObjectByRid(linkRid);

        // 如果分摊源死亡，直接返回伤害
        if (linkOb.CheckStatus("DIED"))
            return null;

        // 获取源伤害
        int sourceDmg = damageMap.GetValue<LPCMapping>("points").GetValue<int>("hp");

        // 获取分摊百分比计算实际伤害
        int fixDmg = (1000 - statusData.GetValue<int>("value_rate")) * sourceDmg / 1000;

        // 重新包装伤害结构
        LPCMapping fixPoints = new LPCMapping();
        fixPoints.Add("hp", fixDmg);
        damageMap.Add("points", fixPoints);

        // 构建分摊者受创信息
        LPCMapping linkDmgMap = new LPCMapping();
        linkDmgMap.Append(damageMap);
        LPCMapping linkPoints = new LPCMapping();
        linkPoints.Add("hp", sourceDmg - fixDmg);
        linkDmgMap.Add("points", linkPoints);
        linkDmgMap.Add("damage_type", damgeType);
        linkDmgMap.Add("link_id", 1);
        linkDmgMap.Add("target_rid", linkOb.GetRid());

        (linkOb as Char).ReceiveDamage(sourceProfile, damgeType, linkDmgMap);

        return damageMap;
    }

    private static LPCMapping B_SACRIFICE_ALL(Property who, int damgeType, LPCMapping damageMap, LPCMapping sourceProfile, LPCMapping oriPoints)
    {
        List<LPCMapping> allStatus = who.GetStatusCondition("B_SACRIFICE_ALL");
        if (allStatus.Count == 0)
            return null;

        // 确保只有一个生命链接状态
        LPCMapping statusData = allStatus[0];

        LPCArray ridList = statusData.GetValue<LPCArray>("rid_list");

        // 获取源伤害
        int sourceDmg = damageMap.GetValue<LPCMapping>("points").GetValue<int>("hp");

        // 获取分摊百分比计算实际伤害
        int fixDmg = (1000 - statusData.GetValue<int>("value_rate")) * sourceDmg / 1000;

        // 重新包装伤害结构
        LPCMapping fixPoints = new LPCMapping();
        fixPoints.Add("hp", fixDmg);
        damageMap.Add("points", fixPoints);

        for (int i = 0; i < ridList.Count; i++)
        {
            // 通知分摊者受创，如果分摊源死亡，直接返回伤害
            Property linkOb = Rid.FindObjectByRid(ridList[i].AsString);
            if (linkOb.CheckStatus("DIED"))
                continue;

            // 构建分摊者受创信息
            LPCMapping linkDmgMap = new LPCMapping();
            LPCMapping linkPoints = new LPCMapping();
            linkPoints.Add("hp", (sourceDmg - fixDmg) / ridList.Count);
            linkDmgMap.Add("points", linkPoints);
            linkDmgMap.Add("damage_type", CombatConst.DAMAGE_TYPE_ATTACK);
            linkDmgMap.Add("link_id", 1);
            linkDmgMap.Add("target_rid", linkOb.GetRid());

            (linkOb as Char).ReceiveDamage(sourceProfile, CombatConst.DAMAGE_TYPE_ATTACK, linkDmgMap);
        }

        return damageMap;
    }

    private static LPCMapping B_SACRIFICE_ALL_SP(Property who, int damgeType, LPCMapping damageMap, LPCMapping sourceProfile, LPCMapping oriPoints)
    {
        List<LPCMapping> allStatus = who.GetStatusCondition("B_SACRIFICE_ALL_SP");
        if (allStatus.Count == 0)
            return null;

        // 确保只有一个生命链接状态
        LPCMapping statusData = allStatus[0];

        LPCArray ridList = statusData.GetValue<LPCArray>("rid_list");
        LPCArray realRidList = new LPCArray();
        for (int i = 0; i < ridList.Count; i++)
        {
            // 排除自身目标收集
            string checkRid = ridList[i].AsString;
            Property checkOb = Rid.FindObjectByRid(checkRid);
            if (!checkOb.CheckStatus("DIED") && !checkRid.Equals(who.GetRid()))
                realRidList.Add(ridList[i].AsString);
        }

        // 分摊列表数大于 1 才进行分摊
        if (realRidList.Count > 1)
        {
            // 获取源伤害
            int sourceDmg = damageMap.GetValue<LPCMapping>("points").GetValue<int>("hp");
            // 计算实际伤害
            int fixDmg = sourceDmg / realRidList.Count;
            // 重新包装伤害结构
            LPCMapping fixPoints = new LPCMapping();
            fixPoints.Add("hp", fixDmg);
            damageMap.Add("points", fixPoints);

            for (int i = 0; i < realRidList.Count; i++)
            {
                // 通知分摊者受创
                string linkRid = realRidList[i].AsString;
                Property linkOb = Rid.FindObjectByRid(linkRid);
                // 如果分摊者死亡则跳过
                if (linkOb.QueryTemp<int>("hp_rate") == 0)
                    continue;

                // 构建分摊者受创信息
                LPCMapping linkDmgMap = new LPCMapping();
                LPCMapping linkPoints = new LPCMapping();
                linkPoints.Add("hp", (sourceDmg - fixDmg) / (realRidList.Count - 1));
                linkDmgMap.Add("points", linkPoints);
                linkDmgMap.Add("damage_type", CombatConst.DAMAGE_TYPE_ATTACK);
                linkDmgMap.Add("link_id", 1);
                linkDmgMap.Add("target_rid", linkOb.GetRid());

                (linkOb as Char).ReceiveDamage(sourceProfile, CombatConst.DAMAGE_TYPE_ATTACK, linkDmgMap);
            }
        }

        return damageMap;
    }

    public static LPCMapping Call(Property who, int damgeType, LPCMapping damageMap, LPCMapping sourceProfile)
    {
        // 没有任何伤害数值
        // 检查伤害分摊过来的类型是否为分摊伤害，如果是分摊伤害则不再继续进行分摊
        if (damageMap.Count == 0 ||
            damageMap.ContainsKey("link_id"))
            return damageMap;

        // 如果是只是mp伤害
        LPCMapping oriPoints = damageMap.GetValue<LPCMapping>("points");
        if (! oriPoints.ContainsKey("hp"))
            return damageMap;

        // 1、优先检查状态是否存在光环分摊特殊生命链接
        LPCMapping linkDataMap = B_SACRIFICE_SP(who, damgeType, damageMap, sourceProfile, oriPoints);
        if (linkDataMap != null)
            return linkDataMap;

        // 2、检查状态是否存在全体生命链接
        linkDataMap = B_SACRIFICE_ALL(who, damgeType, damageMap, sourceProfile, oriPoints);
        if (linkDataMap != null)
            return linkDataMap;

        // 3、检查状态是隐藏版的全体生命链接
        linkDataMap = B_SACRIFICE_ALL_SP(who, damgeType, damageMap, sourceProfile, oriPoints);
        if (linkDataMap != null)
            return linkDataMap;

        // 4、检查状态是否存在生命链接
        linkDataMap = B_SACRIFICE(who, damgeType, damageMap, sourceProfile, oriPoints);
        if (linkDataMap != null)
            return linkDataMap;

        // 没有伤害分摊效果
        return damageMap;
    }
}

/// <summary>
/// 伤害转移
/// </summary>
public class TRANS_DAMAGE : Formula
{
    public static LPCMapping Call(Property who, int damgeType, LPCMapping damageMap, LPCMapping sourceProfile)
    {
        // 没有任何伤害数值
        if (damageMap.Count == 0)
            return damageMap;

        // 检查状态是否存在并执行伤害转移
        if (!who.CheckStatus("B_TEAM_INSTEAD"))
            return damageMap;

        // 如果是转移过的伤害不再继续转移
        if (damageMap.GetValue<int>("trans_id") > 0)
            return damageMap;

        // 记录原伤害信息
        LPCMapping originDmgMap = new LPCMapping();
        originDmgMap.Append(damageMap);

        // 判断是否致命伤
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        if (points.GetValue<int>("hp") < who.Query<int>("hp"))
            return damageMap;

        List<LPCMapping> allStatus = who.GetStatusCondition("B_TEAM_INSTEAD");
        foreach (LPCMapping statusData in allStatus)
        {
            // 获取分摊者rid
            string linkRid = statusData.GetValue<string>("source_rid");

            // 通知分摊者受创
            Property linkOb = Rid.FindObjectByRid(linkRid);

            // 检查技能CD，如果在CD中，检查下一个状态主
            if (CdMgr.SkillIsCooldown(linkOb, statusData.GetValue<int>("skill_id")))
                continue;

            // 如果分摊源死亡，检查下一个状态主
            if (linkOb.CheckStatus("DIED"))
                continue;

            // 如果分摊者血量低于设定百分比，检查下一个状态主。
            int linkObHp = Game.Divided(linkOb.Query<int>("hp"), linkOb.QueryAttrib("max_hp"));
            if (linkObHp < statusData.GetValue<int>("limit_hp_per"))
                continue;

            // 先把damage_type扔进damagemap里
            damageMap.Add("damage_type", damgeType | CombatConst.DAMAGE_TYPE_TRIGGER);
            damageMap.Add("target_rid", linkOb.GetRid());
            damageMap.Add("trans_id", 1);

            // 替身被动进行CD操作
            CdMgr.SkillCooldown(linkOb, statusData.GetValue<int>("skill_id"));

            BloodTipMgr.AddSkillTip(linkOb, statusData.GetValue<int>("skill_id"));
            (linkOb as Char).ReceiveDamage(sourceProfile, damgeType, damageMap);

            // 伤害转移以后，原伤害也要继续进行，只是伤害量为0
            LPCMapping oriPoints = new LPCMapping();
            oriPoints.Add("hp", 0);
            originDmgMap.Add("points", oriPoints);

            return originDmgMap;
        }

        return originDmgMap;
    }
}

/// <summary>
/// 免死伤害计算专用
/// </summary>
public class B_NO_DIE_DAMAGE : Formula
{
    public static LPCMapping Call(Property who, int damageType, LPCMapping damageMap, LPCMapping sourceProfile)
    {
        // 没有任何伤害数值
        if (damageMap.Count == 0)
            return damageMap;

        // 伤害为0直接返回
        LPCMapping oriPoints = damageMap.GetValue<LPCMapping>("points");
        if (oriPoints.GetValue<int>("hp") == 0)
            return damageMap;

        // 1.检查目标是处于免死状态
        if (who.CheckStatus("B_NO_DIE"))
        {
            LPCMapping points = damageMap.GetValue<LPCMapping>("points");
            int finalDmg = points.GetValue<int>("hp");
            int leftHp = who.Query<int>("hp");
            // 如果伤害量超过自身剩余血量，则修正最终伤害为自身剩余血量，修正为剩余血量 - 1，否则不修正
            if (finalDmg >= leftHp)
                finalDmg = leftHp - 1;

            LPCMapping finalPoints = new LPCMapping();
            finalPoints.Add("hp", finalDmg);
            damageMap.Add("points", finalPoints);
            damageMap.Add("damage_type", damageType | CombatConst.DAMAGE_TYPE_IMMUNITY);
            return damageMap;
        }

        return damageMap;
    }
}

/// <summary>
/// 计算能量伤害修正
/// </summary>
public class CALC_MP_DAMAGE_FIXED : Formula
{
    public static LPCMapping Call(Property who, int damgeType, LPCMapping damageMap, LPCMapping sourceProfile)
    {
        // 没有任何伤害数值
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        if (! points.ContainsKey("mp"))
            return damageMap;

        LPCMapping FixedDamageMap = new LPCMapping();
        LPCMapping FixedPoints = new LPCMapping();

        FixedDamageMap.Append(damageMap);
        FixedPoints.Append(points);

        // 能量伤害免疫效果
        if (who.QueryAttrib("mp_dmg_immu") == 1)
            FixedPoints.Add("mp", 0);
        FixedDamageMap.Add("points", FixedPoints);

        return FixedDamageMap;
    }
}

/// <summary>
/// 收到治愈效果处理脚本
/// </summary>
public class WHEN_CURED : Formula
{
    public static void Call(Property who, LPCMapping sourceProfile, int cureType, LPCMapping points)
    {
        // 策划维护该脚本
    }
}

/// <summary>
/// 技能冷却减少免疫检查
/// </summary>
public class IS_CD_REDUCE_IMMU_CHECK : Formula
{
    public static bool Call(int skillId)
    {
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);
        LPCArray cdArgs = skillInfo.Query<LPCArray>("cooldown_arg");

        // 如果有减少冷却免疫，则返回真，否则为假，会继续执行冷却减少操作
        if (cdArgs.Count < 2 || ! cdArgs[1].IsMapping)
            return false;

        // 如果有减少冷却免疫，则返回真，否则为假，会继续执行冷却减少操作
        if (cdArgs[1].AsMapping.GetValue<int>("all_immu") == 1 || cdArgs[1].AsMapping.GetValue<int>("reduce_immu") == 1)
            return true;

        return false;
    }
}

/// <summary>
/// 技能冷却增加免疫检查
/// </summary>
public class IS_CD_ADD_IMMU_CHECK : Formula
{
    public static bool Call(int skillId)
    {
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        LPCArray cdArgs = skillInfo.Query<LPCArray>("cooldown_arg");

        // 如果有减少冷却免疫，则返回真，否则为假，会继续执行冷却减少操作
        if (cdArgs.Count < 2 || ! cdArgs[1].IsMapping)
            return false;

        LPCMapping cdMap = cdArgs[1].AsMapping;

        if (cdMap.GetValue<int>("all_immu") != 1 && cdMap.GetValue<int>("add_immu") != 1)
            return false;

        return true;
    }
}

/// <summary>
/// 计算状态的效果命中几率
/// </summary>
public class CALC_EFFECT_ACCURACY_RATE : Formula
{
    public static int Call(int sourceAcc, int targetResist, int restrain)
    {
        // 如果克制，命中效果提升15%
        // 如果被克制，则命中效果降低15%
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            restrain = CombatConst.RESTRAIN_ACCURACY_VALUE;
        else if (restrain == ElementConst.ELEMENT_DISADVANTAGE)
            restrain = -CombatConst.RESTRAIN_ACCURACY_VALUE;

        int resist = targetResist - (sourceAcc + restrain);
        if (resist < 10)
            resist = 150;

        return resist;
    }
}

/// <summary>
/// 计算自动战斗抽取目标时的顺位权重
/// </summary>
public class CALC_AUTO_COMBAT_TARGET_POS_WEIGHT : Formula
{
    public static Property Call(List<Property> propertyList)
    {
        // 先对角色信息进行排序
        propertyList.Sort(Compare);

        // 获取权重列表
        List<int> weightList = new List<int>();

        // 站位第一顺位50%几率被选择，其他站位平分剩余50%几率被选择。
        int averageWeight = 0;
        int count = propertyList.Count;
        if (count > 1)
            averageWeight = 500 / (count - 1);

        // 计算后续剩余权重
        for (int i = 0; i < count; i++)
        {
            // 站位第一顺位50%几率被选择
            if (i == 0)
            {
                weightList.Add(500);
                continue;
            }

            // 其他站位平分剩余50%几率被选择
            weightList.Add(averageWeight);
        }

        // 根据权重抽取一个目标
        int index = RandomMgr.RandomSelect(weightList);

        // 根据权重抽取目标失败
        if (index == -1)
            return null;

        // 返回权重抽取目标
        return propertyList[index];
    }

    /// <summary>
    /// Compare the specified x and y.
    /// </summary>
    private static int Compare(Property left, Property right)
    {
        int xi = left.FormationPos;
        int yi = right.FormationPos;

        if (xi < yi)
            return -1;

        if (xi > yi)
            return 1;

        return 0;
    }
}

/// <summary>
/// 计算技能升级效果
/// </summary>
public class CALC_SKILL_UPGRADE_EFFECT : Formula
{
    public static int Call(int originalSkillId, int skillLevel, int effect)
    {
        return SkillMgr.GetTotalBaseMainValue(originalSkillId, skillLevel, effect);
    }
}

/// <summary>
/// 计算单条技能升级效果描述
/// </summary>
public class CALC_SKILL_UPGRADE_EFFECT_DESC : Formula
{
    public static string Call(int level, LPCMapping arg)
    {
        string SeffectString = string.Empty;
        string SfinalString = string.Empty;
        string SendString = "%";

        foreach (int Seffect in arg.GetValue<LPCMapping>(level).Keys)
        {
            SeffectString = "skill_lv_desc_" + Seffect.ToString();

            int SeffectValue = arg.GetValue<LPCMapping>(level).GetValue<int>(Seffect) / 10;

            if (Seffect == 2)
            {
                SendString = " " + LocalizationMgr.Get("skill_lv_desc_cd");
                SeffectValue = arg.GetValue<LPCMapping>(level).GetValue<int>(Seffect);
            }
            if (Seffect == 10)
            {
                SendString = " " + LocalizationMgr.Get("skill_lv_desc_mp");
                SeffectValue = arg.GetValue<LPCMapping>(level).GetValue<int>(Seffect);
            }

            SfinalString = level.ToString() + LocalizationMgr.Get("skill_lv_desc_lv") +
            LocalizationMgr.Get(SeffectString) + " " +
            SeffectValue.ToString() + SendString;
        }

        return SfinalString;
    }
}
