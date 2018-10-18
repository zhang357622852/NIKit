/// <summary>
/// AutoCombatMgr.cs
/// Created by fucj 2015-01-21
/// 自动战斗模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public static class AutoCombatMgr
{
    #region 变量

    /// <summary>
    /// 自动战斗缓存标识
    /// </summary>
    /// <value>The monster csv.</value>
    public static Dictionary<int, bool> AutoCombatMap = new Dictionary<int, bool>();

    #endregion

    #region 属性

    /// <summary>
    /// Gets the lock target ob.
    /// </summary>
    public static Property LockTargetOb{ get; private set; }

    /// <summary>
    /// 自动战斗标识（没有人为干预）
    /// </summary>
    /// <value><c>true</c> if auto combat; otherwise, <c>false</c>.</value>
    public static bool AutoCombat{ get; set; }

    #endregion

    #region 公共接口

    /// <summary>
    /// Unlocks the combat target.
    /// </summary>
    public static void UnlockCombatTarget(Property ob)
    {
        // 1. 如果锁定目标为null
        // 2. 当前锁定目标不一致
        if (ob == null || LockTargetOb != ob)
            return;

        // 清除当前锁定对象的LOCK状态
        LockTargetOb.ClearStatus("LOCK");

        // 重置LockTargetOb
        LockTargetOb = null;
    }

    /// <summary>
    /// Locks the combat target.
    /// </summary>
    public static void LockCombatTarget(Property ob)
    {
        // 如果锁定目标为null，返回不处理
        if (ob == null)
            return;

        // 在自动战斗模式下手动选择目标, 取消目标自动战斗
        AutoCombat = false;

        // 如果当前锁定目标ob相同，则表示需要清空锁定目标
        if (LockTargetOb == ob)
        {
            // 清除当前锁定对象的LOCK状态
            UnlockCombatTarget(ob);

            // 返回
            return;
        }

        // 如果宠物已经死亡
        // 或者不是CAMP_TYPE_DEFENCE不允许锁定目标
        if (ob.CheckStatus("DIED") ||
            ob.CampId != CampConst.CAMP_TYPE_DEFENCE)
            return;

        // 清除当前锁定对象的LOCK状态
        if (LockTargetOb != null)
            LockTargetOb.ClearStatus("LOCK");

        // 设置锁定对象
        LockTargetOb = ob;

        // 设置状态
        LockTargetOb.ApplyStatus("LOCK", LPCMapping.Empty);
    }

    /// <summary>
    /// 设置自动施放技能
    /// </summary>
    public static void SetAutoCombat(bool isAuto, bool isLoopFight)
    {
        // UnlockCombatTarget目标
        UnlockCombatTarget(LockTargetOb);

        // 获取当前副本信息
        LPCMapping instance = ME.user.Query<LPCMapping>("instance");

        // 没有副本信息
        if (instance == null)
            return;

        // 获取副本信息
        LPCMapping data = InstanceMgr.GetInstanceInfo(instance.GetValue<string>("id"));
        if (data == null)
            return;

        // 获取地图信息
        int mapId = data.GetValue<int>("map_id");

        // 没有配置的地图
        CsvRow mapData = MapMgr.GetMapConfig(mapId);
        if (mapData == null)
            return;

        // 添加缓存列表
        if (!AutoCombatMap.ContainsKey(mapId))
            AutoCombatMap.Add(mapId, isAuto);
        else
            AutoCombatMap[mapId] = isAuto;

        // 循环战斗不需要保存本地数据
        if (isLoopFight)
            return;

        // 获取地图不能保存自动战斗标识
        if (mapData.Query<int>("save_auto_combat") == 0)
            return;

        // 获取当前自动战斗数据, 修改数据
        LPCValue autoCombat = OptionMgr.GetOption(ME.user, "auto_combat");
        if (autoCombat == null)
            return;

        // 修改数据
        LPCMapping optionValue = autoCombat.AsMapping;
        optionValue.Add(mapId, isAuto ? 1 : 0);

        // 重置数据
        OptionMgr.SetOption(ME.user, "auto_combat", LPCValue.Create(optionValue));
    }

    /// <summary>
    /// 清除临时自动类型
    /// </summary>
    public static void RemoveTempAutoCombat()
    {
        List<int> typeList = new List<int>();

        // 遍历所有数据
        foreach (int mapId in AutoCombatMap.Keys)
        {
            // 获取地图信息,没有配置的类型
            CsvRow data = MapMgr.GetMapConfig(mapId);
            if (data == null || data.Query<int>("save_auto_combat") != 1)
            {
                typeList.Add(mapId);
                continue;
            }
        }

        // 清除数据
        foreach (int type in typeList)
            AutoCombatMap.Remove(type);
    }

    /// <summary>
    /// 根据地图id移除自动战斗的标识
    /// </summary>
    public static void RemoveAutoCombatByMapId(int mapId)
    {
        // 移除指定地图的自动战斗标识
        if (AutoCombatMap.ContainsKey(mapId))
            AutoCombatMap.Remove(mapId);
    }

    /// <summary>
    /// 是否是自动战斗
    /// </summary>
    public static bool IsAutoCombat()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return false;

        // 获取当前副本信息
        LPCMapping instance = ME.user.Query<LPCMapping>("instance");

        // 没有副本信息
        if (instance == null)
            return false;

        // 获取副本信息
        LPCMapping data = InstanceMgr.GetInstanceInfo(instance.GetValue<string>("id"));
        if (data == null)
            return false;

        // 返回是否是自动战斗
        return IsAutoCombat(data.GetValue<int>("map_id"));
    }

    /// <summary>
    /// 是否是自动战斗
    /// </summary>
    public static bool IsAutoCombat(int mapId)
    {
        // 获取地图信息
        CsvRow mapData = MapMgr.GetMapConfig(mapId);
        if (mapData == null)
            return false;

        // 如果在缓存列表中
        if (AutoCombatMap.ContainsKey(mapId))
            return AutoCombatMap[mapId];

        // 获取当前自动战斗数据
        LPCValue autoCombat = OptionMgr.GetOption(ME.user, "auto_combat");
        if (autoCombat == null)
            return false;

        // 获取存档值
        LPCMapping optionValue = autoCombat.AsMapping;
        bool isAuto = (optionValue.GetValue<int>(mapId) == 1);

        // 缓存数据
        AutoCombatMap.Add(mapId, isAuto);

        // 返回是否是自动战斗
        return isAuto;
    }

    #endregion
}
