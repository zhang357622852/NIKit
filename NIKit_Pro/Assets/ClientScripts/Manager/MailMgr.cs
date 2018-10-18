/// <summary>
/// MailMgr.cs
/// Created by xuhd Apr/18/2015
/// 邮箱管理器
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using System;

public class MailMgr
{
    #region 变量

    // 客户端邮件数据
    private static List<LPCMapping> mExpressDetialData = new List<LPCMapping>();

    // 邮件cookie信息
    private static string mCookie = string.Empty;

    #endregion

    #region 属性

    /// <summary>
    /// 新邮件标识
    /// </summary>
    public static bool HasNewExpress { get; private set; }

    /// <summary>
    /// 邮件系统是否已经初始化
    /// </summary>
    public static bool IsInit { get; set; }

    #endregion

    #region 内部函数

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        // 指引没有解锁,没有新邮件
        if (! GuideMgr.IsGuided(GuideConst.ARENA_FINISH) || !HasNewExpress)
            return;

        // 主动打开邮件窗口
        WindowMgr.OpenWnd("MailWnd");
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 注册登陆成功回调
        EventMgr.UnregisterEvent("MailMgr");
        EventMgr.RegisterEvent("MailMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);
    }

    /// <summary>
    /// 判断是否有没有邮件没有读取
    /// </summary>
    public static bool HasUnReadExpress()
    {
        // 有新邮件
        if (HasNewExpress)
            return true;

        // 遍历本地缓存数据判断是否有邮件没有查看
        foreach (LPCMapping expressData in mExpressDetialData)
        {
            // 没有读取的邮件
            if (expressData.GetValue<int>("state") != ExpressStateType.EXPRESS_STATE_READ)
                return true;
        }

        // 没有新邮件
        return false;
    }

    /// <summary>
    /// 设置邮件cookie
    /// </summary>
    public static void SetCookie(string cookie)
    {
        mCookie = cookie;

        // 重置数据
        mExpressDetialData.Clear();
    }

    /// <summary>
    /// 新邮件消息
    /// </summary>
    public static void NotifyNewExpress()
    {
        // 重置邮件数据
        SetCookie(Game.NewCookie("express_get"));

        // 标识新邮件
        HasNewExpress = true;
    }

    /// <summary>
    /// 邮件简易信息
    /// </summary>
    public static void DoCacheExpressDesc(string cookie, LPCMapping expressData)
    {
        // 玩家对象不存在
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        // cookie不一致
        if (cookie != mCookie)
            return;

        // 重置标识
        HasNewExpress = false;

        // 添加到列表中
        mExpressDetialData.Add(expressData);
    }

    /// <summary>
    /// 邮件过期
    /// </summary>
    public static void DoExpressInvalid(string expressRid)
    {
        // 玩家对象不存在
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        // 修改邮件标识
        LPCMapping expressData = GetExpressDetialData(expressRid);

        // 没有获取到数据
        if (expressData == null)
            return;

        // 清除缓存列表
        mExpressDetialData.Remove(expressData);
    }

    /// <summary>
    /// 提取邮件附件
    /// </summary>
    public static void TakeExpressProperty(string expressRid, LPCMapping para)
    {
        if (ME.user == null)
            return;

        // 获取本地邮件数据
        LPCMapping expressData = GetExpressDetialData(expressRid);

        // 没有获取到数据
        if (expressData == null)
            return;

        // 邮件已经提取过不处理
        if (expressData.GetValue<int>("is_rewarded") == 1)
        {
            DialogMgr.Notify(LocalizationMgr.Get("MailWnd_12"));
            return;
        }

        // 邮件失效
        if (expressData.GetValue<int>("state") == ExpressStateType.EXPRESS_STATE_INVALID)
            return;

        // 玩家发送的邮件（友情点数）
        if (expressData.GetValue<string>("from_rid") != ExpressStateType.SYSTEM_EXPRESS)
        {
            LPCValue v = ME.user.Query("fp");
            if (v != null && v.IsInt)
            {
                if (v.AsInt >= GameSettingMgr.GetSettingInt("max_fp_limit"))
                {
                    // 弹出提示框, 友情点达到最大限制
                    DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("MyFriendWnd_10"));

                    return;
                }
            }
        }

