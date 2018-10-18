/// <summary>
/// CALC_USER_MAX_HP.cs
/// Create by zhaozy 2014-11-12
/// 计算玩家最大血量
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 计算玩家包裹最大数量
/// </summary>
public class CALC_USER_MAX_BAGGAGE_SIZE : Formula
{
    public static int Call(User who, int page = ContainerConfig.POS_ITEM_GROUP)
    {
        if (page == ContainerConfig.POS_ITEM_GROUP)
            // 包裹最大容量
            return 150;
        else if (page == ContainerConfig.POS_STORE_GROUP)
            // 仓库最大容量
            return 150;
        else
        {
            // 未知分页
            LogMgr.Trace("未知分页类型 page = {0}", page);
            return 0;
        }
    }
}

/// <summary>
/// 检查判断玩家是否能够治愈
/// </summary>
public class CAN_RECEIVE_CURE : Formula
{
    public static bool Call(Property who, int cureType, LPCMapping points)
    {
        // 策划维护该脚本
        if (who.CheckStatus("DIED") || who.CheckStatus("COMBAT_END"))
            return false;

        // 不限制治疗
        return true;
    }
}

/// <summary>
/// 计算治愈修正处理
/// </summary>
public class CALC_FIXED_CURE_MAP : Formula
{
    public static LPCMapping Call(Property who, LPCMapping points)
    {
        if (points.ContainsKey("hp"))
        {
            int newHpCure = points.GetValue<int>("hp");
            int lostHpRate = 1000 - who.QueryTemp<int>("hp_rate");
            int cureMinRate = who.QueryAttrib("cure_min_rate");

            // 1、亡灵救赎被动
            if (who.QueryAttrib("lost_cure_rate") > 0)
            {
                LPCMapping newPoints = new LPCMapping();

                newPoints.Append(points);

                int fixedRate = 0;

                if (who.QueryTemp<int>("hp_rate") <= cureMinRate)
                    fixedRate = 1000;
                else
                    fixedRate = lostHpRate * who.QueryAttrib("lost_cure_rate") / 100;

                newHpCure += fixedRate * points.GetValue<int>("hp") / 1000;

                newPoints.Add("hp", newHpCure);

                return newPoints;
            }
            // 2、人马意志被动
            if (who.QueryAttrib("lowHp_more_cure") > 0)
            {
                LPCMapping newPoints = new LPCMapping();

                newPoints.Append(points);

                int fixedRate = 0;

                if (who.QueryTemp<int>("hp_rate") <= cureMinRate)
                    fixedRate = who.QueryAttrib("lowHp_more_cure");
 
                newHpCure += fixedRate * points.GetValue<int>("hp") / 1000;

                newPoints.Add("hp", newHpCure);

                return newPoints;
            }
        }

        return points;
    }
}

/// <summary>
/// 检查判断玩家是否能够受创
/// </summary>
public class CAN_RECEIVE_DAMAGE : Formula
{
    public static bool Call(Property who, int damageType)
    {
        // 策划维护该脚本
        if (who.CheckStatus("DIED") || who.CheckStatus("COMBAT_END"))
            return false;

        return true;
    }
}

/// <summary>
/// 受创后处理脚本
/// </summary>
public class RECEICE_DAMAGED : Formula
{
    /// <summary>
    /// 检查南瓜自爆技能复活
    /// </summary>
    /// <param name="who">Who.</param>
    private static void DoReceiveDamageEnd(Property who)
    {
        // 记录凶手信息
        string rid = who.QueryTemp<string>("assailant_info/rid");
        if (string.IsNullOrEmpty(rid))
            return;

        // 找到角色对象
        Property assailanOb = Rid.FindObjectByRid(rid);
        if (assailanOb == null)
            return;

        // 获取记录数据
        LPCMapping blewData = assailanOb.QueryTemp<LPCMapping>("blew_data");
        if (blewData == null)
            return;

        // 和角色没有任何关系
        LPCMapping blewMap = blewData.GetValue<LPCMapping>("blew_map");
        string sourRid = who.GetRid();
        if (! blewMap.ContainsKey(sourRid))
            return;

        // 标识已经处理过
        blewMap.Add(sourRid, 1);
    }

    /// <summary>
    /// Call the specified who.
    /// </summary>
    /// <param name="who">Who.</param>
    public static void Call(Property who)
    {
        // 尝试执行死亡
        TRY_DO_DIE.Call(who);

        // 标识受创结束
        DoReceiveDamageEnd(who);

        // 执行DoBlewRevive操作
        TRY_DO_DIE.DoBlewRevive(who);
    }
}

