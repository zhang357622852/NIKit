/// <summary>
/// FriendAmountWnd.cs
/// Created by fengsc 2017/02/07
/// 好友数量窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FriendAmountWnd : WindowBase<FriendAmountWnd>
{
    // 好友数量
    public UILabel mAmount;

    // Use this for initialization
    void Start ()
    {
        // 监听好友列表变化事件
        EventMgr.RegisterEvent(FriendAmountWnd.WndType, EventMgrEventType.EVENT_FRIEND_NOTIFY_LIST, OnFriendNotifyList);

        // 监听好友操作事件
        EventMgr.RegisterEvent(FriendAmountWnd.WndType, EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, OnFriendOperateDone);

        // 绘制窗口
        Redraw();
    }

    void OnDisable()
    {
        // 解注册事件
        EventMgr.UnregisterEvent(FriendAmountWnd.WndType);
    }

    /// <summary>
    /// 好友操作事件回调
    /// </summary>
    void OnFriendOperateDone(int eventId, MixedValue para)
    {
        // 刷新好友数量
        Redraw();
    }

    /// <summary>
    /// 好友列表变化事件回调
    /// </summary>
    void OnFriendNotifyList(int eventId, MixedValue pare)
    {
        // 刷新好友数量
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 显示好友数量
        mAmount.text = string.Format("{0}/{1}", FriendMgr.FriendList.Count, GameSettingMgr.GetSettingInt("max_friend_amount"));
    }
}
