/// <summary>
/// MemberManageItemWnd.cs
/// Created by fengsc 2018/01/27
/// 公会成员管理基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class MemberManageItemWnd : WindowBase<MemberManageItemWnd>
{
    #region 成员

    // 玩家头像
    public UITexture mIcon;

    // 玩家等级
    public UILabel mLevel;

    // 玩家名称
    public UILabel mName;

    // 登录时间
    public UILabel mLoginTime;

    // 竞技场星级
    public UISprite[] mStars;

    // 竞技场积分
    public UILabel mArenaScore;

    // 查看按钮
    public GameObject mViewBtn;
    public UILabel mViewBtnLb;

    // 解除副会长按钮
    public UILabel mDismissDeputyLeader;

    // 拒绝按钮
    public GameObject mRefuseBtn;

    // 操作确认按钮
    public GameObject mConfirmBtn;

    public GameObject mMask;

    LPCMapping mData = LPCMapping.Empty;

    bool mIsDeputyLeader = false;

    CallBack[] mCallBacks;

    bool mIsOperateGang = false;

    Vector3 mNormalPos = Vector3.zero;

    Vector3 mSinglePos = Vector3.zero;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 踢出公会按钮点击事件
    /// </summary>
    void OnClickDismissGangBtn(GameObject go)
    {
        // 需先解除副会长才能踢出该会员
        if (mData.GetValue<string>("station") == "gang_deputy_leader")
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_30"));

            return;
        }

        DialogMgr.ShowDailog(new CallBack(OnDismissConfirmCallBack), string.Format(LocalizationMgr.Get("GangWnd_29"), mData.GetValue<string>("name")));
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
    /// 操作确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        if (mIsOperateGang)
            return;

        mIsOperateGang = true;

        // 执行回调，具体操作由主界面处理
        if (mCallBacks[0] != null)
            mCallBacks[0].Go(mData);

        mIsOperateGang = false;
    }

    /// <summary>
    /// 拒绝按钮点击回调
    /// </summary>
    void OnClickRefuseBtn(GameObject go)
    {
        if (mIsOperateGang)
            return;

        mIsOperateGang = true;

        if (mCallBacks[1] != null)
            mCallBacks[1].Go(mData);

        mIsOperateGang = false;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mDismissDeputyLeader.gameObject).onClick = OnClickDismissDeputyLeader;
        UIEventListener.Get(mViewBtn).onClick = OnClickViewBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;

        if (mRefuseBtn != null)
            UIEventListener.Get(mRefuseBtn).onClick = OnClickRefuseBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mMask != null)
            mMask.SetActive(false);

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

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        // 竞技场数据
        LPCMapping arenaTop = mData.GetValue<LPCMapping>("arena_top");
        if (arenaTop != null)
        {
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

        int lastLoginTime = mData.GetValue<int>("last_login_time");

        int lastLogoutTime = mData.GetValue<int>("last_logout_time");

        // 好友在线
        if (lastLoginTime > lastLogoutTime)
        {
            mLoginTime.text = string.Format(LocalizationMgr.Get("GangWnd_27"));
        }
        else
        {
            mLoginTime.text = string.Format(LocalizationMgr.Get("GangWnd_25"), TimeMgr.ConvertTimeToSimpleChinese(lastLogoutTime));
        }

        mConfirmBtn.SetActive(false);

        mViewBtnLb.text = LocalizationMgr.Get("GangWnd_28");

        mDismissDeputyLeader.text = LocalizationMgr.Get("GangWnd_26");
        mDismissDeputyLeader.gameObject.SetActive(false);

        mDismissDeputyLeader.gameObject.SetActive(false);

        if (mIsDeputyLeader && mData.GetValue<string>("station") == "gang_deputy_leader")
        {
            mDismissDeputyLeader.gameObject.SetActive(true);

            mViewBtn.SetActive(true);
        }
        else
        {
            if (mData.GetValue<int>("state") == 0)
            {
                mViewBtn.transform.localPosition = mNormalPos;

                mConfirmBtn.SetActive(true);
            }
            else
            {
                mViewBtn.transform.localPosition = mSinglePos;

                mLoginTime.text = string.Format(LocalizationMgr.Get("GangWnd_134"));
            }

            mViewBtn.SetActive(true);
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data, Vector3 nomalPos, Vector3 singlePos, bool isTransferDuptyLeader = false)
    {
        mData = data;

        mIsDeputyLeader = isTransferDuptyLeader;

        mNormalPos = nomalPos;

        mSinglePos = singlePos;

        // 绘制窗口
        Redraw();
    }

    public void SetCallBack(params CallBack[] param)
    {
        mCallBacks = param;
    }

    public void Select(bool isSelect)
    {
        if (mMask != null)
            mMask.SetActive(isSelect);
    }
}
