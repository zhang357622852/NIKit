/// <summary>
/// FORMULA_COMMON.cs
/// Create by fucj 2014-12-08
/// 通用公式
/// </summary>
using System;
using UnityEngine;
using System.Collections.Generic;
using LPC;
using System.Collections;
using System.Linq;

/// <summary>
/// 能否增加包裹格子数量
/// </summary>
public class CAN_ADD_BAGGAGE_AMOUNT : Formula
{
    public static bool Call(Property who, int page, int amount)
    {
        return true;
    }
}

/// <summary>
/// 取得包裹满了的提示颜色
/// </summary>
public class GET_BAGGAGE_FULL_COLOR : Formula
{
    public static string Call()
    {
        return "ff585f";
    }
}

/// <summary>
/// 能否购买商店物品
/// </summary>
public class CAN_BUY_SHOP_ITEM : Formula
{
    public static bool Call(Property who, Property item)
    {
        return true;
    }
}

/// <summary>
/// 计算刷新商店物品消耗
/// </summary>
public class CALC_REFRESH_GOODS_COST : Formula
{
    public static LPCMapping Call(Property who)
    {
        LPCMapping costMap = new LPCMapping();

        // 市集刷新卷
        LPCValue refresh_card = ME.user.Query<LPCValue>("shop_refresh_card");
        if(refresh_card != null && refresh_card.IsInt && refresh_card.AsInt > 0)
            costMap.Add("shop_refresh_card", 1);
        else
            costMap.Add("gold_coin", 3);

        return costMap;
    }
}

/// <summary>
/// 能否刷新商店物品
/// </summary>
public class CAN_REFRESH_SHOP_GOODS : Formula
{
    public static bool Call(Property who)
    {
        return true;
    }
}

/// <summary>
/// 获取UILabel转义字符
/// </summary>
public class GET_EMOTION_ALIAS : Formula
{
    private static Dictionary<int, string> ColorAlias = new Dictionary<int, string>()
    {
        { ColorConfig.NC_RED,    "[ff0000]" },
        { ColorConfig.NC_BLUE,   "[00ffff]" },
        { ColorConfig.NC_YELLOW, "[ffff00]" },
        { ColorConfig.NC_PURPLE, "[ff00ff]" },
        { ColorConfig.NC_WHITE,  "[ffffff]" },
        { ColorConfig.NC_GREEN,  "[00ff00]" }
    };

    public static string Call(string desc, int color)
    {
        if (string.IsNullOrEmpty(desc))
        {
            LogMgr.Trace("传入的飘血描述为空");
            return string.Empty;
        }

        if (!ColorAlias.ContainsKey(color))
        {
            LogMgr.Trace("传入的颜色值不正确，color = {0}", color);
            return string.Empty;
        }

        string colorAlias = ColorAlias[color];
        string emotionAlias = string.Empty;

        if (desc.Equals("miss"))
        {
            emotionAlias = colorAlias + desc;
            return emotionAlias;
        }

        char[] nums = desc.ToCharArray();
        for (int i = 0; i < nums.Length; ++i)
            emotionAlias += (colorAlias + nums[i]);

        return emotionAlias;
    }
}

/// <summary>
/// 获取宠物等级转义字符
/// </summary>
public class GET_LEVEL_ALIAS : Formula
{
    public static string Call(string desc)
    {
        if (string.IsNullOrEmpty(desc))
        {
            LogMgr.Trace("传入的等级为空");
            return string.Empty;
        }

        string levelAlias = string.Empty;

        char[] nums = desc.ToCharArray();
        for (int i = 0; i < nums.Length; ++i)
            levelAlias += ("@lv" + nums[i]);

        return levelAlias;
    }
}


/// <summary>
/// 获得飘血文字的大小
/// </summary>
public class GET_BLOOD_TIPS_FONT_SIZE : Formula
{
    public static int Call(int type)
    {
        // 默认40号字
        int fontSize = 32;

        if ((type & CombatConst.DAMAGE_TYPE_DEADLY) == CombatConst.DAMAGE_TYPE_DEADLY)
            // 暴击，加5号
            fontSize += 5;

        return fontSize;
    }
}



/// <summary>
/// 掉落物品物品自动拾取时间
/// </summary>
public class CALC_DROP_ITEM_AUTO_PICK_TIME : Formula
{
    public static float Call()
    {
        return 4.0f;
    }
}

/// <summary>
/// 随机一个掉落物品的延迟掉落时间
/// </summary>
public class CALC_DROP_ITEM_DROP_DELAY_TIME : Formula
{
    public static float Call()
    {
        return UnityEngine.Random.Range(0f, 0.5f);
    }
}

/// <summary>
/// 随机一个掉落物品的掉落点
/// </summary>
public class CALC_DROP_ITEM_DROP_VELOCITY : Formula
{
    public static Vector2 Call()
    {
        float xOffset = UnityEngine.Random.Range(-0.4f, 0.4f);
        float yOffset = UnityEngine.Random.Range(-0.2f, 0.2f);

        return new Vector2(xOffset, yOffset);
    }
}

/// <summary>
/// 获取掉落物品的光效编号
/// </summary>
public class GET_DROP_ITEM_ICON : Formula
{
    public static string Call(string name)
    {
        switch (name)
        {
            case "money": // 金钱
                return "Drop_money";
            case "diamond": // 钻石
                return "Drop_diamond";
            case "life": // 体力
                return "Drop_life";
            case "arena": // 竞技场劵
                return "Drop_arena";
            default:
                return ""; // 默认
        }
    }
}

/// <summary>
/// 掉落物品拾取音效
/// </summary>
public class GET_DROP_EFFECT_SOUND : Formula
{
    public static string Call(int classId)
    {
        switch (classId)
        {
            case 51000: // 低级血球
            case 51001: // 中级血球
            case 51002: // 高级血球
                return "mp";
            case 51003: // 蓝球
            case 50015: // 经验球
                return "mp";
            case 50000: // 金钱
                return "money";
            default:
                return "sq"; // 默认
        }
    }
}

