/// <summary>
/// MyFriendItemWnd.cs
/// Created by fengsc 2017/01/19
/// 好友基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class MyFriendItemWnd : WindowBase<MyFriendItemWnd>
{
    #region 成员变量

    // 玩家头像
    public UITexture mIcon;

    // 玩家等级
    public UILabel mLevel;

    // 玩家名称
    public UILabel mName;

    //来自邀请ID
    public UILabel mInviteLab;

    // 删除玩家按钮
    public GameObject mDeletePlayerBtn;

    // 最近登陆状态
    public UILabel mLoginState;

    // 段位背景
    public UISprite mRankBg;

    // 段位图标
    public UISprite mRankIcon;

    // 星级
    public UISprite[] mStars;

    // 竞技场积分
    public UILabel mScore;

    // 段位名称
    public UILabel mRankName;

    // 赠送按钮
    public GameObject mGiveBtn;
    public UILabel mGiveBtnLb;
    public GameObject mGiveBtnMask;

    // 查看按钮
    public GameObject mLookBtn;
    public UILabel mLookBtnLb;

    public MyFriendWnd mMyFriendWnd;

    public GameObject mGiveAnima;

    public UILabel FP;

    public TweenPosition mTweenPosition;

    public TweenAlpha mTweenAlpha;

    // 玩家数据
    LPCMapping mUser = LPCMapping.Empty;

    // 初始状态为true
    bool mGivingState = true;

    #endregion

    // Use this for initialization
    void Start ()
    {
        InitText();

        RegisterEvent();
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField(gameObject.name);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mGiveBtnLb.text = LocalizationMgr.Get("MyFriendWnd_3");
        mLookBtnLb.text = LocalizationMgr.Get("MyFriendWnd_4");
        mInviteLab.text = LocalizationMgr.Get("FriendRequestWnd_5");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mDeletePlayerBtn).onClick = OnClickDeletePlayerBtn;
        UIEventListener.Get(mGiveBtn).onClick = OnClickGiveBtn;
        UIEventListener.Get(mLookBtn).onClick = OnClickLookBtn;

        // 关注字段的变化
        ME.user.dbase.RegisterTriggerField(gameObject.name, new string[]{"giving_list"}, new CallBack(OnFieldChange));
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    void OnFieldChange(object para, params object[] param)
    {
        // 刷新按钮的状态
        RefreshGiveButtonState();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        RefreshGiveButtonState();

        // 等级
        mLevel.text = string.Format(LocalizationMgr.Get("MyFriendWnd_1"), mUser.GetValue<int>("level"));

        // 名称
        mName.text = mUser.GetValue<string>("name");

        //来自邀请ID
        mInviteLab.gameObject.SetActive(ME.user.GetRid().Equals(mUser.GetValue<string>("invite_id")));

        // 好友在线
        if (mUser.GetValue<int>("online") == 1)
        {
            mLoginState.text = mLoginState.text = string.Format(LocalizationMgr.Get("MyFriendWnd_6"), mUser.GetValue<int>("chatroom"));
        }
        else
        {
            int lastLogoutTime = mUser.GetValue<int>("last_logout_time");

            if (TimeMgr.GetServerTime() - lastLogoutTime <= 60)
                mLoginState.text = string.Format(LocalizationMgr.Get("MyFriendWnd_2"), LocalizationMgr.Get("MyFriendWnd_11"));
            else
                mLoginState.text = string.Format(LocalizationMgr.Get("MyFriendWnd_2"), TimeMgr.ConvertTimeToSimpleChinese(lastLogoutTime));
        }

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        mRankIcon.spriteName = "ordinary_icon";

        mRankBg.spriteName = "ordinary_bg";

        // 加载玩家头像
        LPCValue iconValue = mUser.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        LPCMapping arenaTop = mUser.GetValue<LPCMapping>("arena_top");

        mScore.text = 0.ToString();

        mRankName.text = string.Empty;

        if (arenaTop == null)
            return;

        // 竞技场排名
        int rank = arenaTop.GetValue<int>("rank") + 1;

        // 竞技场积分
        int score = arenaTop.GetValue<int>("score");

        // 竞技场积分
        mScore.text = Mathf.Max(0, score).ToString();

        // 竞技场阶位
        int step = ArenaMgr.GetStepByScoreAndRank(rank - 1, score);

        // 获取配置表数据
        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);

        if (row == null)
            return;

        mRankIcon.spriteName = row.Query<string>("rank_icon");

        mRankName.text = LocalizationMgr.Get(row.Query<string>("name"));

        mRankBg.spriteName = row.Query<string>("rank_bg");

        // 星级
        int star = row.Query<int>("star");

        for (int i = 0; i < star; i++)
            mStars[i].spriteName = row.Query<string>("star_name");
    }

    /// <summary>
    /// 刷新赠送按钮的状态
    /// </summary>
    void RefreshGiveButtonState()
    {
        // 赠送列表
        LPCArray givingList = ME.user.Query<LPCArray>("giving_list");

        if(givingList == null)
            givingList = LPCArray.Empty;

        bool curState = (givingList.IndexOf(mUser.GetValue<string>("rid")) == -1);

        // 状态不需要刷新
        if (mGivingState == curState)
            return;

        SetGiveButtonState(curState);

        mGivingState = curState;
    }

    /// <summary>
    /// 设置赠送按钮状态
    /// </summary>
    /// <param name="state">If set to <c>true</c> state.</param>
    void SetGiveButtonState(bool state)
    {
        UISprite sprite = mGiveBtn.GetComponent<UISprite>();

        float rgb = state ? 255 / 255f : 128 / 255f;

        sprite.color = new Color(rgb, rgb, rgb);
        mGiveBtnMask.SetActive(state ? false : true);
    }

    /// <summary>
    /// 删除好友点击事件
    /// </summary>
    void OnClickDeletePlayerBtn(GameObject go)
    {
        DialogMgr.ShowDailog(
            new CallBack(ClickDeleteCallBack),
            string.Format(LocalizationMgr.Get("MyFriendWnd_5"), mUser.GetValue<string>("name")),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            WindowMgr.GetWindow(FriendWnd.WndType).transform
        );
    }

    void ClickDeleteCallBack(object para, params object[] param)
    {
        if (!(bool) param[0])
            return;

        // 通知服务器删除好友
        Operation.CmdFriendRemove.Go(mUser.GetValue<string>("rid"));
    }

    /// <summary>
    /// 赠送按钮点击事件
    /// </summary>
    void OnClickGiveBtn(GameObject go)
    {
        if (mUser == null || mUser.Count < 1)
            return;

        LPCValue v = ME.user.Query("fp");
        if (v != null && v.IsInt)
        {
            if (v.AsInt >= GameSettingMgr.GetSettingInt("max_fp_limit"))
            {
                // 弹出提示框, 友情点达到最大限制
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("MyFriendWnd_10"),
                    string.Empty,
                    string.Empty,
                    true,
                    WindowMgr.GetWindow(FriendWnd.WndType).transform
                );

                return;
            }
        }

        // 今天已经赠送过
        if (! mGivingState)
        {
            int limitTime = (int)Game.GetZeroClock(1);

            string timeDesc;

            if (limitTime / 3600 > 0)
                timeDesc = string.Format("{0}{1}", limitTime / 3600, LocalizationMgr.Get("MyFriendWnd_8"));
            else if(limitTime / 60 > 0)
                timeDesc = string.Format("{0}{1}", limitTime / 60, LocalizationMgr.Get("MyFriendWnd_9"));
            else
                timeDesc = string.Format("{0}{1}", 1, LocalizationMgr.Get("MyFriendWnd_9"));

            DialogMgr.ShowSingleBtnDailog(
                null,
                string.Format(LocalizationMgr.Get("MyFriendWnd_7"), timeDesc),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(FriendWnd.WndType).transform
            );
            return;
        }

        FP.text = string.Format("+{0}", GameSettingMgr.GetSettingInt("giving_fp_base_value"));

        if (! mGiveAnima.activeSelf)
            mGiveAnima.SetActive(true);

        EventDelegate.Add(mTweenAlpha.onFinished, new EventDelegate.Callback(OnTweenFinsh));

        mTweenPosition.from = new Vector3(275, go.transform.localPosition.y, 0);

        mTweenPosition.to = new Vector3(275, go.transform.localPosition.y + 36, 0);

        mTweenPosition.PlayForward();

        mTweenAlpha.PlayForward();

        // 通知服务器赠送好友友情点数
        Operation.CmdGiftGiving.Go(mUser.GetValue<string>("rid"));
    }

    void OnTweenFinsh()
    {
        mTweenPosition.ResetToBeginning();

        mTweenAlpha.ResetToBeginning();

        mGiveAnima.SetActive(false);
    }

    /// <summary>
    /// 查看按钮点击事件
    /// </summary>
    void OnClickLookBtn(GameObject go)
    {
        // 先显示界面后填写数据
        GameObject wnd = WindowMgr.OpenWnd(FriendViewWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
        {
            LogMgr.Trace("FriendViewWnd窗口创建失败");
            return;
        }

        // 通知服务器请求数据
        Operation.CmdDetailAppearance.Go(DomainAddress.GenerateDomainAddress("c@" + mUser.GetValue<string>("rid"), "u", 0));
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        if (data == null)
            return;

        mUser = data;
        Redraw();
    }
}
