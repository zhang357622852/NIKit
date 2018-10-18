/// <summary>
/// ApplicationManageWnd.cs
/// Created by fengsc 2018/01/31
/// 申请管理窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ApplicationManageWnd : WindowBase<ApplicationManageWnd>
{
    #region 成员变量

    public UILabel mCheckTips;

    public UILabel mTips1;
    public UILabel mTips2;

    // 申请人数
    public UILabel mAmount;

    // 段位要求
    public GameObject mArenaRankBtn;
    public UILabel mArenaRankBtnLb;

    public GameObject mStars;

    public UISprite[] mArenaRankStars;

    // 是否需要申请条件
    public GameObject mApplicationConditionBtn;
    public UILabel mApplicationConditionBtnLb;

    public GameObject mSelectRankWnd;

    public GameObject mCloseSelectRankWnd;

    public GameObject mApplicationConditionWnd;

    public GameObject mCloseApplicationConditionWnd;

    // 段位条件基础格子
    public GameObject[] mRankConditionItem;

    // 不需要审核
    public GameObject mNoCheckBtn;
    public UILabel mNoCheckLb;

    // 需要审核
    public GameObject mNeedCheckBtn;
    public UILabel mNeedCheckLb;

    public GameObject mRefuseJionBtn;
    public UILabel mRefuseJionLb;

    public GameObject mMemberItem;

    public UIScrollView mUIScrollView;

    // 排序组件
    public UIGrid mGrid;

    int mCheckCondition = -1;

    int mStep = -1;

    LPCArray mAllRequest = LPCArray.Empty;

    List<GameObject> mItems = new List<GameObject>();

    LPCMapping mData = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Awake ()
    {
        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();

        // 创建一批缓存的格子
        CreatedGameObject();
    }

    void OnEnable()
    {
        mAllRequest = LPCArray.Empty;

        // 刷新入会条件
        RefreshCondition(GangMgr.GangDetail.GetValue<LPCMapping>("join_condition"));

        // 获取公会请求数据
        GangMgr.GetAllGangRequest();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("ApplicationManageWnd");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("ApplicationManageWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mCheckTips.text = LocalizationMgr.Get("GangWnd_150");
        mTips1.text = LocalizationMgr.Get("CreateGuildWnd_12");
        mTips2.text = LocalizationMgr.Get("CreateGuildWnd_13");
        mArenaRankBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_14");
        mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_14");
        mNoCheckLb.text = LocalizationMgr.Get("CreateGuildWnd_17");
        mNeedCheckLb.text = LocalizationMgr.Get("CreateGuildWnd_18");
        mRefuseJionLb.text = LocalizationMgr.Get("CreateGuildWnd_19");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mNoCheckBtn).onClick = OnClickNoCheckBtn;
        UIEventListener.Get(mNeedCheckBtn).onClick = OnClickNeedCheckBtn;
        UIEventListener.Get(mRefuseJionBtn).onClick = OnClickRefuseJionBtn;
        UIEventListener.Get(mCloseSelectRankWnd).onClick = OnClickCloseSelectRankWnd;
        UIEventListener.Get(mCloseApplicationConditionWnd).onClick = OnClickCloseApplicationCondition;
        UIEventListener.Get(mArenaRankBtn).onClick = OnClickSelectRankConditinBtn;
        UIEventListener.Get(mApplicationConditionBtn).onClick = OnClickSelectCheckCondition;

        for (int i = 0; i < mRankConditionItem.Length; i++)
            UIEventListener.Get(mRankConditionItem[i]).onClick = OnClickRankConditionItem;

        // 注册EVENT_ALL_GANG_REQUEST事件
        EventMgr.RegisterEvent("ApplicationManageWnd", EventMgrEventType.EVENT_ALL_GANG_REQUEST, OnAllRequestEvent);

        // 注册EVENT_ACCEPT_GANG_REQUEST事件
        EventMgr.RegisterEvent("ApplicationManageWnd", EventMgrEventType.EVENT_ACCEPT_GANG_REQUEST, OnEventAcceptGangRequest);

        // 注册EVENT_CANCEl_GANG_REQUEST事件
        EventMgr.RegisterEvent("ApplicationManageWnd", EventMgrEventType.EVENT_CANCEl_GANG_REQUEST, OnEventCancelGangRequest);

        // 注册公会信息修改事件
        EventMgr.RegisterEvent("ApplicationManageWnd", EventMgrEventType.EVENT_CHANGE_GANG_INFO, OnChangeGangInfoEvent);

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("ApplicationManageWnd", new string[]{"my_gang_info"}, new CallBack(OnMyGangInfoChange));
    }

    /// <summary>
    /// 公会修改事件回调
    /// </summary>
    void OnChangeGangInfoEvent(int eventId, MixedValue para)
    {
        LPCValue extraData = para.GetValue<LPCValue>();
        if (!extraData.IsMapping)
            return;

        LPCMapping data = extraData.AsMapping;
        if (!data.ContainsKey("join_condition") || !data["join_condition"].IsMapping)
            return;

        RefreshCondition(extraData.AsMapping.GetValue<LPCMapping>("join_condition"));

        // 获取公会信息
        GangMgr.GetGangInfo();
    }

    /// <summary>
    /// my_gang_info字段变化回调
    /// </summary>
    void OnMyGangInfoChange(object para, params object[] param)
    {
        if (!gameObject.activeSelf)
            return;

        // 获取所有公会请求
        GangMgr.GetAllGangRequest();
    }

    /// <summary>
    /// 同意申请事件回调
    /// </summary>
    void OnEventAcceptGangRequest(int eventId, MixedValue para)
    {
        DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_111"), mData.GetValue<string>("name")));

        // 获取公会请求数据
        GangMgr.GetAllGangRequest();
    }

    /// <summary>
    /// 取消申请事件回调
    /// </summary>
    void OnEventCancelGangRequest(int eventId, MixedValue para)
    {
        DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_107"), mData.GetValue<string>("name")));

        // 获取公会请求数据
        GangMgr.GetAllGangRequest();
    }

    /// <summary>
    /// EVENT_ALL_GANG_REQUEST事件回调
    /// </summary>
    void OnAllRequestEvent(int eventId, MixedValue para)
    {
        // 绘制窗口
        RefreshData();
    }

    /// <summary>
    /// 刷新入会条件
    /// </summary>
    void RefreshCondition(LPCMapping joinCondition)
    {
        mStep = joinCondition.GetValue<int>("step");

        mCheckCondition = joinCondition.GetValue<int>("check");

        RefreshRankCondition();

        RefreshCheckCondition();
    }

    /// <summary>
    /// 刷新列表数据
    /// </summary>
    void RefreshData()
    {
        mAllRequest = GangMgr.AllRequestList;

        if (mAllRequest == null)
            mAllRequest = LPCArray.Empty;

        // 申请人数
        mAmount.text = string.Format(LocalizationMgr.Get("GangWnd_72"), mAllRequest.Count, GameSettingMgr.GetSettingInt("max_gang_request_amount"));

        if (mAllRequest.Count > mItems.Count)
        {
            int count = mAllRequest.Count - mItems.Count;

            for (int i = 0; i < count; i++)
            {
                GameObject clone = Instantiate(mMemberItem);

                clone.transform.SetParent(mGrid.transform);

                clone.transform.localPosition = Vector3.zero;

                clone.transform.localScale = Vector3.one;

                mItems.Add(clone);
            }
        }

        for (int i = 0; i < mAllRequest.Count; i++)
        {
            GameObject item = mItems[i];

            MemberManageItemWnd script = item.GetComponent<MemberManageItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(mAllRequest[i].AsMapping, new Vector3(121, -2, 0), new Vector3(193, -2, 0));

            script.SetCallBack(new CallBack(OnAgreeCallBack), new CallBack(OnRefuseCallBack));

            item.SetActive(true);
        }

        // 隐藏多余的格子
        for (int j = mAllRequest.Count; j < mItems.Count; j++)
            mItems[j].SetActive(false);

        // 排序控件
        mGrid.Reposition();

        mUIScrollView.ResetPosition();
    }

    void CreatedGameObject()
    {
        if (mMemberItem.activeSelf)
            mMemberItem.SetActive(false);

        for (int i = 0; i < GameSettingMgr.GetSettingInt("max_gang_request_amount"); i++)
        {
            GameObject clone = Instantiate(mMemberItem);

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localPosition = Vector3.zero;

            clone.transform.localScale = Vector3.one;

            mItems.Add(clone);
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 公会详细数据
        LPCMapping gangDetails = GangMgr.GangDetail;

        LPCMapping joinCondition = gangDetails.GetValue<LPCMapping>("join_condition");

        // 刷新入会条件
        RefreshCondition(joinCondition);

        // 段位条件列表
        List<int> rankCondition = CALC_CREATE_GUILD_RANK_CONDITION.Call();

        for (int i = 0; i < mRankConditionItem.Length; i++)
        {
            ConditionItemWnd script = mRankConditionItem[i].GetComponent<ConditionItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(rankCondition[i]);
        }
    }

    /// <summary>
    /// 同意加入回调
    /// </summary>
    void OnAgreeCallBack(object para, params object[] param)
    {
        mData = param[0] as LPCMapping;

        // 同意公会申请
        GangMgr.AcceptGangRequest(mData.GetValue<string>("id"));
    }

    /// <summary>
    /// 拒绝加入回调
    /// </summary>
    void OnRefuseCallBack(object para, params object[] param)
    {
        mData = param[0] as LPCMapping;

        // 取消公会申请
        GangMgr.CancelGangRequest(mData.GetValue<string>("id"));
    }

    /// <summary>
    /// 刷新审核条件显示
    /// </summary>
    void RefreshCheckCondition()
    {
        switch (mCheckCondition)
        {
            case (int) CHECK_CONDITION.NO_CHECK:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_17");
                break;

            case (int) CHECK_CONDITION.NEED_CHECK:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_18");
                break;

            case (int) CHECK_CONDITION.REFUSE_JION:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_19");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 刷新段位条件显示
    /// </summary>
    void RefreshRankCondition()
    {
        mArenaRankBtnLb.gameObject.SetActive(false);

        mStars.SetActive(false);

        if (mStep == -1)
        {
            mArenaRankBtnLb.gameObject.SetActive(true);
            mArenaRankBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_20");
            return;
        }

        mStars.SetActive(true);

        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(mStep);
        if (row == null)
            return;

        for (int i = 0; i < mArenaRankStars.Length; i++)
            mArenaRankStars[i].spriteName = "arena_star_bg";

        for (int i = 0; i < row.Query<int>("star"); i++)
            mArenaRankStars[i].spriteName = row.Query<string>("star_name");
    }

    /// <summary>
    /// 段位条件基础格子点击事件
    /// </summary>
    void OnClickRankConditionItem(GameObject go)
    {
        ConditionItemWnd script = go.GetComponent<ConditionItemWnd>();
        if (script == null)
            return;

        if (mStep != script.mStep)
        {
            mStep = script.mStep;

            // 修改公会条件
            LPCMapping condition = LPCMapping.Empty;
            condition.Add("step", mStep);
            condition.Add("check", mCheckCondition);

            LPCMapping data = LPCMapping.Empty;
            data.Add("join_condition", condition);

            // 需改入会条件
            GangMgr.SetGangInformation(data);
        }

        mSelectRankWnd.SetActive(false);

        // 刷新段位条件显示
        RefreshRankCondition();
    }

    /// <summary>
    /// 审核条件按钮点击事件回调
    /// </summary>
    void OnClickSelectCheckCondition(GameObject go)
    {
        mSelectRankWnd.SetActive(false);
        mApplicationConditionWnd.SetActive(true);

        // 默认选项
        if (mCheckCondition < 0)
            mCheckCondition = (int) CHECK_CONDITION.NO_CHECK;

        switch (mCheckCondition)
        {
            case (int) CHECK_CONDITION.NO_CHECK:
                mNoCheckBtn.GetComponent<UIToggle>().Set(true);
                break;

            case (int) CHECK_CONDITION.NEED_CHECK:
                mNeedCheckBtn.GetComponent<UIToggle>().Set(true);
                break;

            case (int) CHECK_CONDITION.REFUSE_JION:
                mRefuseJionBtn.GetComponent<UIToggle>().Set(true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 段位选择按钮点击事件回调
    /// </summary>
    void OnClickSelectRankConditinBtn(GameObject go)
    {
        mSelectRankWnd.SetActive(true);
        mApplicationConditionWnd.SetActive(false);

        // 默认选项
        if (mStep < -1)
            mStep = -1;

        for (int i = 0; i < mRankConditionItem.Length; i++)
        {
            ConditionItemWnd conditionItemWnd = mRankConditionItem[i].GetComponent<ConditionItemWnd>();
            if (conditionItemWnd == null)
                return;

            if (conditionItemWnd.mStep != mStep)
                continue;

            mRankConditionItem[i].GetComponent<UIToggle>().Set(true);
        }
    }

    /// <summary>
    /// 申请条件窗口关闭点击回调
    /// </summary>
    void OnClickCloseApplicationCondition(GameObject go)
    {
        mApplicationConditionWnd.SetActive(false);

        RefreshCheckCondition();
    }

    /// <summary>
    /// 直接加入按钮点击回调
    /// </summary>
    void OnClickNoCheckBtn(GameObject go)
    {
        if (mCheckCondition != (int) CHECK_CONDITION.NO_CHECK)
        {
            mCheckCondition = (int) CHECK_CONDITION.NO_CHECK;

            // 修改公会条件
            LPCMapping condition = LPCMapping.Empty;
            condition.Add("step", mStep);
            condition.Add("check", mCheckCondition);

            LPCMapping data = LPCMapping.Empty;
            data.Add("join_condition", condition);

            // 需改入会条件
            GangMgr.SetGangInformation(data);
        }

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();
    }

    /// <summary>
    /// 需要审核按钮点击回调
    /// </summary>
    void OnClickNeedCheckBtn(GameObject go)
    {
        if (mCheckCondition != (int) CHECK_CONDITION.NEED_CHECK)
        {
            mCheckCondition = (int) CHECK_CONDITION.NEED_CHECK;

            // 修改公会条件
            LPCMapping condition = LPCMapping.Empty;
            condition.Add("step", mStep);
            condition.Add("check", mCheckCondition);

            LPCMapping data = LPCMapping.Empty;
            data.Add("join_condition", condition);

            // 需改入会条件
            GangMgr.SetGangInformation(data);
        }

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();
    }

    /// <summary>
    /// 拒绝加入按钮点击回调
    /// </summary>
    void OnClickRefuseJionBtn(GameObject go)
    {
        if (mCheckCondition != (int) CHECK_CONDITION.REFUSE_JION)
        {
            mCheckCondition = (int) CHECK_CONDITION.REFUSE_JION;

            // 修改公会条件
            LPCMapping condition = LPCMapping.Empty;
            condition.Add("step", mStep);
            condition.Add("check", mCheckCondition);

            LPCMapping data = LPCMapping.Empty;
            data.Add("join_condition", condition);

            // 需改入会条件
            GangMgr.SetGangInformation(data);
        }

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();
    }

    /// <summary>
    /// 段位选择窗口
    /// </summary>
    void OnClickCloseSelectRankWnd(GameObject go)
    {
        mSelectRankWnd.SetActive(false);

        RefreshRankCondition();
    }
}