/// <summary>
/// 计算掉落物品的分布数量
/// classId：物品id
/// count：数量
/// entityInfo：掉落者信息
/// </summary>
public class CALC_DROP_ITEM_SPLITE : Formula
{
    public static int[] Call(string name, int count, LPCMapping entityInfo)
    {
        // 区分物品类型，使用不同的掉落策略
        // 一个元素代表一堆
        // 元素的值代表数量值
        // 例如：金钱总数403，返回值应该为{81,81,81,80,80}
        // 则表示分为5堆:81 + 81 + 81 + 80 +80
        // 其余均为一个一堆

        if (count <= 0)
            return null;

        switch (name)
        {
            case "money": // 金钱(目前暂时的规则是一个金钱最多为99)
                {
                    int num = (count % 99 == 0) ? count / 99 : (count / 99 + 1);

                    int[] ret = new int[num];

                    for (int i = 0; i < num; i++)
                    {
                        ret[i] = (int)(count / num);
                    }

                    int remain = count - ((int)(count / num)) * (num);

                    for (int i = 0; i < remain; i++)
                    {
                        ret[i] += 1;
                    }

                    return ret;
                }
            case "diamond": // 钻石
                {
                    if (count > 50)
                    {
                        LogMgr.Trace("掉落物品数量个数过多{0}", count);
                        return new int[1]{ count };
                    }

                    // 默认情况是1个物品1堆
                    int[] ret = new int[count];
                    for (int i = 0; i < count; i++)
                        ret[i] = 1;
                    return ret;
                }

            case "life": // 体力
                {
                    if (count > 50)
                    {
                        LogMgr.Trace("掉落物品数量个数过多{0}", count);
                        return new int[1]{ count };
                    }

                    // 默认情况是1个物品1堆
                    int[] ret = new int[count];
                    for (int i = 0; i < count; i++)
                        ret[i] = 1;
                    return ret;
                }
            case "arena": // 晋级场劵
                {
                    if (count > 50)
                    {
                        LogMgr.Trace("掉落物品数量个数过多{0}", count);
                        return new int[1]{ count };
                    }

                    // 默认情况是1个物品1堆
                    int[] ret = new int[count];
                    for (int i = 0; i < count; i++)
                        ret[i] = 1;
                    return ret;
                }
            default:
                {
                    // 默认直接是一个
                    return new int[1]{ count };
                }
        }
    }
}

/// <summary>
/// 决定最终选用哪个BOOL类附加属性
/// </summary>
public class FETCH_BOOL_PROP_MERGE_RESULT : Formula
{
    public static int Call(Property target, string prop_name, LPCArray value_list)
    {
        int count = value_list.Count;
        return value_list[count - 1].AsInt;
    }
}

/// <summary>
/// 检测是否显示受创飘血框
/// </summary>
public class CHECK_SHOW_DAMAGE_TIPS : Formula
{
    public static bool Call(Property ob, int damageType)
    {
        // 同步伤害不需要显示
        if ((damageType & CombatConst.DAMAGE_TYPE_SYNC) == CombatConst.DAMAGE_TYPE_SYNC)
            return false;

        // 玩家自身的召唤怪物
        string employerRid = ob.Query<string>("employer_rid", true);
        if (!string.IsNullOrEmpty(employerRid) && employerRid.Equals(ME.GetRid()))
            return false;

        // 策划维护该脚本公式
        return true;
    }
}

/// <summary>
/// 检测是否显示治愈飘血框
/// </summary>
public class CHECK_SHOW_CURE_TIPS : Formula
{
    public static bool Call(Property ob, int cureType)
    {
        // 同步治愈不需要显示
        if ((cureType & CombatConst.CURE_TYPE_SYNC) == CombatConst.CURE_TYPE_SYNC)
            return false;

        // 玩家自身的召唤怪物
        string employerRid = ob.Query<string>("employer_rid", true);
        if (!string.IsNullOrEmpty(employerRid) && employerRid.Equals(ME.GetRid()))
            return false;

        // 策划维护该脚本公式
        return true;
    }
}

// 装备是否可以被出售
public class CAN_SELL_EQUIP : Formula
{
    public static bool Call(Property who, Property equip)
    {
        if (equip == null)
            return false;

        // 获取装备镶嵌信息
        LPCArray holeInfo = equip.Query<LPCArray>("hole_icon");
        if (equip.Query<int>("reset_count") != 0
            || (holeInfo != null && holeInfo.Count > 0))
            return false;

        return true;
    }
}

// 获取材料来源描述
public class GET_MATERIAL_SOURCE_DESC : Formula
{
    public static string Call(int classId)
    {
        switch (classId)
        {
            case 90000:
                return "可通过分解暗金获得";
            case 90001:
                return "可通过分解套装获得";
            default:
                return string.Empty;
        }
    }
}

/// <summary>
/// 获取皮肤
/// </summary>
public class CALC_ACTOR_SKIN : Formula
{
    /// <summary>
    /// Call the specified ob.
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static string Call(Property ob)
    {
        return Call(ob.Query<int>("rank"));
    }

    /// <summary>
    /// Call the specified ob.
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static string Call(int rank)
    {
        return (rank <= 1) ? "normal" : "awaken";
    }
}

