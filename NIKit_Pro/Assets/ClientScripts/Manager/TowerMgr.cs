/// <summary>
/// TowerMgr.cs
/// Create by zhaozy 2017-09-04
/// 通天塔管理模块
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
public static class TowerMgr
{
    #region 变量

    /// <summary>
    /// 配置表信息
    /// </summary>
    private static Dictionary<int, Dictionary<int, CsvRow>> mTowerFile = new Dictionary<int, Dictionary<int, CsvRow>>();

    /// <summary>
    /// 配置表信息
    /// </summary>
    private static Dictionary<string, CsvRow> mInstanceToTowerFile = new Dictionary<string, CsvRow>();

    /// <summary>
    /// 通天塔资源
    /// </summary>
    private static Dictionary<string, List<CsvRow>> mTowerResource = new Dictionary<string, List<CsvRow>>();

    // 通天塔数据
    private static LPCMapping mRunTowerData = LPCMapping.Empty;

    // 排行榜列表
    private static Dictionary<int, LPCArray> mTopListMap = new Dictionary<int, LPCArray>();

    // 标识对应排行榜列表是否已经更新了
    private static Dictionary<int, bool> mTopDirtyMap = new Dictionary<int, bool>();

    // 宠物排行榜列表
    private static Dictionary<int, LPCMapping> mPetTopListMap = new Dictionary<int, LPCMapping>();

    // 标识对应宠物排行榜列表是否已经更新了
    private static Dictionary<int, bool> mPetTopDirtyMap = new Dictionary<int, bool>();

    // 通天塔奖励数据
    private static Dictionary<int, Dictionary<int, LPCMapping>> mBonusMap = new Dictionary<int, Dictionary<int, LPCMapping>>();

    #endregion

    #region 变量

