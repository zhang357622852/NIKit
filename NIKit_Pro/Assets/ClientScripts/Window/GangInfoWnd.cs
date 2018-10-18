/// <summary>
/// GangInfoWnd.cs
/// Created by fengsc 2018/01/26
/// 公会信息显示界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangInfoWnd : WindowBase<GangInfoWnd>
{
    #region 成员变量

    /// 公会旗帜修改按钮
    public GameObject mFlagChangeBtn;
    public UILabel mFlagChangeBtnLb;

    // 公会旗帜基础格子
    public FlagItemWnd mFlagItemWnd;

    // 公会名称
    public UILabel mGangName;

    // 会长
    public UILabel mGangLeaderTips;
    public UILabel mGangLeaderName;
    public UISprite mGender;

    // 解散公会按钮
    public UILabel mDismissGangBtn;

    // 公会人数
    public UILabel mAmountTips;
    public UILabel mAmount;

    // 成员详情按钮
    public UILabel mMemberBtn;

    // 公会实力
    public UILabel mStrengthTips;
    public UISprite[] mStrengthStars;

    // 公会介绍修改按钮
    public GameObject mIntroduceChangeBtn;
    public UILabel mIntroduceChangeBtnLb;

    // 公会介绍
    public UILabel mIntroduceTips;
    public UILabel mIntroduce;

    // 公会战争按钮
    public GameObject mGangBattleBtn;
    public UILabel mGangBattleBtnLb;

    // 申请加入按钮
    public UISprite mApplicationBtn;
    public UILabel mApplicationBtnLb;
    public GameObject mApplicationBtnMask;

    LPCMapping mGangInfo = LPCMapping.Empty;

    LPCMapping mBingGangInfo = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("GangInfoWnd");

        // 移除字段关注
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("GangInfoWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mFlagChangeBtnLb.text = LocalizationMgr.Get("GangWnd_3");
        mGangLeaderTips.text = LocalizationMgr.Get("GangWnd_6");
        mDismissGangBtn.text = LocalizationMgr.Get("GangWnd_5");
        mAmountTips.text = LocalizationMgr.Get("GangWnd_7");
        mStrengthTips.text = LocalizationMgr.Get("GangWnd_8");
        mIntroduceTips.text = LocalizationMgr.Get("GangWnd_9");
        mApplicationBtnLb.text = LocalizationMgr.Get("GangWnd_33");
        mMemberBtn.text = LocalizationMgr.Get("GangWnd_34");
        mIntroduceChangeBtnLb.text = LocalizationMgr.Get("GangWnd_3");
    }

    /// <summary>
    /// 修改公会信息按钮点击事件
    /// </summary>
    void OnClickChangeInfoBtn(GameObject go)
    {
        // 打开修改公会信息窗口
        GameObject wnd = WindowMgr.OpenWnd(ChangeGangInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<ChangeGangInfoWnd>().Bind(mGangInfo);
    }

    /// <summary>
    /// 公会战争按钮点击事件回调
    /// </summary>
    void OnClickGangBattleBtn(GameObject go)
    {
        DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_51"));
    }

    /// <summary>
    /// 解散公会按钮点击事件
    /// </summary>
    void OnClickDismissGangBtn(GameObject go)
    {
        // 当前公会人数大于一人无法解散
        if (GangMgr.GangMemberList.Count > 1)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_10"));
            return;
        }

        // 显示确认解散提示框
        DialogMgr.ShowDailog(new CallBack(OnDismissConfirmCallback), string.Format(LocalizationMgr.Get("GangWnd_144"), GameSettingMgr.GetSettingInt("lift_gang_cd") / 3600));
    }

    /// <summary>
    /// 确认解散回调
    /// </summary>
    void OnDismissConfirmCallback(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 解散公会
        GangMgr.DismissGang();
    }

    /// <summary>
    /// 成员详情按钮点击事件
    /// </summary>
    void OnClickMemberBtn(GameObject go)
    {
        // 获取公会详情
        GangMgr.GetGangDetails(mGangInfo.GetValue<string>("gang_name"));
    }

    /// <summary>
    /// 申请加入按钮点击事件
    /// </summary>
    void OnClickApplicationBtn(GameObject go)
    {
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            // 已经加入公会
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_73"));

            return;
        }

        // 入会条件
        LPCMapping condition = mGangInfo.GetValue<LPCMapping>("join_condition");

        LPCMapping arenaTop = LPCMapping.Empty;

        // 竞技场数据
        LPCValue value= ME.user.Query<LPCValue>("arena_top");
        if (value != null && value.IsMapping)
            arenaTop = value.AsMapping;

        int step = ArenaMgr.GetStepByScoreAndRank(arenaTop.GetValue<int>("rank"), arenaTop.GetValue<int>("score"));
        if (condition.GetValue<int>("step") != -1 && condition.GetValue<int>("step") < step)
        {
            // 段位不符合入会要求
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_75"));

            return;
        }

        // 申请加入公会
        GangMgr.RequestJoinGang(mGangInfo.GetValue<string>("relation_tag"));
    }

    /// <summary>
    /// 取消申请按钮点击回调
    /// </summary>
    void OnClickCancelApplicationBtn(GameObject go)
    {
        DialogMgr.ShowDailog(new CallBack(OnCancelConfirm), string.Format(LocalizationMgr.Get("GangWnd_115"), mGangInfo.GetValue<string>("gang_name")));
    }

    void OnCancelConfirm(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        GangMgr.CancelGangRequest(string.Format("{0}{1}{2}", ME.user.GetRid(), "_", mGangInfo.GetValue<string>("relation_tag")));
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mFlagChangeBtn).onClick = OnClickChangeInfoBtn;
        UIEventListener.Get(mIntroduceChangeBtn).onClick = OnClickChangeInfoBtn;
        UIEventListener.Get(mGangBattleBtn).onClick = OnClickGangBattleBtn;
        UIEventListener.Get(mDismissGangBtn.gameObject).onClick = OnClickDismissGangBtn;
        UIEventListener.Get(mMemberBtn.gameObject).onClick = OnClickMemberBtn;

        // 注册公会信息修改事件
        EventMgr.RegisterEvent("GangInfoWnd", EventMgrEventType.EVENT_CHANGE_GANG_INFO, OnChangeGangInfoEvent);

        // 注册公会信息修改事件
        EventMgr.RegisterEvent("GangInfoWnd", EventMgrEventType.EVENT_CHANGE_GANG_FLAG, OnChangeGangFlagEvent);

        // 注册获取公会信息事件
        EventMgr.RegisterEvent("GangInfoWnd", EventMgrEventType.EVENT_NOTIFY_GANG_INFO, OnNotifyGangInfo);

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("GangInfoWnd", new string[]{"user_requests"}, new CallBack(OnUserRequestChange));
    }

    /// <summary>
    /// 修改公会旗帜事件回调
    /// </summary>
    void OnChangeGangFlagEvent(int eventId, MixedValue para)
    {
        // 没有加入公会
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v == null || ! v.IsMapping || v.AsMapping.Count == 0)
            return;

        // 刷新公会信息界面
        Redraw();
    }

    /// <summary>
    /// 玩家请求数据变化
    /// </summary>
    void OnUserRequestChange(object para, params object[] param)
    {
        // 刷新申请按钮状态
        RefreshApplicationButtonState();
    }

    /// <summary>
    /// 公会信息变化回调
    /// </summary>
    void OnNotifyGangInfo(int eventId, MixedValue para)
    {
        // 刷新公会信息界面
        Redraw();
    }

    /// <summary>
    /// 公会修改事件回调
    /// </summary>
    void OnChangeGangInfoEvent(int eventId, MixedValue para)
    {
        // 没有加入公会
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v == null || ! v.IsMapping || v.AsMapping.Count == 0)
            return;

        // 刷新公会信息界面
        Redraw();
    }

    /// <summary>
    /// 刷新申请按钮的状态
    /// </summary>
    void RefreshApplicationButtonState()
    {
        if (GangMgr.IsRequest(string.Format("{0}{1}{2}", ME.user.GetRid(), "_", mGangInfo.GetValue<string>("relation_tag"))))
        {
            mApplicationBtn.spriteName = "tower_red_btn";
            UIEventListener.Get(mApplicationBtn.gameObject).onClick = OnClickCancelApplicationBtn;

            mApplicationBtnLb.text = LocalizationMgr.Get("GangWnd_68");
        }
        else
        {
            mApplicationBtn.spriteName = "SuitNormalBtn";

            mApplicationBtnLb.text = LocalizationMgr.Get("GangWnd_33");

            UIEventListener.Get(mApplicationBtn.gameObject).onClick = OnClickApplicationBtn;
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 控件初始化
        mFlagChangeBtn.SetActive(false);
        mDismissGangBtn.gameObject.SetActive(false);
        mIntroduceChangeBtn.SetActive(false);
        mGangBattleBtn.SetActive(false);
        mMemberBtn.gameObject.SetActive(false);
        mApplicationBtn.gameObject.SetActive(false);

        for (int i = 0; i < mStrengthStars.Length; i++)
            mStrengthStars[i].gameObject.SetActive(false);
        mGender.gameObject.SetActive(false);
        mGangLeaderName.text = string.Empty;
        mAmount.text = string.Empty;
        mIntroduce.text = string.Empty;
        mGangName.text = string.Empty;

        if (ME.user == null)
            return;

        // 公会数据
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v == null || !v.IsMapping || v.AsMapping.Count == 0)
        {
            // 没有加入公会
            mGangInfo = mBingGangInfo;

            mApplicationBtnMask.SetActive(false);
            mApplicationBtn.gameObject.SetActive(true);

            // 刷新申请按钮状态
            RefreshApplicationButtonState();

            // 没有任何公会数据
            if (mGangInfo == null || mGangInfo.Count == 0)
            {
                mApplicationBtnMask.SetActive(true);

                mFlagItemWnd.Bind(LPCArray.Empty);

                return;
            }

            mMemberBtn.gameObject.SetActive(true);
        }
        else
        {
            // 已加入公会状态
            mGangInfo = GangMgr.GangDetail;

            mGangBattleBtn.SetActive(true);

            string station = v.AsMapping.GetValue<string>("station");

            // 只有会长可以修改旗帜和解散公会
            if (station == "gang_leader")
            {
                mFlagChangeBtn.SetActive(true);

                mIntroduceChangeBtn.SetActive(true);

                mDismissGangBtn.gameObject.SetActive(true);
            }
            else if (station == "gang_deputy_leader")
            {
                mIntroduceChangeBtn.SetActive(true);
            }
        }

        // 绑定旗帜数据
        LPCValue flag = mGangInfo.GetValue<LPCValue>("flag");
        if (flag != null && flag.IsArray)
            mFlagItemWnd.Bind(flag.AsArray);

        mGender.gameObject.SetActive(true);

        // 公会名称
        mGangName.text = mGangInfo.GetValue<string>("gang_name");

        if (mGangInfo.GetValue<int>("leader_gender") == 1)
        {
            mGender.spriteName = "male";
        }
        else
        {
            mGender.spriteName = "female";
        }

        // 会长名称
        mGangLeaderName.text = mGangInfo.GetValue<string>("leader_name");

        // 公会人数显示
        mAmount.text = string.Format("{0}/{1}", mGangInfo.GetValue<int>("amount"), mGangInfo.GetValue<int>("max_count"));

        // 公会实力显示
        for (int i = 0; i < mStrengthStars.Length; i++)
            mStrengthStars[i].gameObject.SetActive(true);

        // 公会简介
        mIntroduce.text = mGangInfo.GetValue<string>("introduce");
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping gangInfo)
    {
        mBingGangInfo = gangInfo;

        // 绘制窗口
        Redraw();
    }
}