/// <summary>
/// 回合行动开始
/// </summary>
public class ROUND_ACTION_STAR : Formula
{
    private static List<string> statusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };

    public static bool Call(Property ob, int type, string cookie)
    {
        // 非正常回合
        if (type == RoundCombatConst.ROUND_TYPE_NONE)
            return true;

        switch (type)
        {
            // 普通回合，追加回合，插队回合处理方式相同
            case RoundCombatConst.ROUND_TYPE_NORMAL:
            case RoundCombatConst.ROUND_TYPE_ADDITIONAL:
            case RoundCombatConst.ROUND_TYPE_GASSER:
                if (type == RoundCombatConst.ROUND_TYPE_ADDITIONAL)
                {
                    string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.ResTip], LocalizationMgr.Get("tip_add_round"));
                    BloodTipMgr.AddTip(TipsWndType.DamageTip, ob, tips);
                }
                // 正常回合
                // 回蓝如果有缺蓝以及不在无法恢复状态
                if (!ob.CheckStatus("D_NO_MP_CURE") && ob.QueryAttrib("skl_no_mp_cure") == 0)
                {
                    if (ob.Query<int>("mp") <= ob.QueryAttrib("max_mp"))
                    {
                        LPCMapping sourceProfile = ob.GetProfile();
                        sourceProfile.Add("cookie", cookie);
                        int mp = Math.Min(ob.QueryAttrib("max_mp") - ob.Query<int>("mp"), 1);
                        LPCMapping cureMap = new LPCMapping();
                        cureMap.Add("mp", mp);
                        (ob as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC | CombatConst.CURE_TYPE_ROUND_MAGIC, cureMap);
                    }
                }

                // 执行回合开始时起作用的状态
                StatusMgr.DoRoundApplyStatus(ob, StatusConst.ROUND_TYPE_START, string.Empty);

                // 太阳图腾治疗
                if (ob.CheckStatus("B_HP_RECOVER_SP") && !ob.CheckStatus(statusList))
                {
                    List<LPCMapping> allstatus = ob.GetStatusCondition("B_HP_RECOVER_SP");
                    int hpCure = allstatus[0].GetValue<int>("hp_cure");
                    LPCMapping sourceProfile = allstatus[0].GetValue<LPCMapping>("source_profile");
                    LPCMapping cureMap = new LPCMapping();
                    // 计算额外影响
                    hpCure = CALC_EXTRE_CURE.Call(hpCure, ob.QueryAttrib("skill_effect"), ob.QueryAttrib("reduce_cure"));
                    cureMap.Add("hp", hpCure);
                    // 技能提示
                    BloodTipMgr.AddSkillTip(ob, allstatus[0].GetValue<int>("skill_id"));
                    sourceProfile.Add("cookie", cookie);
                    // 执行回血操作
                    (ob as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC, cureMap);
                }

                break;

            case RoundCombatConst.ROUND_TYPE_JOINT:
                // 协同回合（该类型回合不能有导致角色回合延迟因素）

                break;

            case RoundCombatConst.ROUND_TYPE_COUNTER:
                // 反击回合（该类型回合不能有导致角色回合延迟因素）

                break;

            case RoundCombatConst.ROUND_TYPE_RAMPAGE:
                // 暴走回合

                break;

            case RoundCombatConst.ROUND_TYPE_COMBO:
                // 连击回合

                break;

            default:
                break;
        }

        return true;
    }
}

/// <summary>
/// 回合行动结束
/// </summary>
public class ROUND_ACTION_END : Formula
{
    public static bool Call(Property ob, int type, string cookie, LPCMapping para, bool doAction)
    {
        switch (type)
        {
            // 普通回合，追加回合，插队回合处理方式相同
            case RoundCombatConst.ROUND_TYPE_NORMAL:
            case RoundCombatConst.ROUND_TYPE_ADDITIONAL:
            case RoundCombatConst.ROUND_TYPE_GASSER:
                // 正常回合，包含特殊属性时，对没有出手时的处理，
                if (type == RoundCombatConst.ROUND_TYPE_NORMAL &&
                    ob.QueryAttrib("no_round_mp_down") > 0 &&
                    doAction == false)
                {
                    LPCMapping mpData = new LPCMapping();
                    mpData.Add("mp",Math.Min(ob.Query<int>("mp"), ob.QueryAttrib("no_round_mp_down")));
                    ob.CostAttrib(mpData);
                }
                // 清除安魂弥撒状态
                if (ob.CheckStatus("B_NEXT_ROUND_ATK"))
                {
                    // 判断是否同回合内
                    string statusCookie = ob.GetStatusCondition("B_NEXT_ROUND_ATK")[0].GetValue<string>("round_cookie");
                    if (statusCookie != cookie)
                        ob.ClearStatus("B_NEXT_ROUND_ATK");
                }

                // 清除技能cd(没有禁止恢复冷却的前提下才清CD回合)
                if (!ob.CheckStatus("D_NO_CD_CURE"))
                    CdMgr.ReduceSkillCd(ob);
                else
                {
                    string tips = string.Format("{0}{1}", BloodTipMgr.mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("tip_no_cd_cure"));
                    BloodTipMgr.AddTip(TipsWndType.DamageTip, ob, tips);
                }

                // 清除状态cd
                StatusMgr.DoRoundApplyStatus(ob, StatusConst.ROUND_TYPE_END, cookie);

                // 触发属性cd
                TriggerPropMgr.DoReduceCd(ob);

                // 极冷之壁专用
                LPCArray freezeList = para.GetValue<LPCArray>("freeze_list");
                if (freezeList != null && freezeList.Count == 0)
                {
                    // 执行扣血处理
                    int maxHp = ob.QueryAttrib("max_hp");
                    int selfCostHp = Game.Multiple(maxHp, ob.QueryAttrib("self_cost_hp"), 1000);
                    int curHp = ob.Query<int>("hp");
                    // 误差检查
                    if (selfCostHp * 10 < maxHp)
                        selfCostHp += 1;
                    ob.Set("hp", LPCValue.Create(Math.Max(curHp - selfCostHp, 0)));

                    // 判断是否需要执行死亡操作
                    if (selfCostHp >= curHp)
                    {
                        ob.Set("hp", LPCValue.Create(0));
                        TRY_DO_DIE.Call(ob);
                    }
                }

                break;

            case RoundCombatConst.ROUND_TYPE_JOINT:
                // 协同回合

                // 触发属性cd
                TriggerPropMgr.DoReduceCd(ob);

                break;

            case RoundCombatConst.ROUND_TYPE_COUNTER:
                // 反击回合

                // 触发属性cd
                TriggerPropMgr.DoReduceCd(ob);

                break;

            case RoundCombatConst.ROUND_TYPE_RAMPAGE:
                // 暴走回合

                // 触发属性cd
                TriggerPropMgr.DoReduceCd(ob);

                break;

            case RoundCombatConst.ROUND_TYPE_COMBO:
                // 连击回合

                break;

            default:
                break;
        }

        // 回合结束判断是否需要添加陷阱冻结状态
        LPCMapping trap = ob.QueryTemp<LPCMapping>("trap");
        if (trap != null)
        {
            // 删除陷阱数据
            ob.DeleteTemp("trap");

            // 初次检查成功率
            if (RandomMgr.GetRandom() < trap.GetValue<int>("debuff_rate"))
            {
                // 计算效果命中
                // 攻击者效果命中、防御者效果抵抗、克制关系
                int rate = CALC_EFFECT_ACCURACY_RATE.Call(trap.GetValue<int>("trap_source_acc"),
                               trap.GetValue<int>("target_resist_rate"),
                               trap.GetValue<int>("restrain"));

                // 概率检测不通过
                if (RandomMgr.GetRandom() >= rate)
                {
                    LPCMapping condition = new LPCMapping();
                    condition.Add("round", trap.GetValue<int>("trap_effect_round"));
                    condition.Add("source_profile", trap.GetValue<LPCMapping>("trap_source_profile"));
                    condition.Add("round_cookie", cookie);

                    // 附加状态
                    ob.ApplyStatus(trap.GetValue<string>("trap_effect"), condition);
                }
                else
                {
                    string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("tip_resist"));
                    BloodTipMgr.AddTip(TipsWndType.DamageTip, ob, tips);
                }
            }
        }

