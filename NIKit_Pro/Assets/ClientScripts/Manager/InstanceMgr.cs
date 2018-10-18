/// <summary>
/// InstanceMgr.cs
/// Create by zhaozy 2014-11-12
/// 副本管理模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System;
using LPC;
using UnityEngine.SceneManagement;

/// 装备管理器
public static class InstanceMgr
{
    #region 变量

    public static LPCMapping InstanceConfig{ get { return mInstance; } }

    // 配置表信息
    private static LPCMapping mInstance = new LPCMapping();
    private static LPCMapping mInstanceEvent = new LPCMapping();
    private static Dictionary<int, List<string>> mMapInstance = new Dictionary<int, List<string>>();
    private static Dictionary<string, string> mPreInstance = new Dictionary<string, string>();
    private static Dictionary<string, List<CsvRow>> mInstanceFormation = new Dictionary<string, List<CsvRow>>();

    // 副本场景列表
    private static Dictionary<string, List<KeyValuePair<string, bool>>> mInstancePreloadList = new Dictionary<string, List<KeyValuePair<string, bool>>>();
    private static Dictionary<string, List<string>> mInstanceSceneList = new Dictionary<string, List<string>>();

    // 副本资源
    private static Dictionary<string, List<CsvRow>> mInstanceResource = new Dictionary<string, List<CsvRow>>();

    // 副本掉落列表
    private static LPCMapping DropBounsMap = new LPCMapping();

    private static Dictionary<string, bool> mLoopFights = new Dictionary<string, bool>();

    // 精英副本配置表
    private static Dictionary<string, LPCMapping> mPetDungeonAttrib = new Dictionary<string, LPCMapping>();

    #endregion

    #region 属性

    // 是否已经提示过队长位置没有队长技能
    public static bool IsTipsLeaderSkill { get; set;}

    #endregion

    #region 内部接口

    /// <summary>
    /// 载入副本资源配置表
    /// </summary>
    private static void LoadInstance(string fileName)
    {
        // 载入副本配置表
        CsvFile instances = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (instances == null)
            return;

        // 重置数据
        mInstance = LPCMapping.Empty;
        mMapInstance.Clear();
        mPreInstance.Clear();

        // 遍历各行数据
        foreach (CsvRow data in instances.rows)
        {
            // 转换数据格式为LPCMapping数据格式
            LPCMapping dataMap = data.ConvertLpcMap();

            // 获取副本id
            string instanceId = dataMap.GetValue<string>("instance_id");
            int mapId = dataMap.GetValue<int>("map_id");
            string nextInstanceId = dataMap.GetValue<string>("next_instance_id");

            // 记录数据
            mInstance.Add(instanceId, dataMap);

            // 记录副本所属地图信息
            if (!mMapInstance.ContainsKey(mapId))
                mMapInstance.Add(mapId, new List<string>());

            mMapInstance[mapId].Add(instanceId);

            // 记录副本下一关副本ID
            if (!string.IsNullOrEmpty(nextInstanceId))
                mPreInstance.Add(nextInstanceId, instanceId);
        }
    }

    /// <summary>
    /// 加载副本资源配置表
    /// </summary>
    private static void LoadInstanceResource(string fileName)
    {
        // 载入副本配置表
        CsvFile resources = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (resources == null)
            return;

        // 重置副本资源
        mInstanceResource.Clear();

        string instanceId = string.Empty;

        // 遍历各行数据
        foreach (CsvRow data in resources.rows)
        {
            // 获取副本id
            instanceId = data.Query<string>("instance_id");

            // 批次资源信息初始化
            if (mInstanceResource.ContainsKey(instanceId))
                mInstanceResource[instanceId].Add(data);
            else
                mInstanceResource.Add(instanceId, new List<CsvRow>(){ data });
        }
    }

    /// <summary>
    /// 载入副本事件配置表
    /// </summary>
    private static void LoadIntsnaceEvent(string fileName)
    {
        // 载入副本配置表
        CsvFile events = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (events == null)
            return;

        // 重置全部数据
        mInstancePreloadList.Clear();
        mInstanceSceneList.Clear();
        mInstanceEvent = LPCMapping.Empty;
        IsTipsLeaderSkill = false;

        // 遍历各行数据
        foreach (CsvRow data in events.rows)
        {
            // 转换数据格式为LPC mapping数据格式
            LPCMapping dataMap = data.ConvertLpcMap();

            // 获取副本id
            string instanceId = dataMap["instance_id"].AsString;
            int stepId = dataMap["step"].AsInt;

            // 统计副本场景
            if (InstanceConst.INSTANCE_ENTER_MAP == dataMap["event"].AsInt)
            {
                LPCMapping eventArgs = dataMap["event_arg"].AsMapping;
                string sceneId = eventArgs.GetValue<string>("scene_id");
                if (! string.IsNullOrEmpty(sceneId))
                {
                    // 添加副本场景列表中
                    if (! mInstancePreloadList.ContainsKey(instanceId))
                        mInstanceSceneList.Add(instanceId, new List<string>());
                    mInstanceSceneList[instanceId].Add(sceneId);

                    // 添加预加载列表中
                    if (!mInstancePreloadList.ContainsKey(instanceId))
                        mInstancePreloadList.Add(instanceId, new List<KeyValuePair<string, bool>>());
                    mInstancePreloadList[instanceId].Add(
                        new KeyValuePair<string, bool>(string.Format("Assets/Prefabs/Scene/{0}.prefab", sceneId), false));
                }
            }

            // 资源批次数据
            LPCValue stepData = LPCValue.CreateMapping();
            if (mInstanceEvent.ContainsKey(instanceId))
                stepData.AsMapping = mInstanceEvent[instanceId].AsMapping;

            // 获取事件列表
            LPCValue eventList = LPCValue.CreateArray();

            if (stepData.AsMapping.ContainsKey(stepId))
                eventList.AsArray = stepData.AsMapping[stepId].AsArray;

            // 记录数据
            eventList.AsArray.Add(dataMap);
            stepData.AsMapping.Add(stepId, eventList);
            mInstanceEvent.Add(instanceId, stepData);
        }
    }

    /// <summary>
    /// 载入副本阵容配置表
    /// </summary>
    private static void LoadInstanceFormation(string fileName)
    {
        // 载入副本配置表
        CsvFile events = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (events == null)
            return;

        // 清除数据
        mInstanceFormation.Clear();

        // 遍历各行数据
        foreach (CsvRow data in events.rows)
        {
            // 获取副本id
            string instanceId = data.Query<string>("instance_id");

            // 批次资源信息初始化
            if (mInstanceFormation.ContainsKey(instanceId))
                mInstanceFormation[instanceId].Add(data);
            else
                mInstanceFormation.Add(instanceId, new List<CsvRow>(){ data });
        }
    }

