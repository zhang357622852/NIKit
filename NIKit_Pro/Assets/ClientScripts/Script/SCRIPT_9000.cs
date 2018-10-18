/// <summary>
/// SCRIPT_9000.cs
/// Create by wangxw 2014-11-27
/// 战斗表现脚本：ActionSet参数收集
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// <summary>
/// 主角信息收集脚本
/// </summary>
public class SCRIPT_9000 : Script
{
    // 收集护盾状态列表
    private static List<string> shieldStatusList = new List<string>()
    {
        "B_EN_SHIELD", "B_HP_SHIELD", "B_EQPT_SHIELD",
    };

    public override object Call(params object[] _param)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        // 获取攻击者对象
        Property who = _param[0] as Property;
        if (who == null)
            return new LPCMapping();

        // 获取技能释放信息
        int skillId = (int)_param[1];
        LPCMapping actionArgs = _param[2] as LPCMapping;

        //CsvRow skillInfo = (CsvRow)_param[3];

        // 获取最终属性列表
        LPCMapping finalAttribMap = who.GetFinalAttrib();

        // 收集场上敌方死亡单位数量
        int campId = (who.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        int targetDieCount = 0;
        int targetLiveCount = 0;
        int ourDieCount = 0;
        int ourLiveCount = 0;

        // 收集场上所有死亡单位
        List<Property> allList = RoundCombatMgr.GetPropertyList();
        foreach (Property deathOb in allList)
        {
            // 统计对方存活情况
            if (deathOb.CampId == campId)
            {
                if (deathOb.CheckStatus("DIED"))
                    targetDieCount++;
                else
                    targetLiveCount++;

                continue;
            }

            // 统计己方存活情况
            if (deathOb.CheckStatus("DIED"))
                ourDieCount++;
            else
                ourLiveCount++;
        }

        // 获取自身增益状态数量、减益状态数量
        int buffCount = 0;
        int debuffCount = 0;
        int shieldCount = 0;
        int statusType = 0;
        List<LPCMapping> allStatus = who.GetAllStatus();

        // 遍历当前状态，统计状态情况
        foreach (LPCMapping statusMap in allStatus)
        {
            statusType = statusMap.GetValue<int>("status_type");

            // 如果是TYPE_BUFF
            if (statusType == StatusConst.TYPE_BUFF)
            {
                buffCount++;

                string statusAlias = StatusMgr.GetStatusAlias(statusMap.GetValue<int>("status_id"));
                if (shieldStatusList.IndexOf(statusAlias) == -1)
                    continue;

                // 统计护盾
                shieldCount++;
                continue;
            }

            // 如果是TYPE_DEBUFF
            if (statusType == StatusConst.TYPE_DEBUFF)
            {
                debuffCount++;
                continue;
            }
        }

        // 构建参数
        LPCMapping sourceProfile = new LPCMapping();

        // 获取技能配置信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        // 获取当前血量情况
        int maxHp = who.QueryAttrib("max_hp");
        int curHp = who.Query<int>("hp");

        // 基础数据
        sourceProfile.Add("rid", who.GetRid());
        sourceProfile.Add("name", who.GetName());
        sourceProfile.Add("level", who.GetLevel());
        sourceProfile.Add("element", who.Query<int>("element"));
        sourceProfile.Add("hp", curHp);
        sourceProfile.Add("lost_hp", maxHp - curHp);
        sourceProfile.Add("hp_rate", who.QueryTemp<int>("hp_rate"));
        sourceProfile.Add("skill_id", skillId);
        sourceProfile.Add("skill_range_type", skillInfo.Query<int>("range_type"));
        sourceProfile.Add("cookie", actionArgs["cookie"]);
        sourceProfile.Add("original_cookie", actionArgs["original_cookie"]);
        sourceProfile.Add("hit_index", actionArgs["hit_index"]);
        sourceProfile.Add("type", actionArgs.GetValue<int>("type"));
        sourceProfile.Add("target_die_count", targetDieCount);
        sourceProfile.Add("target_live_count", targetLiveCount);
        sourceProfile.Add("our_die_count", ourDieCount);
        sourceProfile.Add("our_live_count", ourLiveCount);
        sourceProfile.Add("base_atk", who.QueryAttrib("attack", false));
        sourceProfile.Add("imp_atk", finalAttribMap.GetValue<int>("imp_atk"));
        sourceProfile.Add("buff_count", buffCount);
        sourceProfile.Add("debuff_count", debuffCount);

        // 检查自身是否有护盾状态下必定暴击属性
        if (who.QueryAttrib("shield_crt") > 0 && shieldCount > 0)
            sourceProfile.Add("extra_crt_id", 1);

        // 添加发起协同者信息
        if (actionArgs.ContainsKey("joint_rid"))
            sourceProfile.Add("joint_rid", actionArgs["joint_rid"]);

        // 添加无效凶手标识，不夺取击杀归属
        if (actionArgs.ContainsKey("invalid_assailant"))
            sourceProfile.Add("invalid_assailant", actionArgs["invalid_assailant"]);

        // 检查反击回合添加反击减伤系数
        if (actionArgs.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_COUNTER)
            sourceProfile.Add("counter_reduce_dmg", CombatConst.DEFAULT_COUNTER_DAMAGE);

        // copy finalAttribMap中属性到sourceProfile中
        sourceProfile.Append(finalAttribMap);

#if UNITY_EDITOR
        // 调试相关
        sourceProfile.Add("ignore_damage_floating_exchange", who.QueryTemp<int>("ignore_damage_floating_exchange"));
#endif

        // 返回数据
        return sourceProfile;
    }
}