        // 暂时在这里统一判断出手后清除愤怒的仙人掌状态，以后情况复杂了再做处理
        if (ob.CheckStatus("B_DMG_UP_HALO"))
        {
            ob.ClearStatus("B_DMG_UP_HALO");
            ob.SetTemp("add_dmg_up_value", LPCValue.Create(0));
        }

        return true;
    }
}

/// <summary>
/// 复活后宠物的相关操作
/// </summary>
public class REVIVE_AFTER_PET_OPERATION:Formula
{
    public static void Call(Property ob)
    {
        // 满血复活
        ob.Set("hp", LPCValue.Create(ob.QueryAttrib("max_hp")));
    }
}

/// <summary>
/// 检测技能能否释放（选中）
/// </summary>
public class CHEK_SKILL_CAN_CAST:Formula
{
    public static bool Call(Property ob, int skillId)
    {
        // 检查是否有沉默状态
        if (ob.CheckStatus("D_SILENCE"))
        {
            CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);
            // 获取技能位置类型
            int skillPosType = skillInfo.Query<int>("skill_pos_type");
            if (skillPosType == SkillType.SKILL_TYPE_1)
                return true;
            else
                return false;
        }

        return true;
    }
}

/// <summary>
/// 副本过图处理
/// </summary>
public class INSTANCE_CROSS_MAP
{
    private static List<string> hpBanStatusList = new List<string>(){ "D_NO_CURE", "B_CAN_NOT_CHOOSE", "B_FURY" };
    private static List<string> mpBanStatusList = new List<string>(){ "D_NO_MP_CURE", "B_CAN_NOT_CHOOSE", };

    public static bool Call(Property ob)
    {
        // 1. 清除相关状态
        StatusMgr.CrossMapClearStatus(ob);

        // 2. 清除最大属性消减效果
        ob.Delete("attrib_addition");

        // 3. 过图回血、回能量
        int cureRate = CombatConst.DEFAULT_CROSS_MAP_CURE;
        LPCMapping sourceProfile = new LPCMapping();

        // 如果是验证客户端千万不能增加技能信息
        // 否则会导致mp恢复不上的问题
        // 具体原因是由于是验证客户端没有过图相关表现是直接瞬间过图将会导致播放的回血action被打断
        // 从而导致mp恢复失败
        if (! AuthClientMgr.IsAuthClient)
            sourceProfile.Add("skill_id", 11000);

        LPCMapping cureMap = new LPCMapping();
        if (!ob.CheckStatus(hpBanStatusList))
            cureMap.Add("hp", Game.Multiple(ob.QueryAttrib("max_hp"), cureRate));
        if (!ob.CheckStatus(mpBanStatusList))
            cureMap.Add("mp", Math.Min(ob.QueryAttrib("max_mp") - ob.Query<int>("mp"), 1));

        // 执行回复操作
        if (cureMap.Count != 0)
        {
            sourceProfile.Add("cookie", Game.NewCookie(ob.GetRid()));
            (ob as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_ROUND_MAGIC, cureMap);
        }

        // 4、过图减CD
        LPCArray skills = ob.GetAllSkills();
        foreach (LPCValue mks in skills.Values)
        {
            // 判断技能id是否一致
            int skillId = mks.AsArray[0].AsInt;

            // 减技能cd
            CdMgr.DoReduceSkillCd(ob, skillId, 1);
        }

        return true;
    }
}

/// <summary>
/// 宠物能否强化
/// </summary>
public class PET_CAN_STRENGTHEN
{
    public static string Call(Property ob)
    {
        //TODO 宠物强化界面，判断该宠物能否强化（包括升级和升星）
        // 比如某些1星天使怪不能强化，比如6星满级不能强化等。
        // 如果不能强化请返回原因，需要弹框显示，能强化直接返回""

        return "";
    }
}

/// <summary>
/// 宠物强化材料宠物状态图标显示
/// </summary>
public class PET_STRENGTHEN_MATERIAL_STATE_ICON
{
    public static string Call(Property ob)
    {
        //TODO 目前主要有锁定宠物，主要宠物（共享宠物），
        //   EXP（放入经验池），塔防宠物等等几种。
        //   图标分别对应"lock","lock_share","lock_exp","lock_defense"
        //  如果几种状态共存，应该显示哪个？没有直接返回""

        return string.Empty;
    }
}

/// <summary>
/// 包裹界面宠物需要显示的状态图标
/// </summary>
public class PET_BAGGAGE_STATE_ICON
{
    public static string Call(Property ob)
    {
        if (ME.user == null)
            return string.Empty;

        // 玩家的共享宠物    share_pet:({})
        LPCValue sharePet = ME.user.Query<LPCValue>("share_pet");

        if (sharePet != null && sharePet.IsString &&
            sharePet.AsString.Equals(ob.GetRid()))
            return "lock_share";

        // 获取宠物的锁定信息
        LPCValue isLock = ob.Query<LPCValue>("is_lock");

        if (isLock != null && isLock.IsInt
            && isLock.AsInt != 0)
            return "lock";

        return string.Empty;
    }
}