    /// <summary>
    /// 载入精英地下城属性配置表
    /// </summary>
    /// <param name="fileName">File name.</param>
    private static void LoadPetDungeonAttrib(string fileName)
    {
        // 载入副本配置表
        CsvFile events = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (events == null)
            return;

        // 清除数据
        mPetDungeonAttrib.Clear();

        LPCMapping dataMap;
        string keyId;

        // 遍历各行数据
        foreach (CsvRow data in events.rows)
        {
            // 转换数据格式
            dataMap = data.ConvertLpcMap();

            // 构建数据存储id
            keyId = string.Format("{0}_{1}_{2}_{3}",
                data.Query<int>("pet_id"),
                data.Query<string>("instance_id"),
                data.Query<int>("batch"),
                data.Query<int>("pos")
            );

            // 删除部分信息
            dataMap.Remove("pet_id");
            dataMap.Remove("instance_id");
            dataMap.Remove("batch");
            dataMap.Remove("pos");

            // 记录数据
            mPetDungeonAttrib[keyId] = dataMap;
        }
    }

    /// <summary>
    /// Gets the pet dungeon attrib.
    /// </summary>
    /// <returns>The pet dungeon attrib.</returns>
    /// <param name="petId">Pet identifier.</param>
    /// <param name="instanceId">Instance identifier.</param>
    /// <param name="batch">Batch.</param>
    /// <param name="pos">Position.</param>
    public static LPCMapping GetPetDungeonAttrib(int petId, string instanceId, int batch, int pos)
    {
        // 构建数据存储id
        string keyId = string.Format("{0}_{1}_{2}_{3}", petId, instanceId, batch, pos);

        // 查询配置信息
        LPCMapping data;
        if (!mPetDungeonAttrib.TryGetValue(keyId, out data))
            return LPCMapping.Empty;

        // 返回配置信息
        return data;
    }

    /// <summary>
    /// 副本通关回调
    /// </summary>
    private static void WhenInstanceClearance(int eventId, MixedValue para)
    {
        // 获取副本通关参数
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null)
            return;

        // 获取副本通关结果
        int result = data.GetValue<int>("result");

