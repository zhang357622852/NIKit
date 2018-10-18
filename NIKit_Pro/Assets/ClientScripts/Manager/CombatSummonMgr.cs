/// <summary>
/// CombatSummonMgr.cs
/// Created by zhaozy 2017-02-09
/// 战斗召唤管理器
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using LPC;

/// <summary>
/// 召唤管理器
/// </summary>
public static class CombatSummonMgr
{
    #region 私有接口

    /// <summary>
    /// Die事件回调
    /// </summary>
    private static void WhenDie(int eventId, MixedValue para)
    {
        // 获取参数获取targetOb
        Property target = para.GetValue<Property>();
        if (target == null)
            return;

        // 如果不是召唤怪物，不在这个地方处理
        string summonerRid = target.Query<string>("summoner_rid");
        if (string.IsNullOrEmpty(summonerRid))
            return;

        // 抛出公式判断是否能够执行析构操作
        if ((bool) CHECK_NEED_REBORN.Call(target))
            return;

        // 从召唤者列表中删除
        Property summoner = Rid.FindObjectByRid(summonerRid);
        if (summoner != null)
        {
            // 获取已经创建的怪物列表,召唤列表格式({ monster1, monster2, monster3 })
            LPCArray summonList = summoner.QueryTemp<LPCArray>("summon_list");
            if (summonList == null)
                summonList = LPCArray.Empty;

            // 从列表中移除
            summonList.Remove(target.GetRid());
            summoner.SetTemp("summon_list", LPCValue.Create(summonList));
        }

        // 怪物退场
        target.LeaveCombatMap();

        // 将召唤实体从找都列表中移除
        RoundCombatMgr.RemoveCombatEntity(target);

        // 主动解除占有关系
        target.UnOccupy();

        // 直接析构
        target.Destroy();
    }

    /// <summary>
    /// 战斗准备
    /// </summary>
    private static void WhenReadyCombat(int eventId, MixedValue para)
    {
        // 玩家不存在
        LPCMapping eventArgs = para.GetValue<LPCMapping>();

        // rid为Empty
        string rid = eventArgs.GetValue<string>("rid");
        if (string.IsNullOrEmpty(rid))
            return;

        // 角色对象不存在
        Property ob = Rid.FindObjectByRid(rid);
        if (ob == null)
            return;

        // 如果不是召唤怪物，不在这个地方处理
        string summonerRid = ob.Query<string>("summoner_rid");
        if (string.IsNullOrEmpty(summonerRid))
            return;

        // 将战斗对象添加到回合战斗驱动列表中
        RoundCombatMgr.AddCombatEntity(ob);
    }

