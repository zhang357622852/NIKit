/// <summary>
/// LotteryBonusMgr.cs
/// Created by zhaozy 2016/11/17
/// 抽奖管理模块
/// </summary>

using System.Collections;
using UnityEngine;
using LPC;
using QCCommonSDK;

public static class LotteryBonusMgr
{
    #region 变量

    // 抽奖奖励配置表
    private static CsvFile mLotteryBonusCsv;

    #endregion

    #region 属性

    // 获取抽奖奖励配置表
    public static CsvFile LotteryBonusCsv { get { return mLotteryBonusCsv; } }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入抽奖奖励配置表
        mLotteryBonusCsv = CsvFileMgr.Load("lottery_bonus");
    }

    /// <summary>
    /// 获取抽奖配置信息
    /// </summary>
    public static CsvRow GetLotteryBonus(int lotteryId)
    {
        return LotteryBonusCsv.FindByKey(lotteryId);
    }

    #endregion
}
