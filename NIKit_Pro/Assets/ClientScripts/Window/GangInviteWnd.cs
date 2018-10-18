/// <summary>
/// GangInviteWnd.cs
/// Created by fengsc 2018/02/05
/// 公会邀请窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangInviteWnd : WindowBase<GangInviteWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    public UILabel mInputTips;

    // 名称输入框
    public UIInput mInput;

    // 搜索按钮
    public GameObject mQueryBtn;

    // 刷新列表按钮
    public UILabel mChangeBtn;

    public UILabel mRecommendTips;

    public UILabel mTips1;
    public UILabel mTips2;

    // 段位要求
    public GameObject mArenaRankBtn;
    public UILabel mArenaRankBtnLb;

    public GameObject mStars;

    public UISprite[] mArenaRankStars;

    public GameObject[] mRankConditionItem;

    public GameObject mSelectRankWnd;

    public GameObject mCloseSelectRankWnd;

    public GameObject mMemberItem;

    public UIScrollView mUIScrollView;

    public TweenScale mTweenScale;

    // 排序组件
    public UIGrid mGrid;

    bool misOperateGang = false;

    bool mIsFind = false;

    int mStep = -1;

    // 玩家列表
    LPCArray mUserList = LPCArray.Empty;

    List<GameObject> mItems = new List<GameObject>();

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
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("GangInviteWnd");

        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_GET_RECOMMEND_USER", "GangInviteWnd");

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        if (ME.user == null)
            return;

        // 移除字段关注
        ME.user.dbase.RemoveTriggerField("GangInviteWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mRecommendTips.text = LocalizationMgr.Get("GangWnd_104");
        mTips1.text = LocalizationMgr.Get("CreateGuildWnd_12");
        mTips2.text = LocalizationMgr.Get("GangWnd_105");
        mArenaRankBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_14");
        mTitle.text = LocalizationMgr.Get("GangWnd_101");
        mInputTips.text = LocalizationMgr.Get("GangWnd_102");
        mInput.defaultText = LocalizationMgr.Get("GangWnd_103");
        mChangeBtn.text = LocalizationMgr.Get("GangWnd_106");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseSelectRankWnd).onClick = OnClickCloseSelectRankWnd;
        UIEventListener.Get(mArenaRankBtn).onClick = OnClickSelectRankConditinBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mQueryBtn).onClick = OnClickQueryBtn;
        UIEventListener.Get(mChangeBtn.gameObject).onClick = OnClickChangeBtn;

        for (int i = 0; i < mRankConditionItem.Length; i++)
            UIEventListener.Get(mRankConditionItem[i]).onClick = OnClickRankConditionItem;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenScaleFinish);

        // 监听玩家信息请求事件
        EventMgr.RegisterEvent("GangInviteWnd", EventMgrEventType.EVENT_SEARCH_DETAIL_INFO_SUCC, OnEventCallBack);

        // 关注MSG_GET_RECOMMEND_USER消息
        MsgMgr.RegisterDoneHook("MSG_GET_RECOMMEND_USER", "GangInviteWnd", OnMsgGetRecommendUser);

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("GangInviteWnd", new string[]{ "my_gang_info" }, new CallBack(OnMyGangInfoChange));
    }

    /// <summary>
    /// 玩家信息请求事件回调
    /// </summary>
    void OnEventCallBack(int eventId, MixedValue para)
    {
        if (!mIsFind)
            return;

        LPCMapping args = para.GetValue<LPCMapping>();

        if (args == null || args.Count == 0)
            return;

        LPCMapping detailsData = args["data"].AsMapping;
        if (detailsData == null || detailsData.Count == 0)
            return;

        LPCMapping arenaTop = LPCMapping.Empty;

        if (detailsData.ContainsKey("arena_top"))
            arenaTop = detailsData.GetValue<LPCMapping>("arena_top");

        // 构造简要数据
        LPCMapping data = LPCMapping.Empty;
        data.Add("name", detailsData.GetValue<string>("name"));
        data.Add("icon", detailsData.GetValue<string>("icon"));
        data.Add("level", detailsData.GetValue<int>("level"));
        data.Add("rid", detailsData.GetValue<string>("rid"));
        data.Add("rank", arenaTop.GetValue<int>("rank"));
        data.Add("score", arenaTop.GetValue<int>("score"));
        data.Add("last_logout_time", detailsData.GetValue<int>("last_logout_time"));
        data.Add("last_login_time", detailsData.GetValue<int>("last_login_time"));

        // 清空输入框
        mInput.value = string.Empty;

        mUserList = LPCArray.Empty;

        mUserList.Add(data);

        // 刷新数据
        RefreshData();

        mUIScrollView.ResetPosition();

        mIsFind = false;
    }

    /// <summary>
    /// my_gang_info字段变化
    /// </summary>
    void OnMyGangInfoChange(object para, params object[] param)
    {
        // 刷新数据
        RefreshData();
    }

    /// <summary>
    /// MSG_GET_RECOMMEND_USER消息回调
    /// </summary>
    void OnMsgGetRecommendUser(string cmd, LPCValue para)
    {
        // 玩家列表
        mUserList = para.AsMapping.GetValue<LPCArray>("user_list");

        // 刷新数据
        RefreshData();

        mUIScrollView.ResetPosition();
    }

    void OnTweenScaleFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mStep = -1;

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

        RefreshRankCondition();

        // 获取推荐玩家
        GangMgr.GetRecommendUser(mStep);
    }

    /// <summary>
    /// 刷新列表数据
    /// </summary>
    void RefreshData()
    {
        if (mUserList.Count > mItems.Count)
        {
            int count = mUserList.Count - mItems.Count;

            if (mMemberItem.activeSelf)
                mMemberItem.SetActive(false);

            for (int i = 0; i < count; i++)
            {
                GameObject clone = Instantiate(mMemberItem);

                clone.transform.SetParent(mGrid.transform);

                clone.transform.localPosition = Vector3.zero;

                clone.transform.localScale = Vector3.one;

                mItems.Add(clone);
            }
        }

        LPCArray gangRequests = LPCArray.Empty;

        // 公会数据
        LPCValue value = ME.user.Query<LPCValue>("my_gang_info");
        if (value != null && value.IsMapping)
        {
            gangRequests = value.AsMapping.GetValue<LPCArray>("gang_requests");

            if (gangRequests == null)
                gangRequests = LPCArray.Empty;
        }

        for (int i = 0; i < mUserList.Count; i++)
        {
            if (! mUserList[i].IsMapping)
                continue;

            GameObject item = mItems[i];

            MemberManageItemWnd script = item.GetComponent<MemberManageItemWnd>();
            if (script == null)
                continue;

            LPCMapping userData = mUserList[i].AsMapping;

            // 构造数据
            LPCMapping data = LPCMapping.Empty;
            LPCMapping arenaTop = LPCMapping.Empty;
            arenaTop.Add("rank", userData.GetValue<int>("rank"));
            arenaTop.Add("score", userData.GetValue<int>("score"));

            data.Add("arena_top", arenaTop);
            data.Add("icon", userData.GetValue<string>("icon"));
            data.Add("name", userData.GetValue<string>("name"));
            data.Add("level", userData.GetValue<int>("level"));
            data.Add("rid", userData.GetValue<string>("rid"));
            data.Add("last_logout_time", userData.GetValue<int>("last_logout_time"));
            data.Add("last_login_time", userData.GetValue<int>("last_login_time"));

            // 绑定数据
            script.Bind(data, new Vector3(126.4f, -6, 0), new Vector3(200, -6, 0));

            script.SetCallBack(new CallBack(OnInviteCallBack));

            item.SetActive(true);

            // 请求id
            string requestId = string.Format("{0}{1}{2}", userData.GetValue<string>("rid"), "_", GangMgr.GangDetail.GetValue<string>("relation_tag"));

            if (gangRequests.IndexOf(requestId) == -1)
                script.Select(false);
            else
                script.Select(true);
        }

        // 隐藏多余的格子
        for (int i = mUserList.Count; i < mItems.Count; i++)
            mItems[i].SetActive(false);

        // 排序控件
        mGrid.Reposition();
    }

    /// <summary>
    /// 邀请按钮点击事件回调
    /// </summary>
    void OnInviteCallBack(object para, params object[] param)
    {
        if (misOperateGang)
            return;

        misOperateGang = true;

        // 玩家数据
        LPCMapping data = param[0] as LPCMapping;

        string name = data.GetValue<string>("name");
        if (name == ME.user.GetName())
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_129"));

            misOperateGang = false;

            return;
        }

        GangMgr.InviteJoinGang(name);

        misOperateGang = false;
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

        mStep = script.mStep;

        mSelectRankWnd.SetActive(false);

        // 刷新段位条件显示
        RefreshRankCondition();

        // 获取推荐玩家
        GangMgr.GetRecommendUser(mStep);
    }

    /// <summary>
    /// 段位选择按钮点击事件回调
    /// </summary>
    void OnClickSelectRankConditinBtn(GameObject go)
    {
        mSelectRankWnd.SetActive(true);

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
    /// 段位选择窗口
    /// </summary>
    void OnClickCloseSelectRankWnd(GameObject go)
    {
        mSelectRankWnd.SetActive(false);

        RefreshRankCondition();
    }

    /// <summary>
    /// 查询按钮点击回调
    /// </summary>
    void OnClickQueryBtn(GameObject go)
    {
        if (misOperateGang)
            return;

        misOperateGang = true;

        if (string.IsNullOrEmpty(mInput.value))
        {
            misOperateGang = false;
            return;
        }

        string str = string.Empty;

        str = mInput.value;

        if (str.Equals(ME.user.GetName()))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_130"));

            misOperateGang = false;
            return;
        }

        if (string.IsNullOrEmpty(str))
        {
            misOperateGang = false;
            return;
        }

        mIsFind = true;

        // 通知服务器查找玩家，获取玩家详细信息
        Operation.CmdDetailAppearance.Go(string.Empty, str);

        misOperateGang = false;
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 刷新玩家列表按钮点击回调
    /// </summary>
    void OnClickChangeBtn(GameObject go)
    {
        if (misOperateGang)
            return;

        misOperateGang = true;

        // 获取推荐玩家
        GangMgr.GetRecommendUser(mStep);

        misOperateGang = false;
    }
}
