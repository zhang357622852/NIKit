/// <summary>
/// AutoCombatSelectTypeMgr.cs
/// Created by fengsc 2017/05/18
/// 自动战斗选择类型管理模块
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public static class AutoCombatSelectTypeMgr
{
    #region 变量

    // 配置表信息
    private static CsvFile mAttackTypeCsv;

    #endregion

    /// <summary>
    /// Gets the attack type csv.
    /// </summary>
    /// <value>The attack type csv.</value>
    public static CsvFile AttackTypeCsv { get { return mAttackTypeCsv; } }

    #region 外部接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入配置表
        mAttackTypeCsv = CsvFileMgr.Load("auto_combat_select_type");
    }

    /// <summary>
    /// 获取怪物类型的信息
    /// </summary>
    public static CsvRow GetSelectTypeConfig(string type)
    {
        return mAttackTypeCsv.FindByKey(type);
    }

    /// <summary>
    /// 缓存选择列表
    /// </summary>
    public static void SetSelectMap(Property who, string instanceId, LPCMapping ploy)
    {
        // 玩家对象不存在
        if (who == null)
            return;

        // 获取Option设置
        LPCValue combatPloyOption = OptionMgr.GetOption(who, "combat_ploy");
        if (combatPloyOption == null)
            return;

        // 转换数据
        LPCMapping combatPloy = combatPloyOption.AsMapping;

        // 副本配置信息
        LPCMapping instanceConfig = InstanceMgr.GetInstanceInfo(instanceId);
        if (instanceConfig == null)
            return;

        int mapId = instanceConfig.GetValue<int>("map_id");

        // 地图配置信息
        CsvRow mapConfig = MapMgr.GetMapConfig(mapId);
        if (mapConfig == null)
            return;

        if (mapConfig.Query<int>("map_type").Equals(MapConst.DUNGEONS_MAP_2))
        {
            LPCMapping atkMap = combatPloy.GetValue<LPCMapping>(mapId);
            if (atkMap == null)
                atkMap = LPCMapping.Empty;

            combatPloy.Add(mapId, ploy);
        }
        else
        {
            LPCMapping atkMap = combatPloy.GetValue<LPCMapping>(instanceId);
            if (atkMap == null)
                atkMap = LPCMapping.Empty;

            combatPloy.Add(instanceId, ploy);
        }

        OptionMgr.SetOption(who, "combat_ploy", LPCValue.Create(combatPloy));
    }

    /// <summary>
    /// 获取缓存的攻击列表
    /// </summary>
    public static LPCMapping GetSelectMap(Property who, string instanceId)
    {
        // 玩家对象不存在
        if (who == null)
            return LPCMapping.Empty;

        // 获取Option设置
        LPCValue combatPloyOption = OptionMgr.GetOption(who, "combat_ploy");
        if (combatPloyOption == null)
            return LPCMapping.Empty;

        // 转换数据格式
        LPCMapping combatPloy = combatPloyOption.AsMapping;

        // 副本配置信息
        LPCMapping instanceConfig = InstanceMgr.GetInstanceInfo(instanceId);
        if (instanceConfig == null)
            return LPCMapping.Empty;

        int mapId = instanceConfig.GetValue<int>("map_id");

        // 地图配置信息
        CsvRow mapConfig = MapMgr.GetMapConfig(mapId);
        if (mapConfig == null)
            return LPCMapping.Empty;

        if (mapConfig.Query<int>("map_type").Equals(MapConst.DUNGEONS_MAP_2))
        {
            if (!combatPloy.ContainsKey(mapId))
                return LPCMapping.Empty;

            return combatPloy.GetValue<LPCMapping>(mapId);
        }
        else
        {
            if (!combatPloy.ContainsKey(instanceId))
                return LPCMapping.Empty;

            return combatPloy.GetValue<LPCMapping>(instanceId);
        }
    }

    #endregion
}
