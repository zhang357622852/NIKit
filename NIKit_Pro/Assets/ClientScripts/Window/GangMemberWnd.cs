/// <summary>
/// GangMemberWnd.cs
/// Created by fengsc 2018/01/26
/// 公户成员信息
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangMemberWnd : WindowBase<GangMemberWnd>
{
    #region 成员变量

    // 公会成员数量
    public UILabel mMemberAmount;

    // 转让会长按钮
    public UILabel mTransferLeaderBtn;

    // 转让副会长按钮
    public UILabel mTransferDeputyLeaderBtn;

    // 公会成员基础格子
    public GameObject mGangMemberItemWnd;

    // 排序控件
    public UIGrid mGrid;

    List<GameObject> mItemList = new List<GameObject>();

    #endregion

    // Use this for initialization
    void Awake ()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mTransferLeaderBtn.gameObject).onClick = OnClickTransferLeaderBtn;
        UIEventListener.Get(mTransferDeputyLeaderBtn.gameObject).onClick = OnClicktransferDeputyLeaderBtn;

        // 初始化本地化文本
        InitText();
    }

    void OnEnable()
    {
        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }

    void OnDisable()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("GangMemberWnd");

        if (ME.user == null)
            return;

        // 移除字段关注
        ME.user.dbase.RemoveTriggerField("GangMemberWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTransferLeaderBtn.text = LocalizationMgr.Get("GangWnd_18");
        mTransferDeputyLeaderBtn.text = LocalizationMgr.Get("GangWnd_19");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册获取公会成员列表事件
        EventMgr.RegisterEvent("GangMemberWnd", EventMgrEventType.EVENT_GET_GANG_MEMBER_LIST, OnGetGangMemberListEvent);

        // 注册获取公会信息事件
        EventMgr.RegisterEvent("GangMemberWnd", EventMgrEventType.EVENT_NOTIFY_GANG_INFO, OnNotifyGangInfo);

        // 注册转让会长事件
        EventMgr.RegisterEvent("GangMemberWnd", EventMgrEventType.EVENT_TRANSFER_LEADER, OnTransferLeaderEvent);

        // 注册转让会长事件
        EventMgr.RegisterEvent("GangMemberWnd", EventMgrEventType.EVENT_TRANSFER_DEPUTY_LEADER, OnTransferLeaderEvent);

        // 注册移除公会成员事件
        EventMgr.RegisterEvent("GangMemberWnd", EventMgrEventType.EVENT_REMOVE_GANG_MEMBER, OnRemoveGangMemberEvent);
    }

    /// <summary>
    /// 移除公会成功
    /// </summary>
    void OnRemoveGangMemberEvent(int eventId, MixedValue para)
    {
        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 转让会长成功事件回调
    /// </summary>
    void OnTransferLeaderEvent(int eventId, MixedValue para)
    {
        // 绘制窗口
        Redraw();

        // 获取公会信息
        GangMgr.GetGangInfo();
    }

    void OnNotifyGangInfo(int eventId, MixedValue para)
    {
        // 显示公会成员数量
        LPCMapping info = GangMgr.GangDetail;

        mMemberAmount.text = string.Format(LocalizationMgr.Get("GangWnd_17"), info.GetValue<int>("amount"), info.GetValue<int>("max_count"));
    }

    /// <summary>
    /// 转让会长按钮点击事件
    /// </summary>
    void OnClickTransferLeaderBtn(GameObject go)
    {
        // 打开公会长转让窗口
        WindowMgr.OpenWnd(TransferLeaderWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 转让公会副会长按钮点击事件
    /// </summary>
    void OnClicktransferDeputyLeaderBtn(GameObject go)
    {
        // 打开转让会长窗口
        WindowMgr.OpenWnd(TransferDeputyLeaderWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 获取公会成员列表事件回调 
    /// </summary>
    void OnGetGangMemberListEvent(int eventId, MixedValue para)
    {
        // 刷新成员信息
        RefreshMemberData();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 公会数据
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            LPCMapping gangInfo = v.AsMapping;

            // 不是会长
            if (gangInfo.GetValue<string>("station") != "gang_leader")
            {
                mTransferLeaderBtn.gameObject.SetActive(false);
                mTransferDeputyLeaderBtn.gameObject.SetActive(false);
            }
            else
            {
                mTransferLeaderBtn.gameObject.SetActive(true);
                mTransferDeputyLeaderBtn.gameObject.SetActive(true);
            }

            // 获取公会成员列表
            GangMgr.GetGangMemberList(gangInfo.GetValue<string>("relation_tag"));
        }
    }

    /// <summary>
    /// 刷新成员信息
    /// </summary>
    void RefreshMemberData()
    {
        mGangMemberItemWnd.SetActive(false);

        // 公会成员列表
        LPCArray memberList = GangMgr.GangMemberList;

        for (int i = 0; i < memberList.Count; i++)
        {
            // 创建基础格子
            if (i + 1 > mItemList.Count)
                CreatedGameObject();

            if (i + 1 > mItemList.Count)
                continue;

            GangMemberItemWnd script = mItemList[i].GetComponent<GangMemberItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(memberList[i].AsMapping);

            mItemList[i].SetActive(true);
        }

        // 隐藏多余的基础格子
        for (int i = memberList.Count; i < mItemList.Count; i++)
            mItemList[i].SetActive(false);

        // 排序子控件
        mGrid.Reposition();
    }

    /// <summary>
    /// 创建基础格子
    /// </summary>
    void CreatedGameObject()
    {
        GameObject clone = Instantiate(mGangMemberItemWnd);

        // 设置父级
        clone.transform.SetParent(mGrid.transform);

        clone.transform.localScale = Vector3.one;
        clone.transform.localPosition = Vector3.zero;

        clone.SetActive(true);

        mItemList.Add(clone);
    }
}