/// <summary>
/// 角色解锁后处理
/// </summary>
public class UNLOCK_END : Formula
{
    public static void Call(Property who)
    {
        // 尝试执行死亡
        TRY_DO_DIE.Call(who);

        // 执行DoBlewRevive操作
        TRY_DO_DIE.DoBlewRevive(who);
    }
}


/// <summary>
/// 尝试执行死亡操作
/// </summary>
public class TRY_DO_DIE : Formula
{
    /// <summary>
    /// 检查南瓜自爆技能复活
    /// </summary>
    /// <param name="who">Who.</param>
    public static void DoBlewRevive(Property who)
    {
        // 如果角色当前处于被锁定状态暂时不能死亡
        // 角色在解锁的时主动判断是否需要执行死亡
        if (who.IsLocked())
            return;

        // 记录凶手信息
        string rid = who.QueryTemp<string>("assailant_info/rid");
        if (string.IsNullOrEmpty(rid))
            return;

        // 找到角色对象
        Property assailanOb = Rid.FindObjectByRid(rid);
        if (assailanOb == null)
            return;

        // 获取记录数据
        LPCMapping blewData = assailanOb.QueryTemp<LPCMapping>("blew_data");
        if (blewData == null)
            return;

        // 和角色没有任何关系
        LPCMapping blewMap = blewData.GetValue<LPCMapping>("blew_map");
        string sourRid = who.GetRid();
        if (! blewMap.ContainsKey(sourRid) || blewMap[sourRid].AsInt != 1)
            return;

        // 是否有角色已经死亡
        bool hasDied = false;
        string cookie = blewData.GetValue<string>("cookie");

        // 遍历数据
        foreach (string id in blewMap.Keys)
        {
            // 还有角色没有处理完成
            if (blewMap[id].AsInt != 1)
                return;

            // 查找角色对象
            Property ob = Rid.FindObjectByRid(id);
            if (ob == null)
                continue;

            // 角色还处于锁定状态
            if (ob.IsLocked())
                return;

            // 角色没有死亡
            if (! ob.CheckStatus("DIED"))
                continue;

            // 标识有角色已经死亡了
            hasDied = true;
        }

        // 删除临时数据
        assailanOb.DeleteTemp("blew_data");

        // 如果有角色死亡了
        if (hasDied)
        {
            // 风南瓜专用
            if (assailanOb.Query<int>("class_id") == 1042)
            {
                // 播放复活序列(需要添加回合)
                string newCookie = Game.NewCookie(assailanOb.GetRid());
                RoundCombatMgr.AddRoundAction(newCookie, assailanOb);

                // 执行假复活
                assailanOb.Set("hp", LPCValue.Create(Math.Min(blewData.GetValue<int>("reborn_hp"), assailanOb.QueryAttrib("max_hp"))));

                // 执行源技能CD
                CdMgr.SkillCooldown(assailanOb, blewData.GetValue<int>("origin_skill_id"));

                // 目标取消当前正在播放技能的序列
                assailanOb.Actor.CancelActionSet(cookie);

                LPCMapping para = LPCMapping.Empty;
                para.Add("rid", assailanOb.GetRid());

                // 播放新节点
                assailanOb.Actor.DoActionSet("703_1_cast", newCookie, para);
            }
        }
        else
        {
            // 风南瓜专用
            if (assailanOb.Query<int>("class_id") == 1042)
            {
                // 直接设置目标位置
                assailanOb.SetPosition(assailanOb.MoveBackPos);

                // 目标取消当前正在播放技能的序列
                assailanOb.Actor.CancelActionSet(cookie);

                // 附加死亡状态
                (assailanOb as Char).DoDie();
            }
            // 极冷之壁专用
            if (assailanOb.Query<int>("class_id") == 8013)
            {
                // 自身扣血
                int maxHp =assailanOb.QueryAttrib("max_hp");
                int selfCostHp = Game.Multiple(maxHp,assailanOb.QueryAttrib("self_cost_hp"),1000);
                // 误差检查
                if (selfCostHp * 10 < maxHp)
                    selfCostHp += 1;
                int curHp = assailanOb.Query<int>("hp");
                assailanOb.Set("hp", LPCValue.Create(Math.Max(curHp - selfCostHp, 0)));
                if (selfCostHp >= curHp)
                    (assailanOb as Char).DoDie();
            }
        }
    }

