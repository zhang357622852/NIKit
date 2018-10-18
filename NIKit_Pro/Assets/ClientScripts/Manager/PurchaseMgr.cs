/// <summary>
/// PurchaseMgr.cs
/// Create by zhaozy 2015-03-04
/// 内购管理管理模块
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using LPC;
using QCCommonSDK;
using QCCommonSDK.Addition;

/// <summary>
/// 内购管理管理模块
/// </summary>
public static class PurchaseMgr
{
    #region 变量

    // 定时轮训时间间隔
    private static float mIntervalTime = 0f;

    // 订单列表信息
    private static Dictionary<string, string> mOrderMap = new Dictionary<string, string>();

    #endregion

    #region 内部接口

    /// <summary>
    /// 内购结束的回调
    /// </summary>
    private static void PurchaseHook(QCEventResult result)
    {
        // 获取窗口
        GameObject wnd = WindowMgr.GetWindow(WaittingWnd.WndType);

        // 判断等待窗口是否已经存在
        if (wnd == null || !wnd.activeSelf)
            return;

        // 账号还没有通过认证
        // 不执行任何内购相关逻辑
        if (!AccountMgr.Ischecked)
        {
            // 异常直接关闭等待窗口
            WindowMgr.HideWindow(wnd);

            return;
        }

        // 获取返回error code信息
        int resultCode = (int)result.error;

        // 内购异常
        if (resultCode != 0 && resultCode != -200)
        {
            // 异常直接关闭等待窗口
            WindowMgr.HideWindow(wnd);

            // 返回
            return;
        }

        if (resultCode == -200)
        {
#if !UNITY_EDITOR && UNITY_IPHONE
            // 设置窗口绑定的提示信息
            wnd.GetComponent<WaittingWnd>().SetTipMsg(LocalizationMgr.Get("PurchaseMgr_2"));
#endif
        }

        // 获取订单号
        string orderId = result.result.ContainsKey("order") ? result.result ["order"] : "";
        if (string.IsNullOrEmpty(orderId))
            return;

        // 缓存数据
        if (!mOrderMap.ContainsKey(orderId))
            mOrderMap.Add(orderId, result.result ["sku"]);

        // 绑定订单号
        wnd.GetComponent<WaittingWnd>().SetBindOrderId(orderId);

#if !UNITY_EDITOR && UNITY_ANDROID
        // 设置窗口绑定的提示信息
        wnd.GetComponent<WaittingWnd>().SetTipMsg(LocalizationMgr.Get("PurchaseMgr_1"));
#endif

#if  !UNITY_EDITOR

        // 生成渠道account
        string channelAccount = string.Format("{0}{1}",
        QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL"),
        Communicate.AccountInfo.Query("account", ""));

        DataAnalyzeSupport.RecordEvent("purchase", new Dictionary<string, string>() {
        {"account", channelAccount},
        {"role", ME.user.GetRid()},
        {"role_name", ME.user.GetName()},
        {"server", Communicate.AccountInfo.Query("server_id", "")},
        {"grade", ME.user.GetLevel().ToString()},
        {"gold_coin", ME.user.Query<int>("gold_coin").ToString()},
        {"money", ME.user.Query<int>("money").ToString()},
        {"create_time",  ME.user.Query<int>("create_time").ToString()},
        {"order_id", result.result ["order"]},
        {"pay_type", result.result ["pay_type"]},
        {"currency", result.result ["currency"]},
        {"price", result.result ["price"]},
        {"sku_id",  result.result ["sku"]}
        });

#endif
    }

    /// <summary>
    /// MSG_DO_CHECK_PURCHASE回调
    /// </summary>>
    private static void OnMsgDoCheckPurchase(string cmd, LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        string order_id = args.GetValue<string>("order_id");
        string skuid = string.Empty;

        // 判断是否在缓存列表中
        if (mOrderMap.ContainsKey(order_id))
        {
            // 获取商品id
            skuid = mOrderMap[order_id];

            // 清除缓存列表
            mOrderMap.Remove(order_id);
        }

    }


    #endregion

    #region 公共接口

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
        // 清空数据
        mOrderMap.Clear();

        // 关注角色购买消息MSG_DO_CHECK_PURCHASE
        MsgMgr.RemoveDoneHook("MSG_DO_CHECK_PURCHASE", "PurchaseMgr");
        MsgMgr.RegisterDoneHook("MSG_DO_CHECK_PURCHASE", "PurchaseMgr", OnMsgDoCheckPurchase);

#if ! UNITY_EDITOR
        // 非编辑器模式下的
        PurchaseSupport.OnPurchaseResult -= PurchaseHook;
        PurchaseSupport.OnPurchaseResult += PurchaseHook;
        PurchaseSupport.DoInit();
#endif
    }

    /// <summary>
    /// 清除对象列表
    /// </summary>
    public static void CleanOrderMap()
    {
        mOrderMap.Clear();
    }

    /// <summary>
    /// 自动对账处理
    /// </summary>
    public static void Update()
    {
        // 没有订单信息
        if (mOrderMap.Count == 0)
            return;

        // 累计流逝时间
        mIntervalTime -= Time.unscaledDeltaTime;

        // 时间还不到
        if (mIntervalTime > 0f)
            return;

        // 主动向服务器询问时间间隔为2s
        mIntervalTime = 2f;

        // 发起向服务器对账操作
        foreach (KeyValuePair<string, string> data in mOrderMap)
            Operation.CmdDoCheckPurchase.Go(data.Key);
    }

    /// <summary>
    /// 购买道具
    /// </summary>
    /// <param name="skuId">商品id</param>
    public static void DoPurchase(string skuId, string payType = "")
    {
#if ! UNITY_EDITOR
        // 玩家对象不存在
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        // 发起购买操作
        PurchaseSupport.Purchase(
            Communicate.AccountInfo.Query("account", ""),
            ME.GetRid(),
            Communicate.AccountInfo.Query("server_id", ""),
            skuId,
            payType
        );
#endif
    }

    #endregion

}