    /// <summary>
    /// Loads the tower file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    public static LPCMapping RunTowerData
    {
        get
        {
            return mRunTowerData;
        }
        set
        {
            mRunTowerData = value;
        }
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 载入通天塔配置表
    /// </summary>
    private static void LoadTowerFile(string fileName)
    {
        // 清空配置表现信息
        mTowerFile.Clear();
        mInstanceToTowerFile.Clear();
        mBonusMap.Clear();

        // 载入通天塔配置表
        CsvFile tower = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (tower == null)
            return;

        int difficulty = 0;
        int layer = 0;
        string instanceId = string.Empty;

        // 遍历数据
        foreach(CsvRow data in tower.rows)
        {
            // 获取副本信息
            instanceId = data.Query<string>("instance_id");
            mInstanceToTowerFile[instanceId] = data;

            // 获取配置信息
            layer = data.Query<int>("layer");
            difficulty = data.Query<int>("difficulty");

            // 初始化数据
            if (!mTowerFile.ContainsKey(difficulty))
                mTowerFile.Add(difficulty, new Dictionary<int, CsvRow>());

            // 添加数据
            mTowerFile[difficulty].Add(layer, data);

            // 初始化数据
            if(!mBonusMap.ContainsKey(difficulty))
                mBonusMap.Add(difficulty, new Dictionary<int, LPCMapping>());

            mBonusMap[difficulty].Add(layer, InstanceMgr.GetInstanceClearanceBonus(instanceId));

        }
    }

    /// <summary>
    /// 载入通天塔资源配置表
    /// </summary>
    private static void LoadTowerResource(string fileName)
    {
        // 清空配置表现信息
        mTowerResource.Clear();

        // 载入副本配置表
        CsvFile resources = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (resources == null)
            return;

        string group = string.Empty;

        // 遍历各行数据 
        foreach (CsvRow data in resources.rows)
        {
            // 获取资源group
            group = data.Query<string>("group");

            // 批次资源信息初始化
            if (mTowerResource.ContainsKey(group))
                mTowerResource[group].Add(data);
            else
                mTowerResource.Add(group, new List<CsvRow>(){ data });
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入通天塔配置表
        LoadTowerFile("tower");

        // 载入通天塔资源配置表
        LoadTowerResource("tower_resource");
    }

    /// <summary>
    /// 判断是否已经解锁
    /// </summary>
    public static bool IsUnlocked(Property user, int difficulty, int layer)
    {
        // 获取配置信息
        CsvRow data = GetTowerInfo(difficulty, layer);

        // 没有配置的数据不能解锁
        if (data == null)
            return false;

        // 判断前驱层级是否已经通关, 如果上一次塔还没有通关
        // 则认为当前层级还没有解锁
        int preLayer = layer - 1;
        if (preLayer >= 0 &&
            ! IsClearanced(user, difficulty, preLayer))
            return false;

        // 没有配置解锁脚本
        int scriptNo = data.Query<int>("unlock_script");
        if (scriptNo == 0)
            return true;

        // 调用脚本判断是否可以解锁
        return  (bool) ScriptMgr.Call(scriptNo, user, data);
    }

    /// <summary>
    /// 判断是否已经通关
    /// </summary>
    public static bool IsClearanced(Property user, int difficulty, int layer)
    {
        // 获取玩家tower数据
        LPCMapping tower = user.Query<LPCMapping>("tower");
        if (tower == null ||
            !string.Equals(tower.GetValue<string>("cookie"), RunTowerData.GetValue<string>("cookie")))
            return false;

        // 获取玩家当前tower数据
        // 没有相关难度通关数据
        LPCMapping clearance = tower.GetValue<LPCMapping>("clearance");
        if (clearance == null ||
            ! clearance.ContainsKey(difficulty))
            return false;

        // 返回已经通关
        return (clearance[difficulty].AsInt >= layer);
    }

    /// <summary>
    /// 获取tower配置信息
    /// </summary>
    /// <returns>The tower info.</returns>
    /// <param name="difficulty">难度.</param>
    /// <param name="layer">层级.</param>
    public static CsvRow GetTowerInfo(int difficulty, int layer)
    {
        Dictionary<int, CsvRow> towerMap;
        if (!mTowerFile.TryGetValue(difficulty, out towerMap))
            return null;

        CsvRow data;
        if (! towerMap.TryGetValue(layer, out data))
            return null;

        // 返回数据
        return data;
    }

    /// <summary>
    /// 获取tower配置信息
    /// </summary>
    /// <returns>The tower info by instance.</returns>
    /// <param name="instanceId">副本id.</param>
    public static CsvRow GetTowerInfoByInstance(string instanceId)
    {
        // 不存在的配置信息
        if (! mInstanceToTowerFile.ContainsKey(instanceId))
            return null;

        // 返回配置信息
        return mInstanceToTowerFile[instanceId];
    }

    /// <summary>
    /// 获取资源
    /// </summary>
    /// <returns>The resources.</returns>
    /// <param name="groupId">资源组编号.</param>
    public static List<CsvRow> GetResources(string groupId)
    {
        // 不包含资源
        if (!mTowerResource.ContainsKey(groupId))
            return new List<CsvRow>();

        // 添加资源
        return mTowerResource[groupId];
    }

    /// <summary>
    /// 获取资源
    /// </summary>
    /// <returns>The tower resources.</returns>
    /// <param name="difficulty">难度.</param>
    /// <param name="layer">层级.</param>
    public static Dictionary<int, List<CsvRow>> GetTowerResources(int difficulty, int layer)
    {
        // 没有数据
        if (RunTowerData == null ||
            RunTowerData.Count == 0)
            return new Dictionary<int, List<CsvRow>>();

        // 通天塔资源
        LPCMapping towerResource = RunTowerData.GetValue<LPCMapping>("tower_resource");
        if (towerResource == null || towerResource.Count == 0)
            return new Dictionary<int, List<CsvRow>>();

        // 获取该难度数据
        LPCMapping difficultyMap = towerResource.GetValue<LPCMapping>(difficulty);
        if (difficultyMap == null || difficultyMap.Count == 0)
            return new Dictionary<int, List<CsvRow>>();

        // 获取相应难度下的资源数据
        LPCMapping resMap = difficultyMap.GetValue<LPCMapping>(layer);
        if (resMap == null || resMap.Count == 0)
            return new Dictionary<int, List<CsvRow>>();

        Dictionary<int, List<CsvRow>> resources = new Dictionary<int, List<CsvRow>>();
        string groupId;

        // 收集资源
        foreach (int level in resMap.Keys)
        {
            // 获取该关卡资源
            groupId = resMap.GetValue<string>(level);

            // 不包含资源
            if (!mTowerResource.ContainsKey(groupId))
            {
                resources.Add(level, new List<CsvRow>());
                continue;
            }

            // 添加资源
            resources.Add(level, mTowerResource[groupId]);
        }

        // 返回资源
        return resources;
    }

    /// <summary>
    /// 获取boss关卡资源组编号
    /// </summary>
    public static string GetTowerBossResourceGroup(int difficulty, int layer)
    {
        // 没有数据
        if (RunTowerData == null || RunTowerData.Count == 0)
            return string.Empty;

        // 通天塔资源
        LPCMapping towerResource = RunTowerData.GetValue<LPCMapping>("tower_resource");
        if (towerResource == null || towerResource.Count == 0)
            return string.Empty;

        // 获取该难度数据
        LPCMapping difficultyMap = towerResource.GetValue<LPCMapping>(difficulty);
        if (difficultyMap == null || difficultyMap.Count == 0)
            return string.Empty;

        // 获取相应难度下的资源数据
        LPCMapping resMap = difficultyMap.GetValue<LPCMapping>(layer);
        if (resMap == null || resMap.Count == 0)
            return string.Empty;

        // 获取boss关卡数据
        int bossLevel = -1;
        foreach (int level in resMap.Keys)
        {
            // 关卡不需要变化
            if (bossLevel >= level)
                continue;

            // 记录数据
            bossLevel = level;
        }

        // 返回boss关卡资源组
        return resMap.GetValue<string>(bossLevel);
    }

    /// <summary>
    /// 获取boss关卡资源
    /// </summary>
    public static List<CsvRow> GetTowerBossLevelResources(int difficulty, int layer, out int batch)
    {
        batch = 0;

        // 获取指定难度指定层的资源
        Dictionary<int, List<CsvRow>> resources = GetTowerResources(difficulty, layer);
        if (resources == null)
            return null;

        List<int> levelList = new List<int>();

        foreach (int level in resources.Keys)
        {
            if (!levelList.Contains(level))
                levelList.Add(level);
        }

        if (levelList.Count == 0)
            return null;

        // 升序排序
        levelList.Sort();

        batch = levelList[levelList.Count - 1];

        // 临时复制一份数据防止内存被修改导致验证客户端验证异常
        List<CsvRow> listRow = new List<CsvRow>();
        foreach (CsvRow data in resources[batch])
            listRow.Add(data);

        // 按照剩余站位排序列表
        listRow.Sort((Comparison<CsvRow>)delegate(CsvRow pos1, CsvRow pos2){
            return pos1.Query<int>("pos").CompareTo(pos2.Query<int>("pos"));
        });

        return listRow;
    }

    /// <summary>
    /// 根据难度获取通天塔难度副本列表
    /// </summary>
    public static List<CsvRow> GetTowerListByDiff(int difficulty)
    {
        Dictionary<int, CsvRow> towerMap;
        if (!mTowerFile.TryGetValue(difficulty, out towerMap))
            return null;

        List<CsvRow> list = new List<CsvRow>();

        foreach (CsvRow row in towerMap.Values)
            list.Add(row);

        return list;
    }

    /// <summary>
    /// 通天之塔下次刷新的时间
    /// </summary>
    public static int CalcNextTime()
    {
        // 获取服务器时间
        int time = TimeMgr.GetServerTime();

        // 当前月份有多少天
        DateTime dt = TimeMgr.ConvertIntDateTime(time);

        // 通天塔重置时间
        LPCMapping towerResetTime = GameSettingMgr.GetSetting<LPCMapping>("tower_reset_time");
        if (towerResetTime == null)
            return 0;

        int month = dt.Month;

        int year = dt.Year;

        // 当月刷新的时间
        int curMonthTime = (int)TimeMgr.ConvertDateTimeInt(
                             new DateTime(
                                 year,
                                 month,
                                 towerResetTime.GetValue<int>("mday"),
                                 towerResetTime.GetValue<int>("hour"),
                                 towerResetTime.GetValue<int>("min"),
                                 towerResetTime.GetValue<int>("sec")
                             ));

        // 当前时间大于本月刷新时间,则计算下一次的刷新时间
        if (time >= curMonthTime)
        {
            if (month < 12)
            {
                month += 1;
            }
            else
            {
                month = 1;

                year += 1;
            }

            // 下次刷新的时间
            curMonthTime = (int) TimeMgr.ConvertDateTimeInt(
                new DateTime(
                    year,
                    month,
                    towerResetTime.GetValue<int>("mday"),
                    towerResetTime.GetValue<int>("hour"),
                    towerResetTime.GetValue<int>("min"),
                    towerResetTime.GetValue<int>("sec")
                ));
        }

        // 每个月16号0点重置
        return curMonthTime;
    }

    /// <summary>
    /// 更新排行榜数据
    /// </summary>
    public static void RefreshAllData()
    {
        // 标记为dirty
        List<int> dirtyKeys = new List<int>(mTopDirtyMap.Keys);
        for (int i = 0; i < dirtyKeys.Count; i++)
            mTopDirtyMap[dirtyKeys[i]] = true;

        // 清空排行榜
        List<int> listKeys = new List<int>(mTopListMap.Keys);
        for (int i = 0; i < listKeys.Count; i++)
            mTopListMap[listKeys[i]] = LPCArray.Empty;

        // 标记为dirty
        dirtyKeys = new List<int>(mPetTopDirtyMap.Keys);
        for (int i = 0; i < dirtyKeys.Count; i++)
            mTopDirtyMap[dirtyKeys[i]] = true;

        // 清空宠物排行榜
        listKeys = new List<int>(mPetTopListMap.Keys);
        for (int i = 0; i < listKeys.Count; i++)
            mPetTopListMap[listKeys[i]] = LPCMapping.Empty;
    }

    /// <summary>
    /// 更新排行榜
    /// </summary>
    public static void UpdateTopList(LPCMapping para)
    {
        int difficulty = para.GetValue<int>("difficulty");
        LPCArray topList = para.GetValue<LPCArray>("top_list");

        // 标识isDirty为false
        if (mTopDirtyMap.ContainsKey(difficulty))
            mTopDirtyMap[difficulty] = false;
        else
            mTopDirtyMap.Add(difficulty, false);

        if (mTopListMap.ContainsKey(difficulty))
            mTopListMap[difficulty] = topList;
        else
            mTopListMap.Add(difficulty, topList);
    }

    /// <summary>
    /// 更新排行榜
    /// </summary>
    public static void UpdatePetTopList(LPCMapping para)
    {
        int difficulty = para.GetValue<int>("difficulty");
        LPCMapping topList = para.GetValue<LPCMapping>("top_list");

        // 标识isDirty为false
        if (mPetTopDirtyMap.ContainsKey(difficulty))
            mPetTopDirtyMap[difficulty] = false;
        else
            mPetTopDirtyMap.Add(difficulty, false);

        if (mPetTopListMap.ContainsKey(difficulty))
            mPetTopListMap[difficulty] = topList;
        else
            mPetTopListMap.Add(difficulty, topList);
    }

    /// <summary>
    /// 请求排行榜列表
    /// </summary>
    /// <returns><c>true</c>, if top list was requested, <c>false</c> otherwise.</returns>
    /// <param name="difficulty">difficulty.</param>
    public static bool RequestTopList(int difficulty)
    {
        // 排行榜没有更新，直接使用原来的即可
        if (mTopDirtyMap.ContainsKey(difficulty) && ! mTopDirtyMap[difficulty])
            return false;

        // 默认传递传送0,100，现在是默认获取排行榜前100的玩家角色
        int maxAmount = GameSettingMgr.GetSettingInt("tower_top_show_max_pieces");
        Operation.CmdGetTowerTopList.Go(difficulty, 0, maxAmount - 1);

        return true;
    }

    /// <summary>
    /// 请求宠物排行榜列表
    /// </summary>
    /// <returns><c>true</c>, if top list was requested, <c>false</c> otherwise.</returns>
    /// <param name="difficulty">difficulty.</param>
    public static bool RequestPetTopList(int difficulty)
    {
        // 排行榜没有更新，直接使用原来的即可
        if (mPetTopDirtyMap.ContainsKey(difficulty) && ! mPetTopDirtyMap[difficulty])
            return false;

        // 默认传递传送0,9，现在是默认获取排行榜前10的使魔信息
        int maxAmount = GameSettingMgr.GetSettingInt("tower_pet_top_show_max_pieces");
        Operation.CmdGetTowerPetTopList.Go(difficulty, 0, maxAmount - 1);

        return true;
    }

    /// <summary>
    /// 获取排行榜数据
    /// </summary>
    /// <returns><c>true</c>, if top list was gotten, <c>false</c> otherwise.</returns>
    /// <param name="difficulty">difficulty.</param>
    public static LPCArray GetTopList(int difficulty)
    {
        if (!mTopListMap.ContainsKey(difficulty))
            return LPCArray.Empty;

        return mTopListMap[difficulty];
    }

    /// <summary>
    /// 获取对应层数的使魔排行榜数据
    /// </summary>
    /// <returns>The pet top list by layer.</returns>
    /// <param name="difficulty">Difficulty.</param>
    /// <param name="layer">Layer.</param>
    public static LPCMapping GetPetTopListByLayer(int difficulty, int layer)
    {
        // 只能取每10层的宠物信息
        if ((layer + 1) % 10 != 0)
            return LPCMapping.Empty;

        if (!mPetTopListMap.ContainsKey(difficulty))
            return LPCMapping.Empty;

        if (!mPetTopListMap[difficulty].ContainsKey(layer))
            return LPCMapping.Empty;

        return mPetTopListMap[difficulty][layer].AsMapping;
    }

    /// <summary>
    /// 设置排行榜数据已经过时标识
    /// </summary>
    public static void SetTopListDirty(LPCMapping para)
    {
        int difficulty = para.GetValue<int>("difficulty");

        if (mTopDirtyMap.ContainsKey(difficulty))
            mTopDirtyMap[difficulty] = true;
        else
            mTopDirtyMap.Add(difficulty, true);
    }

    /// <summary>
    /// 设置排行榜数据已经过时标识
    /// </summary>
    public static void SetPetTopListDirty(LPCMapping para)
    {
        int difficulty = para.GetValue<int>("difficulty");

        if (mPetTopDirtyMap.ContainsKey(difficulty))
            mPetTopDirtyMap[difficulty] = true;
        else
            mPetTopDirtyMap.Add(difficulty, true);
    }

    /// <summary>
    /// 获取最大通关层数
    /// </summary>
    public static int GetMaxClearanceLayer(Property user, int difficulty)
    {
        if (user == null)
            return 0;

        LPCMapping tower = user.Query<LPCMapping>("tower");
        if (tower == null)
            return 0;

        LPCMapping clearance = tower.GetValue<LPCMapping>("clearance");
        if (clearance == null ||
            ! clearance.ContainsKey(difficulty))
            return 0;

        // 返回最大层数
        return clearance[difficulty].AsInt;
    }
     
    /// <summary>
    /// 获取奖励
    /// </summary>
    /// <returns>The bonus by layer.</returns>
    /// <param name="difficulty">Difficulty.</param>
    /// <param name="layer">Layer.</param>
    public static LPCMapping GetBonusByLayer(int difficulty, int layer)
    {
        if (!mBonusMap.ContainsKey(difficulty))
            return null;

        if (!mBonusMap[difficulty].ContainsKey(layer))
            return null;

        return mBonusMap[difficulty][layer];
    }

    /// <summary>
    /// 或取某个难度的所有奖励
    /// </summary>
    /// <returns>The all bonus.</returns>
    /// <param name="difficulty">Difficulty.</param>
    public static Dictionary<int, LPCMapping> GetAllBonus(int difficulty)
    {
        if (!mBonusMap.ContainsKey(difficulty))
            return null;

        return mBonusMap[difficulty];
    }

    #endregion
}