    /// <summary>
    /// Call the specified who.
    /// </summary>
    /// <param name="who">Who.</param>
    public static void Call(Property who)
    {
        // 不是角色不处理
        if (!(who is Char))
            return;

        // 如果角色当前处于被锁定状态暂时不能死亡
        // 角色在解锁的时主动判断是否需要执行死亡
        if (who.IsLocked())
            return;

        // 如果角色已经死亡不处理
        if (who.CheckStatus("DIED"))
            return;

        // 如果本次攻击没有造成角色的血量小于0，或者处于免死状态则不处理
        if (who.Query<int>("hp") > 0 || who.CheckStatus("B_NO_DIE"))
            return;

        // 执行死亡
        (who as Char).DoDie();

        // 如果当前死亡的怪物是主怪，则需要清除场上同阵营的其他怪物
        if (who.Query<int>("is_main_monster") == 0 ||
            ! who.CheckStatus("DIED"))
            return;

        // 收集同阵营的其他怪物
        List<Property> entityList = RoundCombatMgr.GetPropertyList(who.CampId);
        entityList.Remove(who);

        // 没有实体
        if (entityList.Count == 0)
            return;

        // 清除怪物
        foreach (Property ob in entityList)
        {
            // 对象不存在
            if (ob == null)
                continue;

            // 如果怪物已经死亡不处理
            if (ob.CheckStatus("DIED"))
                continue;

            // 将角色的hp置为0
            ob.Set("hp", LPCValue.Create(0));

            // 尝试执行死亡操作
            TRY_DO_DIE.Call(ob);
        }
    }
}

/// <summary>
/// 实体升级后处理脚本
/// </summary>
public class WHEN_UPGRADE : Formula
{
    public static bool Call(Property who)
    {
        // 获取玩家的hp，mp情况
        int maxHp = who.QueryAttrib("max_hp");
        int maxMp = who.QueryAttrib("max_mp");

        // 升级后将玩家的hp和mp恢复满
        who.Set("hp", LPCValue.Create(maxHp));
        who.Set("mp", LPCValue.Create(maxMp));

        return true;
    }
}

/// <summary>
/// 刷新使魔生命、能量
/// </summary>
public class REFRESH_PET_HP_MP : Formula
{
    public static bool Call(Property who)
    {
        // 进入副本时需要满血满状态

        // 获取玩家的hp，mp情况
        who.Set("hp", LPCValue.Create(who.QueryAttrib("max_hp")));
        // 如果有特殊需求，需要在此处重新处理初始mp
        if (who.QueryAttrib("start_mp_ctrl") > 0)
            who.Set("mp", LPCValue.Create(who.QueryAttrib("start_mp_ctrl") - 1));

        return true;
    }
}

/// <summary>
/// 计算离线期间击杀的怪物数量.
/// </summary>
public class CALC_OFFLINE_KILL_MONSTERS_COUNT : Formula
{
    public static int Call(Property who, int offlineTime)
    {
        // 计算击杀的怪物数量(标准怪物)
        int killMonstersCount = offlineTime * 9;

        return killMonstersCount;
    }
}

/// <summary>
/// 计算离线挂机的属性奖励.
/// </summary>
public class CALC_OFFLINE_ATTRIB_BONUS : Formula
{
    public static LPCArray Call(Property who, int offlineTime)
    {
        LPCArray bonusAttribArray = new LPCArray();

        int level = who.Query<int>("level");

        // 获取玩家身上的金钱掉落提升属性
        LPCValue improvement = who.QueryTemp("improvement");
        LPCMapping improvementMap = new LPCMapping();
        
        if (improvement != null && improvement.IsMapping)
            improvementMap = improvement.AsMapping;
        int gold_find = improvementMap.GetValue<int>("gold_find");

        // 计算玩家离线offlineTime秒的属性奖励, 包括：金币、经验
        // 返回一个LPCArray，格式({ ([ "money" : 金钱数 ]), (["exp" : 经验数 ]) )}
        int money_num = offlineTime * 14 * CALC_STD_MONEY.Call(level) * (1000 + gold_find) / 1000;
        LPCMapping moneyMap = new LPCMapping();
        moneyMap.Add("money", money_num);
        bonusAttribArray.Add(LPCValue.Create(moneyMap));

        int exp_num = offlineTime * 9 * CALC_BE.Call(level);
        LPCMapping expMap = new LPCMapping();
        expMap.Add("exp", exp_num);
        bonusAttribArray.Add(LPCValue.Create(expMap));

        return bonusAttribArray;
    }
}
