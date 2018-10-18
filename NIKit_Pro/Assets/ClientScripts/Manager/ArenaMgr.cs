/// <summary>
/// ArenaMgr.cs
/// Create by zhaozy 2016-09-22
/// top管理模块
/// </summary>

using System;
using System.Diagnostics;
using System.Collections.Generic;
using LPC;

/// 排行榜管理器
public static class ArenaMgr
{
    #region 变量

    // 排行榜列表
    private static LPCArray mTopList = new LPCArray();

    // 排行榜奖励信息
    private static LPCMapping mTopBonus = new LPCMapping();
    private static int mSettlementTime = 0;

    // 排行榜奖励配置表信息
    private static CsvFile mTopBonusCsv;

    // 竞技场属性加成配置表信息
    private static CsvFile mArenaBuffCsv;

    // 标识排行榜列表是否已经更新了
    private static bool mIsDirty = true;

    #endregion

    #region 属性

    /// <summary>
    /// 排行榜奖励配置表信息.
    /// </summary>
    public static CsvFile TopBonusCsv { get { return mTopBonusCsv; } }

    /// <summary>
    /// 竞技场buff配置表信息.
    /// </summary>
    public static CsvFile ArenaBuffCsv { get { return mArenaBuffCsv; } }

    public static int PlaybackPage { get; set; }

    #endregion

    #region 功能接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入竞技场buff配置表
        mArenaBuffCsv = CsvFileMgr.Load("arena_buff");

        // 如果是验证客户端不需要如下初始化
        if (AuthClientMgr.IsAuthClient)
            return;

