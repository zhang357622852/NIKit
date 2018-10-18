/// <summary>
/// GangDetailsWnd.cs
/// Created by fengsc 2018/01/31
/// 公会详情界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangDetailsWnd : WindowBase<GangDetailsWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 公会旗帜基础格子
    public FlagItemWnd mFlagItemWnd;

    // 公会名称
    public UILabel mGangName;

    // 会长
    public UILabel mGangLeaderTips;
    public UILabel mGangLeaderName;
    public UISprite mGender;

    // 公会人数
    public UILabel mAmountTips;
    public UILabel mAmount;

    // 公会实力
    public UILabel mStrengthTips;
    public UISprite[] mStrengthStars;

    // 公会介绍
    public UILabel mIntroduceTips;
    public UILabel mIntroduce;

    // 申请加入按钮
    public UISprite mApplicationBtn;
    public UILabel mApplicationBtnLb;

    public TweenScale mTweenScale;

    public GameObject mDetalisItem;

    public UIScrollView mUIScrollView;

    // 排序控件
    public UIGrid mGrid;

    LPCMapping mGangInfo = LPCMapping.Empty;

    // 成员列表
    LPCArray mMemberList = LPCArray.Empty;

    List<GameObject> mItems = new List<GameObject>();

    #endregion

    // Use this for initialization
    void Start ()
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
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 解注册事件
        EventMgr.UnregisterEvent("GangDetailsWnd");

        // 移除字段关注
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("GangDetailsWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mGangLeaderTips.text = LocalizationMgr.Get("GangWnd_6");
        mAmountTips.text = LocalizationMgr.Get("GangWnd_7");
        mStrengthTips.text = LocalizationMgr.Get("GangWnd_8");
        mIntroduceTips.text = LocalizationMgr.Get("GangWnd_9");
        mTitle.text = LocalizationMgr.Get("GangWnd_67");
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
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// tween动画播放完成回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        if (ME.user == null)
            return;

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField("GangDetailsWnd", new string[]{"user_requests"}, new CallBack(OnUserRequestChange));
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
        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);

        if (mGangInfo == null || mGangInfo.Count == 0)
            return;

        mUIScrollView.ResetPosition();

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

        RefreshApplicationButtonState();

        RefreshMemberList();
    }

    /// <summary>
    /// 刷新成员列表
    /// </summary>
    void RefreshMemberList()
    {
        if (mDetalisItem.activeSelf)
            mDetalisItem.SetActive(false);

        if (mMemberList == null || mMemberList.Count == 0)
            return;

        if (mMemberList.Count > mItems.Count)
        {
            int count = mMemberList.Count - mItems.Count;

            for (int i = 0; i < count; i++)
            {
                GameObject clone = Instantiate(mDetalisItem);

                clone.transform.SetParent(mGrid.transform);

                clone.transform.localPosition = Vector3.zero;

                clone.transform.localScale = Vector3.one;

                mItems.Add(clone);
            }
        }

        for (int i = 0; i < mMemberList.Count; i++)
        {
            GameObject item = mItems[i];
            if (item == null)
                continue;

            item.SetActive(true);

            GangDetalisItemWnd script = item.GetComponent<GangDetalisItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(mMemberList[i].AsMapping);
        }

        // 隐藏多余的格子
        for (int i = mMemberList.Count; i < mItems.Count; i++)
            mItems[i].SetActive(false);

        // 排序子控件
        mGrid.Reposition();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mGangInfo = data.GetValue<LPCMapping>("relation_data");

        // 成员列表
        mMemberList = GangMgr.SortGangMemberList(data.GetValue<LPCArray>("members"));
    }
}
