/// <summary>
/// GangMemberItemWnd.cs
/// Created by fengsc 2018/01/26
/// 公会成员基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangMemberItemWnd : WindowBase<GangMemberItemWnd>
{
    #region 成员

    // 玩家头像
    public UITexture mIcon;

    // 玩家等级
    public UILabel mLevel;

    // 玩家名称
    public UILabel mName;

    // 踢出公会按钮
    public GameObject mDismissGangBtn;

    // 登录时间
    public UILabel mLoginTime;

    // 会长、副会长标识
    public UILabel mLeader;

    // 解除副会长按钮
    public UILabel mDismissDeputyLeader;

    // 竞技场星级
    public UISprite[] mStars;

    // 竞技场积分
    public UILabel mArenaScore;

    // 查看按钮
    public GameObject mViewBtn;
    public UILabel mViewBtnLb;

    public GameObject mViewBtnMask;

    public GameObject mBgMask;

    LPCMapping mData = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        mViewBtnLb.text = LocalizationMgr.Get("GangWnd_28");
    }

    /// <summary>
    /// 踢出公会按钮点击事件
    /// </summary>
    void OnClickDismissGangBtn(GameObject go)
    {
        // 退出公会
        if (mData.GetValue<string>("rid") == ME.user.GetRid())
        {
            if (mData.GetValue<string>("station") == "gang_leader")
                return;

            DialogMgr.ShowDailog(new CallBack(OnQuitCallBack), string.Format(LocalizationMgr.Get("GangWnd_144"), GameSettingMgr.GetSettingInt("lift_gang_cd") / 3600));

            return;
        }

        // 需先解除副会长才能踢出该会员
        if (mData.GetValue<string>("station") == "gang_deputy_leader")
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_30"));

            return;
        }

        DialogMgr.ShowDailog(new CallBack(OnDismissConfirmCallBack), string.Format(LocalizationMgr.Get("GangWnd_29"), mData.GetValue<string>("name")));
    }

    /// <summary>
    /// 退出公会确认回调
    /// </summary>
    void OnQuitCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 退出公会
        GangMgr.LeftGang();
    }

    /// <summary>
    /// 提出公会确认框回调
    /// </summary>
    void OnDismissConfirmCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 踢出公会成员
        GangMgr.RemoveGroupMember(mData.GetValue<string>("rid"));
    }

    /// <summary>
    /// 解除副会长按钮点击事件
    /// </summary>
    void OnClickDismissDeputyLeader(GameObject go)
    {
        // 确认框
        DialogMgr.ShowDailog(new CallBack(OnDismissDeputyLeaderCallback), LocalizationMgr.Get("GangWnd_31"));
    }

    void OnDismissDeputyLeaderCallback(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 解除副会长
        GangMgr.AppointDeputyLeader(mData.GetValue<string>("rid"), false);
    }

    /// <summary>
    /// 查看玩家信息按钮点击事件
    /// </summary>
    void OnClickViewBtn(GameObject go)
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
        Operation.CmdDetailAppearance.Go(DomainAddress.GenerateDomainAddress("c@" + mData.GetValue<string>("rid"), "u", 0));
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mDismissGangBtn).onClick = OnClickDismissGangBtn;
        UIEventListener.Get(mDismissDeputyLeader.gameObject).onClick = OnClickDismissDeputyLeader;
        UIEventListener.Get(mViewBtn).onClick = OnClickViewBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 加载玩家头像
        LPCValue iconValue = mData.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        // 显示玩家等级
        mLevel.text = string.Format(LocalizationMgr.Get("GangWnd_22"), mData.GetValue<int>("level"));

        // 显示玩家名称
        mName.text = mData.GetValue<string>("name");

        // 置灰自身的信息查看按钮
        if (mData.GetValue<string>("rid") == ME.user.GetRid())
        {
            mViewBtnMask.SetActive(true);
        }
        else
        {
            mViewBtnMask.SetActive(false);
        }

        // 好友在线
        if (mData.GetValue<int>("online") == 1)
        {
            mLoginTime.text = string.Format(LocalizationMgr.Get("GangWnd_27"));
        }
        else
        {
            int lastLogoutTime = mData.GetValue<int>("last_logout_time");
            mLoginTime.text = string.Format(LocalizationMgr.Get("GangWnd_25"), TimeMgr.ConvertTimeToSimpleChinese(lastLogoutTime));
        }

        mDismissDeputyLeader.text = LocalizationMgr.Get("GangWnd_26");
        mDismissDeputyLeader.gameObject.SetActive(false);

        mLoginTime.transform.localPosition = new Vector3(-376, -19.5f, 0);
        mDismissGangBtn.SetActive(false);

        mBgMask.SetActive(false);

        LPCMapping myGangInfo = LPCMapping.Empty;

        // 公会数据
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
            myGangInfo = v.AsMapping;

        string myStation = myGangInfo.GetValue<string>("station");

        string station = mData.GetValue<string>("station");

        mLeader.gameObject.SetActive(true);

        if (myStation == "gang_leader")
        {
            switch (station)
            {
                // 会长
                case "gang_leader":

                    mLeader.text = LocalizationMgr.Get("GangWnd_23");

                    mLoginTime.transform.localPosition = new Vector3(-405.8f, -19.5f, 0);

                    break;

                // 副会长
                case "gang_deputy_leader":

                    mLeader.text = LocalizationMgr.Get("GangWnd_24");

                    mDismissDeputyLeader.gameObject.SetActive(true);

                    mDismissGangBtn.SetActive(true);

                    break;

                // 普通会员
                default:

                    mLeader.gameObject.SetActive(false);

                    mBgMask.SetActive(true);

                    mDismissGangBtn.SetActive(true);

                    break;
            }
        }
        else if (myStation == "gang_deputy_leader")
        {
            switch (station)
            {
                // 会长
                case "gang_leader":

                    mLeader.text = LocalizationMgr.Get("GangWnd_23");

                    mLoginTime.transform.localPosition = new Vector3(-405.8f, -19.5f, 0);

                    break;

                    // 副会长
                case "gang_deputy_leader":

                    mLeader.text = LocalizationMgr.Get("GangWnd_24");

                    mDismissGangBtn.SetActive(true);

                    break;

                    // 普通会员
                default:

                    mLeader.gameObject.SetActive(false);

                    mBgMask.SetActive(true);

                    mDismissGangBtn.SetActive(true);

                    break;
            }
        }
        else
        {
            switch (station)
            {
                // 会长
                case "gang_leader":

                    mLeader.text = LocalizationMgr.Get("GangWnd_23");

                    mLoginTime.transform.localPosition = new Vector3(-405.8f, -19.5f, 0);

                    break;

                // 副会长
                case "gang_deputy_leader":

                    mLeader.text = LocalizationMgr.Get("GangWnd_24");

                    mLoginTime.transform.localPosition = new Vector3(-405.8f, -19.5f, 0);

                    break;

                // 普通会员
                default:

                    mLeader.gameObject.SetActive(false);

                    mBgMask.SetActive(true);

                    if (mData.GetValue<string>("rid") == ME.user.GetRid())
                        mDismissGangBtn.SetActive(true);

                    break;
            }
        }

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        // 竞技场数据
        LPCMapping arenaTop = mData.GetValue<LPCMapping>("arena_top");
        if (arenaTop == null)
            return;

        // 竞技场积分
        int score = arenaTop.GetValue<int>("score");

        mArenaScore.text = score.ToString();

        // 段位
        int step = ArenaMgr.GetStepByScoreAndRank(arenaTop.GetValue<int>("rank"), score);

        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
        if (row != null)
        {
            for (int j = 0; j < row.Query<int>("star"); j++)
                mStars[j].spriteName = row.Query<string>("star_name");
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mData = data;

        // 绘制窗口
        Redraw();
    }
}
