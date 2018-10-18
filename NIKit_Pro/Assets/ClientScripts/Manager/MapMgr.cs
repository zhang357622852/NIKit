/// <summary>
/// MapMgr.cs
/// Created by fucj 2014-11-14
/// 地图模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LPC;

public static class MapMgr
{
    #region 变量

    // 地图配置表信息
    private static CsvFile mMapCsv = new CsvFile("map");

    // 新开地图列表
    private static LPCArray mNewMaps = LPCArray.Empty;

    // 所有开启的地图
    private static LPCArray mAllOpenMap = LPCArray.Empty;

    // 道具掉落映射表
    private static Dictionary<int, List<int>> mDropItemMap = new Dictionary<int, List<int>>();

    #endregion

    #region 属性

    // 地图配置表信息
    public static CsvFile MapCsv { get { return mMapCsv; } }

    #endregion

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
        // 载入配置表信息
        LoadMapCsv("map");

        // 注册登陆成功回调
        EventMgr.UnregisterEvent("MapMgr");
        EventMgr.RegisterEvent("MapMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);
    }


    /// <summary>
    /// 设置新开地图的操作列表
    /// </summary>
    public static void SetNewMapOper(Property user, LPCArray operList)
    {
        // 玩家对象不存在
        if (user == null)
            return;

        if (operList == null)
            operList = LPCArray.Empty;

        LPCArray list = LPCArray.Empty;
        LPCValue newMapOperList = OptionMgr.GetOption(user, "new_map_oper_list");
        if (newMapOperList != null)
            list = newMapOperList.AsArray;

        // 如果数据相同不处理
        if (list.Equals(operList))
            return;

        // 缓存到本地
        OptionMgr.SetOption(user, "new_map_oper_list", LPCValue.Create(operList));
    }

    /// <summary>
    /// 获取缓存的新开地图的操作列表
    /// </summary>
    public static LPCArray GetNewMapOper(Property user)
    {
        if (user == null)
            return LPCArray.Empty;

        // 新地图的操作列表
        LPCValue newMapOperList = OptionMgr.GetOption(user, "new_map_oper_list");
        if (newMapOperList == null)
            return LPCArray.Empty;

        // 返回Option设置信息
        return newMapOperList.AsArray;
    }

