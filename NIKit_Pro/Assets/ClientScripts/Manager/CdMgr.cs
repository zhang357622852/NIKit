/// <summary>
/// CdMgr.cs
/// Create by zhaozy 2014-11-6
/// cd管理模块
/// </summary>
using System;
using System.Diagnostics;
using System.Collections.Generic;
using LPC;

/// 道具管理器
public static class CdMgr
{
    #region 功能接口

    /// <summary>
    /// 是否有技能处于cd中
    /// </summary>
    public static bool HasSkillICooldown(Property who)
    {
        // 取所有技能的CD信息
        LPCMapping cdSkills = who.QueryTemp<LPCMapping>("cd_skills");
        if (cdSkills == null || cdSkills.Count == 0)
            return false;

        // 回合制技能CD是否已到了
        foreach (LPCValue cd in cdSkills.Values)
        {
            // 如果技能处于cd中
            if (cd.AsInt > 0)
                return true;
        }

        // 没有技能处于cd中
        return false;
    }

    /// <summary>
    /// 判断技能是否在CD中
    /// </summary>
    public static bool SkillIsCooldown(Property who, int skillId)
    {
        if (skillId < 0)
            return false;

        // 取所有技能的CD信息
        LPCMapping cdSkills = who.QueryTemp<LPCMapping>("cd_skills");
        if (cdSkills == null || !cdSkills.ContainsKey(skillId))
            return false;

        // 回合制技能CD是否已到了
        if (cdSkills[skillId].AsInt <= 0)
        {
            // 冷却已到期，记录到清除列表
            cdSkills.Remove(skillId);

            // 有清除了冷却信息
            who.SetTemp("cd_skills", LPCValue.Create(cdSkills));

            // 技能不在CD中
            return false;
        }

        // 正在cd中
        return true;
    }

    ///<summary>
    /// 增加角色技能cd
    /// </summary>
    public static void AddSkillCd(Property who, int skillId, int cdTimes)
    {
        // 获取当前的cd信息
        LPCMapping cdSkills = who.QueryTemp<LPCMapping>("cd_skills");
        if (cdSkills == null)
            cdSkills = LPCMapping.Empty;

        // 计算新cd回合数
        int newCd = Math.Max(cdTimes + cdSkills.GetValue<int>(skillId), 0);

        // 设置cd信息
        cdSkills.Add(skillId, newCd);

        // 重置技能cd
        who.SetTemp("cd_skills", LPCValue.Create(cdSkills));
    }

    ///<summary>
    /// 回合结束，CD要减少
    /// </summary>
    public static void ReduceSkillCd(Property who)
    {
        // 获取当前的cd信息
        LPCMapping cdSkills = who.QueryTemp<LPCMapping>("cd_skills");
        if (cdSkills == null || cdSkills.Count == 0)
            return;

        // 需要清除列表
        LPCMapping newCdSkills = LPCMapping.Empty;

        // 遍历各个技能cd
        foreach (int skillId in cdSkills.Keys)
        {
            // 技能cd需要清除
            if (cdSkills[skillId].AsInt <= 1)
                continue;

            // 技能cd减少一回合
            newCdSkills.Add(skillId, Math.Max(cdSkills[skillId].AsInt - 1, 0));
        }

        // 重置技能cd
        who.SetTemp("cd_skills", LPCValue.Create(newCdSkills));
    }

    /// <summary>
    /// 技能cd
    /// </summary>
    /// <param name="who">玩家对象</param>
    /// <param name="skillId">技能ID</param>
    public static void SkillCooldown(Property who, int skillId)
    {
#if UNITY_EDITOR
        // 当前屏蔽了技能冷却
        if (! AuthClientMgr.IsAuthClient &&
            ME.user.QueryTemp<int>("ignore_cd") == 1)
            return;
#endif

        // 技能Id错误，找不到该技能的配置信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);
        if (skillInfo == null)
            return;

        // 回合制技能使用后执行冷却处理
        RtSkillCd(skillId, who);
    }