        // 通知服务器同步数据
        Operation.CmdExpressTakeProperty.Go(expressRid, para);
    }

    /// <summary>
    /// 提取所有邮件, 目前只有友情点数可以领取，后续有其他需求再增加
    /// </summary>
    public static void TakeAllExpressProperty(Property user)
    {
        if (user == null)
            return;

        LPCValue v = user.Query("fp");
        if (v != null && v.IsInt)
        {
            if (v.AsInt >= GameSettingMgr.GetSettingInt("max_fp_limit"))
            {
                // 弹出提示框, 友情点达到最大限制
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("MyFriendWnd_10"));

                return;
            }
        }

        // 领取所有的友情点数邮件
        Operation.CmdTakeAllFriendPoint.Go();
    }

    /// <summary>
    /// 读取指定邮件
    /// </summary>
    public static void DoReadExpress(string expressRid)
    {
        // 获取本地邮件数据
        LPCMapping expressData = GetExpressDetialData(expressRid);

        // 没有获取到数据或者邮件已经读取过不处理
        if (expressData == null ||
            expressData.GetValue<int>("state") == ExpressStateType.EXPRESS_STATE_READ)
            return;

        // 标识邮件为一度状态
        expressData.Add("state", ExpressStateType.EXPRESS_STATE_READ);

        // 通知服务器同步数据
        Operation.CmdExpressRead.Go(expressRid);

        // 模拟服务器下发消息
        LPCMapping msgArgs = new LPCMapping();
        msgArgs.Add("oper", "read");
        msgArgs.Add("result", 1);

        // 模拟服务器下发MSG_EXPRESS_OPERATE_DONE消息
        MsgMgr.Execute("MSG_EXPRESS_OPERATE_DONE", LPCValue.Create(msgArgs));
    }

    /// <summary>
    /// 获取邮件详细信息
    /// </summary>
    public static LPCMapping GetExpressDetialData(string expressRid)
    {
        foreach (LPCMapping expressData in mExpressDetialData)
        {
            // 不是指定的邮件
            if (expressRid != expressData.GetValue<string>("rid"))
                continue;

            // 返回邮件详细信息
            return expressData;
        }

        // 返回null
        return null;
    }

    /// <summary>
    /// 请求获取邮件列表
    /// </summary>
    public static void RequestGetExpressList()
    {
        // 有新邮件或者没有初始化过
        if (HasNewExpress || !IsInit)
        {
            // 通知服务器同步数据
            Operation.CmdExpressGetList.Go();
            return;
        }

        // 模拟服务器下发消息
        LPCMapping msgArgs = new LPCMapping();
        msgArgs.Add("oper", "get_list");
        msgArgs.Add("result", 1);

        // 模拟服务器下发MSG_EXPRESS_OPERATE_DONE消息
        MsgMgr.Execute("MSG_EXPRESS_OPERATE_DONE", LPCValue.Create(msgArgs));
    }

    /// <summary>
    /// 重置新邮件标识
    /// </summary>
    public static void ResetNewExpress(bool isNew)
    {
        HasNewExpress = isNew;
    }

    /// <summary>
    /// 重置邮件数据
    /// </summary>
    public static void DoResetAll()
    {
        // 初始化邮件系统
        IsInit = false;
        HasNewExpress = false;
        mCookie = string.Empty;

        // 清除缓存信息
        mExpressDetialData.Clear();
    }

    /// <summary>
    /// 获取邮件列表
    /// </summary>
    public static List<LPCMapping> GetExpressList()
    {
        // 邮件排序
        return SortExpress(mExpressDetialData);
    }

    /// <summary>
    /// 获取系统邮件列表
    /// </summary>
    public static List<LPCMapping> GetSystemExpressList()
    {
        List<LPCMapping> systemList = new List<LPCMapping>();

        foreach (LPCMapping data in mExpressDetialData)
        {
            if (!data.GetValue<string>("from_rid").Equals(ExpressStateType.SYSTEM_EXPRESS)
                || data.GetValue<int>("state") == ExpressStateType.EXPRESS_STATE_INVALID)
                continue;

            if (TimeMgr.GetServerTime() > data.GetValue<int>("expire"))
                continue;

            systemList.Add(data);
        }

        // 返回一个排序的列表
        return SortExpress(systemList);
    }

    /// <summary>
    /// 获取玩家发送的邮件列表
    /// </summary>
    public static List<LPCMapping> GetUserExpressList()
    {
        List<LPCMapping> systemList = new List<LPCMapping>();

        foreach (LPCMapping data in mExpressDetialData)
        {
            if (data.GetValue<string>("from_rid").Equals(ExpressStateType.SYSTEM_EXPRESS)
                || data.GetValue<int>("state") == ExpressStateType.EXPRESS_STATE_INVALID)
                continue;

            if (TimeMgr.GetServerTime() > data.GetValue<int>("expire"))
                continue;

            systemList.Add(data);
        }

        // 返回一个排序的列表
        return SortExpress(systemList);
    }

    /// <summary>
    /// 排序邮件.
    /// </summary>
    private static List<LPCMapping> SortExpress(List<LPCMapping> expressList)
    {
        // 没有读取的邮件列表
        List<LPCMapping> mNoReadMail = new List<LPCMapping>();

        // 已经读取的邮件
        List<LPCMapping> mReadMail = new List<LPCMapping>();

        if (expressList == null || expressList.Count <= 0)
            return new List<LPCMapping>();

        for (int i = 0; i < expressList.Count; i++)
        {
            LPCMapping data = expressList[i];
            if (data == null)
                continue;

            if (data.GetValue<int>("state") == ExpressStateType.EXPRESS_STATE_READ)
                mReadMail.Add(data);
            else
                mNoReadMail.Add(data);
        }

        // 按照剩余时间排序列表
        mNoReadMail.Sort((Comparison<LPCMapping>)delegate(LPCMapping a, LPCMapping b){
            return a.GetValue<int>("expire").CompareTo(b.GetValue<int>("expire"));
        });

        // 按照剩余时间排序列表
        mReadMail.Sort((Comparison<LPCMapping>)delegate(LPCMapping a, LPCMapping b){
            return a.GetValue<int>("expire").CompareTo(b.GetValue<int>("expire"));
        });

        mNoReadMail.AddRange(mReadMail);

        return mNoReadMail;
    }

    #endregion
}