/// <summary>
/// 主角信息收集脚本（暗死神使者被动所带来的攻击技能专用）
/// </summary>
public class SCRIPT_9001 : Script
{
    // 收集护盾状态列表
    private static List<string> shieldStatusList = new List<string>()
    {
        "B_EN_SHIELD", "B_HP_SHIELD", "B_EQPT_SHIELD",
    };

    public override object Call(params object[] _param)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        // 获取攻击者对象
        Property who = _param[0] as Property;
        if (who == null)
            return new LPCMapping();

        // 获取技能释放信息
        int skillId = (int)_param[1];
        LPCMapping actionArgs = _param[2] as LPCMapping;

        //CsvRow skillInfo = (CsvRow)_param[3];

        // 获取最终属性列表
        LPCMapping finalAttribMap = who.GetFinalAttrib();

        // 收集场上敌方死亡单位数量、收集场上所有死亡单位数量
        int campId = (who.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        int targetDieCount = 0;
        int targetLiveCount = 0;
        int ourDieCount = 0;

        // 收集场上所有死亡单位
        List<Property> allList = RoundCombatMgr.GetPropertyList();
        foreach (Property deathOb in allList)
        {
            // 统计对方存活情况
            if (deathOb.CampId == campId)
            {
                if (deathOb.CheckStatus("DIED"))
                    targetDieCount++;
                else
                    targetLiveCount++;

                continue;
            }

            // 统计己方存活情况
            if (deathOb.CheckStatus("DIED"))
                ourDieCount++;
        }

        // 获取自身增益状态数量、减益状态数量
        int buffCount = 0;
        int debuffCount = 0;
        int shieldCount = 0;
        int statusType = 0;
        List<LPCMapping> allStatus = who.GetAllStatus();

        // 遍历当前状态，统计状态情况
        foreach (LPCMapping statusMap in allStatus)
        {
            statusType = statusMap.GetValue<int>("status_type");

            // 如果是TYPE_BUFF
            if (statusType == StatusConst.TYPE_BUFF)
            {
                buffCount++;

                string statusAlias = StatusMgr.GetStatusAlias(statusMap.GetValue<int>("status_id"));
                if (shieldStatusList.IndexOf(statusAlias) == -1)
                    continue;

                // 统计护盾
                shieldCount++;
                continue;
            }

            // 如果是TYPE_DEBUFF
            if (statusType == StatusConst.TYPE_DEBUFF)
            {
                debuffCount++;
                continue;
            }
        }

        // 构建参数
        LPCMapping sourceProfile = new LPCMapping();

        // 获取技能配置信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        // 基础数据
        sourceProfile.Add("rid", who.GetRid());
        sourceProfile.Add("name", who.GetName());
        sourceProfile.Add("level", who.GetLevel());
        sourceProfile.Add("element", who.Query<int>("element"));
        sourceProfile.Add("hp", who.Query<int>("hp"));
        sourceProfile.Add("hp_rate", who.QueryTemp<int>("hp_rate"));
        sourceProfile.Add("skill_id", skillId);
        sourceProfile.Add("skill_range_type", skillInfo.Query<int>("range_type"));
        sourceProfile.Add("cookie", actionArgs["cookie"]);
        sourceProfile.Add("original_cookie", actionArgs["original_cookie"]);
        sourceProfile.Add("hit_index", actionArgs["hit_index"]);
        sourceProfile.Add("type", actionArgs.GetValue<int>("type"));
        sourceProfile.Add("target_die_count", targetDieCount);
        sourceProfile.Add("target_live_count", targetLiveCount);
        sourceProfile.Add("base_atk", who.QueryAttrib("attack", false) * (1000 + who.QueryAttrib("dead_atk_up")) / 1000);
        sourceProfile.Add("imp_atk", finalAttribMap.GetValue<int>("imp_atk"));
        sourceProfile.Add("buff_count", buffCount);
        sourceProfile.Add("debuff_count", debuffCount);

        // 检查自身是否有护盾状态下必定暴击属性
        if (who.QueryAttrib("shield_crt") > 0 && shieldCount > 0)
            sourceProfile.Add("extra_crt_id", 1);

        // 添加发起协同者信息
        if (actionArgs.ContainsKey("joint_rid"))
            sourceProfile.Add("joint_rid", actionArgs["joint_rid"]);

        // 添加无效凶手标识，不夺取击杀归属
        if (actionArgs.ContainsKey("invalid_assailant"))
            sourceProfile.Add("invalid_assailant", actionArgs["invalid_assailant"]);

        // 检查反击回合添加反击减伤系数
        if (actionArgs.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_COUNTER)
            sourceProfile.Add("counter_reduce_dmg", CombatConst.DEFAULT_COUNTER_DAMAGE);

        // copy finalAttribMap中属性到sourceProfile中
        sourceProfile.Append(finalAttribMap);

#if UNITY_EDITOR
        // 调试相关
        sourceProfile.Add("ignore_damage_floating_exchange", who.QueryTemp<int>("ignore_damage_floating_exchange"));
#endif

        // 返回数据
        return sourceProfile;
    }
}

