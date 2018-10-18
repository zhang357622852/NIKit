/// <summary>
/// ShareMgr.cs
/// Created by zhaozy 2018-06-27
/// 分享管理
/// </summary>

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using LPC;

/// <summary>
/// 1. 读取内置配置，作为全局配置
/// 2. 合并配置信息，保存
/// </summary>
public static class ShareMgr
{
    #region 成员变量;
    // 帮助信息配置表;
    private static CsvFile mShareCsv;
    #endregion

    #region 属性
    // 帮助信息配置表
    public static CsvFile ShareCsv { get { return mShareCsv; } }
    #endregion

    #region 外部接口

    /// <summary>
    /// Gets the shara platform list.
    /// </summary>
    /// <returns>The shara platform list.</returns>
    public static List<string> GetSharePlatformList()
    {
#if UNITY_EDITOR

        // 编辑器下如果是非国内版本
        if (! ConfigMgr.IsCN)
            return new List<string>(){ SharePlatformType.Facebook, SharePlatformType.Twitter, SharePlatformType.Line };

        // 默认是是"wechat", "wechat_moments", "sina"
        return new List<string>(){ SharePlatformType.Wechat, SharePlatformType.WechatMoments, SharePlatformType.Sina };

#else

        // 获取share_platform列表
        IList sharePlatforms = (IList) ConfigMgr.Get<JsonData> ("share_platform", null);
        if (sharePlatforms == null ||
            sharePlatforms.Count == 0)
            return new List<string>();

        // 转换数据格式
        List<string> platformList = new List<string>();
        for(int i = 0; i < sharePlatforms.Count; i++)
            platformList.Add(sharePlatforms[i].ToString());

        // 返回platformList
        return platformList;

#endif
    }

    /// <summary>
    /// 是否开启分享
    /// </summary>
    /// <returns><c>true</c> if is open share; otherwise, <c>false</c>.</returns>
    public static bool IsOpenShare()
    {
        return GetSharePlatformList().Count > 0;
    }

    /// <summary>
    ///初始化数据
    /// </summary>
    public static void Init()
    {
        mShareCsv = CsvFileMgr.Load("share");
    }

    public static CsvRow GetShareCsvById(int id)
    {
        return mShareCsv.FindByKey(id);
    }

    /// <summary>
    /// 获取随机分享界面背景组合Csv数据
    /// </summary>
    /// <returns></returns>
    public static CsvRow FetchShareRule()
    {
        LPCArray weightList = LPCArray.Empty;
        for (int i = 0; i < mShareCsv.count; i++)
            weightList.Add(mShareCsv[i].Query<int>("weight"));

        int index = RandomMgr.CompleteRandomSelect(weightList);
        if (index == -1)
            return null;

        // 返回抽取
        return mShareCsv[index];
    }

    #endregion
}
