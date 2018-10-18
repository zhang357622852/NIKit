/// <summary>
/// GuideMgr.cs
/// Created by zhaozy 2017-10-25
/// 指引管理模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using System.Reflection;
using System;

public static class GuideMgr
{
    #region 变量声明

    // 通天塔开启指引分组
    public static int TOWER_GROUP = 8;

    // 分享按钮显示判断引导组
    public static int SHARE_SHOW_GUIDE_GROUP = 4;

    // 指引默认步骤
    private static int DEFAULT_STEP = 0;

    // 配置信息
    private static CsvFile mGuideCsv;

    // 指引分组列表
    private static Dictionary<int, Dictionary<int, CsvRow>> mGuideMap = new Dictionary<int, Dictionary<int, CsvRow>>();

    // 指引组信息
    private static Dictionary<int, CsvRow> mGuideGroupMap = new Dictionary<int, CsvRow>();

    // 触发规则列表
    private static Dictionary<int, List<int>> mTriggerRuleMap = new Dictionary<int, List<int>>();

    #endregion

    #region 属性

    /// <summary>
    /// Gets the guide csv.
    /// </summary>
    /// <value>The guide csv.</value>
    public static CsvFile GuideCsv { get { return mGuideCsv; } }

    /// <summary>
    /// 当前指引对象
    /// </summary>
    public static Guide GuideOb { get; set; }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入配置表
        LoadFile();
    }

    /// <summary>
    /// 是否正在指引中
    /// </summary>
    /// <returns><c>true</c> if is guiding; otherwise, <c>false</c>.</returns>
    public static bool IsGuiding()
    {
        return (GuideOb != null);
    }

    /// <summary>
    /// 执行登陆指引
    /// 1. 继续上一次指引
    /// 2. 第一次登陆指引
    /// </summary>
    public static bool DoLoginGuide()
    {
        // 判断玩家是否有指引需要继续
        LPCValue guide = ME.user.Query<LPCValue>("guide");
        if (guide != null && guide.IsMapping)
        {
            // 转换数据格式
            LPCMapping guideMap = guide.AsMapping;

            // 判断是否有指引中断需要继续
            foreach (int group in guideMap.Keys)
            {
                if (! mGuideMap.ContainsKey(group))
                    continue;

                // 如果指引没有指引结束，需要继续指引
                if (guideMap[group].AsInt < (mGuideMap[group].Count - 1))
                {
                    // 继续上一次指引的下一步
                    return DoGuide(group, guideMap[group].AsInt + 1, null);
                }
            }
        }

        // 判断是否有新指引需要继续
        foreach (int group in mGuideGroupMap.Keys)
        {
            // 逐个判断是否需要触发
            if (DoGuide(group, DEFAULT_STEP, null))
                return true;
        }

        // 没有指引需要触发
        return false;
    }

    /// <summary>
    /// 返回至某一步指引
    /// </summary>
    public static bool ReturnGuide(int group, int step, CallBack cb = null)
    {
        // 结束指引
        GuideMgr.GuideOb = null;

        // 继续指引
        return GuideMgr.DoGuide(group, step);
    }

    /// <summary>
    /// 执行某组指引
    /// </summary>
    /// <returns><c>true</c>, if guide was triggered, <c>false</c> otherwise.</returns>
    /// <param name="group">Group.</param>
    /// <param name="cb">Cb.</param>
    public static bool DoGuide(int group, CallBack cb = null)
    {
        return DoGuide(group, DEFAULT_STEP, cb);
    }

    /// <summary>
    /// 执行某组指引
    /// </summary>
    public static bool DoGuide(int group, int step, CallBack cb = null, MixedValue para = null)
    {
        // 如果当前正在止指引中
        if (IsGuiding())
            return false;

        // 如果已经指引过了，不在重复指引
        if (IsGuided(group))
            return false;

        // 获取配置信息, 没有该组指引信息
        if (! mGuideMap.ContainsKey(group))
            return false;

        // 获取配置信息
        CsvRow data = null;
        if (! mGuideMap[group].TryGetValue(step, out data))
            return false;

        // 判断该组指引前驱指引是否已经完成
        // 如果前驱指引没有完成，则该组指引不能执行
        int preGroup = data.Query<int>("pre_group");
        if (preGroup != 0 && ! IsGuided(preGroup))
            return false;

        // 需要检查脚本判断
        int scriptNo = data.Query<int>("check_script");
        if (scriptNo > 0)
        {
            // 执行脚本
            bool ret = (bool) ScriptMgr.Call(scriptNo,
                data.Query<LPCValue>("check_args"),
                para);

            // 如果检查失败
            if (! ret)
                return false;
        }

        // 构建指引对象对象
        Guide guideOb = new Guide(group, step, cb, para);

        // 创建指引对象失败
        if (guideOb == null)
            return false;

        // 保存guideOb
        GuideOb = guideOb;

        // 开始指引
        GuideOb.DoStart();

        // 执行指引成功
        return true;
    }

    /// <summary>
    /// Gets the guide data.
    /// </summary>
    /// <returns>The guide data.</returns>
    /// <param name="group">Group.</param>
    /// <param name="id">Identifier.</param>
    public static CsvRow GetGuideData(int group, int id)
    {
        // 获取配置信息, 没有该组指引信息
        if (! mGuideMap.ContainsKey(group))
            return null;

        // 查找id相关信息
        CsvRow data;
        if (!mGuideMap[group].TryGetValue(id, out data))
            return null;

        // 返回配置信息
        return data;
    }

    /// <summary>
    /// 指引是否已经显示结束
    /// </summary>
    public static bool IsGuided(int group)
    {
        // 获取配置信息, 默认获取该组指引第一步
        CsvRow data = GetGuideData(group, 0);

        // 没有配置的数据
        if (data == null)
            return false;

        // 如果是游戏外指引
        if (data.Query<int>("in_game") == 0)
        {
            LPCMapping localGuide = LPCMapping.Empty;

            // 标识本组指引已经结束
            LPCValue account = Communicate.AccountInfo.Query("account");
            if (account != null && account.IsString)
            {
                string accountStr = account.AsString;

                LPCValue v = OptionMgr.GetAccountOption(accountStr, "guide");
                if (v != null && v.IsMapping)
                    localGuide = v.AsMapping;
            }

            // 返回是否通关
            return (localGuide.GetValue<int>(group) == 1);
        }

        // 获取玩家详细指引信息
        LPCValue guide = ME.user.Query<LPCValue>("guide");
        if (guide == null || ! guide.IsMapping)
            return false;

        // 还没有该指引组信息
        LPCMapping guideMap = guide.AsMapping;
        if (! guideMap.ContainsKey(group))
            return false;

        // 获取当前该组指引进行阶段
        // 该组指引已经完成
        return (guideMap.GetValue<int>(group) >= (mGuideMap[group].Count - 1));
    }

    /// <summary>
    /// 某阶段指引是否完成
    /// </summary>
    public static bool StepUnlock(int group, int step)
    {
        // 获取玩家详细指引信息
        LPCValue guide = ME.user.Query<LPCValue>("guide");
        if (guide == null || ! guide.IsMapping)
            return false;

        // 还没有该指引组信息
        LPCMapping guideMap = guide.AsMapping;
        if (! guideMap.ContainsKey(group))
            return false;

        return (guideMap.GetValue<int>(group) >= step);
    }

    // 重置指引对象
    public static void ResetGuide()
    {
        GuideOb = null;
    }

    /// <summary>
    /// 所有指引是否完成
    /// </summary>
    public static bool IsGuideEnd()
    {
        foreach (int group in mGuideMap.Keys)
        {
            CsvRow row = mGuideMap[group][0];
            if (row == null)
                continue;

            if (row.Query<int>("in_game") == 1 && !IsGuided(group))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 指引是否完成,分游戏内,游戏外
    /// </summary>
    public static bool IsGuideEnd(bool inGame)
    {
        foreach (int group in mGuideMap.Keys)
        {
            int in_game = mGuideMap[group][DEFAULT_STEP].Query<int>("in_game");


            if (inGame)
            {
                if (in_game == 1 && !IsGuided(group))
                    return false;
            }
            else
            {
                if (in_game != 1 && !IsGuided(group))
                    return false;
            }
        }

        return true;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 载入配置表
    /// </summary>
    private static void LoadFile()
    {
        // 载入字段配置表
        mGuideCsv = CsvFileMgr.Load("guide");
        if (mGuideCsv == null)
            return;

        // 清除指引数据
        mGuideMap.Clear();
        mTriggerRuleMap.Clear();
        mGuideGroupMap.Clear();

        // 遍历数据获取各个组指引的触发规则
        foreach (CsvRow data in mGuideCsv.rows)
        {
            if (data == null)
                continue;

            // 获取指引信息group
            int group = data.Query<int>("group");
            int id = data.Query<int>("id");

            // 初始化数据
            if (!mGuideMap.ContainsKey(group))
                mGuideMap.Add(group, new Dictionary<int, CsvRow>());

            if (mGuideMap[group].ContainsKey(id))
            {
                Debug.LogError(group);
                Debug.LogError(id);
            }

            // 添加数据
            mGuideMap[group].Add(id, data);

            // 获取改组第一个指引触发事件
            if (!mGuideGroupMap.ContainsKey(group))
                mGuideGroupMap.Add(group, data);
            else if(mGuideGroupMap[group].Query<int>("id") > id)
                mGuideGroupMap[group] = data;
        }

        // 遍历数据
        foreach(CsvRow data in mGuideGroupMap.Values)
        {
            if (data == null)
                continue;

            // 获取触发事件
            string trigger = data.Query<string>("trigger");
            if (string.IsNullOrEmpty(trigger))
                continue;

            // 如果没有触发事件，不处理
            int emet = EventMgrEventType.GetEventTypeByAlias(trigger);
            if (emet == EventMgrEventType.EVENT_NULL)
                continue;

            // 添加触发规则列表
            if (!mTriggerRuleMap.ContainsKey(emet))
                mTriggerRuleMap.Add(emet, new List<int>() { data.Query<int>("group") });
            else
                mTriggerRuleMap[emet].Add(data.Query<int>("group"));

            // 注册触发事件
            EventMgr.RegisterEvent("GuideMgr", emet, TryDoGuide);
        }
    }

    /// <summary>
    /// Tries the trigger guide.
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private static void TryDoGuide(int eventId, MixedValue para)
    {
        // 该事件不需要触发，不处理
        if (!mTriggerRuleMap.ContainsKey(eventId))
            return;

        // 尝试触发各个指引
        foreach (int group in mTriggerRuleMap[eventId])
        {
            // 尝试指引, 如果有指引需要指引，则不能再发起其他指引
            if (DoGuide(group, DEFAULT_STEP, null, para))
                break;
        }
    }

    #endregion
}
