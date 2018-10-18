/// <summary>
/// CommonBonusMgr.cs
/// Created by cql 2015/04/25
/// 通用奖励管理
/// </summary>

using LPC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonBonusMgr
{

    #region 成员变量

    // 奖励类型
    public const int NEW_USER_BONUS = 1;
    public const int SIGN_BONUS = 2;
    public const int LEVEL_BONUS = 3;

    // 奖励信息缓存
    private static Dictionary<int, List<CsvRow>> mtypeBonus = new Dictionary<int, List<CsvRow>>();

    #endregion

    #region

    #endregion

    #region 内部函数

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        // 如果没有有新的签到
        if (! GuideMgr.IsGuided(GuideConst.ARENA_FINISH) || !HasNewSign(ME.user))
            return;

        // 打开签到窗口
        GameObject go = WindowMgr.OpenWnd("SignWnd");

        go.GetComponent<SignWnd>().SetLoginFlag(true);
    }

    #endregion

    #region 公共函数

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 注册登陆成功回调
        EventMgr.UnregisterEvent("CommonBonusMgr");
        EventMgr.RegisterEvent("CommonBonusMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);

        // 载入配置表
        CsvFile bonusCsv = CsvFileMgr.Load("common_bonus");
        if (bonusCsv == null)
            return;

        // 清除数据
        mtypeBonus.Clear();

        // 遍历各行数据
        foreach (CsvRow data in bonusCsv.rows)
        {
            // 获取奖励类型
            int bonusType = data.Query<int>("bonus_type");

            // 判断是否已经添加了该奖励类型
            if (mtypeBonus.ContainsKey(bonusType))
                mtypeBonus[bonusType].Add(data);
            else
                mtypeBonus.Add(bonusType, new List<CsvRow>(){ data });
        }
    }

    /// <summary>
    /// 获取某个类型的奖励配置列表
    /// </summary>
    public static List<CsvRow> GetBonusList(int type)
    {
        // 判断是否已经添加了该奖励类型
        if (!mtypeBonus.ContainsKey(type))
            return new List<CsvRow>();

        // 返回数据
        return mtypeBonus[type];
    }

    /// <summary>
    /// 是否可以领取等级奖励
    /// </summary>
    public static bool CanReceiveLevelBonus(Property user, int level)
    {
        // 获取可以领取奖励列表
        LPCArray bonus = user.Query<LPCArray>("level_bonus/bonus");

        // 没有可以领取列表
        if (bonus == null)
            return  false;

        // 返回是否可以领取
        return (bonus.IndexOf(level) != -1);
    }

    /// <summary>
    /// 等级奖励是否已经领取
    /// </summary>
    public static bool IsReceivedLevleBonus(Property user, int level)
    {
        // 获取已经领取列表
        LPCArray received = user.Query<LPCArray>("level_bonus/received");

        // 没有可以领取列表
        if (received == null)
            return  false;

        // 返回是否已经领取
        return (received.IndexOf(level) != -1);
    }

    /// <summary>
    /// 领取日常奖励
    /// </summary>
    public static void DoSign(Property who)
    {
        // 没有新的签到，不能处理
        if (!HasNewSign(who))
            return;

        // 通知服务器领取签到
        Operation.CmdSign.Go();
    }

    /// <summary>
    /// 是否有新签到
    /// </summary>
    public static bool HasNewSign(Property who)
    {
        // 对象已经析构或者正在析构过程中
        if (who == null || who.IsDestroyed)
            return false;

        // 获取玩家的签到标识
        int signFlag = who.Query<int>("sign_bonus/sign_flag");

        // 判断是否有新签到
        return (signFlag == 1);
    }

    /// <summary>
    /// 是否有可领取的奖励
    /// </summary>
    /// <returns><c>true</c> if has receivng level bonus the specified who; otherwise, <c>false</c>.</returns>
    /// <param name="who">Who.</param>
    public static bool HasReceivngLevelBonus(Property who)
    {
        // 获取可以领取奖励列表
        LPCArray bonus = who.Query<LPCArray>("level_bonus/bonus");

        // 没有可以领取列表
        if (bonus == null || bonus.Count == 0)
            return  false;

        // 返回是否已经领取
        return true;
    }

    #endregion

}