/// <summary>
/// 宠物不能作为强化材料宠物状态描述
/// </summary>
public class PET_STRENGTHEN_MATERIAL_STATE_DESC
{
    public static string Call(Property ob)
    {
        //TODO 目前主要有锁定宠物，主要宠物（共享宠物），
        //   EXP（放入经验池），塔防宠物等等几种。
        //   给玩家的提示，如"当前宠物为主要宠物，不能当成材料"
        //  如果几种状态共存，应该提示哪个,没有，直接返回"".

        if (ME.user == null)
            return string.Empty;

        // 共享宠物，不能作为材料
        if (PetMgr.IsSharePet(ob.GetRid()))
            return LocalizationMgr.Get("PetStrengthenWnd_31");

        // 宠物被锁定，不能作为材料
        if (PetMgr.IsLockPet(ob))
            return LocalizationMgr.Get("PetStrengthenWnd_32");

        // 防守宠物,不鞥作为材料
        if (PetMgr.IsDefenceTroop(ME.user, ob))
            return LocalizationMgr.Get("PetStrengthenWnd_33");

        // 指引宠物，不能作为
        if (PetMgr.IsGuidePet(ME.user, ob.GetRid()))
            return LocalizationMgr.Get("PetStrengthenWnd_32");

        return string.Empty;
    }
}

/// <summary>
/// 获取宠物升级时的消耗
/// </summary>
public class CALC_PET_UPGRADE_COST
{
    public static LPCMapping Call(Property pet_ob, List<Property> material_list)
    {
        LPCMapping cost_map = new LPCMapping();

        cost_map.Add("money", material_list.Count * 100);

        return cost_map;
    }
}

/// <summary>
/// 获取使用道具宠物升级时的消耗金币
/// </summary>
public class CALC_PET_UPGRADE_ITEM_COST
{
    public static LPCMapping Call(LPCArray materialArray)
    {
        LPCMapping mapping;
        int sumMoney = 0;
        for (int i = 0; i < materialArray.Count; i++)
        {
            mapping = materialArray[i].AsMapping;

            CsvRow csv = ItemMgr.GetRow(mapping["class_id"].AsInt);

            if (csv == null)
                continue;

            sumMoney += (int)ScriptMgr.Call(csv.Query<int>("exp_cost_script"), mapping["amount"].AsInt, csv.Query<int>("exp_cost_arg"));
        }


        LPCMapping costMap = LPCMapping.Empty;
        costMap.Add("money", sumMoney);
        return costMap;
    }
}

/// <summary>
/// 检测能否升级（点击升级按钮时的检测）
/// </summary>
public class CHECK_PET_CAN_UPGRADE
{
    public static string Call(Property pet_ob, List<Property> material_list)
    {
        // 如不能升级，请给出原因,如能升级，直接返回""
        return string.Empty;
    }
}

/// <summary>
/// 竞技场排位战刷新对战列表的消耗
/// </summary>
public class CALC_ARENA_RETAIN_BUFF_REFRESH_COST
{
    public static LPCMapping CALL(int refreshAmount)
    {
        // TODO:refreshAmount 刷新的次数
        LPCMapping bonus = new LPCMapping();
        bonus.Add("gold_coin", 10);
        return bonus;
    }
}

public class CALC_EQUIP_INTENSIFY_TIPS
{
    public static string CALL(int rank, int rarity, int maxIntensifyLv)
    {
        // rank: 当前的强化等级
        if (rank > 0 && (rank + 1) % 3 == 0 && (rank + 1) != maxIntensifyLv)
        {
            if (rarity == 1 && rank < 3)
                return string.Format(LocalizationMgr.Get("EquipStrengthenWnd_13"), rank + 1);
            if (rarity == 2 && rank < 6)
                return string.Format(LocalizationMgr.Get("EquipStrengthenWnd_13"), rank + 1);
            if (rarity == 3 && rank < 9)
                return string.Format(LocalizationMgr.Get("EquipStrengthenWnd_13"), rank + 1);
            if (rarity == 4 && rank < 12)
                return string.Format(LocalizationMgr.Get("EquipStrengthenWnd_13"), rank + 1);

            return string.Format(LocalizationMgr.Get("EquipStrengthenWnd_10"), rank + 1);
        }

        if ((rank + 1) == maxIntensifyLv)
            return string.Format(LocalizationMgr.Get("EquipStrengthenWnd_14"), rank + 1);

        return string.Empty;
    }
}

/// <summary>
/// 取得技能总的描述 (基础描述和等级描述)
/// </summary>
public class GET_SKILL_SUM_DESC
{
    public static string CALL(int skillId, int level)
    {
        if (skillId <= 0)
            return string.Empty;

        string cdDesc = string.Empty;

        // 取得技能CD
        int cd = CdMgr.GetBaseSkillCd(skillId, level);

        if (cd > 1)
            cdDesc = "[FDE3C8FF](" + string.Format(LocalizationMgr.Get("SkillLevelUpWnd_1"), cd) + ")\n";

        string baseDesc = SkillMgr.GetBaseEffectDesc(skillId, level);

        string levDesc = SkillMgr.GetSkillLevelDesc(skillId, level);

        return string.Format("{0}\n{1}\n{2}", baseDesc, cdDesc, levDesc);
    }
}

/// <summary>
/// 检测是否包含技能升级材料
/// </summary>
public class CHECK_INCLUDE_SKILL_LEVELUP_MATERIAL
{
    public static bool CALL(Property pet_ob, Property material_list)
    {
        return false;
    }
}

/// <summary>
/// 计算装备卸下的消耗
/// </summary>
public class CALC_UNLOAD_EQUIP_COST
{
    public static LPCMapping CALL(Property equipOb, Property user)
    {
        LPCMapping cost = new LPCMapping();

        // 开启了免费卸装活动
        if (ActivityMgr.IsAcitvityValid(ActivityConst.FREE_UNEQUIP))
        {
            cost.Add("money", 0);
            return cost;
        }

        // 装备卸载卷数据
        LPCValue unequp_data = user.Query<LPCValue>("free_unequip_data");
        if (unequp_data != null &&
            unequp_data.IsMapping &&
            unequp_data.AsMapping.GetValue<int>("end_time") > TimeMgr.GetServerTime())
        {
            cost.Add("money", 0);
            return cost;
        }

        int star = equipOb.GetStar();
        int money = 0;

        switch (star)
        {
            case 1:
                money = 10;
                break;
            case 2:
                money = 100;
                break;
            case 3:
                money = 300;
                break;
            case 4:
                money = 10000;
                break;
            case 5:
                money = 25000;
                break;
            case 6:
                money = 50000;
                break;
        }

        cost.Add("money", money);
        return cost;
    }
}

/// <summary>
/// 计算竞技场npc是否解锁
/// </summary>
public class CACL_ARENA_NPC_IS_UNLOCKED
{
    public static bool CALL(int userLevel, int npcInitLevel)
    {
        return true;
    }
}

