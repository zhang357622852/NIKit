/// <summary>
/// Skill.cs
/// Copy from zhangyg 2014-10-22
/// 角色技能特性
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 角色技能特性
/// </summary>    
public class Skill
{
    Property owner;

    public Skill(Property property)
    {
        this.owner = property;

        // 初始化技能信息
        this.owner.dbase.Set("skills", LPCValue.CreateArray());
    }

    public void Destroy()
    {
    }

    // 获取所有的技能
    public LPCArray GetAllSkills()
    {
        // 对象不存在
        if (owner == null)
            return LPCArray.Empty;

        // 获取技能信息
        LPCValue skills = owner.Query<LPCValue>("skills");
        if (skills == null || ! skills.IsArray)
            return LPCArray.Empty;

        // 获取试题的技能信息
        return skills.AsArray;
    }

    // 删除技能
    public void Delete(int skillId)
    {
        // 获取技能信息
        LPCArray skills = this.owner.Query<LPCArray>("skills");
        if (skills == null)
            return;

        // 遍历查找技能
        foreach (LPCValue mks in skills.Values)
        {
            // 技能id不一致
            if (mks.AsArray[0].AsInt != skillId)
                continue;

            // 删除技能
            skills.Remove(mks);
            break;
        }

        // 重新设置技能
        this.owner.dbase.Set("skills", LPCValue.Create(skills));
    }

    // 设置技能信息
    public void Set(int skillId, int level)
    {
        // 获取技能信息
        LPCArray skills = this.owner.Query<LPCArray>("skills");
        if (skills == null)
            skills = LPCArray.Empty;

        // 技能原始位置
        int index = -1;

        // 遍历查找技能
        for (int i = 0; i < skills.Count; i++)
        {
            // 技能id不一致
            if (skills[i].AsArray[0].AsInt != skillId)
                continue;

            // 找到了技能
            index = i;
            break;
        }

        // 记录技能信息
        if (index != -1)
            skills.Set(index, new LPCArray(skillId, level));
        else
            skills.Add(new LPCArray(skillId, level));

        // 通过set操作刷新以触发登记的触发器
        this.owner.dbase.Set("skills", LPCValue.Create(skills));
    }

    // 获取技能等级
    public int GetLevel(int skillId)
    {
        // 获取技能原始技能id
        int originalSkillId = SkillMgr.GetOriginalSkillId(skillId);

        // 获取技能信息
        LPCArray skills = this.owner.Query<LPCArray>("skills");
        if (skills == null || skills.Count == 0)
            return 0;

        // 遍历查找技能
        foreach (LPCValue mks in skills.Values)
        {
            // 技能id不一致
            if (mks.AsArray[0].AsInt != originalSkillId)
                continue;

            // 返回技能等级
            return mks.AsArray[1].AsInt;
        }

        // 返回技能等级为0
        return 0;
    }

    /// <summary>
    /// 判断是否可用支付技能开销
    /// </summary>
    public bool CanCastCost(int skillId)
    {
        // owner对象不存在
        if (owner == null)
            return false;

        // 获取技能开销
        LPCMapping costMap = SkillMgr.GetCasTCost(owner, skillId);

        // 不需要任何开销
        if (costMap.Count == 0)
            return true;

        // 判断是否可用支付技能开销
        return owner.CanCostAttrib(costMap);
    }
}