    /// <summary>
    /// 执行占有
    /// </summary>
    /// <param name="ob">Ob.</param>
    private static void DoOccupy(Property ob)
    {
        // 获取该位置原始uniqueId
        int uniqueId = ob.CampId * 10000 +
            FormationConst.GetRawID(ob.FormationRaw) * 1000 +
            ob.FormationId * 100 +
            ob.FormationPos * 10;

        // 获取该位置原始角色
        Property originalOb = Property.FindObjectByUniqueId(uniqueId);
        if (originalOb == null)
            return;

        // 占有目标
        ob.Occupy(originalOb);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// SummonMgr初始化
    /// </summary>
    public static void InIt()
    {
        EventMgr.UnregisterEvent("CombatSummonMgr");

        // 注册死亡事件
        EventMgr.RegisterEvent("CombatSummonMgr", EventMgrEventType.EVENT_DIE, WhenDie);

        // 注册战斗准备的回调
        EventMgr.RegisterEvent("CombatSummonMgr", EventMgrEventType.EVENT_READY_COMBAT, WhenReadyCombat);
    }

    /// <summary>
    /// 召唤创建实体
    /// </summary>
    /// <param name="summoner">Summoner.</param>
    /// <param name="classId">Class identifier.</param>
    /// <param name="campId">Camp identifier.</param>
    /// <param name="sceneId">Scene identifier.</param>
    /// <param name="formationId">Formation identifier.</param>
    /// <param name="formationPos">Formation position.</param>
    /// <param name="formationRaw">Formation raw.</param>
    /// <param name="summonInfo">Summon info.</param>
    public static Property SummonEntity(Property summoner, int classId, int campId, string sceneId, int formationId, int formationPos, string formationRaw, LPCMapping summonInfo)
    {
        // 召唤者已经不存在了，不能召唤怪物
        if (summoner == null || summoner.IsDestroyed)
            return null;

        // 获取怪物信息
        CsvRow monsterInfo = MonsterMgr.MonsterCsv.FindByKey(classId);

        // 是没有配置的怪物不处理
        if (monsterInfo == null)
            return null;

        // 构建必要参数
        LPCMapping dbase = LPCMapping.Empty;
        dbase.Add("rid", Rid.New());
        dbase.Add("class_id", classId);
        dbase.Add("instance", summoner.Query("instance"));
        dbase.Add("summoner_rid", summoner.GetRid());
        dbase.Add("model", monsterInfo.Query<string>("model"));
        dbase.Add("batch", summoner.Query<int>("batch"));
        dbase.Add("direct_enter_map", 1); // 召唤怪物默认原地进场

        // 添加is_combat_actor标识
        dbase.Add("is_combat_actor", 1);

        // 添加summonInfo
        dbase.Append(summonInfo);

        // 创建一个怪物对象
        Property entity = PropertyMgr.CreateProperty(dbase);

        // 创建资源失败
        if (entity == null)
            return null;

        // 记录召唤者的召唤列表
        LPCArray summonList = summoner.QueryTemp<LPCArray>("summon_list");
        if (summonList == null)
            summonList = new LPCArray(entity.GetRid());
        else
            summonList.Add(entity.GetRid());

        // 记录summon_list数据
        summoner.SetTemp("summon_list", LPCValue.Create(summonList));

        // 设置角色站位信息
        entity.CampId = campId;
        entity.FormationId = formationId;
        entity.FormationPos = formationPos;
        entity.FormationRaw = formationRaw;

        // 执行占有操作
        DoOccupy(entity);

        // 构建进入地图策略参数
        LPCMapping tacticsArg = LPCMapping.Empty;
        tacticsArg.Add("scale", summoner.GetWorldScale());
        tacticsArg.Add("scene_id", sceneId);

        // 执行进场策略
        TacticsMgr.DoTactics(entity, TacticsConst.TACTICS_TYPE_ENTER_MAP, tacticsArg);

        // 返回实体对象
        return entity;
    }

    /// <summary>
    /// 获取召唤列表
    /// </summary>
    /// <returns>The employee list.</returns>
    /// <param name="summoner">Summoner.</param>
    public static List<Property> GetEmployeeList(Property summoner)
    {
        // 召唤对象不存在
        if (summoner == null)
            return new List<Property>();

        // 获取已经创建的怪物列表,召唤列表格式({ monster1, monster2, monster3 })
        LPCArray summonList = summoner.QueryTemp<LPCArray>("summon_list");

        // 角色没有召唤怪物
        if (summonList == null || summonList.Count == 0)
            return new List<Property>();

        // 召唤列表
        List<Property> summonEntityList = new List<Property>();

        // 遍历清除召唤实体
        foreach (LPCValue rid in summonList.Values)
        {
            Property entity = Rid.FindObjectByRid(rid.AsString);
            if (entity == null || entity.IsDestroyed)
                continue;

            // 移除自身的召唤实体
            summonEntityList.Add(entity);
        }

        // 返回召唤实体列表
        return summonEntityList;
    }

    /// <summary>
    /// 清除角色召唤实体列表
    /// </summary>
    public static void RemoveEmployeeList(Property summoner, bool isContainsStandin = true)
    {
        // 召唤对象不存在
        if (summoner == null)
            return;

        // 获取已经创建的怪物列表,召唤列表格式({ monster1, monster2, monster3 })
        LPCArray summonList = summoner.QueryTemp<LPCArray>("summon_list");

        // 角色没有召唤怪物
        if (summonList == null || summonList.Count == 0)
            return;

        // 新招换列表
        LPCArray newSummonList = LPCArray.Empty;
        string rid;

        // 遍历清除召唤实体
        do
        {
            // 角色列表为空
            if (summonList.Count == 0)
                break;

            // 获取rid
            rid = summonList[0].AsString;
            summonList.RemoveAt(0);

            // 获取角色对象
            Property entity = Rid.FindObjectByRid(rid);

            // 如果对象不存在不允许被析构
            if (entity == null || entity.IsDestroyed)
                continue;

            // 如果不包含替身，则需要保留替身
            if (! isContainsStandin && entity.Query<int>("is_standin") != 0)
            {
                newSummonList.Add(rid);
                continue;
            }

            // 移除自身的召唤实体
            RemoveEmployeeList(entity);

            // 将召唤实体从Combat列表中移除
            RoundCombatMgr.RemoveCombatEntity(entity);

            // 怪物主动退场
            entity.LeaveCombatMap();

            // 主动解除占有关系
            entity.UnOccupy();

            // 直接析构
            entity.Destroy();

        } while(true);

        // 清除数据
        if (newSummonList.Count == 0)
            summoner.DeleteTemp("summon_list");
        else
            summoner.SetTemp("summon_list", LPCValue.Create(newSummonList));
    }

    #endregion
}