/// <summary>
/// 计算购买包裹格子消耗
/// </summary>
public class CACL_UPGRADE_BAGGAGE_COST
{
    public static LPCMapping CALL(Property user)
    {
        List<int> moneyCost = new List<int>()
            { 2000, 4000, 10000, 15000, 20000, 25000, 30000, 45000, 60000, 75000, 90000, 105000, 115000, 140000, 165000, 190000, 210000 };

        List<int> goldCoinCost = new List<int>()
            { 10, 10, 30, 30, 30, 30, 30, 60, 60, 60, 100, 100, 100, 100, 100, 100, 100 };

        // 已购买次数，第一次购买值为0
        int times = 0;

        LPCValue upgrade_baggage = user.Query<LPCValue>("upgrade_baggage");

        // 获取宠物格子升级次数
        if (upgrade_baggage != null && upgrade_baggage.IsMapping)
            times = upgrade_baggage.AsMapping.GetValue<int>(ContainerConfig.POS_PET_GROUP);

        if (times >= moneyCost.Count || times >= goldCoinCost.Count)
            return new LPCMapping();


        LPCMapping cost = new LPCMapping();
        cost.Add("money", moneyCost[times]);
        cost.Add("gold_coin", goldCoinCost[times]);

        return cost;
    }
}

/// <summary>
/// 计算购买仓库格子消耗
/// </summary>
public class CACL_UPGRADE_STORE_COST
{
    public static LPCMapping CALL(Property user)
    {
        LPCValue upgrade_baggage = user.Query<LPCValue>("upgrade_baggage");

        List<int> moneyCost = new List<int>()
            { 10000, 60000, 120000 };

        List<int> goldCoinCost = new List<int>()
            { 10, 20, 30 };

        int times = 0;

        // 获取宠物格子升级次数
        if (upgrade_baggage != null && upgrade_baggage.IsMapping)
            times = upgrade_baggage.AsMapping.GetValue<int>(ContainerConfig.POS_STORE_GROUP);

        // times是指已购买的次数，没有购买过times为0
        LPCMapping cost = new LPCMapping();
        if (times > 2)
        {
            cost.Add("money", moneyCost[2]);
            cost.Add("gold_coin", goldCoinCost[2]);
        }
        else
        {
            cost.Add("money", moneyCost[times]);
            cost.Add("gold_coin", goldCoinCost[times]);
        }

        return cost;
    }
}

/// <summary>
/// 宠物存入仓库
/// </summary>
public class STORE_PET_SHOW_ICON
{
    public static string Call(Property ob)
    {
        //TODO 目前主要有锁定宠物，主要宠物（共享宠物），
        //   EXP（放入经验池），塔防宠物等等几种。
        //   图标分别对应"lock","lock_share","lock_exp","lock_defense"
        //  如果几种状态共存，应该显示哪个？没有直接返回""

        if (ME.user == null)
            return string.Empty;

        // 玩家的共享宠物    share_pet:({})
        LPCValue sharePet = ME.user.Query<LPCValue>("share_pet");

        if (sharePet != null && sharePet.IsString &&
            sharePet.AsString.Equals(ob.GetRid()))
            return "lock_share";

        // 获取防守宠物
        LPCValue defensePet = ME.user.Query<LPCValue>("defense_troop");

        if (defensePet != null && defensePet.IsArray
            && defensePet.AsArray.IndexOf(ob.GetRid()) != -1)
            return "lock_defense";

        // 获取宠物的锁定信息
        LPCValue isLock = ob.Query<LPCValue>("is_lock");

        if (isLock != null && isLock.IsInt
            && isLock.AsInt != 0)
            return "lock";

        return string.Empty;
    }
}

/// <summary>
/// 宠物存入仓库
/// </summary>
public class TAKE_PET_SHOW_ICON
{
    public static string Call(Property ob)
    {
        if (ME.user == null)
            return string.Empty;

        // 获取宠物的锁定信息
        LPCValue isLock = ob.Query<LPCValue>("is_lock");

        if (isLock != null && isLock.IsInt
            && isLock.AsInt != 0)
            return "lock";

        return string.Empty;
    }
}

/// <summary>
/// 仓库中是否显示宠物图标遮盖
/// </summary>
public class STORE_PET_SHOW_ICON_COVER
{
    public static bool Call(Property ob)
    {
        if (ME.user == null)
            return false;

        // 玩家的共享宠物    share_pet:({})
        LPCValue sharePet = ME.user.Query<LPCValue>("share_pet");

        if (sharePet != null && sharePet.IsString &&
            sharePet.AsString.Equals(ob.GetRid()))
            return true;

        // 获取防守宠物
        LPCValue defensePet = ME.user.Query<LPCValue>("defense_troop");

        if (defensePet != null && defensePet.IsArray
            && defensePet.AsArray.IndexOf(ob.GetRid()) != -1)
            return true;

        return false;
    }
}

/// <summary>
/// 宠物不能存入仓库状态描述
/// </summary>
public class CAN_STORE_PET_DESC
{
    public static string Call(Property ob)
    {
        //TODO 目前主要有锁定宠物，主要宠物（共享宠物），
        //   EXP（放入经验池），塔防宠物等等几种。
        //   给玩家的提示，如"当前宠物为主要宠物，不能当成材料"
        //  如果几种状态共存，应该提示哪个,没有，直接返回"".

        if (ME.user == null)
            return string.Empty;

        // 玩家的共享宠物    share_pet:({})
        LPCValue sharePet = ME.user.Query<LPCValue>("share_pet");

        if (sharePet != null &&
            sharePet.IsString &&
            sharePet.AsString.Equals(ob.GetRid()))
            return LocalizationMgr.Get("PetStrengthenWnd_31");

        // 获取防守宠物
        LPCValue defensePet = ME.user.Query<LPCValue>("defense_troop");

        if (defensePet != null && defensePet.IsArray
            && defensePet.AsArray.IndexOf(ob.GetRid()) != -1)
            return LocalizationMgr.Get("PetStrengthenWnd_33");

        return string.Empty;
    }
}

