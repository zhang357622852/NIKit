/// <summary>
/// CombatHit.cs
/// Copy from zhangyg 2014-10-22
/// 逻辑战斗hit事件
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// 逻辑战斗hit事件
/// </summary>
public static class CombatHit
{
    #region 内部接口

    /// <summary>
    /// 受创事件回调
    /// </summary>
    private static void DoHit(int eventId, MixedValue para)
    {
        LPCMapping actionArgs = para.GetValue<LPCMapping>();

#if UNITY_EDITOR
        // hit事件参数必须包含
        // 1. rid : 攻击者rid
        // 2. skill_id : 技能id
        // 3. cookie : cookie信息
        if (!actionArgs.ContainsKey("rid") ||
            !actionArgs.ContainsKey("skill_id") ||
            !actionArgs.ContainsKey("cookie"))
            return;
#endif

        // 读取skill表中相关技能的信息
        int skillId = actionArgs["skill_id"].AsInt;
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        // 没有配置的技能不处理
        if (skillInfo == null)
            return;

        // 获取信息
        string rid = actionArgs["rid"].AsString;

        // 查找攻击源对象
        Property sourceOb = Rid.FindObjectByRid(rid);

        // 对象不存在不处理
        if (sourceOb == null)
            return;

        // 获取hit_index
        int hitIndex = actionArgs.GetValue<int>("hit_index");

        // 获取指定index的hitScript
        LPCArray hitScript = GetHitScript(skillInfo, hitIndex);
        if (hitScript == null)
            return;

        // 1. 收集攻击者信息
        int scriptNo = hitScript[0].AsInt;
        LPCMapping sourceProfile = LPCMapping.Empty;
        if (scriptNo != 0)
            sourceProfile = ScriptMgr.Call(scriptNo, sourceOb, skillId, actionArgs,
                skillInfo, hitScript[3]) as LPCMapping;

        // 获取hit目标
        LPCMapping hitEntityMap = actionArgs.GetValue<LPCMapping>("hit_entity_map");
        if (hitEntityMap == null || !hitEntityMap.ContainsKey(hitIndex))
            return;

        // 没有收集到任何目标
        LPCArray selectList = hitEntityMap[hitIndex].AsArray;
        if (selectList.Count == 0)
            return;

        // 没有apply脚本
        scriptNo = hitScript[2].AsInt;
        if (scriptNo == 0)
            return;

        // hitPara
        LPCMapping extraArgs = LPCMapping.Empty;
        LPCMapping hitPara = LPCMapping.Empty;
        Property ob = null;

        // 逐个目标作用
        for (int i = 0; i < selectList.Count; i++)
        {
            // 查找目标对象
            rid = selectList[i].AsString;
            ob = Rid.FindObjectByRid(rid);

            // 目标对象不存在
            if (ob == null)
                continue;

            // 调用apply脚本
            object ret = ScriptMgr.Call(scriptNo, ob, sourceOb, skillId, actionArgs,
                             sourceProfile, skillInfo, hitScript[3]);

            // 添加数据
            if (ret is LPCMapping)
                hitPara.Add(rid, ret);
            else
                hitPara.Add(rid, LPCValue.Create());
        }

        // 添加参数
        extraArgs.Add(hitIndex, hitPara);

        // 更新ActionSet.ExtraArgs
        sourceOb.Actor.AddActionSetExtraArgs(actionArgs.GetValue<string>("cookie"),
            "hit_extra_args", extraArgs);
    }

    /// <summary>
    /// Ges the hit script.
    /// </summary>
    /// <returns>The hit script.</returns>
    private static LPCArray GetHitScript(CsvRow skillInfo, int hitIndex)
    {
        // 获取配置信息
        LPCValue hitScripts = skillInfo.Query<LPCValue>("hit_scripts");

#if UNITY_EDITOR
        // 技能命中脚本配置错误
        if (!hitScripts.IsArray ||
            hitScripts.AsArray.Count <= hitIndex)
        {
            LogMgr.Trace("技能{0}命中脚本配置错误, hitIndex = {1}",
                skillInfo.Query<int>("skill_id"), hitIndex);

            return null;
        }

        // 获取指定index的hitScript
        // hitScript单元格式({ 信息收集脚本, 目标收集脚本, 作用脚本, 参数 })
        LPCArray hitScript = hitScripts.AsArray[hitIndex].AsArray;
        if (hitScript == null ||
            hitScript.Count != 4 ||
            !hitScript[0].IsInt ||
            !hitScript[1].IsInt ||
            !hitScript[2].IsInt)
        {
            LogMgr.Trace("技能{0}命中脚本配置错误hitIndex = {1}, 正确格式({ source_script, collect_script, apply_script, agrs })",
                skillInfo.Query<int>("skill_id"), hitIndex);

            return null;
        }

        // 返回hitScript
        return hitScript;
#else
        // 非编辑器模式直接返回数据
        return hitScripts.AsArray[hitIndex].AsArray;
#endif
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 逻辑战斗hit初始化
    /// </summary>
    public static void InIt()
    {
        // 注册事件
        EventMgr.UnregisterEvent("CombatHit");
        EventMgr.RegisterEvent("CombatHit", EventMgrEventType.EVENT_HIT, DoHit);
    }

    #endregion
}
