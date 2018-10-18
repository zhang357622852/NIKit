/// <summary>
/// GangWnd.cs
/// Created by fengsc 2018/01/26
/// 公会界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangWnd : WindowBase<GangWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 公会商店
    public GameObject mGangMarketBtn;
    public UILabel mGangMarketBtnLb;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public GangInfoWnd mGangInfoWnd;

    public TweenScale mTweenScale;

    // 公会名称输入框
    public UIInput mQueryInput;

    // 查找公会按钮
    public GameObject mQueryBtn;

    // 我的信息查看按钮
    public GameObject mMyInfoBtn;
    public UILabel mMyInfoBtnLb;
    public GameObject mMyInfoBtnMask;

    // 创建公会按钮
    public GameObject mCreateGangBtn;
    public UILabel mCreateGangBtnLb;
    public GameObject mCreateGangBtnMask;

    // 申请管理按钮
    public GameObject mApplicationManageBtn;
    public UILabel mApplicationManageBtnLb;

    // 申请管理数量提示
    public UILabel mTipsAmount;

    // 成员信息按钮
    public GameObject mMemberInfoBtn;
    public UILabel mMemberInfoBtnLb;

    // 成员信息窗口
    public GameObject mMemberInfoWnd;

    // 申请管理窗口
    public GameObject mApplicationManageWnd;

    // 推荐公会窗口
    public GameObject mRecommendGangWnd;

    public UILabel mMyInfoTips;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 获取公会信息
        GangMgr.GetGangInfo();

        // 刷新界面
        Redraw();

        // 刷新公会申请提示
        RefreshMyInfoTips();

        // 申请管理提示
        RefreshGangRequestTips();
    }

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        EventMgr.UnregisterEvent("GangWnd");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("GangWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("GangWnd_1");
        mGangMarketBtnLb.text = LocalizationMgr.Get("GangWnd_2");
        mApplicationManageBtnLb.text = LocalizationMgr.Get("GangWnd_21");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 公会商店按钮点击事件
    /// </summary>
    void OnClickGangMarketBtn(GameObject go)
    {
        // 打开商城界面
        WindowMgr.OpenWnd(MarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 我的信息按钮点击事件回调
    /// </summary>
    void OnClickMyInfoBtn(GameObject go)
    {
        // 打开我的信息查看界面
        WindowMgr.OpenWnd(GangApplicationInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 邀请入会按钮点击事件
    /// </summary>
    void OnClickInviteBtn(GameObject go)
    {
        // 打开邀请玩家窗口
        WindowMgr.OpenWnd(GangInviteWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 创建公会按钮点击事件
    /// </summary>
    void OnClickCreateGangBtn(GameObject go)
    {
        // 打开创建公会界面
        WindowMgr.OpenWnd(CreateGangWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 公会宣传按钮点击事件
    /// </summary>
    void OnClickPublicityBtn(GameObject go)
    {
        // 打开公会宣传界面
        WindowMgr.OpenWnd(GangSloganWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 公会查询按钮点击事件
    /// </summary>
    void OnClickQueryGangBtn(GameObject go)
    {
        string value = mQueryInput.value.Trim();
        if (string.IsNullOrEmpty(value))
            return;

        // 获取公会详情
        GangMgr.GetGangDetails(value);
    }

    /// <summary>
    /// 申请管理按钮点击事件
    /// </summary>
    void OnClickApplicationManageBtn(GameObject go)
    {
        if (mApplicationManageWnd == null)
            return;

        if (mApplicationManageWnd.activeSelf || mApplicationManageWnd.activeInHierarchy)
            return;

        // 打开成员信息窗口
        mApplicationManageWnd.SetActive(true);

        if (mMemberInfoWnd != null)
            mMemberInfoWnd.SetActive(false);
    }

    /// <summary>
    /// 成员信息按钮点击事件
    /// </summary>
    void OnClickMemberInfoBtn(GameObject go)
    {
        if (mMemberInfoWnd == null)
            return;

        if (mMemberInfoWnd.activeSelf || mMemberInfoWnd.activeInHierarchy)
            return;

        // 打开成员信息窗口
        mMemberInfoWnd.SetActive(true);

        if (mApplicationManageWnd != null)
            mApplicationManageWnd.SetActive(false);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mGangMarketBtn).onClick = OnClickGangMarketBtn;
        UIEventListener.Get(mQueryBtn).onClick = OnClickQueryGangBtn;

        EventDelegate.Add(mTweenScale.onFinished, OnFinish);

        // 注册获取公会信息事件
        EventMgr.RegisterEvent("GangWnd", EventMgrEventType.EVENT_NOTIFY_GANG_INFO, OnNotifyGangInfo);

        // 注册公会创建成功事件
        EventMgr.RegisterEvent("GangWnd", EventMgrEventType.EVENT_CRETAE_GANG_SUCCESS, OnCreateGangSuccess);

        // 注册获取公会详情
        EventMgr.RegisterEvent("GangWnd", EventMgrEventType.EVENT_GET_GANG_DETAILS, OnGetGangDetailsEvent);

        // 注册获取公会信息事件
        EventMgr.RegisterEvent("GangWnd", EventMgrEventType.EVENT_CANCEl_GANG_REQUEST, OnEventCancelRequest);

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("GangWnd", new string[]{"my_gang_info"}, new CallBack(OnMyGangInfoChange));

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("GangWnd", new string[]{"user_requests"}, new CallBack(OnUserRequestChange));
    }

    /// <summary>
    /// 取消申请成功
    /// </summary>
    /// <param name="para">Para.</param>
    void OnEventCancelRequest(int eventId, MixedValue para)
    {
        // 获取公会列表
        GangMgr.GetAllGangRequest();
    }

    /// <summary>
    /// 获取公会详情事件回调
    /// </summary>
    void OnGetGangDetailsEvent(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null || data.Count == 0)
            return;

        GameObject wnd = WindowMgr.OpenWnd(GangDetailsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        wnd.GetComponent<GangDetailsWnd>().Bind(data.GetValue<LPCMapping>("details"));
    }

    /// <summary>
    /// 玩家申请数量变化
    /// </summary>
    void OnUserRequestChange(object para, params object[] param)
    {
        // 刷新我的申请提示
        RefreshMyInfoTips();
    }

    /// <summary>
    /// 刷新玩家申请提示
    /// </summary>
    void RefreshGangRequestTips()
    {
        if (ME.user == null)
        {
            mTipsAmount.gameObject.SetActive(false);
            return;
        }

        // 没有加入公会
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v == null || ! v.IsMapping || v.AsMapping.Count == 0)
        {
            if (mTipsAmount.gameObject.activeSelf)
                mTipsAmount.gameObject.SetActive(false);

            return;
        }

        // 不是会长或者公会长
        string station = v.AsMapping.GetValue<string>("station");
        if (station != "gang_leader" && station != "gang_deputy_leader")
            return;

        LPCArray gangRequest = LPCArray.Empty;

        LPCValue request = v.AsMapping.GetValue<LPCValue>("gang_requests");
        if (request != null && request.IsArray)
            gangRequest = request.AsArray;

        if (gangRequest.Count > 0)
        {
            if (!mTipsAmount.gameObject.activeSelf)
                mTipsAmount.gameObject.SetActive(true);

            mTipsAmount.text = gangRequest.Count.ToString();
        }
        else
        {
            if (mTipsAmount.gameObject.activeSelf)
                mTipsAmount.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 刷新我的申请提示
    /// </summary>
    void RefreshMyInfoTips()
    {
        if (ME.user == null)
        {
            mMyInfoTips.gameObject.SetActive(false);
            return;
        }

        // 已经加入公会
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            if (mMyInfoTips.gameObject.activeSelf)
                mMyInfoTips.gameObject.SetActive(false);

            return;
        }

        LPCArray userRequest = LPCArray.Empty;

        LPCValue request = ME.user.Query<LPCValue>("user_requests");
        if (request != null && request.IsArray)
            userRequest = request.AsArray;

        if (userRequest.Count > 0)
        {
            if (!mMyInfoTips.gameObject.activeSelf)
                mMyInfoTips.gameObject.SetActive(true);

            mMyInfoTips.text = userRequest.Count.ToString();
        }
        else
        {
            if (mMyInfoTips.gameObject.activeSelf)
                mMyInfoTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 创建公会成功事件回调
    /// </summary>
    void OnCreateGangSuccess(int eventId, MixedValue para)
    {
        // 打开创建公会成功动画窗口
        WindowMgr.OpenWnd(CreateGangAnimationWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 获取公会信息事件回调
    /// </summary>
    void OnNotifyGangInfo(int eventId, MixedValue para)
    {
        if (mApplicationManageWnd.activeSelf)
        {
            // 刷新公会数据
            LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
            if (v != null && v.IsMapping && v.AsMapping.Count != 0)
                mGangInfoWnd.Bind(LPCMapping.Empty);

            return;
        }

        // 重绘界面
        Redraw();
    }

    /// <summary>
    /// my_gang_info字段变化回调
    /// </summary>
    void OnMyGangInfoChange(object para, params object[] param)
    {
        // 刷新提示信息
        RefreshGangRequestTips();

        // 公会数据
        LPCMapping myGangInfo = null;

        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            myGangInfo = v.AsMapping;

            if (mApplicationManageWnd.activeSelf)
            {
                if (myGangInfo.GetValue<string>("station") == "gang_member")
                {
                    // 刷新界面
                    Redraw();

                    // 获取公会信息
                    GangMgr.GetGangInfo();
                }

                // 获取公会请求数据
                GangMgr.GetAllGangRequest();
            }

            if (mMemberInfoWnd.activeSelf || mRecommendGangWnd.activeSelf)
            {
                // 刷新界面
                Redraw();

                // 获取公会信息
                GangMgr.GetGangInfo();
            }
        }
        else
        {
            // 刷新界面
            Redraw();

            // 获取公会信息
            GangMgr.GetGangInfo();
        }
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);

        if (mApplicationManageWnd != null)
            mApplicationManageWnd.SetActive(false);

        mMemberInfoWnd.SetActive(false);

        mApplicationManageBtn.SetActive(false);
        mMemberInfoBtn.SetActive(false);

        mGangInfoWnd.gameObject.SetActive(true);

        mMyInfoBtnMask.SetActive(false);
        mCreateGangBtnMask.SetActive(false);

        mRecommendGangWnd.SetActive(false);

        mApplicationManageWnd.SetActive(false);

        // 公会数据
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            // 已经加入公会
            mMyInfoBtnLb.text = LocalizationMgr.Get("GangWnd_13");
            mCreateGangBtnLb.text = LocalizationMgr.Get("GangWnd_14");
            UIEventListener.Get(mMyInfoBtn).onClick = OnClickInviteBtn;
            UIEventListener.Get(mCreateGangBtn).onClick = OnClickPublicityBtn;

            // 绑定数据
            mGangInfoWnd.Bind(LPCMapping.Empty);

            // 打开成员信息窗口
            if (mMemberInfoWnd != null)
                mMemberInfoWnd.SetActive(true);

            mMemberInfoBtnLb.text = LocalizationMgr.Get("GangWnd_20");

            mMemberInfoBtn.SetActive(true);

            mMemberInfoBtn.GetComponent<UIToggle>().Set(true);

            string station = v.AsMapping.GetValue<string>("station");

            switch (station)
            {
                // 会长，  副会长
                case "gang_leader":
                case "gang_deputy_leader":

                    mApplicationManageBtn.SetActive(true);

                    mApplicationManageBtn.GetComponent<UIToggle>().Set(false);

                    UIEventListener.Get(mApplicationManageBtn).onClick = OnClickApplicationManageBtn;

                    UIEventListener.Get(mMemberInfoBtn).onClick = OnClickMemberInfoBtn;

                    break;

                    // 普通会员
                default:

                    UIEventListener.Get(mMemberInfoBtn).onClick -= OnClickMemberInfoBtn;

                    mMyInfoBtnMask.SetActive(true);
                    mCreateGangBtnMask.SetActive(true);

                    break;
            }
        }
        else
        {
            // 没有公会
            mMyInfoBtnLb.text = LocalizationMgr.Get("GangWnd_15");
            mCreateGangBtnLb.text = LocalizationMgr.Get("GangWnd_16");

            UIEventListener.Get(mApplicationManageBtn).onClick -= OnClickApplicationManageBtn;
            UIEventListener.Get(mMemberInfoBtn).onClick -= OnClickMemberInfoBtn;

            UIEventListener.Get(mMyInfoBtn).onClick = OnClickMyInfoBtn;;
            UIEventListener.Get(mCreateGangBtn).onClick = OnClickCreateGangBtn;

            mMemberInfoBtnLb.text = LocalizationMgr.Get("GangWnd_32");

            if (mRecommendGangWnd != null)
                mRecommendGangWnd.SetActive(true);

            mMemberInfoBtn.SetActive(true);
        }
    }
}