/// <summary>
/// 计算抽奖消耗
/// </summary>
public class CACL_LOTTERY_BONUS_COST
{
    public static LPCMapping Call()
    {
        LPCMapping costMap = new LPCMapping();

        // 市集刷新卷
        LPCValue lottery_card = ME.user.Query<LPCValue>("lottery_card");
        if(lottery_card != null && lottery_card.IsInt && lottery_card.AsInt > 0)
            costMap.Add("lottery_card", 1);
        else
            costMap.Add("gold_coin", 20);
        return costMap;
    }
}

/// <summary>
/// 解锁市集商品列表格子消耗
/// </summary>
public class CALC_UNLOCK_SHOP_ITEM_COST
{
    public static LPCMapping Call(int shopPos)
    {
        LPCMapping cost = new LPCMapping();

        switch (shopPos)
        {
            case 5:
                cost.Add("money", 1000);
                return cost;
            case 6:
                cost.Add("gold_coin", 10);
                return cost;
            case 7:
                cost.Add("money", 10000);
                return cost;
            case 8:
                cost.Add("gold_coin", 25);
                return cost;
            case 9:
                cost.Add("money", 30000);
                return cost;
            case 10:
                cost.Add("gold_coin", 40);
                return cost;
            case 11:
                cost.Add("money", 50000);
                return cost;
            case 12:
                cost.Add("gold_coin", 55);
                return cost;
            default:
                cost.Add("gold_coin", 55);
                return cost;
        }
    }
}

/// <summary>
/// 计算疲劳恢复的时间间隔
/// </summary>
public class CALC_RECOVER_LIFE_INTERVAL
{
    public static int Call(Property user)
    {
        return GameSettingMgr.GetSettingInt("recover_interval");
    }
}


/// <summary>
/// 计算疲劳恢复的恢复量
/// </summary>
public class CALC_RECOVER_BASE_VALUE
{
    public static int Call(Property user)
    {
        return GameSettingMgr.GetSettingInt("recover_life_base_value");
    }
}

/// <summary>
/// 恢复竞技场券的时间间隔
/// </summary>
public class CALC_RECOVER_AP_INTERVAL
{
    public static int Call(Property user)
    {
        return GameSettingMgr.GetSettingInt("recover_ap_interval");
    }
}

/// <summary>
/// 计算宠物的排序方式
/// </summary>
public class CALC_MONSTER_SORT_RULE
{
    public static string Call(Property ob, int sortType)
    {
        //  大冒险参考

        /// 规则：new>类型>等级>颜色>战力。
        /// 类型的顺序设定：武器>手套，盔甲>头盔，鞋子>腰带，项链>戒指，药品>宝箱。
        /// 道具等级顺序设定：高级别>低级别
        /// 道具颜色顺序设定：暗金>金>蓝>白

        // 星级标识
        int star = ob.Query<int>("star");

        // 等级标识
        int level = ob.Query<int>("level");

        // 属性标识
        int element = MonsterConst.sortElement[ob.Query<int>("element")];

        // 使用标识
        int rank = ob.Query<int>("rank");

        switch (sortType)
        {
            case MonsterConst.SORT_BY_STAR:
                return string.Format("@{0:D2}{1:D2}{2:D2}{3:D2}", star, level, element, rank);
            case MonsterConst.SORT_BY_LEVEL:
                return string.Format("@{0:D2}{1:D2}{2:D2}{3:D2}", level, star, element, rank);
            case MonsterConst.SORT_BY_PROPERTY:
                return string.Format("@{0:D2}{1:D2}{2:D2}{3:D2}", element, star, level, rank);
            case MonsterConst.SORT_BY_LATELY:
                return string.Format("@{0:D2}{1:D2}{2:D2}{3:D2}", rank, star, level, element);
            default:
                return string.Format("@{0:D2}{1:D2}{2:D2}{3:D2}", star, level, element, rank );
        }
    }
}

/// <summary>
/// 计算装备的排序方式
/// </summary>
public class CALC_EQUIP_SORT_RULE
{
    public static string Call(Property ob)
    {
        // 装备类型
        int equipType = EquipConst.sortEquipType[ob.Query<int>("equip_type")];

        // 装备星级
        int star = ob.Query<int>("star");

        // 装备强化等级
        int rank = ob.Query<int>("rank");

        // 装备品质
        int rarity = ob.GetRarity();

        return string.Format("@{0:D2}{1:D2}{2:D2}{3:D2}", equipType, star, rank, rarity );
    }
}

/// <summary>
/// 多选出售装备的排序方式
/// </summary>
public class CALC_EQUIP_ATTRIB_SORT_RULE
{
    public static string Call(Property ob, LPCArray minor_prop)
    {
        // 装备类型
        int equipType = EquipConst.sortEquipType[ob.Query<int>("equip_type")];

        // 装备星级
        int star = ob.Query<int>("star");

        // 装备强化等级
        int rank = ob.Query<int>("rank");

        // 装备品质
        int rarity = ob.GetRarity();

        LPCMapping prop = ob.Query<LPCMapping>("prop");

        LPCArray minor = prop.GetValue<LPCArray>(EquipConst.MINOR_PROP);
        if (minor == null)
            minor = LPCArray.Empty;

        LPCArray array = new LPCArray(0, 0);

        foreach (LPCValue value in minor.Values)
        {
            if (value == null || !value.IsArray)
                continue;

            LPCArray arr = value.AsArray;

            for (int i = 0; i < minor_prop.Count; i++)
            {
                if (minor_prop[i].Equals(arr[0]))
                    array[i] = arr[1];
            }
        }

        return string.Format("@{0:D2}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}", array[0].AsInt, array[1].AsInt, equipType, star, rank, rarity );
    }
}

/// <summary>
/// 宠物能不能作为和合成材料宠物状态描述
/// </summary>
public class PET_SYNTHESIS_AUTO_SELECT_MATERIAL
{
    public static Property Call(Property user, List<Property> petList)
    {
        if (user == null)
            return null;

        if (petList.Count == 0)
            return null;

        Property pet;

        // 合成材料只有一个
        if (petList.Count == 1)
        {
            pet = petList[0];
        }
        else
        {
            // TODO 根据装备和技能等筛选出最适合的宠物
            // 此处不需要检测“锁定”，“共享”，“防守阵营”，“仓库”等，petlist已作了筛选。

            // 测试代码
            pet = petList[0];
        }

        return pet;
    }
}

