/// <summary>
/// WaittingWnd.cs
/// Created by xuhd Feb/09/2015
/// 通信中窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class WaittingWnd : WindowBase<WaittingWnd>
{
    #region 公共字段

    public UILabel mTips;      // 文字提示

    #endregion

    #region 内部成员

    private string mTipsString = string.Empty;

    // 窗口绑定mOrderId
    private string mOrderId = string.Empty;

    #endregion

    #region 内部接口

    // Use this for initialization
    void Start()
    {
        mTips.text = mTipsString;

        // 屏幕适配
        UIPanel panel = GetComponent<UIPanel>();
        panel.SetAnchor(WindowMgr.UIRoot);
        panel.leftAnchor.Set(0f, -10f);
        panel.rightAnchor.Set(1f, 10f);
        panel.bottomAnchor.Set(0f, -10f);
        panel.topAnchor.Set(1f, 10f);

        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 销毁窗口的处理.
    /// </summary>
    void OnDestroy()
    {
        UnRegisterEvent();
    }

    /// <summary>
    /// 显示时注销回调消息
    /// </summary>
    void OnEnable()
    {
        // 重置mOrderId
        mOrderId = string.Empty;
    }

    /// <summary>
    /// 隐藏时注销回调消息
    /// </summary>
    void OnDisable()
    {
    }

    /// <summary>
    /// 注册事件.
    /// </summary>
    private void RegisterEvent()
    {
        // 关注角色购买消息MSG_DO_CHECK_PURCHASE
        MsgMgr.RemoveDoneHook("MSG_DO_CHECK_PURCHASE", gameObject.name);
        MsgMgr.RegisterDoneHook("MSG_DO_CHECK_PURCHASE", gameObject.name, OnMsgDoCheckPurchase);
    }

    /// <summary>
    /// 取消注册事件.
    /// </summary>
    private void UnRegisterEvent()
    {
        // 关注角色列表数据
        MsgMgr.RemoveDoneHook("MSG_DO_CHECK_PURCHASE", gameObject.name);
    }

    /// <summary>
    /// MSG_DO_CHECK_PURCHASE回调
    /// </summary>>
    private void OnMsgDoCheckPurchase(string cmd, LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        string orderId = args.GetValue<string>("order_id");

        // 不是需要等待的订单编号
        if (! string.Equals(orderId, mOrderId))
            return;

        // 关闭本窗口
        WindowMgr.HideWindow(gameObject);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 设置提示信息
    /// </summary>
    public void SetTipMsg(string tips)
    {
        if (! gameObject.activeSelf)
            return;

        mTipsString = tips;
        mTips.text = mTipsString;
    }

    /// <summary>
    /// 设置绑定的orderId
    /// 需要等待服务器处理完成后关闭该窗口
    /// </summary>
    public void SetBindOrderId(string orderId)
    {
        if (! gameObject.activeSelf)
            return;

        mOrderId = orderId;
    }

    #endregion
}