    /// <summary>
    /// 获取技能冷却时间(算上装备，技能等级等等的CD)
    /// </summary>
    /// <returns>技能冷却时间 秒</returns>
    /// <param name="who">技能所有者，记录技能等级</param>
    /// <param name="skillId">技能Id.</param>
    public static int GetSkillCd(Property who, int skillId)
    {
        CsvRow skill = SkillMgr.GetSkillInfo(skillId);
        if (skill == null)
            return 0;

        // 获取cd脚本
        int scriptNo = skill.Query<int>("cooldown_script");
        if (scriptNo == 0)
            return 0;

        // 通过脚本计算cd
        return (int)ScriptMgr.Call(scriptNo, who, skillId, skill.Query<LPCArray>("cooldown_arg"));
    }

    /// <summary>
    /// 获取基础技能冷却时间(技能表中配置的CD)
    /// </summary>
    /// <returns>The skill cd.</returns>
    /// <param name="who">Who.</param>
    /// <param name="skillId">Skill identifier.</param>
    public static int GetBaseSkillCd(int skillId, int level)
    {
        CsvRow skill = SkillMgr.GetSkillInfo(skillId);
        if (skill == null)
            return 0;

        // 获取cd脚本
        int scriptNo = skill.Query<int>("base_cooldown_script");
        if (scriptNo == 0)
            return 0;

        // 通过脚本计算cd
        return (int)ScriptMgr.Call(scriptNo, skillId, level, skill.Query<LPCArray>("cooldown_arg"));
    }


    /// <summary>
    /// 获取技能CD信息(战斗，剩余几回合的CD)
    /// </summary>
    public static int GetSkillCdRemainRounds(Property who, int skillId)
    {
        // 技能不在CD中
        if (!SkillIsCooldown(who, skillId))
            return 0;

        // 获取当前的cd信息
        LPCMapping cdSkills = who.QueryTemp<LPCMapping>("cd_skills");
        if (cdSkills == null)
            return 0;

        // 获取cd结束时间
        return Math.Max(cdSkills[skillId].AsInt, 0);
    }

    /// <summary>
    /// 降低技能的cd
    /// </summary>
    public static void DoReduceSkillCd(Property who, int skillId, int ReduceRound)
    {
        // 技能不在CD中
        if (!SkillIsCooldown(who, skillId))
            return;

        // 获取cd信息
        LPCMapping cdSkills = who.QueryTemp<LPCMapping>("cd_skills");
        if (cdSkills == null)
            return;

        // 减少cd
        cdSkills.Add(skillId, Math.Max(cdSkills[skillId].AsInt - ReduceRound, 0));
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 计算技能的释放对其他技能cooldown造成的影响
    /// 目前技能冷却的规则和物品冷却规则一致
    /// 通过组进行冷却计算
    /// </summary>
    /// <param name="skillId">技能Id</param>
    /// <param name="now">当前回合数 Tick</param>
    /// <param name="who">释放对象</param>
    private static void RtSkillCd(int skillId, Property who)
    {
        // 该技能不需要冷却
        if (GetSkillCd(who, skillId) <= 0)
            return;

        // 技能正在使用中
        if (SkillIsCooldown(who, skillId))
            return;

        // 处于CD的技能，([ id : 1 ])
        LPCMapping cdSkills = who.QueryTemp<LPCMapping>("cd_skills");
        if (cdSkills == null)
            cdSkills = new LPCMapping();

        // 记录技能的CD到期的的回合数 这里加1是因为施放的回合不算在内，自己的回合结束才开始计算CD
        cdSkills.Add(skillId, GetSkillCd(who, skillId));

        // 这里记录的原因是，初始, ME上的"cd_skills"可能不存在
        // 此时，就必须写，不然cd_items的更新就丢失了
        who.SetTemp("cd_skills", LPCValue.Create(cdSkills));
    }

    #endregion
}
