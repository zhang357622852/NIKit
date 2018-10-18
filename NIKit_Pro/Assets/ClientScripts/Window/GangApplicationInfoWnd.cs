/// <summary>
/// GangApplicationInfoWnd.cs
/// Created by fengsc 2018/01/31
/// 申请、邀请信息窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangApplicationInfoWnd : WindowBase<GangApplicationInfoWnd>
{
    #region 成员管理

    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    public UILabel mTips;

    // 申请、邀请数量
    public UILabel mApplicationAmount;

    // 拒绝所有按钮
    public UILabel mRefuseAllBtn;

    // 申请、邀请基础格子
    public GameObject mApplicationItemWnd;

    // 排序组件
    public UIGrid mGrid;

    public TweenScale mTweenScale;

    public UIScrollView mUIScrollView;

    // 缓存item
    List<GameObject> mItems = new List<GameObject>();

    // 申请列表
    LPCArray mApplicationList = LPCArray.Empty;

    LPCMapping mData = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitText();

        // 获取公会所有请求数据
        GangMgr.GetAllGangRequest();
    }

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 解注册事件
        EventMgr.UnregisterEvent("GangApplicationInfoWnd");

        if (ME.user == null)
            return;

        // 移除字段关注
        ME.user.dbase.RemoveTriggerField("GangApplicationInfoWnd");
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("GangWnd_62");
        mTips.text = LocalizationMgr.Get("GangWnd_58");
        mRefuseAllBtn.text = LocalizationMgr.Get("GangWnd_59");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mRefuseAllBtn.gameObject).onClick = OnClickRefuseAllBtn;

        // 注册动画播放完成回调
        EventDelegate.Add(mTweenScale.onFinished, OnTweensScaleFinish);

        // 注册EVENT_ALL_GANG_REQUEST事件
        EventMgr.RegisterEvent("GangApplicationInfoWnd", EventMgrEventType.EVENT_ALL_GANG_REQUEST, OnAllRequestEvent);

        // 注册EVENT_ACCEPT_GANG_REQUEST事件
        EventMgr.RegisterEvent("GangApplicationInfoWnd", EventMgrEventType.EVENT_ACCEPT_GANG_REQUEST, OnEventAcceptGangRequest);

        // 注册EVENT_CANCEl_GANG_REQUEST事件
        EventMgr.RegisterEvent("GangApplicationInfoWnd", EventMgrEventType.EVENT_CANCEl_GANG_REQUEST, OnEventCancelGangRequest);

        // 注册申请公会成功事件
        EventMgr.RegisterEvent("GangApplicationInfoWnd", EventMgrEventType.EVENT_APPLICATION_GANG_SUCCESS, OnEventApplicationGangSucc);

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("GangApplicationInfoWnd", new string[]{"user_requests"}, new CallBack(OnUserRequestChange));

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("GangApplicationInfoWnd", new string[]{"my_gang_info"}, new CallBack(OnMyGangInfoChange));
    }

    /// <summary>
    /// my_gang_info字段变化
    /// </summary>
    void OnMyGangInfoChange(object para, params object[] param)
    {
        // 玩家已经在公会中，关闭当前窗口
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
            WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 字段变化
    /// </summary>
    void OnUserRequestChange(object para, params object[] param)
    {
        if (!GangMgr.IsGetAllRequest(ME.user, true))
            return;

        // 获取公会请求数据
        GangMgr.GetAllGangRequest();
    }

    /// <summary>
    /// 申请公会成功事件回调
    /// </summary>
    void OnEventApplicationGangSucc(int eventId, MixedValue para)
    {
        // 获取公会请求数据
        GangMgr.GetAllGangRequest();
    }

    /// <summary>
    /// 同意申请事件回调
    /// </summary>
    void OnEventAcceptGangRequest(int eventId, MixedValue para)
    {
        DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_116"), mData.GetValue<string>("gang_name")));

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 取消申请事件回调
    /// </summary>
    void OnEventCancelGangRequest(int eventId, MixedValue para)
    {
        if (mData.GetValue<int>("state") == 1)
            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_117"), mData.GetValue<string>("gang_name")));

        // 获取公会请求数据
        GangMgr.GetAllGangRequest();
    }

    /// <summary>
    /// EVENT_ALL_GANG_REQUEST事件回调
    /// </summary>
    void OnAllRequestEvent(int eventId, MixedValue para)
    {
        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 获取公会请求数据
        mApplicationList = GangMgr.AllRequestList;
        if (mApplicationList == null)
            mApplicationList = LPCArray.Empty;

        // 公会申请、邀请数量
        mApplicationAmount.text = string.Format("{0}/{1}", mApplicationList.Count, GameSettingMgr.GetSettingInt("max_gang_request_amount"));

        if (mApplicationList.Count > mItems.Count)
        {
            if (mApplicationItemWnd.activeSelf)
                mApplicationItemWnd.SetActive(false);

            for (int i = mItems.Count; i < mApplicationList.Count; i++)
            {
                GameObject clone = Instantiate(mApplicationItemWnd);

                clone.transform.SetParent(mGrid.transform);

                clone.transform.localPosition = Vector3.zero;

                clone.transform.localScale = Vector3.one;

                // 缓存
                mItems.Add(clone);
            }
        }

        for (int i = 0; i < mApplicationList.Count; i++)
        {
            LPCMapping data = mApplicationList[i].AsMapping;

            ApplicationItemWnd script = mItems[i].GetComponent<ApplicationItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(data);

            script.SetCallBack(new CallBack(OnAgreeCallBack), new CallBack(OnRefuseCallBack));

            mItems[i].SetActive(true);
        }

        // 隐藏多余的格子
        for (int i = mApplicationList.Count; i < mItems.Count; i++)
            mItems[i].SetActive(false);

        // 排序子控件
        mGrid.Reposition();

        mUIScrollView.ResetPosition();
    }

    /// <summary>
    /// 拒绝回调
    /// </summary>
    void OnRefuseCallBack(object para, params object[] param)
    {
        mData = param[0] as LPCMapping;

        if (mData.GetValue<int>("state") == 0)
        {
            DialogMgr.ShowDailog(new CallBack(OnRefuseConfirm), string.Format(LocalizationMgr.Get("GangWnd_115"), mData.GetValue<string>("gang_name")));
        }
        else
        {
            GangMgr.CancelGangRequest(mData.GetValue<string>("id"));
        }
    }

    void OnRefuseConfirm(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        GangMgr.CancelGangRequest(mData.GetValue<string>("id"));
    }

    /// <summary>
    /// 同意邀请回调
    /// </summary>
    void OnAgreeCallBack(object para, params object[] param)
    {
        mData = param[0] as LPCMapping;

        GangMgr.AcceptGangRequest(mData.GetValue<string>("id"));
    }

    /// <summary>
    /// tween动画播放完成回调
    /// </summary>
    void OnTweensScaleFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 窗口关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 拒绝所有请求按钮点击事件回调
    /// </summary>
    void OnClickRefuseAllBtn(GameObject go)
    {
        LPCArray userRequest = LPCArray.Empty;

        LPCValue value = ME.user.Query<LPCValue>("user_requests");
        if (value != null && value.IsArray)
            userRequest = value.AsArray;

        if (userRequest.Count == 0)
            return;

        // 拒绝所有的公会邀请
        GangMgr.CancelGangRequest(string.Empty);
    }
}