    /// <summary>
    /// 移除新开启的地图
    /// </summary>
    public static void RemoveNewMap(Property user, int mapId)
    {
        mNewMaps.Remove(mapId);

        // 已经操作过的副本列表
        LPCArray oper = LPCArray.Empty;

        for (int i = 0; i < mAllOpenMap.Count; i++)
        {
            int id = mAllOpenMap[i].AsInt;

            // 已经开启的地图，并且不是新地图
            if (mNewMaps.IndexOf(id) != -1)
                continue;

            // 已经操作的地图，不是新开地图
            oper.Add(id);
        }

        // 保存至本地已经操作的地图
        SetNewMapOper(user, oper);

        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEARANCE_DATA_UPDATE, null);
    }

    /// <summary>
    /// 是否有新开启的地图
    /// </summary>
    public static bool HasNewMap(Property user)
    {
        // 已经解锁的地图列表
        mAllOpenMap = GetUnlockedList(user);

        // 新地图的操作列表
        LPCArray operList = GetNewMapOper(user);

        bool hasNewMap = false;

        for (int i = 0; i < mAllOpenMap.Count; i++)
        {
            int mapid = mAllOpenMap[i].AsInt;

            CsvRow mapConfig = MapMgr.GetMapConfig(mapid);

            // 没有配置过的地图
            if (mapConfig == null)
                continue;

            if (!mapConfig.Query<int>("map_type").Equals(MapConst.INSTANCE_MAP_1))
                continue;

            // 过滤已经操作过的地图
            if (operList.IndexOf(mapid) != -1)
                continue;

            if (mNewMaps.IndexOf(mapid) == -1)
                mNewMaps.Add(mapid);

            hasNewMap = true;
        }

        return hasNewMap;
    }

    /// <summary>
    /// 是否是新地图
    /// </summary>
    public static bool IsNewMap(int mapID)
    {
        return mNewMaps.IndexOf(mapID) == -1 ? false : true;
    }

    /// <summary>
    /// 取得地图配置
    /// </summary>
    /// <returns>The map config.</returns>
    /// <param name="rno">Rno.</param>
    public static CsvRow GetMapConfig(int rno)
    {
        return MapCsv.FindByKey(rno);
    }

    ///<summary>
    /// 获取地图loading图
    /// </summary>
    public static string GetLoading(int rno)
    {
        // 查询配置表
        CsvRow data = MapCsv.FindByKey(rno);

        // 未配置的rno
        if (data == null)
            return string.Empty;

        // 返回loading名字
        return data.Query<string>("loading");
    }

    /// <summary>
    /// 获取某个类型解锁的最后一个地图
    /// </summary>
    public static int GetUnlockLastMapId(Property who, int type)
    {
        List<int> mapIdList = new List<int>();

        foreach (CsvRow row in MapCsv.rows)
        {
            if (row == null)
                continue;

            if (row.Query<int>("map_type") != type)
                continue;

            mapIdList.Add(row.Query<int>("rno"));
        }

        for (int i = 0; i < mapIdList.Count; i++)
        {
            if (!IsUnlocked(who, mapIdList[i]))
                return mapIdList[Math.Max(0, i - 1)];
        }

        if (mapIdList.Count == 0)
            return 1;

        return mapIdList[mapIdList.Count - 1];
    }

    /// <summary>
    /// 判断地图是否已经解锁
    /// </summary>
    public static bool IsUnlocked(Property who, int mapId)
    {
        // 对象不存在
        if (who == null)
            return false;

        // 没有配置过的地图
        CsvRow data = GetMapConfig(mapId);
        if (data == null)
            return false;

        // 地图没有解锁条件
        LPCValue unlockScript = data.Query<LPCValue>("unlock_script");
        if (unlockScript == null ||
            ! unlockScript.IsInt ||
            unlockScript.AsInt == 0)
            return true;

        // 通过脚本判断地图是否需要解锁
        return (bool) ScriptMgr.Call(unlockScript.AsInt, who, data.Query<LPCValue>("unlock_args"), mapId);
    }

    /// <summary>
    /// 获取已经解锁的地图列表
    /// </summary>
    public static LPCArray GetUnlockedList(Property user)
    {
        LPCArray maps = LPCArray.Empty;

        // 玩家对象不存在
        if (ME.user == null)
            return maps;

        // 遍历全部地图
        foreach (CsvRow data in MapCsv.rows)
        {
            int mapId = data.Query<int>("rno");

            // 判断地图是否已经解锁
            if (! IsUnlocked(user, mapId))
                continue;

            // 添加到列表中
            maps.Add(mapId);
        }

        // 返回已经解锁地图列表
        return maps;
    }

    /// <summary>
    /// 判断地图是否已经解锁
    /// </summary>
    public static int GetMapUnlockStar(Property who, int mapId)
    {
        // 对象不存在
        if (who == null)
            return 0;

        // 没有配置过的地图
        CsvRow data = GetMapConfig(mapId);
        if (data == null)
            return 0;

        // 该地图没有配置任何副本
        List<string> instanceList = InstanceMgr.GetInstanceByMapId(mapId);
        if (instanceList.Count == 0)
            return 0;

        // 不是需要匹配的难度;
        int star = 0;
        LPCMapping info = LPCMapping.Empty;

        // 获取玩家的通关数据
        foreach (string instanceId in instanceList)
        {
            //根据副本id获取副本信息;
            info = InstanceMgr.GetInstanceInfo(instanceId);

            // 该副本还没有通关过
            if (! InstanceMgr.IsClearanced(who, instanceId))
            {
                star = Mathf.Min(info.GetValue<int>("difficulty") - 1, star);
                continue;
            }

            // 通关新建等于当前难度
            star = Mathf.Max(info.GetValue<int>("difficulty"), star);
        }

        // 判断解锁条件是否满足需求
        return star;
    }

    /// <summary>
    /// 获取地下城固定地图
    /// </summary>
    public static Dictionary<int, CsvRow> GetDungeonsFixedMap(Property who)
    {
        Dictionary<int, CsvRow> map = new Dictionary<int, CsvRow>();

        // 遍历全部地图
        foreach (CsvRow data in MapCsv.rows)
        {
            // 获取地图类型
            if (! string.Equals(MapConst.DUNGEONS_MAP_2, data.Query<int>("map_type")))
                continue;

            // 地图没解锁
            if (!IsUnlocked(who, data.Query<int>("rno")))
                continue;

            // 获取解锁参数
            LPCMapping args = data.Query<LPCMapping>("unlock_args");

            // 过滤掉动态副本
            if (args.ContainsKey("is_dynamic") &&
                args.GetValue<int>("is_dynamic").Equals(1))
                continue;

            // 添加到列表中
            map.Add(data.Query<int>("rno"), data);
        }

        return map;
    }

    /// <summary>
    /// 获取动态地图
    /// </summary>
    public static LPCMapping GetDynamicMap(Property who)
    {
        if (who == null)
            return LPCMapping.Empty;

        // 获取动态地图信息
        LPCValue dynamicMap = who.Query<LPCValue>("dynamic_map");
        if (dynamicMap == null || !dynamicMap.IsMapping)
            return LPCMapping.Empty;

        return dynamicMap.AsMapping;
    }

    /// <summary>
    /// 尝试结解锁地图
    /// </summary>
    public static void TryUnlockMap(Property user, int mapId)
    {
        if (user == null)
            return;

        if (!IsUnlocked(user, mapId))
            return;

        LPCArray unlockMaps = user.QueryTemp<LPCArray>("unlock_maps");
        if (unlockMaps == null)
            unlockMaps = LPCArray.Empty;

        // 有新开地图
        if (unlockMaps.IndexOf(mapId) == -1)
        {
            unlockMaps.Add(mapId);

            user.SetTemp("unlock_maps", LPCValue.Create(unlockMaps));

            user.SetTemp("unlock_map", LPCValue.Create(mapId));
        }
    }

    /// <summary>
    /// 获取item掉落的地图列表
    /// </summary>
    /// <param name="classId">Class identifier.</param>
    public static List<int> GetItemDropMapList(int classId)
    {
        if (!mDropItemMap.ContainsKey(classId))
            return new List<int>();

        return mDropItemMap[classId];
    }

    /// <summary>
    /// 获取地图通关奖励
    /// </summary>
    public static LPCMapping GetMapClearanceBonus(int mapId, LPCMapping extraPara = null)
    {
        CsvRow row = GetMapConfig(mapId);
        if (row == null)
            return null;

        // 获取地图通关奖励
        object ret = ScriptMgr.Call(
            row.Query<int>("clearance_bonus_script"),
            row.Query<LPCMapping>("clearance_bonus_args"),
            row.Query<int>("rno"),
            extraPara
        );

        if (ret == null)
            return null;

        return ret as LPCMapping;
    }

    /// <summary>
    /// 载入地图配置表
    /// </summary>
    private static void LoadMapCsv(string fileName)
    {
        // 载入副本配置表
        mMapCsv = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (mMapCsv == null)
            return;

        // 重置数据
        mDropItemMap.Clear();

        // 遍历各行数据 
        foreach (CsvRow data in mMapCsv.rows)
        {
            if (data == null)
                continue;

            object ret = ScriptMgr.Call(
                             data.Query<int>("clearance_bonus_script"),
                             data.Query<LPCMapping>("clearance_bonus_args"),
                             data.Query<int>("rno"),
                             LPCMapping.Empty
                         );

            if (ret == null)
                continue;

            // 地图通关奖励
            LPCMapping clearanceBonus = ret as LPCMapping;

            LPCArray item = clearanceBonus.GetValue<LPCArray>("item_id");

            // 收集item
            if (item != null)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (!item[i].IsInt)
                        continue;

                    int itemId = item[i].AsInt;

                    if(!mDropItemMap.ContainsKey(itemId))
                        mDropItemMap.Add(itemId, new List<int>());

                    mDropItemMap[itemId].Add(data.Query<int>("rno"));
                }
            }
        }
    }

    /// <summary>
    /// 玩家登录回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        // 抛出事件, 副本数据更新
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEARANCE_DATA_UPDATE, null);

        LPCArray unlockMaps = ME.user.QueryTemp<LPCArray>("unlock_maps");
        if (unlockMaps == null)
            unlockMaps = LPCArray.Empty;

        foreach (CsvRow row in mMapCsv.rows)
        {
            if (row == null)
                continue;

            int mapId = row.Query<int>("rno");

            if (!IsUnlocked(ME.user, mapId))
                continue;

            if (unlockMaps.IndexOf(mapId) == -1)
                unlockMaps.Add(mapId);
        }

        ME.user.SetTemp("unlock_maps", LPCValue.Create(unlockMaps));
    }
}
