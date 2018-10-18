/// <summary>
/// FormationMgr.cs
/// create by zhaozy 2016-05-05
/// 阵型管理模块
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using LPC;

/// <summary>
/// 阵型管理模块
/// </summary>
public class FormationMgr
{
    #region 变量

    /// <summary>
    /// 阵型配置信息列表
    /// </summary>
    private static Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, CsvRow>>>> mFormationMap =
        new Dictionary<string, Dictionary<string, Dictionary<int, Dictionary<int, CsvRow>>>>();

    #endregion

    #region 公共接口

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
        // 载入阵型配置表
        LoadFormationFile("formation");
    }

    /// <summary>
    /// 载入阵型配置表
    /// </summary>
    private static void LoadFormationFile(string fileName)
    {
        mFormationMap.Clear();

        // 载入阵型配置表
        CsvFile fileData = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (fileData == null)
            return;

        int posId = 0;
        int formationId = 0;
        string row = string.Empty;
        string sceneId = string.Empty;

        // 遍历各行数据
        foreach (CsvRow data in fileData.rows)
        {
            sceneId = data.Query<string>("scene_id");
            if (!mFormationMap.ContainsKey(sceneId))
                mFormationMap.Add(sceneId, new Dictionary<string, Dictionary<int, Dictionary<int, CsvRow>>>());

            // 获取row标识
            row = data.Query<string>("row");
            if (!mFormationMap[sceneId].ContainsKey(row))
                mFormationMap[sceneId].Add(row, new Dictionary<int, Dictionary<int, CsvRow>>());

            // 添加数据
            formationId = data.Query<int>("formation_id");
            if (!mFormationMap[sceneId][row].ContainsKey(formationId))
                mFormationMap[sceneId][row].Add(formationId, new Dictionary<int, CsvRow>());

            // 添加位置信息
            posId = data.Query<int>("pos_id");
            mFormationMap[sceneId][row][formationId].Add(posId, data);
        }
    }

    /// <summary>
    /// 获取阵型指定位置配置信息
    /// </summary>
    public static CsvRow GetFormationPosData(string sceneId, string raw, int posId, int formationId)
    {
        // 如果没有配置则默认场景信息
        if (!mFormationMap.ContainsKey(sceneId))
            sceneId = "default";

        // 没有配置该阵型信息
        if (! mFormationMap.ContainsKey(sceneId) &&
            ! mFormationMap[sceneId].ContainsKey(raw) &&
            ! mFormationMap[sceneId][raw].ContainsKey(formationId))
            return null;

        // 返回配置信息
        return mFormationMap[sceneId][raw][formationId][posId];
    }

    /// <summary>
    /// 获取阵型配置信息
    /// </summary>
    public static List<Property> GetArchiveFormation(string instanceId)
    {
        // 获取副本配置信息
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return new List<Property>();

        // 地图id
        int mapId = instanceInfo.GetValue<int>("map_id");

        CsvRow mapConfig = MapMgr.GetMapConfig(mapId);
        if (mapConfig == null)
            return new List<Property>();

        // 地图类型
        int mapType = mapConfig.Query<int>("map_type");

        // 副本最大出战宠物数量
        int maxPetAmount = instanceInfo.GetValue<int>("max_pet_amount");


        // 获取当前自动战斗数据, 修改数据
        LPCValue formationOption = OptionMgr.GetOption(ME.user, "formation");
        if (formationOption == null)
            return new List<Property>();

        // 转换数据格式
        LPCMapping optionValue = formationOption.AsMapping;

        LPCArray petRidList = LPCArray.Empty;

        if (mapType.Equals(MapConst.INSTANCE_MAP_1))
        {
            petRidList = optionValue.GetValue<LPCArray>(maxPetAmount);
        }
        else
        {
            petRidList = optionValue.GetValue<LPCArray>(mapId);
        }

        // 没有缓存阵型数据
        if (petRidList == null || petRidList.Count == 0)
            return new List<Property>();

        // 遍历数据
        List<Property> petList = new List<Property>();
        string petRid;
        for (int i = 0; i < petRidList.Count; i++)
        {
            // 获取宠物rid
            petRid = petRidList[i].AsString;

            // 如果该位置没有宠物
            if (string.IsNullOrEmpty(petRid))
            {
                petList.Add(null);
                continue;
            }

            // 添加宠物数据
            petList.Add(Rid.FindObjectByRid(petRid));
        }

        // 返回数据
        return petList;
    }

    /// <summary>
    /// 保存阵型配置信息
    /// </summary>
    public static void SetArchiveFormation(string instanceId, List<Property> petList)
    {
        // 获取副本配置信息
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return;

        // 地图id
        int mapId = instanceInfo.GetValue<int>("map_id");

        CsvRow mapConfig = MapMgr.GetMapConfig(mapId);
        if (mapConfig == null)
            return;

        // 地图类型
        int mapType = mapConfig.Query<int>("map_type");

        // 副本最大出战宠物数量
        int maxPetAmount = instanceInfo.GetValue<int>("max_pet_amount");

        // 获取当前自动战斗数据, 修改数据
        LPCValue formationOption = OptionMgr.GetOption(ME.user, "formation");
        if (formationOption == null)
            return;

        // 转换数据格式
        LPCMapping optionValue = formationOption.AsMapping;
        LPCArray petRidList = LPCArray.Empty;

        // 遍历数据
        foreach (Property petOb in petList)
        {
            // 如果该位置没有宠物
            if (petOb == null)
            {
                petRidList.Add(string.Empty);
                continue;
            }

            // 添加宠物数据
            petRidList.Add(petOb.GetRid());
        }

        if (mapType.Equals(MapConst.INSTANCE_MAP_1))
        {
            if (petRidList.Equals(optionValue.GetValue<LPCArray>(maxPetAmount)))
                return;

            optionValue.Add(maxPetAmount, petRidList);
        }
        else
        {
            if (petRidList.Equals(optionValue.GetValue<LPCArray>(mapId)))
                return;

            optionValue.Add(mapId, petRidList);
        }

        // 记录数据
        optionValue.Add(instanceId, petRidList);

        // 记录存档数据
        OptionMgr.SetOption(ME.user, "formation", LPCValue.Create(optionValue));
    }

    #endregion
}

