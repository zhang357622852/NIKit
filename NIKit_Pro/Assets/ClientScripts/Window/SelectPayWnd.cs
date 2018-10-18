/// <summary>
/// SelectPayWnd.cs
/// Created by fengsc 2018/05/22
/// 选择支付窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class SelectPayWnd : WindowBase<SelectPayWnd>
{
    public GameObject mAlipayBtn;

    public GameObject mWeChatPayBtn;

    public GameObject mMask;

    private string mPurchaseId;

    // Use this for initialization
    void Start ()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mAlipayBtn).onClick = OnClickAlipayBtn;
        UIEventListener.Get(mWeChatPayBtn).onClick = OnClickWeChatPayBtn;
        UIEventListener.Get(mMask).onClick = OnClickMask;
    }

    void OnClickMask(GameObject go)
    {
        // 删除当前窗口
        WindowMgr.DestroyWindow(SelectPayWnd.WndType);
    }

    void OnClickAlipayBtn(GameObject go)
    {
        // 执行支付
        DoPurchase("alipay", mPurchaseId);

        // 删除当前窗口
        WindowMgr.DestroyWindow(SelectPayWnd.WndType);
    }

    void OnClickWeChatPayBtn(GameObject go)
    {
        // 执行支付
        DoPurchase("wechat_pay", mPurchaseId);

        // 删除当前窗口
        WindowMgr.DestroyWindow(SelectPayWnd.WndType);
    }

    // 执行支付
    void DoPurchase(string payType, string purchaseId)
    {
#if ! UNITY_EDITOR
        // 显示购买等待窗口
#if UNITY_IPHONE
        WindowMgr.AddWaittingWnd("do_purchase", LocalizationMgr.Get("MarketWnd_22"));
#else
        WindowMgr.AddWaittingWnd("do_purchase");
#endif
        // 执行内购流程
        PurchaseMgr.DoPurchase(purchaseId, payType);
#endif
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string purchaseId)
    {
        mPurchaseId = purchaseId;
    }
}