        // 播放副本对应地图相应的场景背景音效
        GameSoundMgr.PlayGroupSound(result == 1 ? "success" : "fail");
    }

    /// <summary>
    /// 角色移动到达目标点
    /// </summary>
    private static void WhenMoveArrived(int eventId, MixedValue para)
    {
        // 玩家不存在
        LPCMapping paraMap = para.GetValue<LPCMapping>();

        // 不是进入地图不处理
        if (paraMap.GetValue<int>("enter_map") != 1)
            return;

        // rid为Empty
        string rid = paraMap.GetValue<string>("rid");
        if (string.IsNullOrEmpty(rid))
            return;

        // 玩家不存在
        Property charOb = Rid.FindObjectByRid(rid);
        if (charOb == null)
            return;

        // 需要出场动作和出场光效等参数
        LPCValue appearAction = charOb.Query("appear_action");
        if (appearAction != null)
        {
            string actionName = string.Empty;
            if (appearAction.IsInt)
                actionName = ScriptMgr.Call(appearAction.AsInt, charOb) as string;
            else if (appearAction.IsString)
                actionName = appearAction.AsString;

            // 有出场动作，则播放actionName
            if (!string.IsNullOrEmpty(actionName))
            {
                // 构建参数
                LPCMapping skillPara = new LPCMapping();
                skillPara.Add("action", actionName);
                skillPara.Add("rid", charOb.GetRid());

                // 通知在战斗系统出场技能
                charOb.Actor.DoActionSet(actionName, Game.NewCookie(charOb.GetRid()), skillPara);

                // 需要等到进场动画播放完成后再抛出进程完成事件
                return;
            }
        }

        // 抛出实体进场完成
        LPCMapping eventArgs = new LPCMapping();
        eventArgs.Add("rid", charOb.GetRid());
        EventMgr.FireEvent(EventMgrEventType.EVENT_READY_COMBAT, MixedValue.NewMixedValue<LPCMapping>(eventArgs), true, true);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入副本配置表
        LoadInstance("instance");

        // 载入副本资源配置表
        LoadInstanceResource("instance_resource");

        // 载入副本事件配置表
        LoadIntsnaceEvent("instance_event");

        // 载入副本阵容配置表
        LoadInstanceFormation("instance_formation");

        // 载入副本阵容配置表
        LoadPetDungeonAttrib("pet_dungeon_attrib");

        // 注销回调
        EventMgr.UnregisterEvent("InstanceMgr");

        // 注册到达目标点的回调
        EventMgr.RegisterEvent("InstanceMgr", EventMgrEventType.EVENT_MOVE_ARRIVED, WhenMoveArrived);

        //注册副本通关事件;
        EventMgr.RegisterEvent("InstanceMgr", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, WhenInstanceClearance);
    }

    /// <summary>
    /// 获取副本预加载列表
    /// </summary>
    public static List<KeyValuePair<string, bool>> GetPreloadList(string instanceId)
    {
        // 如果配置表中没有配置则返回空列表
        if (! mInstancePreloadList.ContainsKey(instanceId))
            return new List<KeyValuePair<string, bool>>();

        // 返回配置信息
        return mInstancePreloadList[instanceId];
    }

    /// <summary>
    /// 获取副本场景列表
    /// </summary>
    public static List<string> GetScenes(string instanceId)
    {
        // 如果配置表中没有配置则返回空列表
        if (! mInstanceSceneList.ContainsKey(instanceId))
            return new List<string>();

        // 返回配置信息
        return mInstanceSceneList[instanceId];
    }

    /// <summary>
    /// 获取副本名称
    /// </summary>
    public static string GetInstanceName(string instanceId, LPCMapping para)
    {
        // 没有数据返回string.Empty
        if (! mInstance.ContainsKey(instanceId))
            return string.Empty;

        // 获取副本配置信息
        LPCMapping data = mInstance[instanceId].AsMapping;

        // 获取name配置信息
        LPCValue name = data.GetValue<LPCValue>("name");

        // 如果直接是配置的名称
        if (name.IsInt)
            return (string) ScriptMgr.Call(name.AsInt, instanceId, para);

        // 返回副本配置信息
        return LocalizationMgr.Get(name.AsString);
    }

    /// <summary>
    /// 获取副本配置信息
    /// </summary>
    public static LPCMapping GetInstanceInfo(string instanceId)
    {
        // 没有数据返回null
        if (!mInstance.ContainsKey(instanceId))
            return null;

        // 返回副本配置信息
        return mInstance[instanceId].AsMapping;
    }

    /// <summary>
    /// 获取所有副本配置信息
    /// </summary>
    public static LPCMapping GetAllInstanceInfo()
    {
        // 返回副本配置信息
        return mInstance;
    }

    /// <summary>
    /// 返回副本最大出战宠物数量
    /// </summary>
    public static int GetMaxCombatPetAmount(string instanceId)
    {
        // 没有数据返回null
        if (!mInstance.ContainsKey(instanceId))
            return 0;

        // 返回副本配置信息
        return mInstance[instanceId].AsMapping.GetValue<int>("max_pet_amount");
    }

    /// <summary>
    /// 获取进入副本开销
    /// </summary>
    public static LPCMapping GetInstanceCostMap(Property user, string instanceId, LPCMapping dynamicPara = null)
    {
        // 没有数据返回null
        if (!mInstance.ContainsKey(instanceId))
            return LPCMapping.Empty;

        LPCMapping instanceInfo = mInstance[instanceId].AsMapping;

        // 免费消耗参数
        LPCArray freeCostArgs = instanceInfo.GetValue<LPCArray>("free_activity");

        // 副本消耗计算参数
        LPCMapping costMapArgs = instanceInfo.GetValue<LPCMapping>("cost_map_args");

        // 脚本编号
        LPCValue scriptNo = instanceInfo.GetValue<LPCValue>("cost_map_script");
        if (scriptNo == null || !scriptNo.IsInt)
            return costMapArgs;

        // 返回计算结果
        return ScriptMgr.Call(scriptNo.AsInt, user, instanceId, costMapArgs, freeCostArgs, dynamicPara) as LPCMapping;
    }

    /// <summary>
    /// 获取副本复活消耗
    /// </summary>
    public static LPCMapping GetInstanceReviveCost(string instanceId)
    {
        // 没有数据返回null
        if (!mInstance.ContainsKey(instanceId))
            return LPCMapping.Empty;

        // 返回副本配置信息
        return mInstance[instanceId].AsMapping.GetValue<LPCMapping>("revive_cost");
    }

    /// <summary>
    /// 根据地图id获取副本列表
    /// </summary>
    public static List<string> GetInstanceByMapId(int mapId)
    {
        // 没有配置的数据
        if (!mMapInstance.ContainsKey(mapId))
            return new List<string>();

        // 返回配置信息
        return mMapInstance[mapId];
    }

    /// <summary>
    /// 根据地图id获取某个难度的副本
    /// </summary>
    public static List<string> GetDifficultyInstanceByMapId(int mapId, int diffculty)
    {
        // 没有配置的数据
        if (!mMapInstance.ContainsKey(mapId))
            return new List<string>();

        List<string> list = new List<string>();

        for (int i = 0; i < mMapInstance[mapId].Count; i++)
        {
            // 获取副本配置信息
            LPCMapping data = InstanceMgr.GetInstanceInfo(mMapInstance[mapId][i]);

            if (data == null || data.Count == 0)
                continue;

            if (!data.GetValue<int>("difficulty").Equals(diffculty))
                continue;

            list.Add(mMapInstance[mapId][i]);
        }

        return list;
    }

    /// <summary>
    /// 获取指定副本的前置副本
    /// </summary>
    public static string GetPreInstanceId(string instanceId)
    {
        // 没有配置的数据
        if (!mPreInstance.ContainsKey(instanceId))
            return string.Empty;

        // 返回配置信息
        return mPreInstance[instanceId];
    }

    /// <summary>
    /// 获取副本资源列表
    /// </summary>
    public static int GetInstanceMapType(string instanceId)
    {
        // 获取副本配置信息
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return MapConst.INVALID_MAP;

        // 获取该副本对应的地图配置
        CsvRow mapConfig = MapMgr.GetMapConfig(instanceInfo.GetValue<int>("map_id"));
        if (mapConfig == null)
            return MapConst.INVALID_MAP;

        // 获取地图类型
        return mapConfig.Query<int>("map_type");
    }

    /// <summary>
    /// 获取副本资源列表
    /// </summary>
    public static List<CsvRow> GetInstanceResources(string instanceId)
    {
        // 没有数据返回null
        if (!mInstanceResource.ContainsKey(instanceId))
            return null;

        // 获取副本资源数据
        return mInstanceResource[instanceId];
    }

    /// <summary>
    /// 获取副本事件列表
    /// </summary>
    public static LPCArray GetInstanceEvents(string instanceId, int stepId)
    {
        // 没有数据返回null
        if (!mInstanceEvent.ContainsKey(instanceId))
            return null;

        // 获取阶段事件数据
        LPCMapping data = mInstanceEvent[instanceId].AsMapping;

        // 没有数据返回null
        if (!data.ContainsKey(stepId))
            return null;

        // 返回事件列表
        return data[stepId].AsArray;
    }

    ///<summary>
    /// 获取显示的副本阵容
    /// </summary>
    public static List<CsvRow> GetInstanceFormation(string instanceId)
    {
        if (!mInstanceFormation.ContainsKey(instanceId))
            return null;

        // 获取副本资源数据
        return mInstanceFormation[instanceId];
    }

    /// <summary>
    /// 创建战斗验证副本
    /// </summary>
    public static AuthInstance DoCreateAuthInstance(string instanceId, LPCMapping dbase)
    {
        // 获取副本信息
        LPCMapping instance = GetInstanceInfo(instanceId);
        if (instance == null)
            return null;

        // 构建副本对象
        AuthInstance instanceOb = new AuthInstance(PropertyMgr.ConvertDbase(dbase));

        // 副本对象创建成功
        if (instanceOb == null)
            return null;

        // 返回创建的副本对象
        return instanceOb;
    }

    /// <summary>
    /// Dos the create instance.
    /// </summary>
    public static PlaybackInstance DoCreatePlaybackInstance(Property who, string instanceId, LPCMapping dbase)
    {
        // 玩家对象不存在或者正在析构中
        if (who == null || who.IsDestroyed)
            return null;

        // 获取副本信息
        LPCMapping instance = GetInstanceInfo(instanceId);
        if (instance == null)
            return null;

        // 构建副本对象
        PlaybackInstance instanceOb = new PlaybackInstance(PropertyMgr.ConvertDbase(dbase));

        // 副本对象创建成功
        if (instanceOb == null)
            return null;

        // 记录玩家当前正在进行中的副本
        LPCMapping data = LPCMapping.Empty;
        data.Add("rid", instanceOb.GetRid());
        data.Add("id", instanceId);

        // 标记当前副本是战斗回放
        data.Add("playback", 1);

        who.Set("instance", LPCValue.Create(data));

        // 返回创建的副本对象
        return instanceOb;
    }

    /// <summary>
    /// 执行副本回放
    /// </summary>
    public static void DoPlaybackInstance(Property who, string instanceId, LPCMapping dbase)
    {
        // 先结束当前正在进行中的副本
        if (IsInInstance(who))
            LeaveInstance(who);

        // 在副本进行中不需要回收资源
        ResourceMgr.AutoRecycle = false;

        // 在进入副本前主动回收一下资源
        ResourceMgr.Recycle(true);

        // 创建副本对象
        PlaybackInstance instanceOb = DoCreatePlaybackInstance(who, instanceId, dbase);

        // 创建副本失败
        if (instanceOb == null)
            return;

        // 副本开始
        instanceOb.DoStart();

        // 记录副本奖励信息
        DropBounsMap = LPCMapping.Empty;
        BonusMgr.ResetBonus(DropBounsMap);

        // 初始化掉落奖励
        DropEffectMgr.Init();
    }

    /// <summary>
    /// 打开副本
    /// 目前只是打开界面load而已，正常创建副本在DoEnterInstance处理
    /// </summary>
    public static void OpenInstance(Property who, string instanceId, string rid)
    {
        // 构建参数
        LPCMapping para = LPCMapping.Empty;
        para.Add("instance_id", instanceId);
        para.Add("rid", rid);

        // 打开副本load界面
        LoadingMgr.ShowLoadingWnd("LoadingWnd", LoadingType.LOAD_TYPE_INSTANCE, para);
    }

    /// <summary>
    /// 进入副本
    /// </summary>
    public static void DoEnterInstance(Property who, string instanceId, string rid, int randomSeed, LPCMapping fighterMap, LPCMapping defenders, LPCMapping DropBonus, LPCMapping extraPara)
    {
        // 先结束当前正在进行中的副本
        if (IsInInstance(who))
            LeaveInstance(who);

        // 在副本进行中不需要回收资源
        ResourceMgr.AutoRecycle = false;

        // 在进入副本前主动回收一下资源
        ResourceMgr.Recycle(true);

        // 创建副本对象
        LPCMapping dbase = LPCMapping.Empty;
        dbase.Add("rid", rid);
        dbase.Add("instance_id", instanceId);
        dbase.Add("random_seed", randomSeed);
        dbase.Add("fighter_map", fighterMap);
        dbase.Add("defenders", defenders);
        dbase.Add("level_actions", LPCMapping.Empty);
        dbase.Append(extraPara);

        // 如果是通天塔副本需要增加通天塔难度和层级相关数据
        CsvRow data = TowerMgr.GetTowerInfoByInstance(instanceId);
        if (data != null)
        {
            dbase.Add("difficulty", data.Query<int>("difficulty"));
            dbase.Add("layer", data.Query<int>("layer"));
        }

        // 创建副本对象
        Instance instanceOb = DoCreateInstance(who, instanceId, dbase);

        // 创建副本失败
        if (instanceOb == null)
            return;

        // 副本开始
        instanceOb.DoStart();

        // 记录副本奖励信息
        DropBounsMap = DropBonus;
        BonusMgr.ResetBonus(DropBonus);

        // 初始化掉落奖励
        DropEffectMgr.Init();
    }

    /// <summary>
    /// 获取攻方列表
    /// </summary>
    public static List<Property> GetFightList()
    {
        // 不在副本中
        if (!IsInInstance(ME.user))
            return new List<Property>();

        // 获取副本rid
        string instanceRid = ME.user.Query<string>("instance/rid");
        Property instanceOb = Rid.FindObjectByRid(instanceRid);

        // 通知副本对象结束
        if (instanceOb == null)
            return new List<Property>();

        // 返回副本中的攻方列表
        return (instanceOb as Instance).FighterList;
    }

    /// <summary>
    /// 获取掉落奖励列表
    /// </summary>
    public static LPCMapping GetDropBonusMap()
    {
        if (DropBounsMap == null)
            return LPCMapping.Empty;

        return DropBounsMap;
    }

    /// <summary>
    /// Dos the create instance.
    /// </summary>
    public static Instance DoCreateInstance(Property who, string instanceId, LPCMapping dbase)
    {
        // 玩家对象不存在或者正在析构中
        if (who == null || who.IsDestroyed)
            return null;

        // 获取副本信息
        LPCMapping instance = GetInstanceInfo(instanceId);
        if (instance == null)
            return null;

        // 构建副本对象
        Instance instanceOb = new Instance(PropertyMgr.ConvertDbase(dbase));

        // 副本对象创建成功
        if (instanceOb == null)
            return null;

        // 记录玩家当前正在进行中的副本
        LPCMapping data = LPCMapping.Empty;
        data.Add("rid", instanceOb.GetRid());
        data.Add("id", instanceId);

        // 添加宠物pet_id
        if (dbase.ContainsKey("pet_id"))
            data.Add("pet_id", dbase["pet_id"]);

        // 构建dynamic_map数据
        if (dbase.ContainsKey("dynamic_map"))
            data.Add("dynamic_map", dbase["dynamic_map"]);

        // 记录当前副本数据
        who.Set("instance", LPCValue.Create(data));

        // 返回创建的副本对象
        return instanceOb;
    }

    /// <summary>
    /// 开始副本
    /// </summary>
    public static bool EnterInstance(Property who, string instanceId, string leaderRid, bool isLoopFight, LPCMapping formationMap, LPCMapping extraPara)
    {
        // 不是user不能走该接口
        if (!(who is User))
            return false;

        // 能否开启副本
        if (!CanEnterInstance(who, instanceId, extraPara))
            return false;

        // 向服务器请求进入副本
        // 阵型暂时写死，默认1
        Operation.CmdEnterInstance.Go(instanceId, leaderRid, formationMap, extraPara);

        // 设置循环战斗标识
        SetLoopFight(instanceId, isLoopFight);

        // 返回成功
        return true;
    }

    /// <summary>
    /// 获取副本回合总数
    /// </summary>
    public static int GetRoundCount(Property user)
    {
        // 不在副本中
        if (! IsInInstance(user))
            return 0;

        // 获取副本rid
        string instanceRid = user.Query<string>("instance/rid");
        Property instanceOb = Rid.FindObjectByRid(instanceRid);

        // 通知副本对象结束
        if (instanceOb == null)
            return 0;

        // 返回该副本回合总数
        return (instanceOb as InstanceBase).RoundCount;
    }

    /// <summary>
    /// 增加副本回合总数
    /// </summary>
    public static void AddRoundCount(Property user, int times = 1)
    {
        // 不在副本中
        if (! IsInInstance(user))
            return;

        // 获取副本rid
        string instanceRid = user.Query<string>("instance/rid");
        Property instanceOb = Rid.FindObjectByRid(instanceRid);

        // 通知副本对象结束
        if (instanceOb == null)
            return;

        // 返回该副本回合总数
        (instanceOb as InstanceBase).AddRoundCount(times);
    }

    /// <summary>
    /// 获取副本玩家复活次数
    /// </summary>
    public static int GetReviveTimes(Property user)
    {
        // 不在副本中
        if (!IsInInstance(user))
            return 0;

        // 获取副本rid
        string instanceRid = user.Query<string>("instance/rid");
        Property instanceOb = Rid.FindObjectByRid(instanceRid);

        // 通知副本对象结束
        if (instanceOb == null)
            return 0;

        // 返回该副本复活次数
        return (instanceOb as InstanceBase).ReviveTimes;
    }

    /// <summary>
    /// 获取npc副本
    /// </summary>
    /// <returns>The npc instance list.</returns>
    /// <param name="user">User.</param>
    public static List<string> GetNpcInstanceList(Property user)
    {
        // 获取配置信息
        List<string> instanceList = GetInstanceByMapId(MapConst.ARENA_NPC_MAP_ID);

        // 获取玩家当前等级
        int userLevel = user.Query<int>("level");
        Dictionary<int, string> flag2map = new Dictionary<int, string>();
        List<int> flagList = new List<int>();
        int flag;

        // 遍历抽取列表
        foreach (string id in instanceList)
        {
            // 获取配置信息
            LPCMapping data = GetInstanceInfo(id);

            // 如果指引副本
            if (string.Equals(id, "arena_npc_guide"))
                continue;

            // 判断是否可以抽取
            LPCMapping show_script_args = data.GetValue<LPCMapping>("show_script_args");
            if (userLevel < show_script_args.GetValue<int>("min_level") ||
                userLevel > show_script_args.GetValue<int>("max_level"))
                continue;

            // 获取副本flag
            flag = data.GetValue<int>("flag");

            // 添加到列表中
            flagList.Add(flag);
            flag2map.Add(flag, id);
        }

        // 按照flagList排序
        flagList.Sort();

        // 排序后重新生成finalInstance
        List<string> finalInstance = new List<string>();
        foreach(int flagId in flagList)
            finalInstance.Add(flag2map[flagId]);

        // 返回最终列表
        return finalInstance;
    }

    /// <summary>
    /// 增加副本玩家复活次数
    /// </summary>
    public static void AddReviveTimes(Property user, int times = 1)
    {
        // 不在副本中
        if (!IsInInstance(user))
            return;

        // 获取副本rid
        string instanceRid = user.Query<string>("instance/rid");
        Property instanceOb = Rid.FindObjectByRid(instanceRid);

        // 通知副本对象结束
        if (instanceOb == null)
            return;

        // 累计副本复活次数
        (instanceOb as InstanceBase).ReviveTimes += times;
    }

    /// <summary>
    /// 复活副本中战斗的宠物
    /// </summary>
    public static bool RevivePet(Property user, int campId)
    {
        // 不在副本中
        if (!IsInInstance(user))
            return false;

        // 获取副本rid
        string instanceRid = user.Query<string>("instance/rid");
        Property instanceOb = Rid.FindObjectByRid(instanceRid);

        // 通知副本对象结束
        if (instanceOb == null)
            return false;

        // 复活攻击方成员
        (instanceOb as Instance).RevivePet(campId);

        // 返回结束副本成功
        return true;
    }

    /// <summary>
    /// 结束副本
    /// </summary>
    public static bool LeaveInstance(Property who, bool isBack = false)
    {
        // 玩家离开副本开启自动回收资源
        ResourceMgr.AutoRecycle = true;

        // 玩家对象不存在
        if (who == null)
            return false;

        // 获取副本详细信息
        LPCMapping instance = who.Query<LPCMapping>("instance");
        if (instance == null)
            return false;

        // 通知副本对象结束
        Property instanceOb = Rid.FindObjectByRid(who.Query<string>("instance/rid"));
        if (instanceOb != null)
            (instanceOb as InstanceBase).DoEnd();

        // 移除副本场景相关
        List<string> sceneList = GetScenes(who.Query<string>("instance/id"));
        foreach(string sceneId in sceneList)
            SceneMgr.UnLoadSubScene(sceneId);

        // 清除玩家副本字段
        who.Delete("instance");

        // 关闭指引窗口
        WindowMgr.DestroyWindow(SpecialGuideWnd.WndType);

        // 返回结束副本成功
        return true;
    }

    /// <summary>
    /// 判断实体是否在在副本中
    /// </summary>
    public static bool IsInInstance(Property who)
    {
        // 如果玩家对象不存在
        if (who == null)
            return false;

        // 获取当前副本信息
        LPCValue instance = who.Query("instance");

        // 没有副本信息肯定不在副本中
        if (instance == null ||
            !instance.IsMapping ||
            instance.AsMapping.Count == 0)
            return false;

        // 在副本中
        return true;
    }

    /// <summary>
    /// 能否开启副本
    /// </summary>
    /// <returns><c>true</c> if is can enter instance; otherwise, <c>false</c>.</returns>
    public static bool CanEnterInstance(Property who, string instanceId, LPCMapping extraPara)
    {
        // 获取副本信息
        LPCMapping instance = GetInstanceInfo(instanceId);

        // 没有配置过的副本不能开启
        if (instance == null)
            return false;

        // 判断副本cd是否允许进入
        if (IsCooldown(who, instanceId))
            return false;

        // 如果配置了进入条件，需要进行进入条件判断
        LPCValue scriptNo = instance.GetValue<LPCValue>("enter_script");

        // 没有进入检测脚本
        if (!scriptNo.IsInt || scriptNo.AsInt == 0)
            return true;

        // 通过脚本判断是否允许进入副本
        return (bool)ScriptMgr.Call(scriptNo.AsInt, who, instanceId, instance.GetValue<LPCValue>("enter_script_args"), extraPara);
    }

    /// <summary>
    /// 判断副本是否在cd中
    /// </summary>
    public static bool IsCooldown(Property who, string instanceId)
    {
        // 获取配置信息
        LPCMapping instance = GetInstanceInfo(instanceId);

        // 没有配置过的副本不能开启
        if (instance == null)
            return false;

        // 副本没有cd时间限制
        int cooldown = instance.GetValue<int>("cooldown");
        if (cooldown == 0)
            return false;

        // 获取副本cd map
        LPCValue instanceCooldown = who.Query("instance_cooldown");
        if (instanceCooldown == null || !instanceCooldown.IsMapping)
            return false;

        // 转换数据格式
        LPCMapping cooldownMap = instanceCooldown.AsMapping;

        // 获取副本分组
        string group = instance.GetValue<string>("group");
        if (string.IsNullOrEmpty(group))
            group = instanceId;

        // 没有该副本的cd信息
        if (!cooldownMap.ContainsKey(group))
            return false;

        // cd已经结束
        if (TimeMgr.GetServerTime() > cooldownMap[group].AsInt)
            return false;

        // 返回正在cd中
        return true;
    }

    /// <summary>
    /// 判断副本是否已经解锁
    /// </summary>
    public static bool IsUnlocked(Property who, string instanceId)
    {
        return IsUnlocked(who, instanceId, LPCMapping.Empty);
    }

    /// <summary>
    /// 判断副本是否已经解锁
    /// </summary>
    public static bool IsUnlocked(Property who, string instanceId, LPCMapping extraPara)
    {
        // 获取副本信息
        LPCMapping instance = GetInstanceInfo(instanceId);

        // 没有配置过的副本
        if (instance == null)
            return false;

        // 判断当前副本的地图是否已经解锁
        int mapId = instance.GetValue<int>("map_id");
        if (! MapMgr.IsUnlocked(who, mapId))
            return false;

        // 如果是通天他副本需要转到TowerMgr判断
        int mapType = InstanceMgr.GetInstanceMapType(instanceId);
        if (mapType == MapConst.TOWER_MAP)
        {
            // 获取副本对应通天塔
            CsvRow data = TowerMgr.GetTowerInfoByInstance(instanceId);
            return TowerMgr.IsUnlocked(who,
                data.Query<int>("difficulty"), data.Query<int>("layer"));
        }

        // 判断该副本难度是否有解锁
        if (!DifficultyIsUnlock(who, mapId, instance.GetValue<int>("difficulty")))
            return false;

        // 判断副本上一个关卡
        string preId = GetPreInstanceId(instanceId);

        // 该副本没有前驱副本
        if (string.IsNullOrEmpty(preId))
            return true;

        return IsClearanced(who, preId, extraPara);
    }

    /// <summary>
    /// 是否满足解锁等级
    /// </summary>
    public static bool IsUnLockLevel(Property who, string instanceId)
    {
        // 玩家对象不存在
        if (who == null)
            return false;

        // 获取副本信息
        LPCMapping instance = GetInstanceInfo(instanceId);

        // 没有获取到副本数据
        if (instance == null)
            return false;

        // 副本解锁等级
        int unlockLevel = instance.GetValue<int>("unlock_level");

        // 最大宠物等级数据
        int historyMaxLevel = who.Query<int>("history_max_level");

        // 是否满足解锁条件
        return historyMaxLevel >= unlockLevel;
    }

    /// <summary>
    /// 副本通关
    /// </summary>
    public static void DoInstanceClearance(string rid, bool result, int endType, string instanceId, LPCMapping dropBonus,
        LPCMapping roundActions, int aliveAmount, int killAmount, int crossTimes, int remainAmount, bool isLoopFight, int clearanceTime, int roundTimes)
    {
        // 玩家对象不存在
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        // 副本通关失败;
        if (!isLoopFight
            && (endType == RoundCombatConst.END_TYPE_WIN)
            && !result
            && IsRevive(instanceId))
        {
            //构建参数
            LPCMapping sendMap = new LPCMapping();
            sendMap.Add("rid", rid);
            sendMap.Add("result", 0);
            sendMap.Add("dropBonus", dropBonus);
            sendMap.Add("roundActions", roundActions);
            sendMap.Add("instanceId", instanceId);
            sendMap.Add("aliveAmount", aliveAmount);
            sendMap.Add("killAmount", killAmount);
            sendMap.Add("crossTimes", crossTimes);
            sendMap.Add("remainAmount", remainAmount);
            sendMap.Add("clearanceTime", clearanceTime);
            sendMap.Add("roundTimes", roundTimes);

            // 抛出副本复活提示事件;
            EventMgr.FireEvent(EventMgrEventType.EVENT_REVIVE_TIPS, MixedValue.NewMixedValue<LPCMapping>(sendMap));

            return;
        }

        // 通知服务器更新
        // 通知服务器副本通过
        // 1. 通关副本id
        // 2. 副本通关时间
        Operation.CmdInstanceClearance.Go(rid, result, dropBonus, roundActions, aliveAmount,
            killAmount, crossTimes, remainAmount, clearanceTime, AutoCombatMgr.AutoCombat, roundTimes);
    }

    /// <summary>
    /// 该副本是否可以复活
    /// </summary>
    public static bool IsRevive(string instanceId)
    {
        LPCMapping instanceInfo = GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return false;

        // 副本复活消耗
        LPCValue costMap = instanceInfo.GetValue<LPCValue>("revive_cost");
        if (costMap == null
            || ! costMap.IsMapping
            || costMap.AsMapping.Count == 0)
            return false;

        return true;
    }

    /// <summary>
    /// 某一个难度的副本是否全部解锁
    /// </summary>
    public static bool DifficultyIsUnlock(Property who, int mapId, int difficulty)
    {
        // 根据地图id获取副本列表信息;
        List<string> instanceArray = GetInstanceByMapId(mapId);

        // 根据该地图id没有获取到相应的副本列表信息;
        if (instanceArray == null || instanceArray.Count == 0)
            return false;

        // 遍历各个副本
        foreach (string id in instanceArray)
        {
            // 根据副本id获取副本信息;
            LPCMapping data = GetInstanceInfo(id);

            // 不是需要匹配的难度;
            // 需要判断小于该难度的全部副本是否解锁
            if (data.GetValue<int>("difficulty") >= difficulty)
                continue;

            // 获取通关标识
            if (! IsClearanced(who, id))
                return false;
        }

        // 判断该难度的副本列表中最后一个列表是否解锁;
        return true;
    }

    /// <summary>
    /// 判断副本是否通关
    /// </summary>
    public static bool IsClearanced(Property who, string instanceId, LPCMapping extraPara)
    {
        // 获取副本配置信息
        LPCMapping data = GetInstanceInfo(instanceId);

        // 如果副本不需要记录通关标识
        LPCValue flag = data.GetValue<LPCValue>("flag");
        if (! flag.IsInt)
            return false;

        // 获取副本地图类型
        int mapType = InstanceMgr.GetInstanceMapType(instanceId);

        // 如果是精英副本
        if (mapType == MapConst.PET_DUNGEONS_MAP)
        {
            // 获取玩家通关数据
            LPCMapping dynamicClearance = who.Query<LPCMapping>("dynamic_clearance");
            if (dynamicClearance == null)
                return false;

            // 动态地图的dynamic_id
            string dynamicId = extraPara.GetValue<string>("dynamic_id");
            if (string.IsNullOrEmpty(dynamicId))
                return false;

            // 没有该动态副本通关信息
            LPCArray dClearanceList = dynamicClearance.GetValue<LPCArray>(dynamicId);
            if (dClearanceList == null)
                return false;

            // 返回是否通关
            return (dClearanceList.IndexOf(flag.AsInt) != -1);
        }

        // 获取玩家通关数据
        LPCValue clearance = who.Query<LPCValue>("clearance");
        if (clearance == null || clearance.IsUndefined)
            return false;

        // 转换数据格式
        LPCArray clearanceList = clearance.AsArray;

        // 计算当前副本的存储位置
        int index = flag.AsInt / 31;

        // 当前分配的长度不足够足够，说明副本还没有通关
        if (clearanceList.Count <= index)
            return false;

        // 计算偏移量
        int offset = 1 << (flag.AsInt % 31);

        // 如果没有通关不处理
        if ((clearanceList[index].AsInt & offset) == 0)
            return false;

        // 副本已经通关
        return true;
    }

    /// <summary>
    /// 判断副本是否通关
    /// </summary>
    public static bool IsClearanced(Property who, string instanceId)
    {
        return IsClearanced(who, instanceId, LPCMapping.Empty);
    }

    /// <summary>
    /// Determines if can repeat clearance the specified instanceId.
    /// </summary>
    /// <returns><c>true</c> if can repeat clearance the specified instanceId; otherwise, <c>false</c>.</returns>
    /// <param name="instanceId">Instance identifier.</param>
    public static bool CanRepeatClearance(string instanceId)
    {
        // 获取副本配置信息
        LPCMapping config = InstanceMgr.GetInstanceInfo(instanceId);

        // 没有配置的副本
        if (config == null)
            return false;

        // 返回副本是否允许重复通关
        return (config.GetValue<int>("repeat_clearance") == 1);
    }

    /// <summary>
    /// 获取副本通关奖励
    /// </summary>
    /// <returns>The instance clearance bonus.</returns>
    public static LPCMapping GetInstanceClearanceBonus(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return null;

        // 获取副本配置信息
        LPCMapping instanceConfig = InstanceMgr.GetInstanceInfo(instanceId);

        if (instanceConfig == null)
            return null;

        // 通关奖励脚本
        LPCValue scriptNo = instanceConfig.GetValue<LPCValue>("clearance_bonus_script");

        if (scriptNo == null || scriptNo.AsInt == 0)
            return null;

        // 通关奖励参数
        LPCMapping bonusArgs = instanceConfig.GetValue<LPCMapping>("clearance_bonus_args");

        object ret = ScriptMgr.Call(scriptNo.AsInt, bonusArgs);

        if (ret == null)
            return null;

        return ret as LPCMapping;
    }

    /// <summary>
    /// 检测是否显示副本
    /// </summary>
    public static bool CheckShowInstance(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return false;

        // 获取副本配置信息
        LPCMapping instanceConfig = InstanceMgr.GetInstanceInfo(instanceId);

        if (instanceConfig == null)
            return false;

        // 检测脚本
        LPCValue scriptNo = instanceConfig.GetValue<LPCValue>("show_script");

        if (scriptNo == null || scriptNo.AsInt == 0)
            return true;

        // 检测参数
        LPCMapping checkShowInstanceArgs = instanceConfig.GetValue<LPCMapping>("show_script_args");

        object ret = ScriptMgr.Call(scriptNo.AsInt, checkShowInstanceArgs);

        if (ret == null)
            return false;

        return (bool)ret;
    }

    /// <summary>
    /// 通过副本id获取地图类型
    /// </summary>
    public static int GetMapTypeByInstanceId(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return -1;

        // 获取副本配置信息
        LPCMapping instanceConfig = GetInstanceInfo(instanceId);

        // 没有获取到该副本配置信息
        if (instanceConfig == null)
            return -1;

        // 获取该副本对应的地图配置
        CsvRow mapConfig = MapMgr.GetMapConfig(instanceConfig.GetValue<int>("map_id"));

        if (mapConfig == null)
            return -1;

        return mapConfig.Query<int>("map_type");

    }

    /// <summary>
    /// 是否允许相同的宠物共同出战
    /// </summary>
    public static bool IsAllowSamePet(string instanceId)
    {
        LPCMapping instanceInfo = GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return false;

        int allowSamePet = instanceInfo.GetValue<int>("allow_same_pet");

        return allowSamePet.Equals(1);
    }

    /// <summary>
    /// 是否可以选择仓库中的宠物
    /// </summary>
    public static bool IsSelectStorePet(string instanceId)
    {
        LPCMapping instanceInfo = GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return false;

        int showStorePet = instanceInfo.GetValue<int>("show_store_pet");

        return showStorePet.Equals(1);
    }

    /// <summary>
    /// 参战宠物是否满足等级限制
    /// </summary>
    public static bool IsFillLevelLimit(int petLevel, string instanceId)
    {
        LPCMapping instanceInfo = GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return false;

        return petLevel >= instanceInfo.GetValue<int>("limit_level");
    }

    /// <summary>
    /// 副本是否可以自动循环战斗
    /// </summary>
    public static bool IsAutoLoopFight(string instanceId)
    {
        LPCMapping instanceInfo = GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return false;

        int isLoop = instanceInfo.GetValue<int>("is_auto_loop");

        return isLoop == 1 ? true : false;
    }

    /// <summary>
    /// 根据副本ID获取地图id
    /// </summary>
    public static int GetMapIdByInstanceId(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return -1;

        // 获取副本配置信息
        LPCMapping instanceInfo = GetInstanceInfo(instanceId);
        if (instanceInfo == null || instanceInfo.Count == 0)
            return -1;

        return instanceInfo.GetValue<int>("map_id");
    }

    /// <summary>
    /// 获取副本的循环战斗标识
    /// </summary>
    public static bool GetLoopFightByInstanceId(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return false;

        if (mLoopFights == null)
            return false;

        if (!mLoopFights.ContainsKey(instanceId))
            return false;

        return mLoopFights[instanceId];
    }

    /// <summary>
    /// 设置循环战斗标识
    /// </summary>
    public static void SetLoopFight(string instanceId, bool isLoopFight)
    {
        if (string.IsNullOrEmpty(instanceId))
            return;

        if (mLoopFights == null)
            mLoopFights = new Dictionary<string, bool>();

        if (mLoopFights.ContainsKey(instanceId))
        {
            mLoopFights[instanceId] = isLoopFight;
            return;
        }

        mLoopFights.Add(instanceId, isLoopFight);
    }

    /// <summary>
    /// 移除循环战斗标识
    /// </summary>
    public static void RemoveLoopFight(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId))
            return;

        if (mLoopFights == null)
            return;

        if (!mLoopFights.ContainsKey(instanceId))
            return;

        mLoopFights.Remove(instanceId);
    }

    /// <summary>
    /// 获取副本可选攻击列表
    /// </summary>
    public static LPCArray GetAtkSelectList(string instanceId)
    {
        // 获取副本配置信息
        LPCMapping instanceInfo = GetInstanceInfo(instanceId);
        if (instanceInfo == null)
            return LPCArray.Empty;

        return instanceInfo.GetValue<LPCArray>("atk_select_list");
    }

    /// <summary>
    /// 获取副本boss等级
    /// </summary>
    public static LPCMapping GetInstanceBossData(string instanceId)
    {
        List<CsvRow> list = GetInstanceFormation(instanceId);

        if (list == null)
            return LPCMapping.Empty;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
                continue;

            LPCMapping init_script_args = list[i].Query<LPCMapping>("init_script_args");

            if (init_script_args.GetValue<int>("is_boss") != 1)
                continue;

            return init_script_args;
        }

        return LPCMapping.Empty;
    }

    /// <summary>
    /// 取消循环战斗
    /// </summary>
    public static void CancelLoopFight(string instanceId, LPCMapping clearanceData)
    {
        List<Property> fightList = GetFightList();

        //获取奖励数据;
        LPCMapping bonusMap = clearanceData.GetValue<LPCMapping>("bonus_map");

        //获取经验奖励;
        LPCMapping expMap = bonusMap.GetValue<LPCMapping>("exp");

        // 遍历攻方列表判断是否有宠物在战斗结束后升至满级
        for (int i = 0; i < fightList.Count; i++)
        {
            Property ob = fightList[i];
            if (ob == null)
                continue;

            int maxLevel = MonsterMgr.GetMaxLevel(ob);

            int curLevel = ob.GetLevel();

            // 过滤已经是最大等级的宠物
            if (maxLevel.Equals(curLevel))
                continue;

            // 宠物星级
            int star = ob.GetStar();

            // 计算玩家升级所需经验
            int costExp = 0;

            // 累计升级到满级需要的经验
            for (int j = curLevel + 1; j <= maxLevel; j++)
                costExp += StdMgr.GetPetStdExp(j, star);

            // 阵容中有宠物升到最大等级，取消自动循环战斗
            // 此时客户端该宠物的经验并没有刷新，利用经验判断该宠物是否升至最大等级
            if((ob.Query<int>("exp") + expMap.GetValue<int>("pet_exp")) >= costExp)
            {
                SetLoopFight(instanceId, false);
                break;
            }
        }
    }

    /// <summary>
    /// 获取已解锁的npc列表
    /// </summary>
    public static List<LPCMapping> GetNpcList()
    {
        List<LPCMapping> list = new List<LPCMapping>();

        LPCMapping allInstanceConfig = GetAllInstanceInfo();
        foreach (string id in allInstanceConfig.Keys)
        {
            // 副本的配置信息
            LPCMapping data = GetInstanceInfo(id);
            if (data == null)
                continue;

            CsvRow mapConfig = MapMgr.GetMapConfig(data.GetValue<int>("map_id"));
            if (mapConfig == null)
                continue;

            if (mapConfig.Query<int>("map_type") != MapConst.ARENA_NPC_MAP)
                continue;

            // 不显示该副本
            if(! InstanceMgr.CheckShowInstance(id))
                continue;

            list.Add(data);
        }

        // 副本排序
        list.Sort((Comparison<LPCMapping>)delegate(LPCMapping a, LPCMapping b)
            {
                return a.GetValue<int>("level").CompareTo(b.GetValue<int>("level"));
            }
        );

        return list;
    }

    /// <summary>
    /// 获取精英地下城副本列表
    /// </summary>
    public static LPCArray GetPetDungeonsList(Property who)
    {
        if (who == null)
            return LPCArray.Empty;

        // 获取动态地图信息
        LPCMapping dynamicMap = who.Query<LPCMapping>("dynamic_map");

        if (dynamicMap == null)
            return LPCArray.Empty;

        LPCArray data = LPCArray.Empty;
        foreach (LPCValue item in dynamicMap.Values)
        {
            if (item == null || !item.IsMapping)
                continue;

            // 如果不是秘密圣域
            if (item.AsMapping.GetValue<int>("type") != MapConst.PET_DUNGEON)
                continue;

            // 添加到列表中
            data.Add(item.AsMapping);
        }

        // 按照剩余时间排序列表
        data.Sort((Comparison<LPCValue>)delegate(LPCValue a, LPCValue b){
            return a.AsMapping.GetValue<int>("start_time").CompareTo(b.AsMapping.GetValue<int>("start_time"));
        });

        return data;
    }

    /// <summary>
    /// 获取秘密地下城副本列表
    /// </summary>
    public static LPCArray GetSecretDungeonsList(Property who)
    {
        if (who == null)
            return LPCArray.Empty;

        // 获取动态地图信息
        LPCMapping dynamicMap = who.Query<LPCMapping>("dynamic_map");

        if (dynamicMap == null)
            return LPCArray.Empty;

        LPCArray data = LPCArray.Empty;
        foreach (LPCValue item in dynamicMap.Values)
        {
            if (item == null || !item.IsMapping)
                continue;

            // 如果不是秘密圣域
            if (item.AsMapping.GetValue<int>("type") != MapConst.SECRET_DUNGEON)
                continue;

            // 添加到列表中
            data.Add(item.AsMapping);
        }

        // 按照剩余时间排序列表
        data.Sort((Comparison<LPCValue>)delegate(LPCValue a, LPCValue b){
            return a.AsMapping.GetValue<int>("start_time").CompareTo(b.AsMapping.GetValue<int>("start_time"));
        });

        return data;
    }

    /// <summary>
    /// 获取某个地图某个
    /// </summary>
    public static LPCArray GetAssignTaskBonus(Property user, string instanceId)
    {
        int taskId = TaskMgr.GetAssignClearanceTask(instanceId);

        // 该副本没有配置通关奖励
        if (taskId == -1)
            return LPCArray.Empty;

        // 任务已完成，不显示奖励
        if (TaskMgr.IsCompleted(user, taskId))
            return LPCArray.Empty;

        return TaskMgr.GetBonus(user, taskId);
    }

    #endregion
}