        // 载入排行榜奖励配置表信息
        mTopBonusCsv = CsvFileMgr.Load("arena_top_bonus");
    }

    /// <summary>
    /// 竞技场等级限制
    /// </summary>
    public static int GetLevelLimit()
    {
        // 竞技场等级限制
        return GameSettingMgr.GetSettingInt("arena_limit_level");
    }

    /// <summary>
    /// 更新排行榜数据
    /// </summary>
    public static void UpdateTopData(LPCMapping para)
    {
        // 更新排行榜奖励信息
        mTopBonus = para.GetValue<LPCMapping>("bonus");

        // 如果包含结算时间信息
        mSettlementTime = para.GetValue<int>("settlement_time");
        // 标识isDirty为false
        mIsDirty = true;

        // 清空排行榜
        mTopList = LPCArray.Empty;
    }

    /// <summary>
    /// 更新结算奖励数据
    /// </summary>
    public static void UpdateSettlementBonusData(Property user, LPCMapping data)
    {
        if (user == null)
            return;

        OptionMgr.SetOption(ME.user, "last_settlement_data", LPCValue.Create(data));
    }

    /// <summary>
    /// 更新排行榜
    /// </summary>
    public static void UpdateTopList(LPCMapping para)
    {
        // 标识isDirty为false
        mIsDirty = false;

        // 更新排行榜列表
        mTopList = para.GetValue<LPCArray>("top_list");
    }

    /// <summary>
    /// 设置排行榜数据已经过时标识
    /// </summary>
    public static void SetTopListDirty()
    {
        mIsDirty = true;
    }

    /// <summary>
    /// Requests the arena top list.
    /// </summary>
    public static bool RequestArenaTopList()
    {
        // 如果本地没有缓存的排行榜数据，而且数据没有变化
        if (mTopList.Count != 0 && !mIsDirty)
            return false;

        // 默认传递传送0,100，现在是默认获取排行榜前100的玩家角色
        int maxAmount = GameSettingMgr.GetSettingInt("top_show_max_pieces");
        Operation.CmdGetArenaTopList.Go(0, maxAmount - 1);

        return true;
    }

    /// <summary>
    /// Requests the arena battle list.
    /// </summary>
    public static void RequestArenaBattleList(int type)
    {
        Operation.CmdGetArenaOpponentList.Go(type);
    }

    /// <summary>
    /// 获取上周排行榜结算奖励数据
    /// </summary>
    public static LPCMapping GetLastSettlement(Property user)
    {
        if (user == null)
            return null;

        LPCValue v = OptionMgr.GetOption(user, "last_settlement_data");
        if (v == null || ! v.IsMapping)
            return null;

        return v.AsMapping;
    }

    /// <summary>
    /// 获取排行榜列表
    /// </summary>
    public static LPCMapping GetTopList()
    {
        LPCMapping data = new LPCMapping();

        if(mTopList.Count < 1)
            return data;

        foreach (LPCValue value in mTopList.Values)
            data.Add(value.AsMapping.GetValue<int>("rank"), value.AsMapping);

        return data;
    }

    /// <summary>
    /// 获取玩家竞技场buff
    /// </summary>
    public static LPCArray GetArenaBuff(int winTimes)
    {
        // 获取玩家竞技场连胜记录信息
        CsvRow data = null;

        do
        {
            // 如果没有winTimes
            if (winTimes <= 0)
                break;

            // 获取配置信息
            data = ArenaBuffCsv.FindByKey(winTimes);

            // 该连胜次数没有buff数据
            if (data != null)
                break;

            // 连胜次数向下取
            winTimes--;

        } while(true);

        // 没有连胜buff
        if (data == null)
            return LPCArray.Empty;

        // 返回配置的属性列表
        return data.Query<LPCArray>("props");
    }

    /// <summary>
    /// 获取竞技场连胜buff描述
    /// </summary>
    public static string GetArenaBuffDesc(int winTimes, bool isWrap = true)
    {
        // 获取玩家竞技场连胜记录信息
        CsvRow data = null;

        do
        {
            // 如果没有winTimes
            if (winTimes < 0)
                break;

            // 获取配置信息
            data = ArenaBuffCsv.FindByKey(winTimes);

            // 该连胜次数没有buff数据
            if (data != null)
                break;

            // 连胜次数向下取
            winTimes--;

        } while(true);

        // 没有连胜buff
        if (data == null)
            return string.Empty;

        string desc = string.Empty;

        LPCArray array = data.Query<LPCArray>("desc");

        for (int i = 0; i < array.Count; i++)
            desc += (LocalizationMgr.Get(array[i].AsString) + " ");

        return desc;
    }

    /// <summary>
    /// 有buff的连胜次数
    /// </summary>
    public static int GetBuffWinsTimes(int currentWinTimes)
    {
        CsvRow data = null;
        do
        {
            if (currentWinTimes < 0)
                break;

            // 获取配置信息
            data = ArenaBuffCsv.FindByKey(currentWinTimes);

            // 该连胜次数没有buff数据
            if (data != null)
                break;

            // 连胜次数向上取
            currentWinTimes--;

        } while(true);

        return currentWinTimes;
    }

    /// <summary>
    /// 根据排名获取名次背景图标
    /// </summary>
    public static string GetRankBgName(int rank)
    {
        string name = string.Empty;

        switch (rank)
        {
            case 1 : name = "frist_top";
                break;
            case 2 : name = "second_top";
                break;
            case 3 : name = "thrid_top";
                break;
            default : name = "other_top";
                break;
        }
        return name;
    }

    /// <summary>
    /// 获取名次字体大小
    /// </summary>
    public static int GetFontSize(int rank)
    {
        int fontSize = 0;
        switch (rank)
        {
            case 1 : fontSize = 60;
                break;
            case 2 : fontSize = 50;
                break;
            case 3 : fontSize = 45;
                break;
            default : fontSize = 35;
                break;
        }
            return fontSize;
    }

    /// <summary>
    /// 根据积分和排名获取阶位
    /// </summary>
    public static int GetStepByScoreAndRank(int rank, int score)
    {
        // 当前阶位
        int step = 0;

        if(TopBonusCsv == null)
            return step;

        foreach (CsvRow row in TopBonusCsv.rows)
        {
            int tempRank = row.Query<int>("rank");
            int tempScore = row.Query<int>("score");

            step = row.Query<int>("step");

            if (tempRank != ArenaConst.ARENA_RANK_SCORE_LIMIT)
            {
                if (rank > tempRank || score < tempScore)
                    continue;
            }
            else
            {
                if (score < tempScore)
                    continue;
            }

            break;
        }

        return step;
    }

    /// <summary>
    /// 获取所有的排行榜奖励信息
    /// </summary>
    public static LPCMapping GetAllBonus()
    {
        return mTopBonus;
    }

    /// <summary>
    /// 返回某个阶位的排行榜信息
    /// </summary>
    public static LPCArray GetBonus(int step)
    {
        return mTopBonus.GetValue<LPCArray>(step, LPCArray.Empty);
    }

    /// <summary>
    /// 是否正在结算中
    /// </summary>
    public static bool IsSettlement()
    {
        // 服务器数据还没有下来
        if (mSettlementTime == 0)
            return false;

        // 否则使用服务器时间和结算时间比较一下
        return TimeMgr.GetServerTime() >= mSettlementTime;
    }

    /// <summary>
    /// 获取npc可以挑战的数量,已经解锁并且不在冷却时间内
    /// </summary>
    public static int GetNpcChallengeAmount()
    {
        LPCValue arenaBattleNpcData = ME.user.Query<LPCValue>("instance_cooldown");

        LPCMapping npcData = new LPCMapping();

        // 没有对战数据
        if (arenaBattleNpcData != null && arenaBattleNpcData.IsMapping)
            npcData = arenaBattleNpcData.AsMapping;

        LPCMapping allInstanceConfig = InstanceMgr.GetAllInstanceInfo();

        int amount = 0;

        foreach (string id in allInstanceConfig.Keys)
        {
            // 获取一个副本的配置信息
            LPCMapping data = InstanceMgr.GetInstanceInfo(id);

            if (data == null)
                continue;

            // 获取地图信息
            CsvRow mapConfig = MapMgr.GetMapConfig(data.GetValue<int>("map_id"));

            if (mapConfig == null)
                continue;

            if (mapConfig.Query<int>("map_type") != MapConst.ARENA_NPC_MAP)
                continue;

            // 不显示该副本
            if(!InstanceMgr.CheckShowInstance(id))
                continue;

            LPCMapping showInstanceArgs = data.GetValue<LPCMapping>("show_script_args");

            bool isUnLocked = CACL_ARENA_NPC_IS_UNLOCKED.CALL(ME.user.Query<int>("level"), showInstanceArgs.GetValue<int>("level"));

            // 没有解锁的
            if(! isUnLocked)
                continue;

            int cdTime = 0;

            // 获取副本分组
            string group = data.GetValue<string>("group");
            if (string.IsNullOrEmpty(group))
                group = id;

            // 获取cd信息
            if(npcData.ContainsKey(group))
                cdTime = npcData[group].AsInt;

            if(Math.Max(cdTime - TimeMgr.GetServerTime(), 0) == 0)
                amount++;
        }
        return amount;
    }

    /// <summary>
    /// 计算是否可以免费刷新对战列表
    /// </summary>
    public static bool IsNormalRefresh(int matchTime, int currentTime)
    {
        if (CalcNextNormalRefreshRemainTime(matchTime, currentTime) > 0)
            return false;
        else
            return true;
    }

    /// <summary>
    /// 计算下次普通刷新剩余的时间
    /// </summary>
    public static int CalcNextNormalRefreshRemainTime(int matchTime, int currentTime)
    {
        // 剩余的时间
        int remainTime = 0;

        // 获取普通刷新的时间间隔
        int normalRefreshTimeSpace = GameSettingMgr.GetSettingInt("battle_list_normal_refresh_time");

        if (currentTime - matchTime >= normalRefreshTimeSpace)
        {
            remainTime = 0;
        }
        else
        {
            remainTime = Math.Min(normalRefreshTimeSpace - (currentTime - matchTime), normalRefreshTimeSpace);
        }

        return remainTime;
    }

    /// <summary>
    /// 显示结算完成窗口
    /// </summary>
    public static void ShowSettlementFinishWnd()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 竞技场结算提示
        LPCValue v = ME.user.Query<LPCValue>("arena_settlement_tips");
        if (v == null || !v.IsInt)
            return;

        // 已经提示过
        if (v.AsInt == 1)
            return;

        LPCMapping settlementData = GetLastSettlement(ME.user);
        if (settlementData == null || settlementData.Count == 0)
            return;

        // 打开结算完毕窗口
        UnityEngine.GameObject go = WindowMgr.OpenWnd(SettlementFinishWnd.WndType);
        if (go == null)
            return;

        go.GetComponent<SettlementFinishWnd>().Bind(settlementData.GetValue<LPCArray>("last_bonus"));
    }

    #endregion
}