/// <summary>
/// 计算宠物合成选择的排序方式
/// </summary>
public class CALC_PET_SYNTHESIS_SELECT_SORT_RULE
{
    public static string Call(Property ob)
    {
        string pos = ob.Query("pos").AsString;

        // 取得最后一位包裹位置
        string last_pos = pos.Substring(pos.LastIndexOf("-") + 1);

        return string.Format("@{0:D3}", last_pos);
    }
}

/// <summary>
/// 计算对象标准价格
/// </summary>
public class CALC_STANDARD_PRICE
{
    public static LPCMapping Call(Property ob)
    {
        // 策划需要维护该脚本，这个地方需要区分装备，道具，宠物，其他等
        LPCMapping price = LPCMapping.Empty;

        if (EquipMgr.IsEquipment(ob))
            price.Add("money", EquipMgr.GetEquipValue(ob));

        // 策划需要维护该脚本
        return price;
    }
}

/// <summary>
/// 计算玩家最大体力
/// </summary>
public class CALC_MAX_LIFE
{
    public static int Call(int level)
    {
        return 30 + level;
    }
}

/// <summary>
/// 计算升星/召唤使魔获得的积分
/// </summary>
public class CALC_STARUP_SUMMON_SCORE
{
    public static LPCArray Call(string source)
    {
        LPCArray list = LPCArray.Empty;

        // 升星
        if (source.Equals("starup"))
        {
            list.Add(20);
            list.Add(50);
            list.Add(150);
            list.Add(350);
        }
        else if (source.Equals("summon"))
        {
            list.Add(20);
        }
        else
        {
        }

        return list;
    }
}

/// <summary>
/// 获取排位战胜利和失败积分
/// </summary>
public class CALC_RANK_BATTLE_SCORE
{
    public static LPCMapping Call()
    {
        // 构造参数
        LPCMapping dbase = LPCMapping.Empty;

        // 胜利获取的积分
        dbase.Add("win_score", 12);

        // 失败获取的积分
        dbase.Add("fail_score", 6);

        return dbase;
    }
}

/// <summary>
/// 计算地下城挑战获得积分
/// </summary>
public class CALC_CHALLENGE_DUNGEONS_SCORE
{
    public static LPCArray Call()
    {
        return new LPCArray(
            new LPCArray(3,6,9,12),   // 魂石各层获得积分
            new LPCArray(6,9,12,15),  // 巨石各层获得积分
            new LPCArray(9,12,15,18), // 魔眼各层获得积分
            new LPCArray(9,12,15,18)  // 核心各层获得积分
        );
    }
}

/// <summary>
/// 判断回合战斗是否是自动战斗
/// </summary>
public class IS_AUTO_COMBAT
{
    public static bool Call(Property ob, int roundType)
    {
        // 检查血是否为指引用自动战斗
        if (ob.QueryAttrib("guide_auto_id") > 0)
            return true;

        // 检查是否有自动释放状态类
        if (ob.CheckStatus("B_NEXT_ROUND_ATK"))
        {
            // 判断回合类型，只在正常回合和追加回合起效
            if (roundType == RoundCombatConst.ROUND_TYPE_NORMAL ||
                roundType == RoundCombatConst.ROUND_TYPE_ADDITIONAL ||
                roundType == RoundCombatConst.ROUND_TYPE_GASSER)
                return true;
        }

        List<LPCMapping> allStatus = ob.GetAllStatus();
        List<LPCMapping> ctrlList = new List<LPCMapping>();
        foreach (LPCMapping statusData in allStatus)
        {
            CsvRow statusInfo;
            statusInfo = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));
            LPCMapping statusMap = statusInfo.Query<LPCMapping>("limit_round_args");
            if (statusMap.GetValue<int>("ctrl_id") > 0)
                ctrlList.Add(statusData);
        }

        // 检查血量低于设定百分比起效类
        if (ob.QueryAttrib("low_hp_cast_skill") > 0 &&
            ! CdMgr.SkillIsCooldown(ob, 702) &&
            ctrlList.Count == 0)
        {
            // 判断血量条件是否满足，不满足则直接跳出判断
            int hpRate = Game.Divided(ob.Query<int>("hp"), ob.QueryAttrib("max_hp"));
            if (hpRate >= ob.QueryAttrib("low_hp_cast_skill"))
                return false;

            // 判断回合类型，只在正常回合和追加回合起效
            if (roundType == RoundCombatConst.ROUND_TYPE_NORMAL ||
                roundType == RoundCombatConst.ROUND_TYPE_ADDITIONAL ||
                roundType == RoundCombatConst.ROUND_TYPE_GASSER)
                return true;
        }

        return false;
    }
}

/// <summary>
/// 计算通天塔怪物防御
/// </summary>
public class CALC_TOWER_MONSTER_DEFENCE : Formula
{
    public static int Call(int type, int mapType, LPCMapping para)
    {
        // 策划自行调整公式1+A11/100(转换为千分位)
        return 1000 + para.GetValue<int>("layer") * 10;
    }
}

/// <summary>
/// 计算公会副会长主数量
/// </summary>
public class CALC_DEPUTY_LEADER_NUM : Formula
{
    public static int Call(int level)
    {
        // 策划自行维护该脚本公式
        return 2;
    }
}

/// <summary>
/// 公会创建段位条件
/// </summary>
public class CALC_CREATE_GUILD_RANK_CONDITION : Formula
{
    public static List<int> Call()
    {
        return new List<int>() {-1, 13, 10, 7, 6, 5, 4, 3, 2};
    }
}

public class CALC_GANG_MEMBER_LIST_SORT_RULE : Formula
{
    public static string Call(LPCMapping data)
    {
        string station = data.GetValue<string>("station");

        int stationType = 0;

        switch (station)
        {
            case "gang_leader":
                stationType = 1;
                break;

            case "gang_deputy_leader":
                stationType = 2;
                break;

            case "gang_member":
                stationType = 3;
                break;

            default:
                break;
        }

        int createTime = data.GetValue<int>("create_time");

        return string.Format("@{0:D2}{1:D10}", stationType, createTime);
    }
}

/// <summary>
/// 百分比属性和敏捷属性。
/// </summary>
public class CALC_EQUIP_FILTER_STANDARD : Formula
{
    public static LPCArray Call()
    {
        return new LPCArray(5, 6, 7, 8, 10, 11, 12, 13);;
    }
}
