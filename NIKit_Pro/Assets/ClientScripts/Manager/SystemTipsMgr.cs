/// <summary>
/// SystemTipsMgr.cs
/// Created by xuhd Nov/20/2014
/// 系统消息管理器
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 系统消息管理器
/// </summary>
public class SystemTipsMgr
{
    #region 变量

    // start配置表信息
    private static Dictionary<string, Dictionary<int, List<CsvRow>>> mStartTipsConfig = new Dictionary<string, Dictionary<int, List<CsvRow>>>();

    private static Dictionary<string, Dictionary<int, List<CsvRow>>> mGroupTipsConfig = new Dictionary<string, Dictionary<int, List<CsvRow>>>();

    #endregion

    #region 外部接口

    /// <summary>
    /// 初始化start
    /// </summary>
    public static void InitStart()
    {
        // 清除数据
        mStartTipsConfig.Clear();

        // 载入配置表
        // 载入配置表信息
        CsvFile tipsCsvFile = CsvFileMgr.Load("system_tips");
        if (tipsCsvFile == null)
            return;

        // 遍历各行数据 
        foreach (CsvRow data in tipsCsvFile.rows)
        {
            // 转换数据格式为LPC mapping数据格式
            LPCValue type = data.Query<LPCValue>("tip_type");
            string tipType = string.Empty;

            if (type.IsInt)
            {
                switch (type.AsInt)
                {
                    case 0:
                        tipType = "main_town";
                        break;

                    case 1:
                        tipType = "instance";
                        break;

                    case 2:
                        tipType = "resource_update";
                        break;
                }
            }
            else
            {
                tipType = type.AsString;
            }

            int group = data.Query<int>("group");

            Dictionary<int, List<CsvRow>> groupDic = null;

            if (!mStartTipsConfig.TryGetValue(tipType, out groupDic))
                groupDic = new Dictionary<int, List<CsvRow>>();

            List<CsvRow> tipsList = null;

            if (!groupDic.TryGetValue(group, out tipsList))
                tipsList = new List<CsvRow>();

            tipsList.Add(data);

            groupDic[group] = tipsList;

            // 记录数据
            mStartTipsConfig[tipType] = groupDic;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 清除数据
        mGroupTipsConfig.Clear();

        // 载入配置表
        // 载入配置表信息
        CsvFile tipsCsvFile = CsvFileMgr.Load("system_tips_ex"); 
        if (tipsCsvFile == null)
            return;

        // 遍历各行数据 
        foreach (CsvRow data in tipsCsvFile.rows)
        {
            // 转换数据格式为LPC mapping数据格式
            string type = data.Query<string>("tip_type");

            int group = data.Query<int>("group");

            Dictionary<int, List<CsvRow>> groupDic = null;

            if (!mGroupTipsConfig.TryGetValue(type, out groupDic))
                groupDic = new Dictionary<int, List<CsvRow>>();

            List<CsvRow> tipList = null;

            if (! groupDic.TryGetValue(group, out tipList))
                tipList = new List<CsvRow>();

            tipList.Add(data);

            groupDic[group] = tipList;

            mGroupTipsConfig[type] = groupDic;
        }
    }

    /// <summary>
    /// 是否有某个类型提示
    /// </summary>
	public static bool HasTipType(string type, bool isStartTip = false)
    {
        Dictionary<string, Dictionary<int, List<CsvRow>>> config = isStartTip ? mStartTipsConfig : mGroupTipsConfig;

        return config.ContainsKey(type);
    }

    /// <summary>
    /// 抽取游戏场景中的tip
    /// </summary>
    /// <returns>The tip.</returns>
    /// <param name="type">Type.</param>
    public static string FetchTip(string type, LPCMapping extra_para = null, int group = 0)
    {
        return DoFetchTip(false, type, extra_para, group);
    }

    /// <summary>
    /// 抽取strart场景中的tip
    /// </summary>
    /// <param name="type">Type.</param>
    public static string FetchStartTip(string type, LPCMapping extra_para = null, int group = 0)
    {
        return DoFetchTip(true, type, extra_para, group);
    }

    /// <summary>
    /// 根据权重随机抽取一条提示信息
    /// </summary>
    private static string DoFetchTip(bool isStartTip, string type, LPCMapping extra_para, int group = 0)
    {
        Dictionary<string, Dictionary<int, List<CsvRow>>> config = isStartTip ? mStartTipsConfig : mGroupTipsConfig;

        Dictionary<int, List<CsvRow>> groupDic;

        // 没有指定类型
        if (! config.TryGetValue(type, out groupDic))
            return string.Empty;

        List<CsvRow> tipList;

        if (! groupDic.TryGetValue(group, out tipList))
            return string.Empty;

        if (tipList == null || tipList.Count == 0)
            return string.Empty;

        // 获取权重列表
        LPCArray weightList = new LPCArray();
        List<CsvRow> TipList = new List<CsvRow>();

        // 遍历提示列表
        foreach (CsvRow data in tipList)
        {
            // 数据格式不正确
            if (data == null)
                continue;

            // 权重weight
            int weight = 0;

            // 由于该表是ab资源在启动游戏时需要，需要和旧表兼容
            if (data.Contains("weight"))
            {
                weight = data.Query<int>("weight");
            }
            else
            {
                // 获取权重脚本
                int scriptNo = data.Query<int>("weight_script");

                // 没有权重脚本
                if (scriptNo == 0)
                    continue;

                // 计算权重
                weight = (int) ScriptMgr.Call(scriptNo, data.Query<LPCValue>("weight_args"), extra_para);
            }

            // 不能选择该提示信息
            if (weight == 0)
                continue;

            // 记录数据
            TipList.Add(data);
            weightList.Add(weight);
        }

        // 根据权重抽取
        int index = RandomMgr.CompleteRandomSelect(weightList);
        if (index == -1)
            return string.Empty;

        // 如果该条提示信息为空，则表示纯属占位置
        string tipDoc = TipList[index].Query<string>("tip_doc");
        if (string.IsNullOrEmpty(tipDoc))
            return string.Empty;

        // 返回提示信息
        return LocalizationMgr.Get(tipDoc, isStartTip ? LocalizationConst.START : LocalizationConst.ZH_CN);
    }

    #endregion
}