/// <summary>
/// 光战阵女王专用主角信息收集脚本
/// </summary>
public class SCRIPT_9003 : Script
{
    // 收集护盾状态列表
    private static List<string> shieldStatusList = new List<string>()
        {
            "B_EN_SHIELD", "B_HP_SHIELD", "B_EQPT_SHIELD",
        };

    public override object Call(params object[] _param)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        // 获取攻击者对象
        Property who = _param[0] as Property;
        if (who == null)
            return new LPCMapping();

        // 获取技能释放信息
        int skillId = (int)_param[1];
        LPCMapping actionArgs = _param[2] as LPCMapping;

        //CsvRow skillInfo = (CsvRow)_param[3];

        // 获取最终属性列表
        LPCMapping finalAttribMap = who.GetFinalAttrib();

        // 收集场上敌方死亡单位数量
        int campId = (who.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        int targetDieCount = 0;
        int targetLiveCount = 0;
        int ourDieCount = 0;
        int ourLiveCount = 0;

        // 收集场上所有死亡单位
        List<Property> allList = RoundCombatMgr.GetPropertyList();
        foreach (Property deathOb in allList)
        {
            // 统计对方存活情况
            if (deathOb.CampId == campId)
            {
                if (deathOb.CheckStatus("DIED"))
                    targetDieCount++;
                else
                    targetLiveCount++;

                continue;
            }

            // 统计己方存活情况
            if (deathOb.CheckStatus("DIED"))
                ourDieCount++;
            else
                ourLiveCount++;
        }

        // 获取自身增益状态数量、减益状态数量
        int buffCount = 0;
        int debuffCount = 0;
        int shieldCount = 0;
        int statusType = 0;
        List<LPCMapping> allStatus = who.GetAllStatus();

        // 遍历当前状态，统计状态情况
        foreach (LPCMapping statusMap in allStatus)
        {
            statusType = statusMap.GetValue<int>("status_type");

            // 如果是TYPE_BUFF
            if (statusType == StatusConst.TYPE_BUFF)
            {
                buffCount++;

                string statusAlias = StatusMgr.GetStatusAlias(statusMap.GetValue<int>("status_id"));
                if (shieldStatusList.IndexOf(statusAlias) == -1)
                    continue;

                // 统计护盾
                shieldCount++;
                continue;
            }

            // 如果是TYPE_DEBUFF
            if (statusType == StatusConst.TYPE_DEBUFF)
            {
                debuffCount++;
                continue;
            }
        }

        // 构建参数
        LPCMapping sourceProfile = new LPCMapping();

        // 获取技能配置信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        // 获取当前血量情况
        int maxHp = who.QueryAttrib("max_hp");
        int curHp = who.Query<int>("hp");

        // 基础数据
        sourceProfile.Add("rid", who.GetRid());
        sourceProfile.Add("name", who.GetName());
        sourceProfile.Add("level", who.GetLevel());
        sourceProfile.Add("element", who.Query<int>("element"));
        sourceProfile.Add("hp", curHp);
        sourceProfile.Add("lost_hp", maxHp - curHp);
        sourceProfile.Add("hp_rate", who.QueryTemp<int>("hp_rate"));
        sourceProfile.Add("skill_id", skillId);
        sourceProfile.Add("skill_range_type", skillInfo.Query<int>("range_type"));
        sourceProfile.Add("cookie", actionArgs["cookie"]);
        sourceProfile.Add("original_cookie", actionArgs["original_cookie"]);
        sourceProfile.Add("hit_index", actionArgs["hit_index"]);
        sourceProfile.Add("type", actionArgs.GetValue<int>("type"));
        sourceProfile.Add("target_die_count", targetDieCount);
        sourceProfile.Add("target_live_count", targetLiveCount);
        sourceProfile.Add("our_die_count", ourDieCount);
        sourceProfile.Add("our_live_count", ourLiveCount);
        sourceProfile.Add("base_atk", who.QueryAttrib("attack", false));
        sourceProfile.Add("imp_atk", finalAttribMap.GetValue<int>("imp_atk"));
        sourceProfile.Add("buff_count", buffCount);
        sourceProfile.Add("debuff_count", debuffCount);

        // 检查自身是否有护盾状态下必定暴击属性
        if (who.QueryAttrib("shield_crt") > 0 && shieldCount > 0)
            sourceProfile.Add("extra_crt_id", 1);

        // 添加发起协同者信息
        if (actionArgs.ContainsKey("joint_rid"))
            sourceProfile.Add("joint_rid", actionArgs["joint_rid"]);

        // 添加无效凶手标识，不夺取击杀归属
        if (actionArgs.ContainsKey("invalid_assailant"))
            sourceProfile.Add("invalid_assailant", actionArgs["invalid_assailant"]);

        // 检查反击回合添加反击减伤系数
        if (actionArgs.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_COUNTER)
            sourceProfile.Add("counter_reduce_dmg", CombatConst.DEFAULT_COUNTER_DAMAGE);

        // 检查自身隐忍被动
        if (who.QueryAttrib("forbear_ignore_def") > 0)
        {
            // 收集己方受控制状态目标的数量
            List<Property> targetList = RoundCombatMgr.GetPropertyList(who.CampId);

            // 收集受控制目标数量
            int ctrlNum = 0;
            foreach (Property targetOb in targetList)
            {
                if (targetOb.GetRid().Equals(who.GetRid()))
                    continue;

                // 遍历自身状态列表
                List <LPCMapping> chackStatus = targetOb.GetAllStatus();
                LPCArray ctrlNumList = LPCArray.Empty;
                for (int i = 0; i < chackStatus.Count; i++)
                {
                    CsvRow statusInfo;
                    statusInfo = StatusMgr.GetStatusInfo(chackStatus[i].GetValue<int>("status_id"));
                    LPCMapping ctrlMap = statusInfo.Query<LPCMapping>("limit_round_args");
                    // 如果是控制状态，则添加数量
                    if (ctrlMap.ContainsKey("ctrl_id"))
                        ctrlNumList.Add(chackStatus[i].GetValue<int>("status_id"));
                }
                if (ctrlNumList.Count > 0)
                    ctrlNum += 1;
            }
            sourceProfile.Add("ignore_def", Math.Min(ctrlNum * who.QueryAttrib("forbear_ignore_def"), 1000));
        }

        // copy finalAttribMap中属性到sourceProfile中
        sourceProfile.Append(finalAttribMap);

        #if UNITY_EDITOR
        // 调试相关
        sourceProfile.Add("ignore_damage_floating_exchange", who.QueryTemp<int>("ignore_damage_floating_exchange"));
        #endif

        // 返回数据
        return sourceProfile;
    }
}